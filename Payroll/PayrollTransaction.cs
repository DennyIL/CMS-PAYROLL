using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;
using log4net;
using BRIChannelSchedulerNew.Payroll.Pocos;
using BRIChannelSchedulerNew.Payroll.Helper;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Data;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;
using System.Xml.Linq;
using BriInterfaceWrapper.BriInterface;


namespace BRIChannelSchedulerNew.Payroll
{
    class PayrollTransaction : IStatefulJob
    {
        private static Configuration cfg;
        private static ISessionFactory factory;
        //private static int limitAccTimeout;
        ISession session;
        #region IJob Members

        void IJob.Execute(JobExecutionContext context)
        {
            ILog logs = LogManager.GetLogger(typeof(PayrollTransaction));
            logs.Info("=== Send Payroll Trx process ===");
            session = null;
            try
            {
                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }
                session = factory.OpenSession();
                try
                {
                    IList<TrxPayroll> lpayroll = session.CreateCriteria(typeof(TrxPayroll))
                      .Add(Expression.Eq("StatusIFT", ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_TRANSFERFILE)) // kasi status abis create file dan dikirim
                      .Add(Expression.Lt("LastUpdate", DateTime.Now))
                      //.Add(Expression.group
                      .AddOrder(Order.Asc("CreatedTime"))
                      .List<TrxPayroll>();

                    if (lpayroll.Count > 0)
                    {
                        foreach (TrxPayroll pay in lpayroll)
                        {
                            try
                            {
                                //rfq, 25062015 (hanya kirim 1 transaksi untuk rek debet sama), comment dulu
                                IList<TrxPayroll> lpayroll2 = session.CreateCriteria(typeof(TrxPayroll))
                                    .Add(Expression.Eq("StatusIFT", ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_MBASE))//6
                                    .Add(Expression.Eq("DebitAccount", pay.DebitAccount))
                                    .List<TrxPayroll>();

                                if (lpayroll2.Count <= 0)//ekse jika tidak ada yang menunggu
                                {
                                    #region eksekusi
                                    String[] desc = pay.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                                    String filename = desc[2].Trim();

                                    //Update last update
                                    pay.LastUpdate = DateTime.Now;
                                    session.Update(pay);
                                    session.Flush();


                                    MessageMBASETransactionResponse response = null;
                                    string out_msg = "";
                                    response = SendTransaction(session, filename, logs, out out_msg);
                                    if (int.Parse(response.statuscode.Trim()) == 1)
                                    {
                                        if (int.Parse(response.msgmbmessage[1].Trim()) == 00)
                                        {
                                            logs.Info("Send Trancode 8630 ok !");
                                            pay.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_MBASE;
                                            pay.ErrorDescription += "||OrigProcessTime:" + pay.ProcessTime.ToString("dd/MM/yyyy HH:mm:ss") + "||";
                                            pay.ProcessTime = DateTime.Now;
                                            session.Save(pay);
                                            session.Flush();
                                        }
                                        else
                                        {
                                            if (out_msg.StartsWith("[ALREADY SEND]"))
                                            {
                                                logs.Info("File already send, waiting response.");
                                                pay.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_MBASE;
                                                pay.ErrorDescription = out_msg;
                                                session.Update(pay);
                                                session.Flush();
                                            }
                                            else
                                            {
                                                logs.Info("Send Trancode 8630 failed !");
                                                //set jeda eksekusi lagi
                                                Parameter paramJeda = session.Load<Parameter>("PAYROLL_EXECUTING_TIMETRYAGAIN");
                                                int jeda = int.Parse(paramJeda.Data);
                                                pay.LastUpdate = DateTime.Now.AddMinutes(jeda);
                                                logs.Info("Send Trancode 8630 failed !, try again in " + DateTime.Now.AddMinutes(jeda).ToString("dd/MM/yyyy HH:mm:ss") + ".||Msg : " + out_msg);
                                                pay.ErrorDescription = "Send Trancode 8630 failed !, try again in " + DateTime.Now.AddMinutes(jeda).ToString("dd/MM/yyyy HH:mm:ss") + ".||Msg : " + out_msg;
                                                session.Update(pay);
                                                session.Flush();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //set jeda eksekusi lagi
                                        Parameter paramJeda = session.Load<Parameter>("PAYROLL_EXECUTING_TIMETRYAGAIN");
                                        int jeda = int.Parse(paramJeda.Data);
                                        pay.LastUpdate = DateTime.Now.AddMinutes(jeda);
                                        logs.Info("MBASE Time Out. Will try again at " + DateTime.Now.AddMinutes(jeda).ToString("dd/MM/yyyy HH:mm:ss") + ".||Msg : " + out_msg);
                                        pay.ErrorDescription += "MBASE Time Out. Will try again at " + DateTime.Now.AddMinutes(jeda).ToString("dd/MM/yyyy HH:mm:ss") + ".||Msg : " + out_msg;
                                        session.Update(pay);
                                        session.Flush();
                                    }
                                    #endregion
                                }
                                else
                                {
                                    pay.LastUpdate = DateTime.Now.AddMinutes(2);
                                    pay.ErrorDescription += "Ada yg lagi di proses dengan rek debet sama. Try again at " + DateTime.Now.AddMinutes(2).ToString("dd/MM/yyyy HH:mm:ss");
                                    session.Update(pay);
                                    session.Flush();
                                }
                            }
                            catch (Exception ee)
                            {
                                //set jeda eksekusi lagi
                                Parameter paramJeda = session.Load<Parameter>("PAYROLL_EXECUTING_TIMETRYAGAIN");
                                int jeda = int.Parse(paramJeda.Data);
                                pay.LastUpdate = DateTime.Now.AddMinutes(jeda);

                                logs.Error("Scheduler Payroll Transaction Error : " + ee.Message + ee.StackTrace);
                                pay.ErrorDescription = "Scheduler Payroll Transaction failed to start : " + ee.Message + ">>>" + ee.StackTrace + ">>>" + ee.InnerException;
                                session.Update(pay);
                                session.Flush();
                            }
                        }
                    }
                }
                catch
                {

                }
            }
            catch (Exception ee)
            {
                logs.Error("Scheduler Payroll Transaction failed to start : " + ee.Message + ee.StackTrace);
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

        private MessageMBASETransactionResponse SendTransaction(ISession session, String filename, ILog logs, out string msg)
        {
            msg = "";
            MessageMBASETransactionRequest request = new MessageMBASETransactionRequest();
            MessageMBASETransactionResponse response = new MessageMBASETransactionResponse();
            briInterfaceService transaction = new briInterfaceService();

            DataTable dSocketHeader = null;
            DataTable dMiddleWareHeader = null;
            DataTable dMbaseHeader = null;
            DataTable dMbaseMessage = null;

            String trancode = "8630";
           

            try
            {
                dSocketHeader = new DataTable("SOCKETHEADER");
                dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
                dMbaseHeader = new DataTable("MBASEHEADER");
                dMbaseMessage = new DataTable("MBASEMESSAGE");

                transaction.initiateMBASE(trancode, ref dSocketHeader, ref dMiddleWareHeader, ref dMbaseHeader, ref dMbaseMessage);

                dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;
                dMbaseHeader.Rows[0]["TRANSACTIONCODE"] = trancode;
                dMbaseHeader.Rows[0]["ACTIONCODE"] = "I"; //"A"; //"I"; // "A";
                dMbaseHeader.Rows[0]["NOOFRECORD"] = "1";
                dMbaseHeader.Rows[0]["ACCOUNT"] = ""; //033001005272504; //"020601000002309"; //"020602000090308";//"020601000012508";//"020601000002309";
                dMbaseHeader.Rows[0]["ACCOUNTTYPE"] = ""; // "D";
                dMbaseHeader.Rows[0]["CIF"] = ""; //"M008474";

                dMbaseMessage.Rows[0]["IFILEID"] = filename.Trim(); //033001005272504;
                //dMbaseMessage.Rows[0]["TLSTYP"] = "";

                request.applicationname = "CMS PAYROLL";
                request.branchcode = "0374"; // "0301";
                request.tellerid = "0374051"; // "0301001";
                request.supervisorid = "";
                request.origjournalseq = ""; //"2601";
                request.dSocketHeader = dSocketHeader;
                request.dMiddleWareHeader = dMiddleWareHeader;
                request.dMbaseHeader = dMbaseHeader;
                request.dMbaseMessage = dMbaseMessage;

                response = transaction.doMBASETransaction(request);

                if (int.Parse(response.statuscode.Trim()) == 1)
                {
                    logs.Info("Send Trancode OK ! ");
                    if (int.Parse(response.msgmbmessage[1].Trim()) == 00)
                    {
                        logs.Info("File sent !");
                        msg = "File sent !";
                    }//sukses
                    if (int.Parse(response.msgmbmessage[1].Trim()) == 01)
                    {
                        logs.Info("File already send to host");
                        msg = "[ALREADY SEND]File already send to host";
                    }; //file pernah dikirim
                }
                else if (int.Parse(response.statuscode.Trim()) == 4)
                {
                    logs.Info("Send Trancode Error ! with error message =" + response.statuscode.Trim() + "||" + response.statusdesc);
                    msg = "Send Trancode Error ! with error message =" + response.statuscode.Trim() + "||" + response.statusdesc;
                }
                else
                {
                    logs.Info("Send Trancode Error !  status = " + response.statuscode.Trim() + "||"+ response.statusdesc);
                    msg = "Send Trancode Error !  status = " + response.statuscode.Trim() + "||" + response.statusdesc;
                }

            }
            catch (Exception ex)
            {
                logs.Info("exception : " + ex.Message + "--->" + ex.StackTrace);
                msg = "exception : " + ex.Message + "--->" + ex.StackTrace;
            }

            logs.Info("response :" + response.statuscode.Trim() + "send trx response : " + response);
            //msg = "response :" + response.statuscode.Trim() + "send trx response : " + response;
            return response;
        }
    }
}
