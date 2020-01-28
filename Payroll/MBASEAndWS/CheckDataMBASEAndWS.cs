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

namespace BRIChannelSchedulerNew.Payroll.MBASEAndWS
{
    class CheckDataMBASEAndWS : IStatefulJob
    {
        private static String SchCode = "CheckDataMBASEAndWS";
        private static String SchName = "CheckDataMBASEAndWS";

        private static Configuration cfg;
        private static ISessionFactory factory;
        ISession session;

        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(CheckDataMBASEAndWS));

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

        private void Commitjob_transaction(ILog log)
        {
            IList<TrxPayrollDetail> txList = session.CreateSQLQuery("select * from trxpayrolldetails where status = ? and instructioncode in ('" + TotalOBHelper.RTGS + "','" + TotalOBHelper.LLG + "') and lastupdate < now()")
                        .AddEntity(typeof(TrxPayrollDetail))
                        .SetInt32(0, ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG)
                        .List<TrxPayrollDetail>();

            if (txList.Count > 0)
            {
                foreach (TrxPayrollDetail pd in txList)
                {
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Start Check TrxId: " + pd.Id);
                    try
                    {
                        
                        /*Modified by Mettaw : 1 Oct 2019 
                         * 
                         * Terjadi perubahan Flow MBASE pada Payroll LLG (Kliring) namun tidak pada Payroll RTGS
                         * Code Sedikit dirombak , bila RTGS pakai flow Lama, yaitu Check ke tbl MBASEandWS
                         * 
                         * 
                         * Sedangkan LLG di Check ke tbl baru ExpressProcess.
                         * 
                         * 
                         */


                        if (pd.InstructionCode == TotalOBHelper.RTGS)
                        {
                            #region Check MBASEandWS
                            // proses update
                            Mbaseandws mw = session.Load<Mbaseandws>(pd.IdMBASEAndWS);

                            if (mw.Status == ParameterHelper.SCH_BOOKSUCCESS)
                            {
                                log.Info(SchCode + " === (TrxID " + pd.Id + ") === MBASEANDWS SUCCESS");
                                pd.Status = ParameterHelper.TRXSTATUS_SUCCESS;
                                pd.Description = mw.Description;
                                pd.LastUpdate = DateTime.Now;
                                session.Update(pd);
                                session.Flush();
                            }
                            else if (mw.Status == ParameterHelper.SCH_BOOKFAILED)
                            {
                                Booking book = session.Load<Booking>(pd.IdBooking);
                                if (book.Status == ParameterHelper.SCH_BOOKSUCCESS)
                                {
                                    if (book.StatusEC == ParameterHelper.SCH_ECSUCCESS)
                                    {
                                        // transaksi berhasil dan sudah sukses EC
                                        pd.Status = ParameterHelper.TRXSTATUS_REJECT;
                                        pd.Description = mw.Description;
                                        pd.LastUpdate = DateTime.Now;
                                        session.Update(pd);
                                        session.Flush();
                                    }
                                    else if (book.StatusEC == 0)
                                    {
                                        book.StatusEC = ParameterHelper.SCH_ECWAITING;
                                        session.Update(book);
                                        session.Flush();
                                    }
                                }
                                else
                                {
                                    pd.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    pd.Description = mw.Description;
                                    pd.LastUpdate = DateTime.Now;
                                    session.Update(pd);
                                    session.Flush();
                                }
                            }
                            else
                                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === TrxId: " + pd.Id + " Not Finished Yet");
                            #endregion
                        }
                        else if (pd.InstructionCode == TotalOBHelper.LLG)
                        {
                            #region Check ExpressProcess
                            ExpressProcess exprocess = session.CreateCriteria(typeof(ExpressProcess))
                                                    .Add(Expression.Eq("TrxID", pd.Id.ToString()))
                                                    .UniqueResult<ExpressProcess>();


                            if (exprocess != null)
                            {
                                log.Info(SchCode + " ===  (TrxID " + pd.Id + " ) Begin Check MBASE Process");


                                if (exprocess.Status == ParameterHelper.SCH_MBASE_PROCESS) /*2: PROCESS MBASE*/
                                {
                                    #region COMPLETE
                                    
                                    /*Trx Payroll*/
                                    pd.Status = ParameterHelper.TRXSTATUS_SUCCESS;
                                    pd.Description = exprocess.Description;
                                    pd.LastUpdate = DateTime.Now;

                                    /*Express Process*/
                                    exprocess.Status = ParameterHelper.SCH_MBASE_SUCCESS; /*3: Success*/
                                    exprocess.Description = ParameterHelper.MBASEDESC_SUCCESSMBASE;

                                    session.Update(exprocess);
                                    session.Update(pd);
                                    session.Flush();

                                    log.Info(SchCode + " === (TrxID " + pd.Id + ") === Check MBASE Express SUCCESS");
                                    #endregion
                                }
                                else if (exprocess.Status == ParameterHelper.SCH_MBASE_FAILED) /*4: FAILED MBASE*/
                                {
                                    #region REVERSAL
                                    Booking book = session.Load<Booking>(pd.IdBooking);
                                    
                                    if (book.Status == ParameterHelper.SCH_BOOKSUCCESS)
                                    {
                                        if (book.StatusEC == ParameterHelper.SCH_ECSUCCESS)
                                        {
                                            #region COMPLETE
                                            // transaksi berhasil dan sudah sukses EC
                                            pd.Status = ParameterHelper.TRXSTATUS_REJECT;
                                            pd.Description = exprocess.Description;
                                            pd.LastUpdate = DateTime.Now;

                                            session.Update(pd);
                                            session.Flush();
                                            #endregion
                                        }
                                        else if (book.StatusEC == 0)
                                        {
                                            #region UPDATE EC
                                            book.StatusEC = ParameterHelper.SCH_ECWAITING;
                                            session.Update(book);
                                            session.Flush();
                                            #endregion
                                        }
                                        else
                                        {
                                            #region ROLLBACK
                                            pd.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG;
                                            pd.Description = "proses express".ToUpper();
                                            pd.LastUpdate = DateTime.Now;

                                            session.Update(pd);
                                            session.Flush();
                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        pd.Status = ParameterHelper.TRXSTATUS_REJECT;
                                        pd.Description = exprocess.Description;
                                        pd.LastUpdate = DateTime.Now;

                                        session.Update(pd);
                                        session.Flush();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    log.Error(SchCode + " === " + SchName + " Transaksi menunggu diproses ");
                                }
                            }
                            else
                            {
                                log.Error(SchCode + " === " + SchName + " Data ExpressPrcess tidak ditemukan ");
                            }
                            #endregion
                        }
                        else
                        {
                            log.Error(SchCode + " === " + SchName + " InstructionCode tidak valid ");
                        }

                        
                        Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Finish Check TrxId: " + pd.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Exception Check TrxId: " + pd.Id);
                        Console.WriteLine("Exception Message: " + ex.Message);
                        Console.WriteLine("Exception StackTrace: " + ex.StackTrace);
                        
                    }
                }
            }
            else
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Tidak ada Trx yang diproses");
        }
    }
}
