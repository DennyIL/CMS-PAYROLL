using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Quartz;
using Quartz.Impl;
using log4net;
using BRIChannelSchedulerNew.Payroll.Pocos;
using BRIChannelSchedulerNew.Payroll.Helper;
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
using System.Net.Sockets;


namespace BRIChannelSchedulerNew.Payroll.BookBRIS
{
    class ReadFIleBRIS : IStatefulJob
    {
        private static int jobNumber = 0;
        private static String SchCode = "Read FIle BRIS";
        private static String SchName = "Sch Payroll Read FIle BRIS";
        
        private static int maxResult = 10;

        private static Configuration cfg;
        private static ISessionFactory factory;
        ISession session;
        private DataTable dte = new DataTable("dte");

        #region IJob Members

        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(ReadFIleBRIS));
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
            
            IList<TrxPayroll> trxList = session.CreateSQLQuery("select * from trxpayrolls where status=? and lastupdate<=? and statusbris = ? order by id ASC")
                .AddEntity(typeof(TrxPayroll))
                .SetInt32(0, ParameterHelper.TRXSTATUS_PAYROLLNEW_READY_BOOK)//22
                .SetDateTime(1, DateTime.Now)//kurang dari last update
                .SetInt32(2, ParameterHelper.TRXSTATUS_PAYROLLNEW_WAITING_RESP_FTP_BRIS)//5
                .SetMaxResults(maxResult)
                .List<TrxPayroll>();

            if (trxList.Count > 0)
            {
                foreach (TrxPayroll ts in trxList)
                {
                    if (PayrollHelper.DownloadFileBRIS(session, ts, log, SchCode))
                    {
                        log.Info(SchCode + "SeqNum  = " + ts.SeqNumber + " - Send FIle BRIS : Success");
                    }
                    else
                    {
                        int retry = 0;
                        int maxretry = 10;

                        if (retry == maxretry)
                        {
                            //Gagal send -  reject BRIS
                            ts.StatusBRIS = ParameterHelper.TRXSTATUS_PAYROLLNEW_FTP_BRIS_ERROR;//4 gagal genrate file
                            ts.LogFtpBris = DateTime.Now.ToString() + "- Sending File BRIS GAGAL|";
                            ts.LastUpdate = DateTime.Now;
                            session.Update(ts);
                            session.Flush();
                            log.Info(SchCode + "SeqNum  = " + ts.SeqNumber + " - Sending FIle BriS : Gagal. Akan di retry 10x");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Tidak ada Trx yang diproses");
            }
        }
    }
}
