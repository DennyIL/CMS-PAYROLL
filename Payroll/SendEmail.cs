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
    class SendEmail : IStatefulJob
    {
        private static String SchCode = "SendEmail";
        private static String SchName = "Scheduller Payroll Send Email";
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
            ILog log = LogManager.GetLogger(typeof(SendEmail));
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
            try
            {
                IList<TrxPayroll> trxList = session.CreateCriteria(typeof(TrxPayroll))
                    .Add(Expression.Eq("StatusEmail", ParameterHelper.PAYROLL_NEEDEMAIL))
                    .Add(Expression.Or(Expression.Eq("Status", ParameterHelper.TRXSTATUS_COMPLETE), Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                    .AddOrder(Order.Asc("CreatedTime"))
                    .List<TrxPayroll>();
                log.Info("Jumlah transaksi butuh email : " + trxList.Count);
                //we've found some data
                foreach (TrxPayroll trx in trxList)
                {
                    try
                    {
                        //update status running
                        trx.StatusEmail = ParameterHelper.PAYROLL_PROCESSEMAIL;
                        session.Update(trx);
                        session.Flush();


                        string user_id = "";
                        int clientID = trx.ClientID;

                        #region get user id
                        UserMap umap = null;
                        if (!string.IsNullOrEmpty(trx.Approver))
                        {
                            String[] tempmaker = trx.Approver.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (String makinfo in tempmaker)
                            {
                                String[] detmake = makinfo.Split(new String[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                                umap = session.CreateCriteria(typeof(UserMap))
                                    .Add(Expression.Like("UserHandle", detmake[0].Trim()))
                                    .Add(Expression.Like("ClientEntity", clientID))
                                    .UniqueResult<UserMap>();
                                user_id = umap.UserEntity.ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(trx.Checker))
                        {
                            String[] tempmaker = trx.Checker.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (String makinfo in tempmaker)
                            {
                                String[] detmake = makinfo.Split(new String[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                                umap = session.CreateCriteria(typeof(UserMap))
                                    .Add(Expression.Like("UserHandle", detmake[0].Trim()))
                                    .Add(Expression.Like("ClientEntity", clientID))
                                    .UniqueResult<UserMap>();
                                user_id += "|" + umap.UserEntity.ToString();
                            }
                        }

                        if (!string.IsNullOrEmpty(trx.Maker))
                        {
                            String[] tempmaker = trx.Maker.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (String makinfo in tempmaker)
                            {
                                String[] detmake = makinfo.Split(new String[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                                umap = session.CreateCriteria(typeof(UserMap))
                                    .Add(Expression.Like("UserHandle", detmake[0].Trim()))
                                    .Add(Expression.Like("ClientEntity", clientID))
                                    .UniqueResult<UserMap>();
                                user_id += "|" + umap.UserEntity.ToString();
                            }
                        }
                        log.Info("User ID will send email = " + user_id);
                        #endregion

                        
                        #region counttrx
                        ////trx.TrxPayrollDetail.
                        string _theTotalTrx = "";
                        IList<TrxPayrollDetail> ListSuccess = trx.TrxPayrollDetail;
                        int countSuccess = 0;
                        int countFail = 0;
                        int countTotal = ListSuccess.Count;
                        double sumSuccess = 0;
                        double sumFail = 0;
                        double sumTotal = 0;
                        foreach (TrxPayrollDetail a in ListSuccess)
                        {
                            if (a.Status == ParameterHelper.TRXSTATUS_SUCCESS)
                            {
                                countSuccess++;
                                sumSuccess += a.Amount;
                            }
                            else if (a.Status == ParameterHelper.TRXSTATUS_REJECT)
                            {
                                countFail++;
                                sumFail += a.Amount;
                            }
                            sumTotal += a.Amount;
                        }

                        //real amount
                        sumSuccess = sumSuccess / 100;
                        sumFail = sumFail / 100;
                        sumTotal = sumTotal / 100;
                        string _countSuccess = countSuccess.ToString();
                        string _sumSuccess = sumSuccess.ToString();
                        string _countFail = countFail.ToString();
                        string _sumFail = sumFail.ToString();
                        string _countTotal = countTotal.ToString();
                        string _sumTotal = sumTotal.ToString();
                        //|berhasil|gagal|total|
                        //|17:56000|:|3:25612541|
                        trx.LastUpdate = DateTime.Now;
                        trx.TotalTrx = "|" + _countSuccess + ":" + _sumSuccess + "|" + _countFail + ":" + _sumFail + "|" + _countTotal + ":" + _sumTotal + "|";
                        _theTotalTrx = "|" + _countSuccess + ":" + _sumSuccess + "|" + _countFail + ":" + _sumFail + "|" + _countTotal + ":" + _sumTotal + "|";
                        log.Info("Trx Id = " + trx.Id + ", Total Trx : " + trx.TotalTrx);
                        session.Update(trx);
                        session.Flush();
                        #endregion



                        #region send email
                        String[] uid_np = user_id.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (String idnp in uid_np)
                        {
                            UserMatrix umx = session.Load<UserMatrix>(int.Parse(idnp));
                            AuthorityHelper ahe = new AuthorityHelper(umx.Matrix);
                            if (ahe.isNeedPendingEmail(100))
                            {
                                User uid = session.Load<User>(int.Parse(idnp));
                                //state=0->inProcess verify; state=1->inProc Approve; state=2->Succed; state=3->reject
                                if (trx.Status == ParameterHelper.TRXSTATUS_COMPLETE)
                                {
                                    EmailNotificationHelper.MassNotification(session, uid.Email, clientID, trx.SeqNumber, "Payroll", 2, trx.FileDescription, _theTotalTrx, "", trx.CreatedTime, trx.Maker, trx.ProcessTime, "Done");
                                }
                                else if (trx.Status == ParameterHelper.TRXSTATUS_REJECT)
                                {
                                    string rejectDesc = "";
                                    if (!string.IsNullOrEmpty(trx.Rejecter)) rejectDesc = "Rejected by " + trx.Rejecter;
                                    else
                                    {
                                        string[] desc = trx.Description.Split(new String[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                                        rejectDesc = desc[0];
                                    }
                                    EmailNotificationHelper.MassNotification(session, uid.Email, clientID, trx.SeqNumber, "Payroll", 3, trx.FileDescription, _countTotal, _sumTotal, trx.CreatedTime, trx.Maker, trx.ProcessTime, "Reject (" + rejectDesc + ")");
                                }
                            }
                        }
                        log.Info("Email Send Successfully");
                        trx.StatusEmail = ParameterHelper.PAYROLL_DONEMAIL;
                        session.Update(trx);
                        session.Flush();




                        //if (trx.Status == ParameterHelper.TRXSTATUS_COMPLETE)
                        //{
                        //    String[] uid_np = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        //    foreach (String idnp in uid_np)
                        //    {
                        //        UserMatrix umx = session.Load<UserMatrix>(int.Parse(idnp));
                        //        AuthorityHelper ahe = new AuthorityHelper(umx.Matrix);
                        //        if (ahe.isNeedPendingEmail(3600))
                        //        {
                        //            User uid = session.Load<User>(int.Parse(idnp));
                        //            //state=0->inProcess verify; state=1->inProc Approve; state=2->Succed; state=3->reject
                        //            EmailNotificationHelper.MassNotification(session, uid.Email, o.ClientId, massSWIFfile.Id, "Mass SWIFT", 0, massSWIFfile.FileDescription, counttrx, totalamount, massSWIFfile.CreatedTime, massSWIFfile.Maker, massSWIFfile.Valuedate, "Need Verify");
                        //        }
                        //    }
                            
                        //}
                        //else if(trx.Status == ParameterHelper.TRXSTATUS_REJECT)
                        //{

                        //}
                        #endregion
                    }
                    catch (Exception e)
                    {
                        log.Error("Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace);
                        trx.ErrorDescription = "Send Email Exception >>" + e.Message + ">>" + e.InnerException + ">>" + e.StackTrace;
                        trx.StatusEmail = ParameterHelper.PAYROLL_EXCEPTIONEMAIL;
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
