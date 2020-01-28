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
using System.Text.RegularExpressions;


namespace BRIChannelSchedulerNew.Payroll
{
    class SendEmailNotifIA : IStatefulJob
    {
        private static String SchCode = "SendEmailNotifIA";
        private static String SchName = "Scheduller Payroll Send Email Notif IA";
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
            ILog log = LogManager.GetLogger(typeof(SendEmailBenef));
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
                DateTime tglEkse = DateTime.ParseExact(ParameterHelper.GetString("PAYROLL_NEXTTIME_SEND_NOTIF_IA", session), "ddMMyyyyHHmm", System.Globalization.CultureInfo.InvariantCulture);

                if (tglEkse < DateTime.Now)
                {
                    log.Info(SchCode + " === Mulai Ekse ===");

                    int result1 = 0;
                    double aBal1 = 0; double hBal1 = 0; double cBal1 = 0; int statRek1 = 0; string namaRek1 = "";
                    int result2 = 0;
                    double aBal2 = 0; double hBal2 = 0; double cBal2 = 0; int statRek2 = 0; string namaRek2 = "";
                    string msg = "";

                    string rekIADebet = ParameterHelper.GetString("PAYROLL_IA_DEBET", session);

                    result1 = PayrollHelper.InquiryHoldAmt(rekIADebet, out aBal1, out hBal1, out cBal1, out statRek1, out namaRek1, out msg);

                    if (result1 != 1)
                    {
                        log.Error(SchCode + " === Gagal Inquiry rekIADebet (" + rekIADebet + ") result: " + result1 + ", msg: "+msg+" ==== Retry in 2 Min");
                        
                        Thread.Sleep(120000);
                        log.Info(SchCode + " === Kelar Ekse ===");
                        return;
                    }

                    string rekIAKredit = ParameterHelper.GetString("PAYROLL_IA_KREDIT", session);
                    result2 = PayrollHelper.InquiryHoldAmt(rekIAKredit, out aBal2, out hBal2, out cBal2, out statRek2, out namaRek2, out msg);

                    if (result2 != 1)
                    {
                        log.Error(SchCode + " === Gagal Inquiry rekIAKredit (" + rekIADebet + ") result: " + result2 + ", msg: " + msg + " ==== Retry in 2 Min");

                        Thread.Sleep(120000);
                        log.Info(SchCode + " === Kelar Ekse ===");
                        return;
                    }

                    string email_content = EmailHelperUniversal.EmailTemplateIADebetKredit(rekIADebet, namaRek1, "IA DEBET", aBal1, hBal1, cBal1, statRek1,
                        rekIAKredit, namaRek2, "IA KREDIT", aBal2, hBal2, cBal2, statRek2);

                    string receiver = ParameterHelper.GetString("PAYROLL_EMAIL_TARGET_MON_IADEBETKREDIT", session);
                    int idEmailTransaction = 0;
                    if (!EmailHelperUniversal.AddEmailTransaction(session, 0, 100, "PAYROLL IADEBETKREDIT MONITORING", 1705, receiver, "Payroll IADebetKredit Daily Monitoring", email_content, "", 2, out idEmailTransaction, out msg))
                    {
                        log.Error(SchCode + " === Gagal Kirim Email, msg: " + msg + " ==== Retry in 2 Min");

                        Thread.Sleep(120000);
                        log.Info(SchCode + " === Kelar Ekse ===");
                        return;
                    }

                    log.Info(SchCode + " === Sukses Kirim Email id : " + idEmailTransaction.ToString() + " ===");
                    int next = ParameterHelper.GetInteger("PAYROLL_NEXTTIME_PARAM", session);
                    Parameter par = session.Load<Parameter>("PAYROLL_NEXTTIME_SEND_NOTIF_IA");

                    par.Data = tglEkse.AddHours(next).ToString("ddMMyyyyHHmm");

                    session.Update(par);
                    session.Flush();
                    log.Info(SchCode + " === Kelar Ekse ===");
                }
                else
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Belum waktu diekse vroh");
                
            }
            catch(Exception e)
            {
                log.Error("Exception Send Email >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace);
            }
        }
    }
}
