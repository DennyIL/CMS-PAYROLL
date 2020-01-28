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
    class SetSeqNumber : IStatefulJob
    {
        private static String SchCode = "SetSeqNumber";
        private static String SchName = "Scheduller Payroll Set SeqNumber";
        private static String schInfo = "";
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
            ILog log = LogManager.GetLogger(typeof(SetSeqNumber));
            try
            {
                //log
                schInfo = "=== " + DateTime.Now + " ";

                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }
                session = factory.OpenSession();
                log.Info(schInfo + SchCode + " === Starting " + SchName + " ===");

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
                log.Info(schInfo + SchCode + " === End " + SchName + " ===");
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
            try
            {
                Parameter seqNumberParam = session.Load<Parameter>("SEQ_NUMBER_PAYROLL");
                Parameter maxTrx = session.Load<Parameter>("PAYROLL_SCH_MAX_TRX");
                int maxResult = int.Parse(maxTrx.Data);

                IList<TrxPayroll> trxList = session.CreateCriteria(typeof(TrxPayroll))
                    .Add(Expression.Eq("Status", ParameterHelper.PAYROLL_WAITINGSEQNUM))
                    .AddOrder(Order.Asc("CreatedTime"))
                    .List<TrxPayroll>();
                log.Info("Jumlah transaksi butuh no antrian : " + trxList.Count);
                //we've found some data
                foreach (TrxPayroll trx in trxList)
                {
                    try
                    {
                        int seqNum = int.Parse(seqNumberParam.Data);
                        trx.SeqNumber = seqNum + 1;
                        trx.Status = ParameterHelper.TRXSTATUS_INQUIRYNAMEPROCESS_START;
                        session.Update(trx);
                        session.Flush();
                        log.Info("For Trx Id = " + trx.Id + ", SeqNumber = " + trx.SeqNumber);

                        //update parameters
                        seqNumberParam.Data = trx.SeqNumber.ToString();
                        session.Update(seqNumberParam);
                        session.Flush();
                    }
                    catch (Exception e)
                    {
                        log.Error("Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace);
                        trx.ErrorDescription = "SetSeqNumber Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace;
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
