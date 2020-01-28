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
    class SendEmailBenef : IStatefulJob
    {
        private static String SchCode = "SendEmailBenef";
        private static String SchName = "Scheduller Payroll Send Email Benef";
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
            try
            {
                IList<TrxPayrollDetail> trxList = session.CreateCriteria(typeof(TrxPayrollDetail))
                    .Add(Expression.Eq("Status", ParameterHelper.TRXSTATUS_SUCCESS))
                    .Add(Expression.Not(Expression.Eq("Email", "")))
                    .Add(Expression.Eq("EmailTransactionId", 0))
                    .AddOrder(Order.Asc("Id"))
                    .List<TrxPayrollDetail>();

                log.Info("Jumlah transaksi butuh email : " + trxList.Count);
                //we've found some data
                foreach (TrxPayrollDetail trx in trxList)
                {
                    try
                    {
                        //get parent
                        TrxPayroll payroll = trx.Parent;
                        string remark = payroll.FileDescription;

                        //get client name
                        Client cli = session.Load<Client>(payroll.ClientID);
                        string lembaga = cli.Name;

                        double amount = trx.Amount / 100;
                        string text_amount = string.Format("{0:N}", amount);

                        //set email content
                        string email_content = EmailHelperUniversal.EmailTemplateInternalTransfertoBenef(session, "PAYROLL", lembaga, trx.Account, trx.Name, "IDR", text_amount, remark, payroll.ProcessTime.ToString("dd/MM/yyyy hh:mm:ss"));

                        //send email
                        int idEmailTransaction = 0;
                        string OutMsg = "";
                        if (EmailHelperUniversal.AddEmailTransaction(session, int.Parse(trx.Id.ToString()), 100, "PAYROLL BENEF", payroll.ClientID, trx.Email, "Payroll Transfer Notification - Cash Management System BRI", email_content, "", 2, out idEmailTransaction, out OutMsg))
                        {
                            trx.EmailTransactionId = idEmailTransaction;
                        }
                        else
                        {
                            trx.ErrorDescription = "Email exception. " + OutMsg;
                        }

                        session.Update(trx);
                        session.Flush();
                    }
                    catch (Exception e)
                    {
                        log.Error("Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace);
                        trx.ErrorDescription = "Send Email Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace;
                        session.Update(trx);
                        session.Flush();
                    }
                }
            }
            catch(Exception e)
            {
                log.Error("Exception Send Email >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace);
            }
        }
    }
}
