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


namespace BRIChannelSchedulerNew.Payroll.Patching
{
    class GetChildStatus : IStatefulJob
    {
        private static String SchCode = "GetChildStatus";
        private static String SchName = "Scheduller Payroll Get Child Status";
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
            ILog log = LogManager.GetLogger(typeof(GetChildStatus));
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
                    Commitjob_transaction(log);
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
            int countSuccess = 0;
            int countInvalid = 0;
            int countTotal = 0;
            double sumSuccess = 0;
            double sumInvalid = 0;
            double sumTotal = 0;
            bool isAllDone = true;


            string feeIFT = "0:0";
            string feeRTG = "0:0";
            string feeLLG = "0:0";
            string reject = "0:0";
            string totalTrx = "0:0";


            try
            {
                IList<TrxPayroll> trxList = session.CreateCriteria(typeof(TrxPayroll))
                    .Add(Expression.Eq("Status", ParameterHelper.PAYROLL_SUCCESS_PARSING))//46
                    .AddOrder(Order.Asc("CreatedTime"))
                    .List<TrxPayroll>();


                log.Info("Jumlah transaksi menunggu checking account : " + trxList.Count);
                //we've found some data
                foreach (TrxPayroll trx in trxList)
                {
                    try
                    {
                        IList<TrxPayrollDetail> ListSuccess = trx.TrxPayrollDetail;
                        
                        //jika tidak ada yg belom dicek berarti OK
                        int count55 = ListSuccess.Where(x => x.Status == ParameterHelper.PAYROLLDETAILS_WAITING_CHECK).Count();
                        if (count55 > 0) isAllDone = false;

                        //trx.LastUpdate = DateTime.Now;
                        if (isAllDone)
                        {
                            log.Info("Trx Id = " + trx.Id + ", isAllDone");

                            countInvalid = trx.TrxPayrollDetail.Where(x => x.Status == ParameterHelper.TRXSTATUS_REJECT).Count();
                            countTotal = trx.TrxPayrollDetail.Count();

                            trx.Status = ParameterHelper.TRXSTATUS_INQUIRYNAMEPROCESS_FINISH;
                            trx.ErrorDescription = "";

                            if (countInvalid >= countTotal)//tidak ada data yang akan di proses
                            {
                                String[] desc = trx.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                                trx.Status = ParameterHelper.TRXSTATUS_REJECT;
                                trx.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                                trx.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - No valid data to process (All data payroll invalid).||" + desc[1];
                                trx.ErrorDescription = "Tidak ada data valid untuk di proses (semua invalid)";
                                session.Update(trx);
                                session.Flush();
                                log.Info("Rejected, tidak ada data yang valid.");

                                //delete transactions
                                Transaction trans = session.CreateCriteria(typeof(Transaction))
                                   .Add(Expression.In("TransactionId", new int[] { 101, 102, 103, 1151 }))
                                   .Add(Expression.Eq("ClientId", trx.ClientID))
                                   .Add(Expression.Like("TObject", trx.Id, MatchMode.Anywhere))
                                   .UniqueResult<Transaction>();
                                if (trans != null)
                                {
                                    session.Delete(trans);
                                    session.Flush();
                                    log.Info("Delete transactions success");
                                }
                            }
                            else//ada data yang akan di proses
                            {
                                log.Info("Trx Id = " + trx.Id + ", CountInvalid = countTotal");

                                //update transactions
                                Transaction trans = session.CreateCriteria(typeof(Transaction))
                                   .Add(Expression.In("TransactionId", new int[] { 101, 102, 103 }))
                                   .Add(Expression.Like("TObject", trx.Id, MatchMode.Anywhere))
                                   .UniqueResult<Transaction>();
                                if (trans != null)//avoid Payroll Musimas
                                {
                                    trans.Status = ParameterHelper.TRXSTATUS_WAITING_SCHEDULLER;//15


                                    /*Ini nih kunci utnuk laporan query dll nyaaa*/
                                    trx.TotalTrx = "|" + feeIFT + "|" + reject + "|" + totalTrx + "|" + feeRTG + "|" + feeLLG + "|";
                                    //trx.TotalTrx = "|" + _countSuccess + ":" + _sumSuccess + "|" + _countInvalid + ":" + _sumInvalid + "|" + _countTotal + ":" + _sumTotal + "|";
                                    log.Info("Trx Id = " + trx.Id + ", Total Trx : " + trx.TotalTrx);

                                    session.Update(trx);
                                    session.Flush();

                                    session.Update(trans);
                                    session.Flush();
                                    log.Info("Trx Id = " + trx.Id + ", Update transactions success");

                                }
                                else
                                {
                                    //payroll musimas
                                    int _countSuccess = trx.TrxPayrollDetail.Where(x => x.Status != ParameterHelper.TRXSTATUS_REJECT).Count();
                                    double _sumSuccess = trx.TrxPayrollDetail.Where(x => x.Status != ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount / 100);
                                    int _countInvalid = trx.TrxPayrollDetail.Where(x => x.Status == ParameterHelper.TRXSTATUS_REJECT).Count();
                                    double _sumInvalid = trx.TrxPayrollDetail.Where(x => x.Status == ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount / 100);
                                    int _countTotal = trx.TrxPayrollDetail.Count();
                                    double _sumTotal = trx.TrxPayrollDetail.Sum(x => x.Amount / 100);

                                    trx.TotalTrx = "|" + _countSuccess.ToString() + ":" + _sumSuccess.ToString() + "|" + _countInvalid.ToString() + ":" + _sumInvalid.ToString() + "|" + _countTotal.ToString() + ":" + _sumTotal.ToString() + "|";
                                    log.Info("Trx Id = " + trx.Id + ", Total Trx : " + trx.TotalTrx);
                                    session.Update(trx);
                                    session.Flush();
                                }
                            }
                        }
                        else
                        {
                            //20170602 - sayedzul, handler masih ada anak yang belum diupdate
                            #region lom semua kumplit, rollback status parent
                            log.Error("Trx Id = " + trx.Id + ", Not AllDone, reload status parent from 46 to 11");
                            trx.Status = ParameterHelper.PAYROLL_WAITING_CHECKACC;
                            session.Update(trx);
                            session.Flush();
                            #endregion
                        }
                        
                    }
                    catch (Exception e)
                    {
                        log.Error("Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace);
                        trx.ErrorDescription = "GetChildStatus Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace;
                        trx.Status = ParameterHelper.PAYROLL_EXCEPTION;
                        session.Update(trx);
                        session.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace);
            }
        }
        
    }
}
