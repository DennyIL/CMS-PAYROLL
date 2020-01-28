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


namespace BRIChannelSchedulerNew.Payroll.jobGenerateAndSendMassInq
{
    class generate_0 : IStatefulJob
    {
        private static String SchCode = "Generate_0";
        private static String SchName = "Scheduller Payroll Generate and Upload Mass Inq";
        private static String schInfo = "=== " + DateTime.Now + " ";
        private static int jobNumber = 0;//trx yang diproses, ambil trxID kelipatan 10
        private static Configuration cfg;
        private static ISessionFactory factory;
        private String name = "";
        private String curr = "";
        protected ISession session;

        
        private DataTable dte = new DataTable("dte");
        #region IJob Members
        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(generate_0));
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
                    //log.Error(SchCode + " === " + SchName + " Process Error Message:: " + ee.Message);
                    //log.Error(SchCode + " === " + SchName + " Process Error Inner Exception:: " + ee.InnerException);
                    //log.Error(SchCode + " === " + SchName + " Process Error StackTrace:: " + ee.StackTrace);

                    //Pengabaian Exception hahahaha
                    if (ee.GetType() != typeof(ThreadAbortException))
                    {
                        //log.Error(SchCode + " === " + SchName + " Process Error Message:: " + ee.Message);
                        log.Error(SchCode + " === " + SchName + " Skip this Exception, Next Process ea ea" + ee.InnerException + "||" + ee.Message + "||" + ee.StackTrace );
                    }
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
            Parameter maxJob = session.Load<Parameter>("PAYROLL_SCH_JOB_GENERATE_MASS_INQ");
            Parameter maxTrx = session.Load<Parameter>("PAYROLL_SCH_GENERATE_MASS_INQ_MAX_TRX");
            int maxResult = int.Parse(maxTrx.Data);


            /*Inisiasi*/
            string temp_query = "";
            string msg = "";
            string outFileName = "";

            IList<TrxPayroll> trxList = session.CreateSQLQuery("select * from trxpayrolls where status=? and (seqnumber%?)=? and lastupdate<=? order by id ASC")
                .AddEntity(typeof(TrxPayroll))
                .SetInt32(0, ParameterHelper.PAYROLL_WAITING_GENERATE_MASS_INQ)//10
                .SetInt32(1, int.Parse(maxJob.Data))//id mod(10)
                .SetInt32(2, jobNumber)//id mod = ini mod 0, ambil trxID kelipatan 10
                .SetDateTime(3, DateTime.Now)//kurang dari last update
                .SetMaxResults(maxResult)
                .List<TrxPayroll>();

