using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using log4net;
using NHibernate;
using NHibernate.Expression;
using BRIChannelSchedulerNew.Payroll.Pocos;
using BRIChannelSchedulerNew.Payroll.Helper;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Serialization;
using BRIVAWrapper.Briva;
using MySql.Data.MySqlClient;
using System.Threading;

/// <summary>
/// Summary description for FundTransferHelper
/// </summary>
public class FundTransferHelperSch
{
    public static String logtime = DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss") + " ";

    public FundTransferHelperSch(){}

 
    


    public static Boolean workflowCheckerPayroll(ISession session, Transaction trx, ILog log)
    {
        //get trxpayrolls object
        XmlDocument doc = new XmlDocument();
        XmlSerializer ser = new XmlSerializer(typeof(TrxPayroll));
        doc.LoadXml(trx.TObject);
        XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
        object obj = ser.Deserialize(reader);

        TrxPayroll ObjPayroll = (TrxPayroll)obj;
        TrxPayroll payroll = session.Load<TrxPayroll>(ObjPayroll.Id);

        bool result = true;
        string HeaderErrorMsg = "||" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + " ";

        //invalid authority setting status
        string StatusInvalidAuth = "";

        try
        {
            int user_id = 0;
            string clientID = trx.ClientId.ToString();

            //sayedzul add - 20161108
            #region cek rek debet notpool or not
            string debAcc = payroll.DebitAccount.Trim().PadLeft(15, '0');
            NotionalPoolingMember npm = session.CreateCriteria(typeof(NotionalPoolingMember))
                        .Add(Expression.Eq("AccountNumber", debAcc))
                        .Add(Expression.Eq("Status", "A"))
                        .UniqueResult<NotionalPoolingMember>();
            if (npm != null)
            {
                String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                payroll.Status = ParameterHelper.TRXSTATUS_REJECT;
                payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Notional pooling account not permitted for payroll transaction||" + desc[1];
                payroll.ErrorDescription += HeaderErrorMsg + "Notional pooling account not permitted for payroll transaction";

                session.Update(payroll);

                //reject child
                string outRejectChild = "";
                if (rejectAllChild(session, payroll, out outRejectChild, log))
                {
                    log.Info("Success Update All Child.");
                    payroll.ErrorDescription += HeaderErrorMsg + " Success reject child";
                    //delete transactions
                    session.Delete(trx);
                }
                else
                {
                    payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                    payroll.ErrorDescription = HeaderErrorMsg + outRejectChild;
                }

                session.Update(payroll);
                session.Flush();
                return true;
            }
            #endregion
            //end sayedzul

            #region get usermaps/user_id
            UserMap umap = null;
            try
            {
                if (!string.IsNullOrEmpty(trx.Approver))
                {
                    String[] tempmaker = trx.Approver.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String makinfo in tempmaker)
                    {
                        String[] detmake = makinfo.Split(new String[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        umap = session.CreateCriteria(typeof(UserMap))
                            .Add(Expression.Like("UserHandle", detmake[0].Trim()))
                            .Add(Expression.Like("ClientEntity", int.Parse(clientID)))
                            .UniqueResult<UserMap>();
                        user_id = umap.UserEntity;
                    }
                }
                else if (!string.IsNullOrEmpty(trx.Checker))
                {
                    String[] tempmaker = trx.Checker.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String makinfo in tempmaker)
                    {
                        String[] detmake = makinfo.Split(new String[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        umap = session.CreateCriteria(typeof(UserMap))
                            .Add(Expression.Like("UserHandle", detmake[0].Trim()))
                            .Add(Expression.Like("ClientEntity", int.Parse(clientID)))
                            .UniqueResult<UserMap>();
                        user_id = umap.UserEntity;
                    }
                }
                else//maker
                {
                    String[] tempmaker = trx.Maker.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String makinfo in tempmaker)
                    {
                        String[] detmake = makinfo.Split(new String[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                        umap = session.CreateCriteria(typeof(UserMap))
                            .Add(Expression.Like("UserHandle", detmake[0].Trim()))
                            .Add(Expression.Like("ClientEntity", int.Parse(clientID)))
                            .UniqueResult<UserMap>();
                        user_id = umap.UserEntity;
                    }
                }
            }
            catch (Exception ex)
            {
                payroll.ErrorDescription = "Exception on Scheduller Payroll Get Auth :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException;
                payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                session.Update(payroll);
                session.Flush();

                log.Error("Exception on Scheduller Payroll Get Auth :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException);
                //EvtLogger.Write("Exception on Scheduller Payroll Get Auth :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
                result = false;
            }
            log.Info("User Id : " + user_id);
            #endregion

            //get total TRX
            //|susccess|fail|total| ==> |1:200|:|2:300|
            //string[] total = payroll.TotalTrx.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            //string[] pecahan = total[2].Split(':');
            //string counttrx = pecahan[0];
            //string totalamount = pecahan[1];

            //trx.TotalTrx = "|" + feeIFT + "|" + reject + "|" + totalTrx + "|" + feeRTG + "|" + feeLLG + "|";
            if (payroll.TotalTrx.Equals("|0:0|0:0|0:0|0:0|0:0|"))
            {
                //countift : sumift| countrejectAll : sumrejectAll | countAll : sumAll | countRTG : sumRTG | countLLG : sumLLG|
                //ga dipake pun, ga usah lah

                int _countSuccess = payroll.TrxPayrollDetail.Where(x => x.Status != ParameterHelper.TRXSTATUS_REJECT).Count();
                double _sumSuccess = payroll.TrxPayrollDetail.Where(x => x.Status != ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount / 100);
                int _countInvalid = payroll.TrxPayrollDetail.Where(x => x.Status == ParameterHelper.TRXSTATUS_REJECT).Count();
                double _sumInvalid = payroll.TrxPayrollDetail.Where(x => x.Status == ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount / 100);
                int _countTotal = payroll.TrxPayrollDetail.Count();
                double _sumTotal = payroll.TrxPayrollDetail.Sum(x => x.Amount / 100);
                payroll.TotalTrx = "|" + _countSuccess.ToString() + ":" + _sumSuccess.ToString() + "|" + _countInvalid.ToString() + ":" + _sumInvalid.ToString() + "|" + _countTotal.ToString() + ":" + _sumTotal.ToString() + "|";
                //log.Info("Trx Id = " + payroll.Id + ", Total Trx : " + payroll.TotalTrx);
                //session.Update(trx);
                //session.Flush();


                //Denny coba
                //string TotalTrxNew = "|0:0|0:0|0:0|0:0|0:0|";
                //TotalTrxNew = "|" + _countSuccess.ToString() + ":" + _sumSuccess.ToString() + "|" + _countInvalid.ToString() + ":" + _sumInvalid.ToString() + "|" + _countTotal.ToString() + ":" + _sumTotal.ToString() + "|";
                
                #region Native update Parent payroll
                //string temp_query2 = "";
                //string msg2 = "";
                //int count2 = 0;
                //string dbResult2 = "";
                //temp_query2 = @"update trxpayrolls set totaltrx = '" + TotalTrxNew + "' where id = '" + payroll.Id + "';";

                //if (!PayrollHelper.ExecuteQueryValue(session, temp_query2, out dbResult2, out msg2))
                //{
                //    log.Error("(TrxID " + payroll.Id + ") Failed Query: " + temp_query2 + " " + msg2);

                //}
                #endregion native update


            }
            string counttrx = payroll.TrxPayrollDetail.Where(x => x.Status != ParameterHelper.TRXSTATUS_REJECT).Count().ToString();
            double totalamount = payroll.TrxPayrollDetail.Where(x => x.Status != ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount / 100);

            //get debit acc
            string[] debAccArr = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            string debitAccId = debAccArr[1];
            ClientAccount ClientAcc = session.Load<ClientAccount>(int.Parse(debitAccId));
            string debitAcc = ClientAcc.Number;

            string valueDate = payroll.ProcessTime.ToString("yyMMdd");
            if (valueDate.Equals("010101"))
                valueDate = "991231";

            #region cek limit-limit
            if (trx.CheckWork == 0 && trx.ApproveWork == 0)
            {
                string outMsgAuthorityError = "";
                if (CekAuthority(session, 100, umap, "PAYROLL", valueDate, totalamount, "IDR", out outMsgAuthorityError))
                {
                    if (!outMsgAuthorityError.Equals("")) StatusInvalidAuth = "[LIMIT]" + outMsgAuthorityError;
                }
                else
                {
                    StatusInvalidAuth = "[EXCEPTION]" + outMsgAuthorityError;
                }
            }
            #endregion


            #region cek Authority Setting
            //Pengecekkan bypass transaksi di Settingan LIsence - Denny
            bool isWithAuthority = true;
            IList<ClientMatrix> cm = session.CreateCriteria(typeof(ClientMatrix))
                   .Add(Expression.Eq("Id", trx.ClientId))
                   .List<ClientMatrix>();
            foreach (ClientMatrix cm1 in cm)
            {
                AuthorityHelper ah = null;
                ah = new AuthorityHelper(cm1.Matrix);
                isWithAuthority = ah.isDirectTransaction(100);

                //Kondisi menggunakan Direct Transactions
                if (trx.CheckTotal == 0 && trx.ApproveTotal == 0 && isWithAuthority)
                {
                    StatusInvalidAuth = "";
                }

                if (trx.CheckWork == 0 && trx.ApproveWork == 0 && isWithAuthority == false)
                {
                    log.Error("debit Account : " + debitAcc);
                    log.Error("total Account : " + totalamount);
                    log.Error("umap Client Entity : " + umap.ClientEntity);
                    //string nextProcessor = UserGroupHelper.getNextAppTemp(session, 100, umap, trx.Approver, "Payroll", debitAcc, "IDR", double.Parse(totalamount));


                    string nextProcessorChecker = "GAK PAKE CHECKER";
                    string nextProcessorApprover = "GAK PAKE APPROVER";
                    if (trx.CheckTotal != 0) nextProcessorChecker = UserGroupHelper.getNextCheTemp(session, 100, umap, "", "Payroll", debitAcc, "IDR", totalamount);
                    if (trx.ApproveTotal != 0) nextProcessorApprover = UserGroupHelper.getNextAppTemp(session, 100, umap, "", "Payroll", debitAcc, "IDR", totalamount);

                    log.Error("Checker : " + nextProcessorChecker);
                    log.Error("Approver : " + nextProcessorApprover);

                    nextProcessorChecker = nextProcessorChecker.Replace("|", "");
                    nextProcessorApprover = nextProcessorApprover.Replace("|", "");

                    if (string.IsNullOrEmpty(nextProcessorChecker)) StatusInvalidAuth += "CHECKER";
                    if (string.IsNullOrEmpty(nextProcessorApprover)) StatusInvalidAuth += " and APPROVER";
                }
            }
        
            #endregion


            if (StatusInvalidAuth.Equals(""))//authority valid
            {
                //transaction complete
                if (trx.CheckWork >= trx.CheckTotal)
                {
                    if (trx.ApproveWork >= trx.ApproveTotal)//FINAL, transactions ready to process
                    {
                        //update trxpayroll
                        String[] des = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                        if (payroll.ProcessTime.ToString("yyyy-MM-dd").Equals("0001-01-01"))
                        {
                            payroll.ProcessTime = DateTime.Now;
                        }

                        //20170423 sayedzul add handler payroll processtime
                        if (payroll.IsPayrollBankLain == 1)
                        {
                            payroll.ProcessTime = PayrollHelper.GetRealProcessTime(session, payroll.ProcessTime);
                        }

                        payroll.Maker = trx.Maker;
                        payroll.Checker = trx.Checker;
                        payroll.Approver = trx.Approver;
                        payroll.Status = ParameterHelper.TRXSTATUS_RUNNING_AFTERCHECKACCOUNT;//19
                        payroll.Description = "PROCESSED" + "||" + des[1];
                        session.Update(payroll);
                        session.Flush();

                        //delete transactions
                        session.Delete(trx);
                        session.Flush();

                        log.Info("Finish. Data deleteled from table transactions");
                    }
                    else
                    {
                        //Masih Butuh Approval
                        log.Info("Total Amount" + totalamount);
                        string nextProcessor = UserGroupHelper.getNextAppTemp(session, 100, umap, trx.Approver, "Payroll", debitAcc, "IDR", totalamount);

                        trx.NextProcessor = nextProcessor;
                        trx.Status = short.Parse(ParameterHelper.TRXSTATUS_APPROVE.ToString());
                        session.Update(trx);
                        log.Info("Masih Butuh Approver");

                        session.Flush();

                        #region Send Notification
                        if (!nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            String[] uid_np = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (String idnp in uid_np)
                            {
                                UserMatrix umx = session.Load<UserMatrix>(int.Parse(idnp));
                                AuthorityHelper ahe = new AuthorityHelper(umx.Matrix);
                                if (ahe.isNeedPendingEmail(100))
                                {
                                    User uid = session.Load<User>(int.Parse(idnp));
                                    //state=0->inProcess verify; state=1->inProc Approve; state=2->Succed; state=3->reject
                                    EmailNotificationHelper.MassNotification(session, uid.Email, trx.ClientId, trx.Id, "Payroll", 1, payroll.FileDescription, counttrx, totalamount.ToString(), payroll.CreatedTime, trx.Maker, payroll.ProcessTime, "Need Approve");
                                }
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    //Masih Butuh Checker
                    Console.Write("===User Handle : " + umap.UserHandle);

                    string nextProcessor = UserGroupHelper.getNextCheTemp(session, 100, umap, trx.Checker, "Payroll", debitAcc, "IDR", totalamount);

                    trx.NextProcessor = nextProcessor;
                    trx.Status = short.Parse(ParameterHelper.TRXSTATUS_VERIFY.ToString());
                    session.Update(trx);

                    session.Flush();
                    log.Info("Next Processor : " + nextProcessor);
                    log.Info("Masih Butuh Checker");

                    #region Send Notification
                    if (!nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        String[] uid_np = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (String idnp in uid_np)
                        {
                            UserMatrix umx = session.Load<UserMatrix>(int.Parse(idnp));
                            AuthorityHelper ahe = new AuthorityHelper(umx.Matrix);
                            if (ahe.isNeedPendingEmail(100))
                            {
                                User uid = session.Load<User>(int.Parse(idnp));
                                //state=0->inProcess verify; state=1->inProc Approve; state=2->Succed; state=3->reject
                                EmailNotificationHelper.MassNotification(session, uid.Email, trx.ClientId, trx.Id, "Payroll", 0, payroll.FileDescription, counttrx, totalamount.ToString(), payroll.CreatedTime, trx.Maker, payroll.ProcessTime, "Need Verify");
                            }
                        }
                    }
                    #endregion
                }
            }
            else//invalid authority setting
            {
                string invalidMsg = "";
                if (StatusInvalidAuth.Equals(" and APPROVER")) invalidMsg = "APPROVER";
                else invalidMsg = StatusInvalidAuth;

                String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                payroll.Status = ParameterHelper.TRXSTATUS_REJECT;
                payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                if (invalidMsg.Contains("[LIMIT]"))
                {
                    payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - " + invalidMsg.Replace("[LIMIT]", "") + "||" + desc[1];
                    payroll.ErrorDescription += HeaderErrorMsg + "Invalid Authority Settting on " + invalidMsg;
                }
                else if (invalidMsg.Contains("[EXCEPTION]"))
                {
                    payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                    payroll.ErrorDescription = HeaderErrorMsg + invalidMsg.Replace("[EXCEPTION]", "");
                }
                else
                {
                    payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Invalid Authority Setting on " + invalidMsg + "||" + desc[1];
                    payroll.ErrorDescription += HeaderErrorMsg + "Invalid Authority Settting on " + invalidMsg;
                }
                session.Update(payroll);

                //reject child
                string outRejectChild = "";
                if (rejectAllChild(session, payroll, out outRejectChild, log))
                {
                    log.Info("Success Update All Child.");
                    payroll.ErrorDescription += HeaderErrorMsg + " Success reject child";
                    //delete transactions
                    session.Delete(trx);
                }
                else
                {
                    payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                    payroll.ErrorDescription = HeaderErrorMsg + outRejectChild;
                }
            }


            session.Update(payroll);
            session.Flush();

            


            result = true;
        }
        catch (Exception ex)
        {
            log.Error("Exception on Scheduller Workflow Check Payroll :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException);
            //EvtLogger.Write("Exception on Scheduller FT Workflow Check :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
            payroll.ErrorDescription += HeaderErrorMsg + "|Exception on Scheduller Workflow Check Payroll :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException;
            payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
            session.Update(payroll);
            session.Flush();
            result = false;
        }
        return result;
    }


    //Payroll Reject Detail
    public static bool rejectAllChild(ISession session, TrxPayroll payroll, out string outMsgUpdate, ILog log)
    {
        outMsgUpdate = "";
        bool result = true;
        try
        {
            log.Info("Reject All Child.");
            //Create Connection
            Parameter dbConf = session.Load<Parameter>("CMS_DBCONNECTION");
            MySqlConnection conn = new MySqlConnection(dbConf.Data);
            conn.Open();
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "update trxpayrolldetails set status=" + ParameterHelper.TRXSTATUS_REJECT + ", description='Rejected by System' where pid='" + payroll.Id + "'";
            int i = command.ExecuteNonQuery();
            log.Info("command Execute = " + i);
            conn.Close();
        }
        catch (Exception e)
        {
            string eMsg = "Error on update child " + e.Message + "==" + e.InnerException + "==" + e.StackTrace;
            log.Error(eMsg);
            outMsgUpdate = eMsg;
            result = false;
        }
        return result;
    }

    //dipake payroll
    private static Boolean CekAuthority(ISession session, int fid, UserMap umap, String trxCode, String valuedate, double trxAmount, String currtrx, out String error)
    {
        bool result = true;
        error = "";
        AuthorityHelper ah = null;

        try
        {
            ClientMatrix cm = session.Load<ClientMatrix>(umap.ClientEntity);
            UserMatrix um = session.Load<UserMatrix>(umap.UserEntity);

            if (!currtrx.Trim().Equals("IDR"))
            {
                Currency curr = session.CreateCriteria(typeof(Currency))
                    .Add(Expression.Like("Code", "%" + currtrx.Trim() + "%"))
                    .UniqueResult<Currency>();

                trxAmount = double.Parse((trxAmount * (double)curr.BookingRate).ToString());
            }

            //Limit Korporasi
            ah = new AuthorityHelper(cm.Matrix);
            if (!ah.OnUserLimit(fid.ToString(), trxAmount))
            {
                error += "Melewati limit per transaksi corporate*";
            }

            //Limit User
            ah = new AuthorityHelper(um.Matrix);
            if (!ah.OnUserLimit(fid.ToString(), trxAmount))
            {
                error += "Melewati limit per transaksi user*";
            }

            //Total Limit Harian
            String resultLimit = TotalOBHelper.onTotalHarianLimit(session, umap.ClientEntity, trxCode, valuedate, currtrx.Trim(), trxAmount);
            if (!resultLimit.Contains("[VALID]"))
            {
                error += resultLimit + "*";
            }
        }
        catch (Exception he)
        {
            error = he.Message + "||" + he.StackTrace + "||" + he.InnerException;
            result = false;
        }

        return result;
    }


    private static String _CurrNo(String Currency)
    {
        switch (Currency)
        {
            case "IDR":
                return ("01");
            case "USD":
                return ("02");
            case "SGD":
                return ("03");
            case "JPY":
                return ("04");
            case "GBP":
                return ("05");
            case "HKD":
                return ("06");
            case "DEM":
                return ("07");
            case "MYR":
                return ("08");
            case "AUD":
                return ("09");
            case "CAD":
                return ("10");
            case "CHF":
                return ("11");
            case "EUR":
                return ("18");
            case "THB":
                return ("19");
            case "SEK":
                return ("20");
            case "SAR":
                return ("21");
            case "CNY":
                return ("27");
            default:
                return (" ");
        }
    }




}
