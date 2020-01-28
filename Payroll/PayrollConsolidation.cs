using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


namespace BRIChannelSchedulerNew.Payroll
{
    class PayrollConsolidation : IStatefulJob
    {
        private static String SchCode = "PayrollConsolidation";
        private static String SchName = "Sch PayrollConsolidation";
        private static int jobNumber = 0;

        private static Configuration cfg;
        private static ISessionFactory factory;
        ISession session;

        #region IJob Members

        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(RunBeforePayroll));

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
                    Parameter param = session.Load<Parameter>("PAYROLL_OPERASIONAL_STATUS");

                    if (param.Data.Equals("1"))
                    {
                        // sayedzul - 2016-03-28 - Holiday parameter

                        //string day = SchedulerHelper.isHoliday(session, 120, DateTime.Now);

                        //if (day.Equals("[WEEKEND]"))
                        //    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Weekend!");
                        //else if (day.Equals("[HOLIDAY]"))
                        //    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Holiday!");
                        //else if (day.Equals("[WEEKDAY]"))
                        //{
                            Parameter paramCute = session.Load<Parameter>("PAYROLL_CUTOFFTIME");
                            DateTime cutoff = DateTime.ParseExact(DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + paramCute.Data + ":00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                            Parameter paramStart = session.Load<Parameter>("PAYROLL_STARTTIME");
                            DateTime start = DateTime.ParseExact(DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + paramStart.Data + ":00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                            if ((cutoff.CompareTo(DateTime.Now) > 0) && (start.CompareTo(DateTime.Now) < 0))
                            {
                                Commitjob_transaction(log);
                            }
                        //}
                        //else
                        //    log.Error(SchCode + " === " + day);

                        //end sayedzul - 2016-03-28 
                    }
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

            IList<TrxPayroll> txList = session.CreateCriteria(typeof(TrxPayroll))
                           .Add(Expression.In("Status", new int[] { ParameterHelper.TRXSTATUS_PAYROLLNEW_READY_BOOK, 
                           ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_IAKREDIT, ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IAKREDIT,
                           ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_REK_NASABAH, ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_REK_NASABAH}))
                           .Add(Expression.Lt("LastUpdate", DateTime.Now))
                           .List<TrxPayroll>();

            if (txList.Count > 0)
            {
                foreach (TrxPayroll ts in txList)
                {
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Start Checking");

                    PayrollHelper.ProcessPayrollConsolidation(session, ts, log, SchCode);

                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Finish Checking");
                }
            }
            else
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Tidak ada Trx yang diproses");

        }
    }
}
