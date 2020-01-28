using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using log4net;
using BRIChannelSchedulerNew.Payroll.Pocos;
using BRIChannelSchedulerNew.Payroll.Helper;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using System.Globalization;

namespace BRIChannelSchedulerNew.Payroll.BookRTGSAndLLG
{
    class RTGSLLGBookingCheck_0 : IStatefulJob
    {
        private static String SchCode = "PAYROLLRTGSCheck";
        private static String SchName = "Sch D/K RTGS";
        private static int jobNumber = 0;

        private static Configuration cfg;
        private static ISessionFactory factory;
        ISession session;

        #region IJob Members

        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(RTGSLLGBookingCheck_0));

            try
            {
                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }
                session = factory.OpenSession();
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Starting " + SchName + " ===");

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
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === End " + SchName + " ===");
            }
            catch (Exception ex)
            {
                log.Error(SchCode + " === " + SchName + " Failed To Start Message:: " + ex.Message);
                log.Error(SchCode + " === " + SchName + " Failed To Start Inner Exception:: " + ex.InnerException);
                log.Error(SchCode + " === " + SchName + " Failed To Start StackTrace:: " + ex.StackTrace);
            }
            finally
            {
                session.Clear();
                session.Close();
                session.Dispose();
                factory.Close();
                factory.Dispose();
                cfg = null;
                GC.Collect();
            }
        }

        #endregion

        private void Commitjob_transaction(ILog log)
        {
            Parameter maxJob = session.Load<Parameter>("PAYROLL_RTGSLLG_SCH_JOB");

            IList<TrxPayrollDetail> txList = session.CreateSQLQuery("select * from trxpayrolldetails where STATUS = ? and INSTRUCTIONCODE in (?, ?) and LASTUPDATE<? and (ID%?)=?")
                .AddEntity(typeof(TrxPayrollDetail))
                .SetInt32(0, ParameterHelper.TRXSTATUS_PAYROLL_WAITINGRESPONSE_CHECK)
                .SetString(1, TotalOBHelper.RTGS)
                .SetString(2, TotalOBHelper.LLG)
                .SetDateTime(3, DateTime.Now)
                .SetInt32(4, int.Parse(maxJob.Data))
                .SetInt32(5, jobNumber)
              .List<TrxPayrollDetail>();

            if (txList.Count > 0)
            {
                foreach (TrxPayrollDetail ts in txList)
                {
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Start Check TrxId: " + ts.Id);
                    PayrollHelper.ProcessRTGSLLGCheckBooking(session, ts, log, jobNumber, SchCode);
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Finish Check TrxId: " + ts.Id);
                }
            }
            else
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Tidak ada Trx yang diproses");




        }

    }
}
