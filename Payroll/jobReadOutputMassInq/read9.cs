using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Quartz;
using Quartz.Impl;
using log4net;
using BRIChannelSchedulerNew.Payroll.Helper;
using BRIChannelSchedulerNew.Payroll.Pocos;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using BriInterfaceWrapper.BriInterface;
using BRIVAWrapper.Briva;
using System.Threading;
using System.Data.SqlClient;
using System.Text.RegularExpressions;


namespace BRIChannelSchedulerNew.Payroll.jobReadOutputMassInq
{
    class read_9 : IStatefulJob
    {
        private static String SchCode = "read_9";
        private static String SchName = "Scheduller Payroll Download and Read Mass Inq";
        private static String schInfo = "=== " + DateTime.Now + " ";
        private static int jobNumber = 9;//trx yang diproses, ambil trxID kelipatan 10
        private static Configuration cfg;
        private static ISessionFactory factory;
        private String name = "";
        private String curr = "";
        protected ISession session;

        
        private DataTable dte = new DataTable("dte");
        #region IJob Members
        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(read_9));
            try
            {
                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }
                session = factory.OpenSession();
                Console.WriteLine(schInfo + SchCode + " === Starting " + SchName + " ===");

                try
                {
                    //CMS Oprasioanal Status dan Payroll Oprasional Status
                    //CMS Oprasional
                    Parameter paramCMS = session.Load<Parameter>("CMS_OPERASIONAL_STATUS");
                    String cmsOprasional = paramCMS.Data;
                    //Payroll Oprasional
                    Parameter paramPayroll = session.Load<Parameter>("PAYROLL_OPERASIONAL_STATUS");
                    String payrollOprasional = paramPayroll.Data;
                    if (cmsOprasional.Equals("1") && payrollOprasional.Equals("1"))
                    {
                        Commitjob_transaction(log);
                    }
                    else Console.WriteLine(schInfo + " === Starting " + SchName + "  CMS or Payroll Not in Oprasional Status ===");
                }
                catch (Exception ee)
                {
                    log.Error(SchCode + " === " + SchName + " Process Error Message:: " + ee.Message);
                    log.Error(SchCode + " === " + SchName + " Process Error Inner Exception:: " + ee.InnerException);
                    log.Error(SchCode + " === " + SchName + " Process Error StackTrace:: " + ee.StackTrace);
                }
                Console.WriteLine(schInfo + SchCode + " === End " + SchName + " ===");
            }
            catch (Exception ex)
            {
                log.Error(SchCode + " === " + SchName + " Failed To Start Message:: " + ex.Message);
                log.Error(SchCode + " === " + SchName + " Failed To Start Inner Exception:: " + ex.InnerException);
                log.Error(SchCode + " === " + SchName + " Failed To Start StackTrace:: " + ex.StackTrace);
            }
            finally
            {
                if (session.IsOpen)
                {
                    session.Clear();
                    session.Close();
                    session.Dispose();
                }
                factory.Close();
                factory.Dispose();

                cfg = null;
                GC.Collect();
            }
        }

        #endregion

        private void Commitjob_transaction(ILog log)
        {
            //initial parameters
            Parameter maxJob = session.Load<Parameter>("PAYROLL_SCH_JOB_READ_MASS_INQ");
            Parameter maxTrx = session.Load<Parameter>("PAYROLL_SCH_GENERATE_MASS_INQ_MAX_TRX");
            int maxResult = int.Parse(maxTrx.Data);

            IList<TrxPayroll> trxList = session.CreateSQLQuery("select * from trxpayrolls where status=? and (seqnumber%?)=? and lastupdate<=? order by id ASC")
                .AddEntity(typeof(TrxPayroll))
                .SetInt32(0, ParameterHelper.PAYROLL_WAITING_CHECKACC)//11 : waiting mass inq output
                .SetInt32(1, int.Parse(maxJob.Data))//id mod(10)
                .SetInt32(2, jobNumber)//id mod = ini mod 0, ambil trxID kelipatan 10
                .SetDateTime(3, DateTime.Now)
                //.SetMaxResults(maxResult)
                .List<TrxPayroll>();

            //we've found some data
            foreach (TrxPayroll trx in trxList)
            {
                string outStatus = "";
                Boolean isReject = false;
                //Pengecekkan retry, jika dr created time sudah 1 jam maka Lastupdate di tambah setMenitBye menit
                int setMenitBye = int.Parse(ParameterHelper.GetString("PAYROLL_SET_MINUTES_TROUBLE_MASSACC", session));//60
                int paramRetryPosition = int.Parse(ParameterHelper.GetString("PAYROLL_PARAMETER__MAX_RETRY_MASSACC", session));//10

                if (trx.RetryPosition > paramRetryPosition)
                {
                    trx.ErrorDescription += "|Transaksi terdeteksi sudah retry sebanyak >  " + paramRetryPosition.ToString() + "  kali, REJECT Transactions";
                    isReject = MassInquiryHelper.rejectPayroll(trx, session, log, trx.RetryPosition);
                    if (isReject)
                    {
                        log.Error(SchCode + " === (TrxID " + trx.Id + ") Transaksi terdeteksi sudah retry sebanyak >  " + paramRetryPosition.ToString() + "  kali, REJECT Transactions");
                    }
                    else
                    {
                        log.Error(SchCode + " === (TrxID " + trx.Id + ") Transaksi terdeteksi sudah retry sebanyak >  " + paramRetryPosition.ToString() + "  kali, GAGAL REJECT Transactions");
                        trx.LastUpdate = DateTime.Now;
                        trx.Status = ParameterHelper.TRXSTATUS_REJECT;//4
                        trx.Description = "ERROR - Problem in Host BRI, please contact Administrator";
                        trx.ErrorDescription += "|Reject Trx : Failed to Delete Transactions or Delete detail Payroll, Set Reject Payroll only";
                        session.Update(trx);
                        session.Flush();
                    }

                }
                else
                {
                    if (MassInquiryHelper.downloadHostResponse(trx, session, out outStatus, log, jobNumber))
                    {
                        /*  
                         * SUKSES file valid maka normal
                         * ERROR file jawaban host mengirim message error generate file lagi akan generate 10 menit
                         * RETRY file blm complete, blm ada footer add 2 minutes
                         * UNKNOWN unknown isi file Host belum dikenal, generate lagi add 10 menit
                         * INVALID cek footer gagal, retry reading 2 minutes
                         */
                        #region download true

                        Parameter retryAja = session.Load<Parameter>("PAYROLL_ADD_MINUTES_RETRY_MASSACC");
                        Parameter retryError = session.Load<Parameter>("PAYROLL_ADD_MINUTES_RETRY_MASSACC2");
                        int retryAjaInt = int.Parse(retryAja.Data);
                        int retryErrorInt = int.Parse(retryError.Data);
                        if (outStatus.Equals("SUKSES"))//jika sukses download
                        {
                            #region sukses
                            log.Info("Success download file.");
                            trx.ErrorDescription += schInfo + "Success download file";

                            if (MassInquiryHelper.parsingFile(trx, session, log, jobNumber, SchCode, out outStatus))
                            {
                                #region cek sudahkah anak2 keupdate semua?
                                string temp_query = "";
                                string msg = "";
                                bool lanjut = true;
                                int count = 0;
                                string dbResult = "";
                                bool result = true;
                                if (result)
                                {
                                    temp_query = @"select count(1) from trxpayrolldetails where pid = '" + trx.Id + "' and status = " + ParameterHelper.PAYROLLDETAILS_WAITING_CHECK + " and instructioncode = '" + TotalOBHelper.IFT + "';";

                                    if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                                    {
                                        log.Error(SchCode + " === (TrxID " + trx.Id + ") Failed Query: " + temp_query + " " + msg);
                                        result = false;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (int.TryParse(dbResult, out count))
                                            {
                                                if (count > 0)
                                                {
                                                    lanjut = false;
                                                    log.Info(SchCode + " === (TrxID " + trx.Id + ") Processing update Detail Transactions not completed, retry parsing again in 1 minutes");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            lanjut = false;
                                            log.Error("=== (TrxID " + trx.Id + ") Msg : " + ex.Message + "Stack  :" + ex.StackTrace + "Stack  :" + ex.InnerException);
                                        }
                                    }
                                }
                                #endregion

                                if (!lanjut && result)
                                {
                                    log.Error(SchCode + " === (TrxID " + trx.Id + ") Ada " + count.ToString() + " anak yang beum keupdate, ulangi transaksi " + retryAja.Data.ToString() + "  menit kemudian!");
                                    trx.LastUpdate = DateTime.Now.AddMinutes(retryAjaInt);
                                    session.Update(trx);
                                    session.Flush();
                                    return;
                                }

                                else if (result && lanjut)
                                {
                                    trx.Status = ParameterHelper.PAYROLL_SUCCESS_PARSING; //46
                                    trx.LastUpdate = DateTime.Now;
                                    log.Info(SchCode + " === (TrxID " + trx.Id + ") Finish Processing");
                                }

                                log.Info("Success Parsing File.");
                            }
                            else
                            {
                                if (outStatus.Equals("[NULL]"))
                                {
                                    //trx.Status = ParameterHelper.PAYROLL_WAITING_GENERATE_MASS_INQ;//10
                                    trx.LastUpdate = DateTime.Now.AddMinutes(retryAjaInt);
                                    trx.ErrorDescription += "|file balikan host 0 KB / Not Completed-Pending 2 minutes : " + trx.LastUpdate + "|";
                                    trx.RetryPosition += 1;
                                }
                                else
                                {
                                    log.Error("Failed in Parsing File-try again to parsing at: " + trx.LastUpdate);
                                    trx.LastUpdate = DateTime.Now.AddMinutes(retryAjaInt);
                                    trx.ErrorDescription += schInfo + "Failed in Parsing File-Pending 2 minutes : " + trx.LastUpdate;
                                    trx.RetryPosition += 1;
                                }
                            }
                            #endregion sukses
                        }

                        else if (outStatus.Equals("ERROR")) // File mendapat message error dr host dan generate lagi
                        {
                            trx.LastUpdate = DateTime.Now.AddMinutes(retryErrorInt);
                            trx.Status = ParameterHelper.PAYROLL_WAITING_GENERATE_MASS_INQ;//10
                            trx.ErrorDescription += schInfo + " Message from Host error, generate again (" + retryErrorInt.ToString() + " minutes) at " + trx.LastUpdate;
                            log.Error("ID : " + trx.SeqNumber + " Message from Host error, generate again (" + retryErrorInt.ToString() + " minutes) at " + trx.LastUpdate);
                            trx.RetryPosition += 1;
                        }
                        else if (outStatus.Equals("RETRY")) // Not Completed
                        {
                            trx.LastUpdate = DateTime.Now.AddMinutes(retryAjaInt);
                            trx.ErrorDescription += schInfo + "Mass Inq file is not Completed, try again (" + retryAjaInt.ToString() + " minutes) to read file at " + trx.LastUpdate;
                            log.Error("ID : " + trx.SeqNumber + " Mass Inq file is not Completed, try again (" + retryAjaInt.ToString() + " minutes) to read file at " + trx.LastUpdate);
                            trx.RetryPosition += 1;
                        }
                        else if (outStatus.Equals("UNKNOWN")) // UNKNOWN
                        {
                            trx.LastUpdate = DateTime.Now.AddMinutes(retryErrorInt);
                            trx.Status = ParameterHelper.PAYROLL_WAITING_GENERATE_MASS_INQ;//10
                            trx.ErrorDescription += schInfo + "Message from host is unknown, generate again (" + retryErrorInt.ToString() + " minutes) at " + trx.LastUpdate;
                            log.Error("ID : " + trx.SeqNumber + " Message from host is unknown, generate again (" + retryErrorInt.ToString() + " minutes) at " + trx.LastUpdate);
                            trx.RetryPosition += 1;
                        }
                        else if (outStatus.Equals("INVALID")) // Cek footer gagal retry aja
                        {
                            trx.LastUpdate = DateTime.Now.AddMinutes(retryAjaInt);
                            trx.ErrorDescription += schInfo + "Failed checking Footer, try again (" + retryAjaInt.ToString() + " minutes) to read file at " + trx.LastUpdate;
                            log.Error("ID : " + trx.SeqNumber + " Failed checking Footer, try again (" + retryAjaInt.ToString() + " minutes) to read file at " + trx.LastUpdate);
                            trx.RetryPosition += 1;
                        }
                        else
                        {
                            /*
                             * JIka sejam retry sampai BOSOKS maksinal 1 jam maka akan reject transakski, eaea dan 
                             * Jika lebih dr 3 menit akan generate Mass Acc Inq, set 10
                             */

                            //if (!MassInquiryHelper.retryMassInq(trx, session, log))
                            //{
                            //    log.Error("Something Wrong In Retry Mass Inq");
                            //    trx.LastUpdate = DateTime.Now.AddMinutes(2);
                            //    trx.ErrorDescription += schInfo + "Something Wrong In Retry Mass Inq, try again at " + trx.LastUpdate;
                            //}
                        #endregion
                        }

                    }
                    else
                    {
                        log.Error("Something Wrong In Download File :(");
                        trx.LastUpdate = DateTime.Now.AddMinutes(2);
                        trx.ErrorDescription += schInfo + "Something Wrong In Download File" + trx.LastUpdate;
                        trx.RetryPosition += 1;
                    }
                }
                session.Update(trx);
                session.Flush();
            }
        }
    }
}