            //we've found some data
            foreach(TrxPayroll trx in trxList)
            {
                #region patching instructioncode null
                //20170530 - sayedzul add handler instructioncode null dan account payrollBRI ga 15 digit
                int jumlahInstCodeNull = trx.TrxPayrollDetail.Where(x => String.IsNullOrEmpty(x.InstructionCode)).Count();
                if (jumlahInstCodeNull > 0)
                {
                    string theQuery = @"update trxpayrolldetails set instructioncode = '" + TotalOBHelper.IFT + "' where pid  = '" + trx.Id + "' and (instructioncode is null or instructioncode = '');";
                    string msg1 = "";
                    PayrollHelper.ExecuteQuery(session, theQuery, out msg1);
                    break;
                }

                int jumlahBenAccGa15Digit = trx.TrxPayrollDetail.Where(x => x.InstructionCode.Equals(TotalOBHelper.IFT) && x.Account.Length < 15).Count();
                if (jumlahBenAccGa15Digit > 0)
                {
                    string theQuery2 = @"update trxpayrolldetails set account = lpad(account, 15, '0') where pid  = '" + trx.Id + "' and instructioncode = '" + TotalOBHelper.IFT + "' and length(account) < 15;";
                    string msg2 = "";
                    PayrollHelper.ExecuteQuery(session, theQuery2, out msg2);
                    break;
                }

                #endregion

                int jumlahTrxIFT = trx.TrxPayrollDetail.Where(x => x.Status != ParameterHelper.TRXSTATUS_REJECT && x.InstructionCode.Equals(TotalOBHelper.IFT)).Count();
                if (jumlahTrxIFT > 0)
                {
                    if (MassInquiryHelper.generate(trx, session, log, SchCode, out outFileName))
                    {
                        log.Info("Success Generate File, update to next process.");
                        trx.ErrorDescription += schInfo + "Success Generate File";

                        if (MassInquiryHelper.sendToHost(trx, session, log, outFileName))
                        {
                            log.Info("Success Generate File, update to next process.");


                            //Skenario : jika file banyak agar respons host blm selesai, ada jeda
                            int tambahMenit = 0;
                            if (jumlahTrxIFT < 10000)
                            {
                                tambahMenit = 1;
                            }
                            else if ((jumlahTrxIFT < 50000) && (jumlahTrxIFT > 10000))
                            {
                                tambahMenit = 2;
                            }
                            else
                            {
                                tambahMenit = int.Parse(Math.Floor((Decimal)(jumlahTrxIFT / 25000)).ToString());
                            }

                            /*Native update*/
                            temp_query = @"update trxpayrolls p
                                    set
                                    p.status = 11, p.filemassinq = '" + outFileName + "', p.lastupdate = '" + DateTime.Now.AddMinutes(tambahMenit).ToString("yyyy-MM-dd HH:mm:ss") + "' where p.id = '" + trx.Id + "';";

                            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                            {
                                log.Error("Failed Execute query. " + msg.ToString());
                                
                            }
                            else log.Info("Trx Id = " + trx.Id + ", mempunyai transaksi IFT: " + jumlahTrxIFT + "||Akan set Lastupdate ::" + tambahMenit + "Minutes");
                            
                        }
                        else
                        {
                            /*Native update*/
                            temp_query = @"update trxpayrolls p
                                    set p.lastupdate = " + DateTime.Now.AddMinutes(2).ToString() + " where p.id = '" + trx.Id + "';";

                            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                            {
                                log.Error("Failed Execute query. " + msg.ToString());

                            }
                            else log.Error("Something Wrong in send :( and try agagain in 2 minutes");
                        }
                    }
                    else
                    {
                        /*Native update*/
                        temp_query = @"update trxpayrolls p
                                    set p.lastupdate = " + DateTime.Now.AddMinutes(2).ToString() + " where p.id = '" + trx.Id + "';";

                        if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                        {
                            log.Error("Failed Execute query. " + msg.ToString());

                        }
                        else log.Error("Something Wrong in generate :( and try agagain in 2 minutes");
                    }
                }
                else
                {
                    trx.Status = ParameterHelper.TRXSTATUS_INQUIRYNAMEPROCESS_FINISH;//17
                    trx.ErrorDescription = "";

                    //update transactions
                    Transaction trans = session.CreateCriteria(typeof(Transaction))
                       .Add(Expression.In("TransactionId", new int[] { 101, 102, 103 }))
                       .Add(Expression.Like("TObject", trx.Id, MatchMode.Anywhere))
                       .UniqueResult<Transaction>();
                    if (trans != null)//avoid Payroll Musimas
                    {
                        trans.Status = ParameterHelper.TRXSTATUS_WAITING_SCHEDULLER;//15

                        //string feeIFT = PayrollHelper.getFeeTransaction(session, trx.Id, TotalOBHelper.IFT, log);
                        //string feeRTG = PayrollHelper.getFeeTransaction(session, trx.Id, TotalOBHelper.RTGS, log);
                        //string feeLLG = PayrollHelper.getFeeTransaction(session, trx.Id, TotalOBHelper.LLG, log);
                        //string reject = PayrollHelper.getCountRejectTransaction(session, trx.Id, TotalOBHelper.IFT, log);
                        ///*Nilai Total*/
                        //string totalTrx = PayrollHelper.getTotalTransaction(session, trx.Id, log);

                        /*Ini nih kunci utnuk laporan query dll nyaaa*/
                        trx.TotalTrx = "|0:0|0:0|0:0|0:0|0:0|";// "|" + feeIFT + "|" + reject + "|" + totalTrx + "|" + feeRTG + "|" + feeLLG + "|";
                        //trx.TotalTrx = "|" + _countSuccess + ":" + _sumSuccess + "|" + _countInvalid + ":" + _sumInvalid + "|" + _countTotal + ":" + _sumTotal + "|";
                        log.Info("Trx Id = " + trx.Id + ", Total Trx : " + trx.TotalTrx);
                       
                        session.Update(trans);
                        session.Flush();
                        log.Info("Trx Id = " + trx.Id + " Update transactions success");
                        session.Update(trx);
                        session.Flush();
                    }
                }
                
            }
        }
    }
}
