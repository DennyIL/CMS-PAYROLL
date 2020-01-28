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
    class RunBeforePayroll : IStatefulJob
    {
        private static String SchCode = "RunBeforePayroll";
        private static String SchName = "Sch RunBeforePayroll";
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
                            else
                                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === CUTOFF TIME");
                            
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
                           .Add(Expression.In("Status", new int[] 
                           { 
                               ParameterHelper.TRXSTATUS_RUNNING_AFTERCHECKACCOUNT, //19
                               ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_IADEBET, //20
                               ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IADEBET //21
                           }))
                           .Add(Expression.Lt("ProcessTime", DateTime.Now))
                           .Add(Expression.Lt("LastUpdate", DateTime.Now))
                           .List<TrxPayroll>();

            if (txList.Count > 0)
            {
                foreach (TrxPayroll ts in txList)
                {
                    bool lanjut = false;
                    

                    //Pengecekkan cut of dilakukan jika ada / mengandung LLG-RTG saja jika IFT dan BRIS maka cut off nya ikut BRIS
                    //Pengecekkan berguna untuk menghandele 
                    #region cek count jika ada LLG maka ikut LLG jika g maka ikut BRIS
                    #endregion end of cek count jika ada LG maka ikut LLG jika g maka BRIS

                    #region cek cutoff ato libur untuk payroll bank lain
                    if (ts.Status == ParameterHelper.TRXSTATUS_RUNNING_AFTERCHECKACCOUNT && ts.IsPayrollBankLain == 1)//19
                    {
                        //Cek jumlah BRIS
                        
                        //CUT OFF SKNBI
                        string day = PayrollHelper.isHoliday(session, 1200, DateTime.Now);
                        if (day.Equals("[WEEKEND]"))
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Weekend!");
                        else if (day.Equals("[HOLIDAY]"))
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Holiday!");
                        else if (day.Equals("[WEEKDAY]"))
                        {
                            Parameter paramCute = session.Load<Parameter>("LLGX_CUTOFFTIME");
                            DateTime cutoff = DateTime.ParseExact(DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + paramCute.Data + ":00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                            Parameter paramStart = session.Load<Parameter>("LLGX_STARTTIME");
                            DateTime start = DateTime.ParseExact(DateTime.Now.Day.ToString("00") + "/" + DateTime.Now.Month.ToString("00") + "/" + DateTime.Now.Year.ToString("0000") + " " + paramStart.Data + ":00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                            if ((cutoff.CompareTo(DateTime.Now) > 0) && (start.CompareTo(DateTime.Now) < 0))
                            {
                                lanjut = true;
                            }
                            else
                                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === CUTOFF TIME");
                        }
                    }
                    else
                        lanjut = true;
                    #endregion

                    if (lanjut)
                    {
                        if (ts.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IADEBET)//21
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (SeqNo " + ts.SeqNumber + ") - Start Checking");
                        else 
                            log.Info(SchCode + " === (SeqNo " + ts.SeqNumber + ") Start Processing");

                        PayrollHelper.ProcessRunBeforePayroll(session, ts, log, SchCode);

                        if (ts.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IADEBET)//21
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (SeqNo " + ts.SeqNumber + ") - Finish Checking");
                        else 
                            log.Info(SchCode + " === (SeqNo " + ts.SeqNumber + ") Finish Processing");
                    }
                }
            }
            else
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Tidak ada Trx yang diproses");

        }
    }
}
