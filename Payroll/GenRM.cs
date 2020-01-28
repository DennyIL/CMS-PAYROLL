using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BRIChannelSchedulerNew.Payroll.Pocos;
using BRIChannelSchedulerNew.Payroll.Helper;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using Quartz;
using Quartz.Impl;
using log4net;

namespace BRIChannelSchedulerNew.Payroll
{
    class GenRM : IStatefulJob
    {
        private static String SchCode = "PayrollCreateRM";
        private static String SchName = "Sch Create RM Payroll";
        private static Configuration cfg;
        private static ISessionFactory factory;
        ISession session;

        #region IJob Members

        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(GenRM));
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
            try
            {
                IList<TrxPayrollDetail> txlist = session.CreateCriteria(typeof(TrxPayrollDetail))
                    .Add(Expression.Eq("Status", ParameterHelper.TRXSTATUS_PAYROLL_WAITINGRESPONSE_REMITNUMB))
                    .Add(Expression.In("InstructionCode", new string[]{TotalOBHelper.LLG, TotalOBHelper.RTGS}))
                    .Add(Expression.Le("LastUpdate", DateTime.Now))
                    .List<TrxPayrollDetail>();

                if (txlist.Count > 0)
                {
                    foreach (TrxPayrollDetail ts in txlist)
                    {
                        if (ts.Status == ParameterHelper.TRXSTATUS_PAYROLL_WAITINGRESPONSE_REMITNUMB)
                        {
                            log.Info(SchCode + " Start Creating Remittance Number TrxId: " + ts.Id + " ===");

                            ts.Status = short.Parse(ParameterHelper.TRXSTATUS_PAYROLL_PROCESS_CREATE_REMITNUMB.ToString());
                            ts.Description = ParameterHelper.TRXDESCRIPTION_PAYROLL_WAITINGRESPONSE_PROCESS;

                            string rtg = TotalOBHelper.RTGS;
                            string llg = TotalOBHelper.LLG;

                            if (ts.InstructionCode.Equals(TotalOBHelper.RTGS)) 
                            {
                                ts.RemittanceNumber = SchHelper.getRemittanceNumber(session, 4, 1, TotalOBHelper.RTGS);
                                ts.Status = short.Parse(ParameterHelper.TRXSTATUS_PAYROLL_WAITINGRESPONSE_PROCESS.ToString());
                                session.Update(ts);
                            }
                            else if (ts.InstructionCode.Equals(TotalOBHelper.LLG)) 
                            {
                                ts.RemittanceNumber = SchHelper.getRemittanceNumber(session, 4, 1, TotalOBHelper.LLG);
                                ts.Status = short.Parse(ParameterHelper.TRXSTATUS_PAYROLL_WAITINGRESPONSE_PROCESS.ToString());
                                session.Update(ts);
                            }

                            session.Update(ts);
                            session.Flush();

                            log.Info(SchCode + " Finish Creating Remittance Number TrxId: " + ts.Id + " ===");
                        }
                    }

                }
                else
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Tidak ada Trx yang diproses");
            }
            catch (Exception ee)
            {
                log.Error(SchCode + " === " + SchName + " Process Error Message:: " + ee.Message);
                log.Error(SchCode + " === " + SchName + " Process Error Inner Exception:: " + ee.InnerException);
                log.Error(SchCode + " === " + SchName + " Process Error StackTrace:: " + ee.StackTrace); ;
            }
        }
    }
}
