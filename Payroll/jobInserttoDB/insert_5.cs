using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Quartz;
using Quartz.Impl;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using BriInterfaceWrapper.BriInterface;
using System.Threading;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using BRIChannelSchedulerNew.Payroll.Pocos;
////using BRIChannelSchedulerNew.Payroll.Helper;

namespace BRIChannelSchedulerNew.Payroll.jobInserttoDB
{
    class insert_5 : IStatefulJob
    {
        private static String SchCode = "PayrollInsert_5";
        private static String SchName = "Scheduller Payroll Insert to DB";
        private static String schInfo = "";
        private static int jobNumber = 5;//trx yang diproses, ambil trxID kelipatan 10
        private static Configuration cfg;
        private static ISessionFactory factory;
        private String name = "";
        private String curr = "";
        protected ISession session;

        
        private DataTable dte = new DataTable("dte");
        #region IJob Members
        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(insert_5));
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
            Parameter maxJob = session.Load<Parameter>("PAYROLL_SCH_JOB_INSERTDB");
            Parameter maxTrx = session.Load<Parameter>("PAYROLL_SCH_MAX_TRX");
            int maxResult = int.Parse(maxTrx.Data);

            IList<TrxPayroll> trxList = session.CreateSQLQuery("select * from trxpayrolls where status=? and (seqnumber%?)=? order by id ASC")
                .AddEntity(typeof(TrxPayroll))
                .SetInt32(0, ParameterHelper.TRXSTATUS_INQUIRYNAMEPROCESS_START)//16 : status insert ke DB
                .SetInt32(1, int.Parse(maxJob.Data))//id mod(10)
                .SetInt32(2, jobNumber)//id mod = ini mod 0, ambil trxID kelipatan 10
                .SetMaxResults(maxResult)
                .List<TrxPayroll>();

            //we've found some data
            foreach(TrxPayroll trx in trxList)
            {
                if (CheckAccountHelper.PayrollParsingFile(trx, session, log, jobNumber))
                {
                    log.Info(SchCode + " === (TrxID " + trx.SeqNumber + ") Success Add to DB");
                }
                else log.Error(SchCode + " === (TrxID " + trx.SeqNumber + ") Failed Add to DB");
            }
        }
    }
}
