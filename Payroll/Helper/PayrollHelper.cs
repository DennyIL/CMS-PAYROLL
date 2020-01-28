using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Quartz;
using Quartz.Impl;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using BRIChannelSchedulerNew.Payroll.Helper;
using BRIChannelSchedulerNew.Payroll.Pocos;
using MySql.Data.MySqlClient;
using BriInterfaceWrapper.BriInterface;
using System.Threading;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;


public class PayrollHelper
{
    private static String SchCode = "PayrollLLGHelper";

    public PayrollHelper()
    {

    }

    private static UTF8Encoding encoder = new UTF8Encoding();
    private static MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();

    public static String MD5(String password)
    {
        String result = "";
        byte[] hash = provider.ComputeHash(encoder.GetBytes(password));
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        foreach (byte b in hash)
        {
            s.Append(b.ToString("x2").ToLower());
        }
        result = s.ToString();

        return result;
    }

    //Denny - 11 April 2017 -- Cek Limit Payroll
    public static Boolean GetLimitIDRTempPayroll(ISession session, string query, out string trxAmt, out string trxCurr, out string message)
    {
        Boolean result = false;

        message = "";
        Dictionary<string, decimal> amtDict = new Dictionary<string, decimal>();
        Dictionary<string, decimal> cntDict = new Dictionary<string, decimal>();
        trxAmt = "";
        trxCurr = "";
        MySqlConnection conn = new MySqlConnection();
        try
        {
            //open connection
            //BRIChannel.Pocos.Parameter dbConf = session.Load<Parameter>("CMS_DBCONNECTION");
            String DBConfig = ParameterHelper.GetString("CMS_DBCONNECTION", session);
            conn = new MySqlConnection(DBConfig);
            conn.Open();

            //create command
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = query;
            command.CommandTimeout = 6000;//dalam second

            //ekse query
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string curr = "IDR";
                decimal amt = Decimal.Parse((String)reader["AMOUNT"]);
                if (!amtDict.ContainsKey(curr))
                    amtDict.Add(curr, amt);
                else
                    amtDict[curr] += amt;
            }
            reader.Close();
            message = "SUKSES";
            result = true;

            //olah data
            if (amtDict.Count > 0)
            {
                foreach (var item in amtDict)
                {
                    if (trxCurr.Equals("")) trxCurr += item.Key;
                    else trxCurr += "|" + item.Key;

                    if (trxAmt.Equals("")) trxAmt += item.Value;
                    else trxAmt += "|" + item.Value;
                }
            }
        }
        catch (Exception e)
        {
            message = e.Message + "||" + e.InnerException + "||" + e.StackTrace;
            result = false;
        }
        finally
        {
            conn.Close();
        }

        return result;
    }
    //end Denny


    //Denny 
    public static void ProcessRunBeforePayroll(ISession session, TrxPayroll o, ILog log, string _schCode)
    {
        string msg = "";
        if (o.IsPayrollBankLain == 0)
        {
            log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") All IFT Trx, Proceeed to booking W/O IA Debet");

            if (UpdateDataToBeReadyToBook(session, o, out msg))
                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Update Data SUCCESS");
            else
            {
                log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Update Data FAILED " + msg);
                o.LastUpdate = DateTime.Now.AddMinutes(2);
                session.Update(o);
                session.Flush();
            }
        }
        else
        {
            log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Payroll RTG/LLG Trx, Proceeed to booking With IA Debet");

            double totAmtIFT = 0; 
            double totFeeIFT = 0; 
            double totAmtRTG = 0; 
            double totFeeRTG = 0; 
            double totAmtLLG = 0; 
            double totFeeLLG = 0;
            double theAmt = 0;

            //BRIS-2020-01-16
            double totAmtBRIS = 0;
            double totFeeBRIS = 0;

            int result = 0;
            double available = 0; 
            double hold = 0; 
            double cBal = 0; 
            int statusrek = 0; 
            string namaRek = "";
            int jSeq = 0;

            string debitAcc = o.DebitAccount;

            if (o.Status == ParameterHelper.TRXSTATUS_RUNNING_AFTERCHECKACCOUNT)//19
            {
                #region DUNIA PERSILATAN HOLD
                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") InquiryHoldAmt");

                result = InquiryHoldAmt(debitAcc, out available, out hold, out cBal, out statusrek, out namaRek, out msg);
                if (result == 1)
                {
                    //sukses inquiry
                    if (statusrek == 1 || statusrek == 4)
                    {
                        theAmt = GetAmountDetailPayroll(session, o, out totAmtIFT, out totFeeIFT, out totAmtRTG, out totFeeRTG, out totAmtLLG, out totFeeLLG, out totAmtBRIS, out totFeeBRIS);
                        string remarkHold = "HOLD PAYROLL SEQ " + o.SeqNumber.ToString();
                        string expDate = DateTime.Now.AddYears(1).ToString("ddMMyy");

                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") InquiryHoldAmt SUCCESS ==== statusrek: " + statusrek + " available: " + available + " hold: " + hold + " cBal: " + cBal + " theAmt: " + theAmt);

                        if (hold == 0 || hold < theAmt)
                        {
                            if (available >= theAmt)
                            {
                                #region hold
                                result = 0;

                                result = HoldAmount(session, debitAcc, "HG", theAmt, remarkHold, expDate, "03", out jSeq);
                                if (result == 1 && jSeq > 0)
                                {
                                    //sukses hold
                                    o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_IADEBET;//20
                                    o.JSeqHold = jSeq.ToString();
                                    log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Hold Amount SUCCESS");
                                }
                                else
                                {
                                    //trx gagal - ga bisa hold
                                    o.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Hold Amount Failed" + "||" + o.Description;
                                    log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Hold Amount FAILED");
                                }
                                #endregion hold
                            }
                            else
                            {
                                //trx gagal - ga punya saldo
                                o.Status = ParameterHelper.TRXSTATUS_REJECT;
                                o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Insufficient Balance" + "||" + o.Description;
                                log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Insufficient Balance");
                            }
                        }
                        else
                        {
                            //sudah punya hold
                            log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Hold Amount Detected, start inquiry hold Amount");

                            #region check hold
                            int i = 1;
                            int seq = 0; string remark = ""; double amtHold = 0; string statusDel = ""; expDate = ""; string rCode = ""; string tipe = ""; string msgErr = "";
                            bool njut = true;
                            bool isTernyataSudahHold = false;
                            int limitSeq = ParameterHelper.GetInteger("PAYROLL_MAX_SEQ_HOLD", session);
                            while (njut)
                            {
                                result = 0;
                                result = inqHoldNew(debitAcc, i.ToString(), out seq, out remark, out amtHold, out statusDel, out expDate, out rCode, out tipe, out msgErr);

                                if (result == 1 && remark.Equals(remarkHold) && amtHold == theAmt && !statusDel.Equals("D"))
                                {
                                    njut = false;
                                    isTernyataSudahHold = true;
                                }

                                if ((result == 2 && msgErr.ToLower().Contains("no records found")) || i > limitSeq)
                                    njut = false;
                                i++;
                            }
                            #endregion

                            if (isTernyataSudahHold)
                            {
                                //simpan seq holdnya
                                o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_IADEBET;//20
                                o.JSeqHold = seq.ToString();
                                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") isTernyataSudahHold: TRUE");
                            }
                            else if (i > limitSeq)
                            {
                                //maksimum inq hold
                                o.Status = ParameterHelper.TRXSTATUS_REJECT;
                                o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Maximum Inquiry Hold Amount" + "||" + o.Description;
                                log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Maximum Inquiry Hold Amount");
                            }
                            else
                            {
                                //belon di hold, ya hold aja klo gt
                                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") isTernyataSudahHold: FALSE");
                                if (available >= theAmt)
                                {
                                    #region hold
                                    result = 0;
                                    jSeq = 0;
                                    result = HoldAmount(session, debitAcc, "HG", theAmt, remarkHold, expDate, "03", out jSeq);
                                    if (result == 1 && jSeq > 0)
                                    {
                                        //sukses hold
                                        o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_IADEBET;//20
                                        o.JSeqHold = jSeq.ToString();
                                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Hold Amount SUCCESS");
                                    }
                                    else
                                    {
                                        //trx gagal - ga bisa hold
                                        o.Status = ParameterHelper.TRXSTATUS_REJECT;
                                        o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Hold Amount Failed" + "||" + o.Description;
                                        log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Hold Amount FAILED");
                                    }
                                    #endregion check hold
                                }
                                else
                                {
                                    //trx gagal - ga punya saldo
                                    o.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Insufficient Balance" + "||" + o.Description;
                                    log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Insufficient Balance");
                                }
                            }
                        }
                        o.LastUpdate = DateTime.Now;
                    }
                    else
                    {
                        //trx gagal - rek ga aktif
                        if (statusrek == 2) msg = "Account " + debitAcc + " Closed";
                        else if (statusrek == 3) msg = "Account " + debitAcc + " Maturity";
                        else if (statusrek == 5) msg = "Account " + debitAcc + " Invalid";
                        else if (statusrek == 6) msg = "Account " + debitAcc + " Restricted";
                        else if (statusrek == 7) msg = "Account " + debitAcc + " Blocked";
                        else if (statusrek == 8) msg = "Account " + debitAcc + " Undefined";
                        else if (statusrek == 9) msg = "Account " + debitAcc + " Dormant";
                        else msg = "Account " + debitAcc + " Undefined";

                        o.Status = ParameterHelper.TRXSTATUS_REJECT;
                        o.LastUpdate = DateTime.Now;
                        o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - " + msg + "||" + o.Description;
                        log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") " + msg);
                    }
                }
                else if (result == 2)
                {
                    //gagal inquiry, rekening ga dikenal
                    o.Status = ParameterHelper.TRXSTATUS_REJECT;
                    o.LastUpdate = DateTime.Now;
                    o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - " + msg + "||" + o.Description;

                    log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Inquiry Acc " + debitAcc + " FAILED: " + msg);
                }
                else
                {
                    //gagal inquiry, coba lagi nanti
                    o.LastUpdate = DateTime.Now.AddMinutes(2);
                }

                session.Update(o);
                session.Flush();
                #endregion
            }

            if (o.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_IADEBET)//20 IA DEBET -> IA KREDIT => Topup
            {
                #region DUNIA PERSILATAN POST TO BOOKING
                int idbooking = 0;
                int fid = 100;
                int clientId = o.ClientID;
                debitAcc = ParameterHelper.GetString("PAYROLL_IA_DEBET", session);
                string debitAccName = "PAYROLL_IA_DEBET";
                string creditAcc = ParameterHelper.GetString("PAYROLL_IA_KREDIT", session);
                string creditAccName = "PAYROLL_IA_KREDIT";
                string fitur = TotalOBHelper.Payroll;
                string _remark = "PINDAH BUKU PAYROLL SEQ " + o.SeqNumber.ToString();
                msg = "";
                if (theAmt <= 0)
                    theAmt = GetAmountDetailPayroll(session, o, out totAmtIFT, out totFeeIFT, out totAmtRTG, out totFeeRTG, out totAmtLLG, out totFeeLLG, out totAmtBRIS, out totFeeBRIS);

                if (BookingHelper.InsertBooking(session, o.SeqNumber.ToString(), fid, fitur, clientId, debitAcc, debitAccName, theAmt,
                            "IDR", creditAcc, creditAccName, theAmt, "IDR", theAmt, "IDR", "", _remark,
                            DateTime.Now, ParameterHelper.BOOKTYPE_IADEBET, "", out idbooking, out msg))
                {
                    log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Insert to Booking Amount SUCCESS");
                    o.IdBooking1 = idbooking;
                    o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IADEBET;//21
                    o.LastUpdate = DateTime.Now;
                }
                else
                {
                    if (idbooking != 0)
                    {
                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Duplicate Booking Amount Detected");
                        o.IdBooking1 = idbooking;
                        o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IADEBET;//21
                        o.LastUpdate = DateTime.Now;

                    }
                    else
                    {
                        log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Insert to Booking Amount Failed ::: " + msg);
                        o.LastUpdate = DateTime.Now.AddMinutes(2);
                    }
                }
                session.Update(o);
                session.Flush();
                #endregion DUNIA PERSILATAN POST TO BOOKING
            }

            if (o.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IADEBET)//21 IA DEBET -> IA KREDIT => Topup IA KREDIT Berhasil hiyahiya- Kalo gagal Topup lgsung reject aja
            {
                #region DUNIA PERSILATAN CHECK BOOKING
                Booking bookAmt = session.Load<Booking>(o.IdBooking1);
                if (bookAmt.Status == ParameterHelper.SCH_BOOKSUCCESS)
                {
                    log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount SUCCESS");
                    o.AmountFromIADebet = bookAmt.CreditAmount;
                    if (UpdateDataToBeReadyToBook(session, o, out msg))//Lakukan pemisahan => ini nih
                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Update Data SUCCESS");
                    else
                    {
                        log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Update Data FAILED " + msg);
                        o.LastUpdate = DateTime.Now.AddMinutes(2);
                        session.Update(o);
                        session.Flush();
                    }
                }
                else if (bookAmt.Status == ParameterHelper.SCH_BOOKFAILED)
                {
                    log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount FAILED: " + bookAmt.Description + " === UnHolding Amount");
                    #region unhold
                    result = 0;
                    result = UnHoldAmount(o.DebitAccount, o.JSeqHold, out msg);
                    if (result == 1)
                    {
                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") UnHold Amount SUCCESS");
                        o.Status = ParameterHelper.TRXSTATUS_REJECT;
                        o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Unable to Book IA to IA " + bookAmt.Description + "||" + o.Description;
                        o.LastUpdate = DateTime.Now;
                    }
                    else if (result == 2)
                    {
                        log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") UnHold Amount FAILED: " + msg);
                        o.Status = ParameterHelper.PAYROLL_EXCEPTION;
                        o.LastUpdate = DateTime.Now;
                    }
                    else
                    {
                        log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") UnHold Amount FAILED, retry in 2 min");
                        o.LastUpdate = DateTime.Now.AddMinutes(2);
                    }
                    session.Update(o);
                    session.Flush();
                    #endregion unhold
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + _schCode + " === (SeqNo " + o.SeqNumber + ") - Not Complete Yet, status " + bookAmt.Status + " " + bookAmt.Description);
                    //o.LastUpdate = DateTime.Now.AddSeconds(30);
                }
                session.Update(o);
                session.Flush();
                #endregion DUNIA PERSILATAN CHECK BOOKING
            }
        }
        session.Update(o);
        session.Flush();
    }

    public static int HoldAmount(ISession session, string noRekHold, string typeHold, double amtHold, string remarkHold, string expHold, string reasonCode, out int jSeq)
    {
        int result = 0;
        jSeq = 0;
        try
        {
            MessageMBASETransactionRequest requestMBASE = new MessageMBASETransactionRequest();
            MessageMBASETransactionResponse responseMBASE = new MessageMBASETransactionResponse();
            briInterfaceService transactionMBASE = new briInterfaceService();

            DataTable dSocketHeader = new DataTable("SOCKETHEADER");
            DataTable dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
            DataTable dMbaseHeader = new DataTable("MBASEHEADER");
            DataTable dMbaseMessage = new DataTable("MBASEMESSAGE");

            string trancodeHold = "2704";

            //string remarkHold = "HOLD AMOUNT UNTUK PAYMENT PELINDO III";         // dapat Request Remark dari Pelindo 3 

            string accountType = "";
            if (noRekHold.Substring(12, 1).Equals("5"))
                accountType = "S";
            else if (noRekHold.Substring(12, 1).Equals("3"))
                accountType = "D";
            else if (noRekHold.Substring(12, 1).Equals("9"))
                accountType = "D";
            else if (noRekHold.Substring(12, 1).Equals("1"))
                accountType = "L";
            else
                accountType = "S";

            double amountHold = amtHold * 100;
            string str_AmountHold = Convert.ToString(Convert.ToInt64(amountHold));

            try
            {
                transactionMBASE.initiateMBASE(trancodeHold, ref dSocketHeader, ref dMiddleWareHeader, ref dMbaseHeader, ref dMbaseMessage);
            }
            catch
            {
                return 99;
            }
            dMiddleWareHeader.Rows[0]["TRANCODE"] = trancodeHold;
            dMbaseHeader.Rows[0]["TRANSACTIONCODE"] = trancodeHold;
            dMbaseHeader.Rows[0]["ACTIONCODE"] = "A";
            dMbaseHeader.Rows[0]["NOOFRECORD"] = "1";
            dMbaseHeader.Rows[0]["BANKNUMBER"] = "2";
            //dMbaseHeader.Rows[0]["BRANCHNUMBER"] = branchRek;                               // uker sesuai No Rekening yg di Hold
            dMbaseMessage.Rows[0]["ACCOUNTNUMBER"] = noRekHold;
            dMbaseMessage.Rows[0]["ACCOUNTTYPE"] = accountType;
            dMbaseMessage.Rows[0]["SEQUENCE"] = "";
            dMbaseMessage.Rows[0]["RECORDID"] = " ";
            dMbaseMessage.Rows[0]["TYPEOFENTRY"] = typeHold;
            dMbaseMessage.Rows[0]["CHECKAMOUNT"] = str_AmountHold;
            dMbaseMessage.Rows[0]["LOWCHECKNUMBER"] = "0";
            dMbaseMessage.Rows[0]["HIGHCHECKNUMBER"] = "0";
            dMbaseMessage.Rows[0]["STOPCHARGE"] = "0";
            dMbaseMessage.Rows[0]["PAYEENAME"] = " ";
            dMbaseMessage.Rows[0]["STOPHOLDREMARKS"] = remarkHold;
            dMbaseMessage.Rows[0]["CHECKRTNUMBER"] = "0";
            dMbaseMessage.Rows[0]["EXPIRATIONDATE"] = expHold;
            dMbaseMessage.Rows[0]["CHECKDATE"] = "0";
            dMbaseMessage.Rows[0]["DATELASTMAINTENANCE"] = "0";
            string skrg = DateTime.Now.ToString("ddMMyy");

            dMbaseMessage.Rows[0]["DATEPLACED"] = skrg;
            dMbaseMessage.Rows[0]["HOLDBYBRANCH"] = "0374";                                          // uker sesuai No Rekening yg di Hold
            dMbaseMessage.Rows[0]["USERID"] = "0374891";                                            // user Teller BRINTERFACE
            dMbaseMessage.Rows[0]["WORKSTATIONID"] = "BRI";
            dMbaseMessage.Rows[0]["TIMECHANGEDMADE"] = DateTime.Now.ToString("HHmmss");
            dMbaseMessage.Rows[0]["REASONCODE"] = reasonCode;

            requestMBASE.applicationname = "PAYROLLCMS";
            requestMBASE.branchcode = "0374";                                               // uker sesuai No Rekening yg di Hold
            requestMBASE.tellerid = "0374891";                                              // user Teller BRINTERFACE
            requestMBASE.supervisorid = "";
            requestMBASE.origjournalseq = ""; //"2601";
            requestMBASE.dSocketHeader = dSocketHeader;
            requestMBASE.dMiddleWareHeader = dMiddleWareHeader;
            requestMBASE.dMbaseHeader = dMbaseHeader;
            requestMBASE.dMbaseMessage = dMbaseMessage;

            responseMBASE = transactionMBASE.doMBASETransaction(requestMBASE);

            if (responseMBASE != null)
            {
                result = int.Parse(responseMBASE.statuscode.Trim());
                if (result == 1)
                    jSeq = int.Parse(responseMBASE.msgmbmessage[2]);
            }
            else
                result = 0;

        }
        catch (Exception ex)
        {
            result = 0;
        }
        return result;
    }

    public static int UnHoldAmount(string noRekHold, string jourSeqHold, out string msg)
    {
        int result = 0;
        msg = "";
        try
        {
            briInterfaceService transactionMBASE = new briInterfaceService();
            MessageMBASETransactionRequest requestMBASE = new MessageMBASETransactionRequest();
            MessageMBASETransactionResponse responseMBASE = new MessageMBASETransactionResponse();

            DataTable dSocketHeader = new DataTable("SOCKETHEADER");
            DataTable dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
            DataTable dMbaseHeader = new DataTable("MBASEHEADER");
            DataTable dMbaseMessage = new DataTable("MBASEMESSAGE");

            string trancode = "2904";

            String rekDebet = noRekHold.PadLeft(15, '0');
            String branchRek = "0374";// rekDebet.Substring(0, 4);
            String tellerID = "0374891";
            String accountTypeDebet = "";

            if (rekDebet.Substring(12, 1).Equals("5"))
                accountTypeDebet = "S";
            else if (rekDebet.Substring(12, 1).Equals("3"))
                accountTypeDebet = "D";
            else if (rekDebet.Substring(12, 1).Equals("9"))
                accountTypeDebet = "D";
            else if (rekDebet.Substring(12, 1).Equals("1"))
                accountTypeDebet = "L";
            else
                accountTypeDebet = "S";

            try
            {
                transactionMBASE.initiateMBASE(trancode, ref dSocketHeader, ref dMiddleWareHeader, ref dMbaseHeader, ref dMbaseMessage);
            }
            catch (Exception ex)
            {
                return 99;
            }

            dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;
            dMbaseHeader.Rows[0]["TRANSACTIONCODE"] = trancode;
            dMbaseHeader.Rows[0]["ACTIONCODE"] = "D";
            dMbaseHeader.Rows[0]["NOOFRECORD"] = "1";
            dMbaseHeader.Rows[0]["BANKNUMBER"] = "2";
            dMbaseHeader.Rows[0]["BRANCHNUMBER"] = branchRek;

            dMbaseMessage.Rows[0]["ACCOUNTNUMBER"] = rekDebet;
            dMbaseMessage.Rows[0]["ACCOUNTTYPE"] = accountTypeDebet;
            dMbaseMessage.Rows[0]["SEQUENCE"] = jourSeqHold;

            requestMBASE.applicationname = "CMS";
            requestMBASE.branchcode = branchRek;
            requestMBASE.tellerid = tellerID;
            requestMBASE.supervisorid = "";
            requestMBASE.origjournalseq = ""; //"2601";
            requestMBASE.dSocketHeader = dSocketHeader;
            requestMBASE.dMiddleWareHeader = dMiddleWareHeader;
            requestMBASE.dMbaseHeader = dMbaseHeader;
            requestMBASE.dMbaseMessage = dMbaseMessage;

            responseMBASE = transactionMBASE.doMBASETransaction(requestMBASE);

            if (responseMBASE != null)
            {
                result = int.Parse(responseMBASE.statuscode.Trim());
                if (result == 2)
                    msg = responseMBASE.msgmbheader[22].Trim();
            }
            else
                result = 0;
        }
        catch (Exception ex)
        {
            result = 0;
        }
        return result;
    }

    /*
         * Fungsi get fee All of my life 
         * Denny Indra L - 11 April 2017
         */
    public static String getFeeTransaction(ISession session, string idPayrol, string instructionCode, ILog log)
    {
        string count = "0";
        string jumlah = "0";
        string summary = "0";
        string msg = "";
        string dbResultl = "0";
        string dbResult2 = "0";
        string countAmount = "0:0";
        try
        {

            //Select Payrolldetail untuk get COUNT transaksi
            string query = @"select count(id) from trxpayrolldetails where pid = '" + idPayrol + "' and instructioncode = '" + instructionCode + "' and status in (" + ParameterHelper.TRXSTATUS_MAKER + ", " + ParameterHelper.TRXSTATUS_VERIFY + ");";

            if (!PayrollHelper.ExecuteQueryValue(session, query, out dbResultl, out msg))
            {
                log.Info("TrxPayrol.ID : " + idPayrol + "Get Count Fee " + instructionCode + " in TrxPayrollDetails gagal");
                dbResultl = "0";
                count = "failed";
            }
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Count Fee " + instructionCode + " in TrxPayrollDetails sukses, Count : " + dbResultl.ToString());
            count = dbResultl.ToString();
            if (string.IsNullOrEmpty(dbResultl))
            {
                dbResultl = "0";
            }

            //Select Payrolldetail untuk get SUM transaksinya
            string query2 = @"select sum(amount) from trxpayrolldetails where pid = '" + idPayrol + "' and instructioncode = '" + instructionCode + "' and status in (" + ParameterHelper.TRXSTATUS_MAKER + ", " + ParameterHelper.TRXSTATUS_VERIFY + ");";

            if (!PayrollHelper.ExecuteQueryValue(session, query2, out dbResult2, out msg))
            {
                log.Info("TrxPayrol.ID : " + idPayrol + "Get Summary Amount " + instructionCode + " in TrxPayrollDetails gagal");
                dbResult2 = "0";
                summary = "failed";
            }
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Summary Amount " + instructionCode + " in TrxPayrollDetails sukses, Count : " + dbResult2.ToString());


            if (string.IsNullOrEmpty(dbResult2))
            {
                dbResult2 = "0";
            }
            summary = (int.Parse(dbResult2) / 100).ToString();

            /*Saatnya makek*/


            //count siap#sum amount
            countAmount = count + ":" + summary;
        }
        catch (Exception ec)
        {
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Fee " + instructionCode + " Exception");
            countAmount = "failed:failed";
        }
        return countAmount;

    }


    /*
     * Fungsi get count rejext
     * Denny Indra L - 11 April 2017
     */
    public static String getCountRejectTransaction(ISession session, string idPayrol, string instructionCode, ILog log)
    {
        string count = "0";
        string jumlah = "0";
        string msg = "";
        string dbResult1 = "0";
        string dbResult2 = "0";
        string countAmount = "0:0";
        try
        {



            //Select Payrolldetail untuk get jumlah transaksi
            string query = @"select count(id) from trxpayrolldetails where pid = '" + idPayrol + "' and instructioncode = '" + instructionCode + "' and status = " + ParameterHelper.TRXSTATUS_REJECT;

            if (!PayrollHelper.ExecuteQueryValue(session, query, out dbResult1, out msg))
            {
                log.Info("TrxPayrol.ID : " + idPayrol + "Get Count Reject " + instructionCode + " in TrxPayrollDetails gagal");
                dbResult1 = "";
                countAmount = "failed";
            }
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Count Reject " + instructionCode + " in TrxPayrollDetails sukses, Count : " + dbResult2.ToString());

            if (string.IsNullOrEmpty(dbResult1))
            {
                dbResult1 = "0";
            }
            count = dbResult1.ToString();

            //Select Payrolldetail untuk get SUM transaksinya
            string query2 = @"select sum(amount) from trxpayrolldetails where pid = '" + idPayrol + "' and instructioncode = '" + instructionCode + "' and status = " + ParameterHelper.TRXSTATUS_REJECT;

            if (!PayrollHelper.ExecuteQueryValue(session, query2, out dbResult2, out msg))
            {
                log.Info("TrxPayrol.ID : " + idPayrol + "Get Summary Amount Reject " + instructionCode + " in TrxPayrollDetails gagal");
                dbResult2 = "0";
                countAmount = "failed";
            }
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Summary Amount Reject" + instructionCode + " in TrxPayrollDetails sukses, Count : " + dbResult2.ToString());
            if (string.IsNullOrEmpty(dbResult2))
            {
                dbResult2 = "0";
            }
            jumlah = (int.Parse(dbResult2) / 100).ToString();
            countAmount = count + ":" + jumlah;
        }
        catch (Exception ec)
        {
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Reject " + instructionCode + " Exception");
            dbResult2 = "0";
            countAmount = "failed:failed";
        }
        return countAmount;

    }


    /*
     * Fungsi get Total All of my life 
     * Denny Indra L - 11 April 2017
     */
    public static String getTotalTransaction(ISession session, string idPayrol, ILog log)
    {
        string count = "0";
        string jumlah = "0";
        string summary = "0";
        string msg = "";
        string dbResult1 = "0";
        string dbResult2 = "0";
        string countAmount = "0:0";
        try
        {
            //Select Payrolldetail untuk get COUNT transaksi
            string query = @"select count(id) from trxpayrolldetails where pid = '" + idPayrol + "' and status in (" + ParameterHelper.TRXSTATUS_MAKER + ", " + ParameterHelper.TRXSTATUS_VERIFY + ");";

            if (!PayrollHelper.ExecuteQueryValue(session, query, out dbResult1, out msg))
            {
                log.Info("TrxPayrol.ID : " + idPayrol + "Get Count Total in TrxPayrollDetails gagal");
                dbResult1 = "";
                count = "failed";
            }
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Count Total in TrxPayrollDetails sukses, Count : " + dbResult1.ToString());
            if (string.IsNullOrEmpty(dbResult1))
            {
                dbResult1 = "0";
            }
            count = dbResult1.ToString();


            //Select Payrolldetail untuk get SUM transaksinya
            string query2 = @"select sum(amount) from trxpayrolldetails where pid = '" + idPayrol + "' and status in (" + ParameterHelper.TRXSTATUS_MAKER + ", " + ParameterHelper.TRXSTATUS_VERIFY + ");";

            if (!PayrollHelper.ExecuteQueryValue(session, query2, out dbResult2, out msg))
            {
                log.Info("TrxPayrol.ID : " + idPayrol + "Get Summary Amount Total in TrxPayrollDetails gagal");
                dbResult2 = "";
                summary = "failed";
            }
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Summary Amount Total in TrxPayrollDetails sukses, Count : " + dbResult2.ToString());
            if (string.IsNullOrEmpty(dbResult2))
            {
                dbResult2 = "0";
            }
            summary = (int.Parse(dbResult2) / 100).ToString();

            /*Saatnya makek*/


            //count siap#sum amount
            countAmount = count + ":" + summary;
        }
        catch (Exception ec)
        {
            log.Info("TrxPayrol.ID : " + idPayrol + "Get Total Exception");
            dbResult1 = "0";
            countAmount = "failed:failed";
        }
        return countAmount;

    }


    public static double GetAmountDetailPayroll(ISession session, TrxPayroll o, out double totAmtIFT, out double totFeeIFT, out double totAmtRTG, out double totFeeRTG, out double totAmtLLG, out double totFeeLLG, out double totAmtBRIS, out double totFeeBRIS)
    {
        totAmtIFT = 0; 
        totFeeIFT = 0; 
        totAmtRTG = 0; 
        totFeeRTG = 0; 
        totAmtLLG = 0; 
        totFeeLLG = 0;
        double result = 0;

        //BRIS - 2020-06-16
        totAmtBRIS = 0;
        totFeeBRIS = 0;

        try
        {
            IList<TrxPayrollDetail> pys = o.TrxPayrollDetail;

            //IFT
            totAmtIFT = pys.Where(x => x.InstructionCode == TotalOBHelper.IFT && x.Status != ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount);
            if (totAmtIFT > 0)
                totAmtIFT = totAmtIFT / 100;

            totFeeIFT = pys.Where(x => x.InstructionCode == TotalOBHelper.IFT && x.Status != ParameterHelper.TRXSTATUS_REJECT).Count() * o.Fee;
            
            //rtgs
            totAmtRTG = pys.Where(x => x.InstructionCode == TotalOBHelper.RTGS && x.Status != ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount);
            if (totAmtRTG > 0)
                totAmtRTG = totAmtRTG / 100;
            totFeeRTG = pys.Where(x => x.InstructionCode == TotalOBHelper.RTGS && x.Status != ParameterHelper.TRXSTATUS_REJECT).Count() * o.FeeRTG;


            //LLG
            totAmtLLG = pys.Where(x => x.InstructionCode == TotalOBHelper.LLG && x.Status != ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount);
            if (totAmtLLG > 0)
                totAmtLLG = totAmtLLG / 100;
            totFeeLLG = pys.Where(x => x.InstructionCode == TotalOBHelper.LLG && x.Status != ParameterHelper.TRXSTATUS_REJECT).Count() * o.FeeLLG;

            //tambahan untuk BRIS
            totAmtBRIS = pys.Where(x => x.InstructionCode == TotalOBHelper.BRIS && x.Status != ParameterHelper.TRXSTATUS_REJECT).Sum(x => x.Amount);
            if (totAmtBRIS > 0)
                totAmtBRIS = totAmtBRIS / 100;

            totFeeBRIS = pys.Where(x => x.InstructionCode == TotalOBHelper.BRIS && x.Status != ParameterHelper.TRXSTATUS_REJECT).Count() * o.FeeBRIS;
            //end tambahan untuk BRIS

            result = totAmtIFT + totAmtLLG + totAmtRTG + totFeeIFT + totFeeLLG + totFeeRTG + totAmtBRIS + totFeeBRIS;
        }
        catch (Exception ex) { result = -99; }
        return result;
    }

    public static bool UpdateDataChildIFTToBeRejected(ISession session, TrxPayroll o, out string message)
    {
        bool result = true;
        string theQuery = "";
        message = "";
        try
        {
            string skrg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            String[] desc = o.Description.Split(new String[] { "||" }, StringSplitOptions.None);

            theQuery = @"update trxpayrolldetails
                set STATUS = " + ParameterHelper.TRXSTATUS_REJECT + ", DESCRIPTION = '" + desc[0] + "', LASTUPDATE = '" + skrg + "' where PID = '" + o.Id + "' and INSTRUCTIONCODE = '" + TotalOBHelper.IFT + "';";

            result = ExecuteQuery(session, theQuery, out message);
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }

    /*
     * Update ready to book / pecah kongsi
     * - Parent set status 22
     * - Parent statusIFt jika ada iftnya set status 1
     * + Detail -> CN/RTG set Detail -> 14
     * + Parent -> BRIS -> Statusbris -> 1
     */

    public static bool UpdateDataToBeReadyToBook(ISession session, TrxPayroll o, out string message)
    {
        bool result = true;
        string theQuery = "";
        message = "";
        try
        {
            string skrg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int totCntIFT = o.TrxPayrollDetail.Where(x => x.InstructionCode == TotalOBHelper.IFT || String.IsNullOrEmpty(x.InstructionCode)).Count();
            int totCntBRIS = o.TrxPayrollDetail.Where(x => x.InstructionCode == TotalOBHelper.BRIS).Count();
//            

            theQuery = @"update trxpayrolls p inner join trxpayrolldetails d on p.ID = d.PID set p.STATUS = " + ParameterHelper.TRXSTATUS_PAYROLLNEW_READY_BOOK //22
                + ", p.LASTUPDATE = '" + skrg + "', "
                + " p.STATUSIFT = CASE WHEN " + (totCntIFT > 0) + " THEN " + ParameterHelper.TRXSTATUS_PAYROLLNEW_READY_BOOK_IFT + ", "//1 
                + " p.STATUSBRIS = CASE WHEN " + (totCntBRIS > 0) + " THEN " + ParameterHelper.TRXSTATUS_PAYROLLNEW_WAITING_CREATEFILE_BRIS //2
                + " else p.STATUSIFT END," + " d.LASTUPDATE = '" + skrg + "', d.STATUS =  " 
                + " CASE " 
                + " WHEN d.INSTRUCTIONCODE = '" + TotalOBHelper.LLG + "' THEN " + ParameterHelper.TRXSTATUS_PAYROLLNEW_WAITINGRESPONSE_REMITNUMB//14
                + " WHEN d.INSTRUCTIONCODE = '" + TotalOBHelper.RTGS + "' THEN " + ParameterHelper.TRXSTATUS_PAYROLLNEW_WAITINGRESPONSE_REMITNUMB//14
                + " else d.STATUS END" 
                + " where p.ID = '" + o.Id + "' and d.STATUS <> " + ParameterHelper.TRXSTATUS_REJECT + ";";

            result = ExecuteQuery(session, theQuery, out message);
        }
        catch (Exception ex)
        {
            result = false;
        }

        return result;
    }

    public static Boolean ExecuteQuery(ISession session, string query, out string message)
    {
        Boolean result = false;
        message = "";
        MySqlConnection conn = new MySqlConnection();
        try
        {
            //open connection
            String DBConfig = ParameterHelper.GetString("CMS_DBCONNECTION", session);
            conn = new MySqlConnection(DBConfig);
            conn.Open();

            //create command
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = query;
            command.CommandTimeout = 6000;//dalam second

            int i = command.ExecuteNonQuery();

            result = true;
            message = "SUKSES";

            //tutup koneksi
            conn.Close();
        }
        catch (Exception e)
        {
            if (e.InnerException == null)
                message = "Message: " + e.Message + "|| StackTrace: " + e.StackTrace;
            else message = "Message: " + e.Message + "|| InnerEx: " + e.InnerException + "|| StackTrace: " + e.StackTrace;
            result = false;
        }
        finally
        {
            conn.Close();
        }

        return result;
    }

    public static bool ExecuteQueryValue(ISession session, string query, out string dbResult, out string message)
    {
        Boolean result = false;
        dbResult = "";
        message = "";
        MySqlConnection conn = new MySqlConnection();
        try
        {
            //open connection
            //BRIChannel.Pocos.Parameter dbConf = session.Load<Parameter>("CMS_DBCONNECTION");
            String DBConfig = ParameterHelper.GetString("CMS_DBCONNECTION", session);
            conn = new MySqlConnection(DBConfig);
            conn.Open();

            //create command
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = query;
            command.CommandTimeout = 6000;//dalam second

            //ekse query
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                    dbResult = reader.GetString(0);
            }
            reader.Close();
            message = "SUKSES";
            result = true;

        }
        catch (Exception e)
        {
            message = e.Message + "||" + e.InnerException + "||" + e.StackTrace;
            result = false;
        }
        finally
        {
            conn.Close();
        }

        return result;
    }

    public static int InquiryHoldAmt(String NoRek, out double available, out double hold, out double cBal, out int statusrek, out string namaRek, out string msgErr)
    {
        int result = 0; available = 0; hold = 0;
        cBal = 0;
        statusrek = 0;
        namaRek = "";
        msgErr = "";

        try
        {
            String curr = SchHelper._Currency(NoRek);
            String accType = SchHelper._AccountType(NoRek);
            String trancode = "1000";

            if (accType.Equals("S"))
            {
                trancode = "2000";
            }
            else if (accType.Equals("L"))
            {
                trancode = "4077";
            }

            MessageABCSTransactionRequest request = new MessageABCSTransactionRequest();
            MessageABCSTransactionResponse response = new MessageABCSTransactionResponse();

            DataTable dSocketHeader = new DataTable("SOCKETHEADER");
            DataTable dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
            DataTable dAbcsHeader = new DataTable("ABCSHEADER");
            DataTable dAbcsMessage = new DataTable("ABCSMESSAGE");

            briInterfaceService transaction = new briInterfaceService();
            try
            {
                transaction.initiateABCS(ref dSocketHeader, ref dMiddleWareHeader, ref dAbcsHeader, ref dAbcsMessage);
            }
            catch (Exception a)
            {
                return 99;
            }
            dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;
            dAbcsMessage.Rows[0]["TRANSACTIONCODE"] = trancode;
            dAbcsMessage.Rows[0]["BUCKET1"] = NoRek;
            dAbcsMessage.Rows[0]["BUCKET7"] = "";
            dAbcsMessage.Rows[0]["BUCKET8"] = "";
            dAbcsMessage.Rows[0]["BUCKET19"] = DateTime.Now.ToString("ddMMyy");  //effective date
            dAbcsMessage.Rows[0]["BUCKET20"] = ""; //orig jseq for reverse
            dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";
            dAbcsMessage.Rows[0]["TELLERREMARK1"] = "Inquiry Account";
            dAbcsMessage.Rows[0]["TELLERREMARK2"] = "CMS";
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY1"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY2"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY3"] = "IDR";

            request.branchcode = "0374";
            request.tellerid = "0374891";
            request.supervisorid = "";
            request.origjournalseq = "";
            request.dSocketHeader = dSocketHeader;
            request.dMiddleWareHeader = dMiddleWareHeader;
            request.dAbcsHeader = dAbcsHeader;
            request.dAbcsMessage = dAbcsMessage;
            request.applicationname = "CMS";

            response = transaction.doABCSTransaction(request);
            if (response != null)
            {
                if (int.Parse(response.statuscode.Trim()) == 1)
                {
                    namaRek = response.msgabmessage[0][7].ToString();
                    if (accType.Equals("L"))
                    {
                        available = double.Parse(response.msgabmessage[0][21].ToString().Trim()) - double.Parse(response.msgabmessage[0][22].ToString());


                        if (available < 0)
                            available = available * (-1);
                        statusrek = 1;

                        String info = response.msgabmessage[0][63].ToString();
                        if (!info.Equals("P")) statusrek = 99;

                        result = 1;
                    }
                    else
                    {
                        available = double.Parse(response.msgabmessage[0][66].ToString());
                        hold = double.Parse(response.msgabmessage[0][21].ToString());
                        cBal = double.Parse(response.msgabmessage[0][23].ToString());
                        statusrek = int.Parse(response.msgabmessage[0][8].ToString()); //bisa dipake buat transfer cuman 1 atau 4, selain itu nga bisa
                        result = 1;
                    }
                }
                else
                    result = int.Parse(response.statuscode.Trim());

                if (result == 2)
                    msgErr = response.msgabmessage[0][4].Trim();

            }
            else
                result = 0;


        }
        catch (Exception ex)
        {
            result = 0;
        }

        return result;
    }

    public static int inqHoldNew(string noRekHold, string jourSeqHold, out int seq, out string remark, out double amtHold, out string statusDel, out string expDate, out string rCode, out string type, out string msgErr)
    {
        int result = 0;
        seq = 0; remark = ""; amtHold = 0; statusDel = ""; expDate = ""; rCode = ""; type = ""; msgErr = "";
        try
        {
            MessageMBASETransactionResponse responseMBASE = new MessageMBASETransactionResponse();
            briInterfaceService transactionMBASE = new briInterfaceService();
            MessageMBASETransactionRequest requestMBASE = new MessageMBASETransactionRequest();

            DataTable dSocketHeader = new DataTable("SOCKETHEADER");
            DataTable dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
            DataTable dMbaseHeader = new DataTable("MBASEHEADER");
            DataTable dMbaseMessage = new DataTable("MBASEMESSAGE");

            string trancode = "2604";                        // trancode untuk Buka Blokir Hold Amount = 2904              

            String rekDebet = noRekHold.PadLeft(15, '0');
            String branchRek = "0374";//rekDebet.Substring(0, 4);
            String tellerID = "0374891";
            String accountTypeDebet = "";

            if (rekDebet.Substring(12, 1).Equals("5"))
                accountTypeDebet = "S";
            else if (rekDebet.Substring(12, 1).Equals("3"))
                accountTypeDebet = "D";
            else if (rekDebet.Substring(12, 1).Equals("9"))
                accountTypeDebet = "D";
            else if (rekDebet.Substring(12, 1).Equals("1"))
                accountTypeDebet = "L";
            else
                accountTypeDebet = "S";

            try
            {
                transactionMBASE.initiateMBASE(trancode, ref dSocketHeader, ref dMiddleWareHeader, ref dMbaseHeader, ref dMbaseMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Message : " + ex.Message);
                Console.WriteLine("InnerEx : " + ex.InnerException);
                Console.WriteLine("StackTr : " + ex.StackTrace);
                //Thread.Sleep(20000000);
                Thread.Sleep(1800000);
                return 99;
            }

            dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;
            dMbaseHeader.Rows[0]["TRANSACTIONCODE"] = trancode;
            dMbaseHeader.Rows[0]["ACTIONCODE"] = "D";
            dMbaseHeader.Rows[0]["NOOFRECORD"] = "1";
            dMbaseHeader.Rows[0]["BANKNUMBER"] = "2";
            dMbaseHeader.Rows[0]["BRANCHNUMBER"] = branchRek;

            dMbaseMessage.Rows[0]["ACCTNO"] = rekDebet;
            dMbaseMessage.Rows[0]["ACTYPE"] = accountTypeDebet;
            dMbaseMessage.Rows[0]["SEQ3"] = jourSeqHold;

            requestMBASE.applicationname = "CMS";
            requestMBASE.branchcode = branchRek;
            requestMBASE.tellerid = tellerID;
            requestMBASE.supervisorid = "";
            requestMBASE.origjournalseq = "";
            requestMBASE.dSocketHeader = dSocketHeader;
            requestMBASE.dMiddleWareHeader = dMiddleWareHeader;
            requestMBASE.dMbaseHeader = dMbaseHeader;
            requestMBASE.dMbaseMessage = dMbaseMessage;

            responseMBASE = transactionMBASE.doMBASETransaction(requestMBASE);

            result = int.Parse(responseMBASE.statuscode.Trim());

            if (result == 1)
            {
                seq = int.Parse(responseMBASE.msgmbmessage[2].Trim());
                statusDel = responseMBASE.msgmbmessage[3];
                type = responseMBASE.msgmbmessage[4];
                double amount = Double.Parse(responseMBASE.msgmbmessage[5]);
                amtHold = amount / 100;
                remark = responseMBASE.msgmbmessage[10].Trim();
                expDate = responseMBASE.msgmbmessage[12];
                rCode = responseMBASE.msgmbmessage[20];
            }
            else if (result == 2)
                msgErr = responseMBASE.msgmbheader[22]; //MBFDD00801 : No records found. 

        }
        catch (Exception ex)
        {
            result = 0;
            Console.WriteLine("Message : " + ex.Message);
            Console.WriteLine("InnerEx : " + ex.InnerException);
            Console.WriteLine("StackTr : " + ex.StackTrace);
            //Thread.Sleep(20000000);
            Thread.Sleep(1800000);
        }
        return result;
    }

    public static void ProcessPayrollConsolidation(ISession session, TrxPayroll o, ILog log, string _schCode)
    {
        /*
         * cek status statusift
         * cek status payrolldetail yg LLG, RTGS, sudah sukses apa belum
         * klo status IFT uda komplit, dan semua rtg LLG komplit, update status jadi sakses untuk kirim imel
         * klo belon, lastupdate + 30 sec
         */

        /* Tambahan eksa 20-01-2020
         * Tambahan cek status BRIS udah kelar apa belom
         * kalo dah kelar jadiin sukses terus kirim imel
         * kalo belom tambahin last update sec buat dicek lg
         */ 
        string msg = "";
        int result = 0;
        if (o.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_READY_BOOK)//22
        {
            #region DUNIA PERSILATAN CHECK PEMBUKUAN SUDAH KOMPLIT SEMUA KAH?
            if (o.IsPayrollBankLain == 0)
            {
                #region pureblood IFT
                if (o.StatusIFT == ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_COMPLETE || o.StatusIFT == ParameterHelper.TRXSTATUS_REJECT)
                {
                    o.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                    o.Status = ParameterHelper.TRXSTATUS_COMPLETE;

                    if (o.StatusIFT == ParameterHelper.TRXSTATUS_REJECT)
                        o.Status = ParameterHelper.TRXSTATUS_REJECT;

                    o.AmountUsed = 0;
                    o.LastUpdate = DateTime.Now;
                    session.Update(o);
                    session.Flush();
                    log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") IsPayrollBankLain = 0, Complete Trx");
                }

                else if (o.StatusIFT == ParameterHelper.PAYROLL_EXCEPTION)//888
                {
                    //handle exception ketika buku, lgsung reject - 2018-08-08 
                    o.Status = ParameterHelper.TRXSTATUS_REJECT;
                    o.AmountUsed = 0;
                    o.LastUpdate = DateTime.Now;
                    session.Update(o);
                    session.Flush();
                    log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") IsPayrollBankLain = 0, Ada kegagalan buku di Host");
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + _schCode + " === (SeqNo " + o.SeqNumber + ") Not complete batch, re-check");
                }
                #endregion

            }
            else
            {
                int totCntIFT = 0; 
                int totCntRTG = 0; 
                int totCntLLG = 0; 
                int totCntIFTSukses = 0; 
                int totCntRTGSukses = 0; 
                int totCntLLGSukses = 0;

                int totCntBRIS = 0; 
                int totCntBRISSukses = 0; 

                double total = GetPayrollDetailSukses(o, out totCntIFT, out totCntRTG, out totCntLLG, out totCntIFTSukses, out totCntRTGSukses, out totCntLLGSukses, out totCntBRIS, out totCntBRISSukses);
                if (total != -99)
                {
                    if (totCntIFT == totCntIFTSukses && totCntRTG == totCntRTGSukses && totCntLLG == totCntLLGSukses && totCntBRIS == totCntBRISSukses)
                    {
                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") All transaction complete, Proceeed to book IA Kredit");
                        o.AmountUsed = total;
                        o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_IAKREDIT;
                        if (o.IsPayrollBankLain == 0)
                        {
                            //pureblood IFT, ga pakek IA perantara, langsung sakses
                            o.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                            o.Status = ParameterHelper.TRXSTATUS_COMPLETE;
                            o.ErrorDescription += "||" + o.Description;
                            if (o.StatusIFT != ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_COMPLETE)
                            {
                                o.Description = ParameterHelper.TRXDESCRIPTION_COMPLETE;
                                o.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_COMPLETE;
                            }
                            o.AmountUsed = 0;
                            log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") IsPayrollBankLain = 0, Complete Trx");
                        }
                        else if (total == o.AmountFromIADebet)
                        {
                            o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_REK_NASABAH;//25
                            log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") AmountFromIADebet: " + o.AmountFromIADebet + " and AmountUsed: " + o.AmountUsed + " Balance, Proceeed to book Rek Nasabah");
                        }

                        o.LastUpdate = DateTime.Now;
                    }
                    else if (o.StatusIFT == ParameterHelper.TRXSTATUS_REJECT && totCntIFTSukses == 0 && totCntIFT > 0)
                    {
                        if (UpdateDataChildIFTToBeRejected(session, o, out msg))
                            log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") StatusIFT = 4, Updating Child Success");
                        else
                            log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") StatusIFT = 4, Updating Child Failed");
                    }
                    else
                    {
                        //o.LastUpdate = DateTime.Now.AddSeconds(30);
                        Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + _schCode + " === (SeqNo " + o.SeqNumber + ") Not complete batch, re-check in 30 sec");

                    }
                    session.Update(o);
                    session.Flush();

                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + _schCode + " === (SeqNo " + o.SeqNumber + ") TOTAL -99");
                }
            }
            #endregion
        }

        if (o.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_IAKREDIT)//23
        {
            #region DUNIA PERSILATAN POST TO BOOKING DEBET IA KREDIT
            int idbooking = 0;
            int fid = 100;
            int clientId = o.ClientID;
            string debitAcc = ParameterHelper.GetString("PAYROLL_IA_KREDIT", session);
            string debitAccName = "PAYROLL_IA_KREDIT";
            string creditAcc = ParameterHelper.GetString("PAYROLL_IA_DEBET", session);
            string creditAccName = "PAYROLL_IA_DEBET";
            string fitur = TotalOBHelper.Payroll;
            string _remark = "KELEBIHAN PAYROLL SEQ " + o.SeqNumber.ToString();
            msg = "";
            double theAmt = o.AmountFromIADebet - o.AmountUsed;

            if (theAmt == 0)
            {
                o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_REK_NASABAH;
                o.LastUpdate = DateTime.Now;
                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") AmountFromIADebet: " + o.AmountFromIADebet + " and AmountUsed: " + o.AmountUsed + " Balance, Proceeed to book Rek Nasabah");
            }
            else if (theAmt < 0)
            {
                o.Status = ParameterHelper.PAYROLL_EXCEPTION;
                o.LastUpdate = DateTime.Now;
                o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + ", Defisit Amount" + "||" + o.Description;
                log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") AmountFromIADebet: " + o.AmountFromIADebet + " and AmountUsed: " + o.AmountUsed + " DEFISIT, pending transaction!");

            }
            else
            {
                if (BookingHelper.InsertBooking(session, o.SeqNumber.ToString(), fid, fitur, clientId, debitAcc, debitAccName, theAmt,
                            "IDR", creditAcc, creditAccName, theAmt, "IDR", theAmt, "IDR", "", _remark,
                            DateTime.Now, ParameterHelper.BOOKTYPE_IAKREDIT, "", out idbooking, out msg))
                {
                    log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Insert to Booking Amount SUCCESS");
                    o.IdBooking2 = idbooking;
                    o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IAKREDIT;
                    o.LastUpdate = DateTime.Now;
                }
                else
                {
                    if (idbooking != 0)
                    {
                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Duplicate Booking Amount Detected");
                        o.IdBooking2 = idbooking;
                        o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IAKREDIT;
                        o.LastUpdate = DateTime.Now;

                    }
                    else
                    {
                        log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Insert to Booking Amount Failed ::: " + msg);
                        o.LastUpdate = DateTime.Now.AddMinutes(2);
                    }
                }
            }
            session.Update(o);
            session.Flush();
            #endregion
        }

        if (o.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IAKREDIT)
        {
            #region DUNIA PERSILATAN CHECK BOOKING DEBET IA KREDIT
            Booking bookAmt = session.Load<Booking>(o.IdBooking2);
            if (bookAmt.Status == ParameterHelper.SCH_BOOKSUCCESS)
            {
                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount IA KREDIT SUCCESS");

                o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_REK_NASABAH;
                o.LastUpdate = DateTime.Now;
            }
            else if (bookAmt.Status == ParameterHelper.SCH_BOOKFAILED)
            {
                #region kalo gagal cek udah lewat max retry apa belom
                Parameter maxRetry = session.Load<Parameter>("MAX_RETRY_BOOK_PAYROLL");
                int rertyBook = int.Parse(maxRetry.Data);
                if (o.RetryPosition <= rertyBook)
                {
                    log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount IA KREDIT FAILED: " + bookAmt.Description + " === UnHolding Amount");
                    o.Status = ParameterHelper.PAYROLL_EXCEPTION;
                    o.LastUpdate = DateTime.Now;
                    o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + ",  Booking Amount IAC Failed" + "||" + o.Description;
                }
                #endregion
                #region kalo udah lewat batas max retry
                else
                {
                    log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount IA KREDIT FAILED: " + bookAmt.Description + " === UnHolding Amount");
                    o.Status = ParameterHelper.PAYROLL_EXCEPTION;
                    o.LastUpdate = DateTime.Now;
                    o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + ",  Booking Amount IAC Failed" + "||" + o.Description;
                }
                #endregion
            }
            session.Update(o);
            session.Flush();
            #endregion
        }

        if (o.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_BOOK_REK_NASABAH)//25
        {
            #region DUNIA PERSILATAN POST TO BOOKING DEBET REK NASABAH KE IA DEBET

            #region unhold
            result = 0;
            result = UnHoldAmount(o.DebitAccount, o.JSeqHold, out msg);

            if (result == 1 && o.Status.Equals(0))
            {
                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") UnHold Amount SUCCESS");
                #region post to book
                int idbooking = 0;
                int fid = 100;
                int clientId = o.ClientID;
                string debitAcc = o.DebitAccount;
                IList<ClientAccount> cas = session.CreateCriteria(typeof(ClientAccount))
                    .Add(Expression.Eq("Number", o.DebitAccount))
                    .Add(Expression.Eq("Pid", o.ClientID))
                    .Add(Expression.Eq("StatusDel", 0))
                    .List<ClientAccount>();
                string debitAccName = cas[0].Name;
                string creditAcc = ParameterHelper.GetString("PAYROLL_IA_DEBET", session);
                string creditAccName = "PAYROLL_IA_DEBET";
                string fitur = TotalOBHelper.Payroll;
                string _remark = o.FileDescription.Trim();//"CMS PAYROLL SEQ " + o.SeqNumber.ToString();
                msg = "";
                double theAmt = o.AmountUsed;


                if (BookingHelper.InsertBooking(session, o.SeqNumber.ToString(), fid, fitur, clientId, debitAcc, debitAccName, theAmt,
                            "IDR", creditAcc, creditAccName, theAmt, "IDR", theAmt, "IDR", "", _remark,
                            DateTime.Now, ParameterHelper.BOOKTYPE_AMOUNT, "", out idbooking, out msg))
                {
                    #region khususon non BRIS
                    if (o.StatusBRIS.Equals(0))
                    {
                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Insert to Booking Amount SUCCESS");
                        o.IdBooking3 = idbooking;
                        o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_REK_NASABAH;
                        o.LastUpdate = DateTime.Now;
                    }
                    else if (o.StatusBRIS.Equals(5))
                    {
                        log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Insert to Booking Amount SUCCESS");
                        o.IdBooking4 = idbooking;
                        o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_REK_NASABAH;
                        o.LastUpdate = DateTime.Now;
                    }
                    #endregion khususon non BRIS - wrwrwrrw
                }
                else
                {
                    if (idbooking != 0)
                    {
                        if (o.StatusBRIS.Equals(0))
                        {
                            log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Duplicate Booking Amount Detected");
                            o.IdBooking3 = idbooking;
                            o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_REK_NASABAH;
                            o.LastUpdate = DateTime.Now;
                        }
                        else if (o.StatusBRIS.Equals(5))
                        {
                            log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Duplicate Booking Amount Detected");
                            o.IdBooking4 = idbooking;
                            o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_REK_NASABAH;
                            o.LastUpdate = DateTime.Now;
                        }
                    }
                    else
                    {
                        log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Insert to Booking Amount Failed ::: " + msg);
                        o.LastUpdate = DateTime.Now.AddMinutes(2);
                    }
                }

                session.Update(o);
                session.Flush();
                #endregion

            }
            else if (result == 2)
            {
                log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") UnHold Amount FAILED: " + msg + ", retry in 2 min");
                o.LastUpdate = DateTime.Now.AddMinutes(2);
            }
            session.Update(o);
            session.Flush();
            #endregion

            #endregion
        }

        if (o.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_REK_NASABAH && o.StatusBRIS.Equals(0))
        {
            #region DUNIA PERSILATAN CHECK BOOKING DEBET REK NASABAH

            Booking bookAmt = session.Load<Booking>(o.IdBooking3);
            if (bookAmt.Status == ParameterHelper.SCH_BOOKSUCCESS)
            {
                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount REK NASABAH SUCCESS");

                o.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                o.Status = ParameterHelper.TRXSTATUS_COMPLETE;
                o.Description = ParameterHelper.TRXDESCRIPTION_COMPLETE;
                o.LastUpdate = DateTime.Now;
            }
            else if (bookAmt.Status == ParameterHelper.SCH_BOOKFAILED)
            {
                log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount REK NASABAH FAILED: " + bookAmt.Description + " === UnHolding Amount");
                o.Status = ParameterHelper.PAYROLL_EXCEPTION;
                o.LastUpdate = DateTime.Now;
                o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + ",  Booking Amount CustAcc Failed" + "||" + o.Description;
            }
            session.Update(o);
            session.Flush();
            #endregion
        }
        else if (o.Status == ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_BOOK_REK_NASABAH && o.StatusBRIS.Equals(5))
        {
            #region KHUSUSON BRIS

            Booking bookAmt = session.Load<Booking>(o.IdBooking4);
            if (bookAmt.Status == ParameterHelper.SCH_BOOKSUCCESS)
            {
                log.Info(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount REK NASABAH SUCCESS");

                o.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                o.Status = ParameterHelper.TRXSTATUS_COMPLETE;
                o.Description = ParameterHelper.TRXDESCRIPTION_COMPLETE;
                o.LastUpdate = DateTime.Now;
            }
            else if (bookAmt.Status == ParameterHelper.SCH_BOOKFAILED)
            {
                log.Error(_schCode + " === (SeqNo " + o.SeqNumber + ") Booking Amount REK NASABAH FAILED: " + bookAmt.Description + " === UnHolding Amount");
                o.Status = ParameterHelper.PAYROLL_EXCEPTION;
                o.LastUpdate = DateTime.Now;
                o.Description = ParameterHelper.TRXDESCRIPTION_REJECT + ",  Booking Amount CustAcc Failed" + "||" + o.Description;
            }
            session.Update(o);
            session.Flush();
            #endregion KHUSUSON BRIS
        }

    }


    //Get payroll detail sukses summmary
    public static double GetPayrollDetailSukses(TrxPayroll o, out int totCntIFT, out int totCntRTG, out int totCntLLG, out int totCntIFTSukses, out int totCntRTGSukses, out int totCntLLGSukses, out int totCntBRIS, out int totCntBRISSukses)
    {
        totCntIFT = 0; 
        totCntRTG = 0; 
        totCntLLG = 0;
        totCntBRIS = 0;
 

        totCntIFTSukses = 0; 
        totCntRTGSukses = 0; 
        totCntLLGSukses = 0;
        totCntBRISSukses = 0;

        double totAmtIFT = 0; 
        double totFeeIFT = 0; 
        double totAmtRTG = 0; 
        double totFeeRTG = 0; 
        double totAmtLLG = 0; 
        double totFeeLLG = 0;
        double result = 0;

        double totAmtBRIS = 0;
        double totFeeBRIS = 0;

        try
        {
            IList<TrxPayrollDetail> pys = o.TrxPayrollDetail;

            totCntIFT = pys.Where(x => x.InstructionCode == TotalOBHelper.IFT || String.IsNullOrEmpty(x.InstructionCode)).Count();
            totCntRTG = pys.Where(x => x.InstructionCode == TotalOBHelper.RTGS).Count();
            totCntLLG = pys.Where(x => x.InstructionCode == TotalOBHelper.LLG).Count();
            totCntBRIS = pys.Where(x => x.InstructionCode == TotalOBHelper.BRIS).Count();

            totCntIFTSukses = pys.Where(x => (x.InstructionCode == TotalOBHelper.IFT || String.IsNullOrEmpty(x.InstructionCode)) && (x.Status == ParameterHelper.TRXSTATUS_REJECT || x.Status == ParameterHelper.TRXSTATUS_SUCCESS)).Count();
            totCntRTGSukses = pys.Where(x => x.InstructionCode == TotalOBHelper.RTGS && (x.Status == ParameterHelper.TRXSTATUS_REJECT || x.Status == ParameterHelper.TRXSTATUS_SUCCESS)).Count();
            totCntLLGSukses = pys.Where(x => x.InstructionCode == TotalOBHelper.LLG && (x.Status == ParameterHelper.TRXSTATUS_REJECT || x.Status == ParameterHelper.TRXSTATUS_SUCCESS)).Count();
            totCntBRISSukses = pys.Where(x => x.InstructionCode == TotalOBHelper.BRIS && (x.Status == ParameterHelper.TRXSTATUS_REJECT || x.Status == ParameterHelper.TRXSTATUS_SUCCESS)).Count();


            totAmtIFT = pys.Where(x => (x.InstructionCode == TotalOBHelper.IFT || String.IsNullOrEmpty(x.InstructionCode)) && x.Status == ParameterHelper.TRXSTATUS_SUCCESS).Sum(x => x.Amount);
            if (totAmtIFT > 0)
                totAmtIFT = totAmtIFT / 100;
            totFeeIFT = pys.Where(x => (x.InstructionCode == TotalOBHelper.IFT || String.IsNullOrEmpty(x.InstructionCode)) && x.Status == ParameterHelper.TRXSTATUS_SUCCESS).Count() * o.Fee;


            totAmtRTG = pys.Where(x => x.InstructionCode == TotalOBHelper.RTGS && x.Status == ParameterHelper.TRXSTATUS_SUCCESS).Sum(x => x.Amount);
            if (totAmtRTG > 0)
                totAmtRTG = totAmtRTG / 100;
            totFeeRTG = pys.Where(x => x.InstructionCode == TotalOBHelper.RTGS && x.Status == ParameterHelper.TRXSTATUS_SUCCESS).Count() * o.FeeRTG;


            totAmtLLG = pys.Where(x => x.InstructionCode == TotalOBHelper.LLG && x.Status == ParameterHelper.TRXSTATUS_SUCCESS).Sum(x => x.Amount);
            if (totAmtLLG > 0)
                totAmtLLG = totAmtLLG / 100;
            totFeeLLG = pys.Where(x => x.InstructionCode == TotalOBHelper.LLG && x.Status == ParameterHelper.TRXSTATUS_SUCCESS).Count() * o.FeeLLG;

            //tambahan buat bris
            totAmtBRIS = pys.Where(x => x.InstructionCode == TotalOBHelper.BRIS && x.Status == ParameterHelper.TRXSTATUS_SUCCESS).Sum(x => x.Amount);
            if (totAmtBRIS > 0)
                totAmtBRIS = totAmtBRIS / 100;
            totFeeBRIS = pys.Where(x => x.InstructionCode == TotalOBHelper.BRIS && x.Status == ParameterHelper.TRXSTATUS_SUCCESS).Count() * o.FeeBRIS;
            //end tambahan bris

            result = totAmtIFT + totAmtLLG + totAmtRTG + totFeeIFT + totFeeLLG + totFeeRTG + totAmtBRIS + totFeeBRIS;

        }
        catch (Exception ex) { result = -99; }
        return result;
    }

    public static void ProcessRTGSLLGCheckBooking(ISession session, TrxPayrollDetail o, ILog log, int jobNumber, string _schCode)
    {
        bool isSaldoTidakCukup = false;
        String SchCode = _schCode;

        //check status di trxpayrolldetail
        #region check amount
        if (o.IdBooking > 0)
        {
            Booking bookAmt = session.Load<Booking>(o.IdBooking);
            if (null != bookAmt)
            {
                if (bookAmt.Status == ParameterHelper.SCH_BOOKSUCCESS)
                {
                    o.Status = ParameterHelper.TRXSTATUS_PAYROLL_POST_MBASEANDWS_RTGSANDLLG;
                    o.Description = ParameterHelper.TRXDESCRIPTION_PAYROLL_POST_MBASEANDWS_RTGSANDLLG;
                    o.LastUpdate = DateTime.Now;
                }
                else if (bookAmt.Status == ParameterHelper.SCH_BOOKFAILED)
                {
                    #region fail book
                    bookAmt.Description = bookAmt.Description.Trim();

                    if (bookAmt.Description.ToLower().Contains("remit"))
                    {
                        #region Remit group already exists
                        o.Status = ParameterHelper.TRXSTATUS_PAYROLL_WAITINGRESPONSE_REMITNUMB;
                        o.Description = "Waiting Create RN";
                        log.Error(SchCode + " === (TrxID " + o.Id + ") Booking Failed: " + bookAmt.Description + " - Retry booking");
                        //session.Delete(bookAmt);
                        #endregion
                    }
                    else if (bookAmt.Description.ToLower().Contains("in use"))
                    {
                        #region account in use
                        bookAmt.Status = ParameterHelper.SCH_WAITINGBOOK;
                        bookAmt.LastUpdate = DateTime.Now.AddMinutes(2);
                        o.LastUpdate = DateTime.Now.AddMinutes(2);
                        bookAmt.JournalSeq = "";
                        bookAmt.Bucket = "";
                        bookAmt.Description = "Book in " + bookAmt.LastUpdate.ToString("HH:mm");
                        session.Update(bookAmt);
                        session.Flush();
                        log.Error(SchCode + " === (TrxID " + o.Id + ") Booking Failed: " + bookAmt.Description + " - Retry booking");
                        #endregion
                    }
                    else
                    {
                        o.Status = ParameterHelper.TRXSTATUS_REJECT;
                        o.Description = "ERROR - " + System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(bookAmt.Description.ToLower());
                        log.Error(SchCode + " === (TrxID " + o.Id + ") Booking Failed: " + bookAmt.Description);
                    }
                    #endregion
                }
                else
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === TrxId: " + o.Id + " - Not Complete Yet, status " + bookAmt.Status + " " + bookAmt.Description);
            }
        }
        #endregion check amount



        session.Update(o);
        session.Flush();
    }


    //Post Booking Kliring
    public static void ProcessRTGSLLGPostToBooking(ISession session, TrxPayrollDetail o, ILog log, int jobNumber, string _schCode)
    {
        TrxPayroll py = o.Parent;
        MessageABCSTransactionResponse response = null;
        string SchCode = _schCode;
        log.Info(SchCode + " === (TrxID " + o.Id + ") === Begin Posting to Booking Process");
        Boolean dibuku = true;
        Boolean segera = false;
        int fid = 100;
        try
        {
            String _debetAcc = ParameterHelper.GetString("PAYROLL_IA_KREDIT", session);
            String _debetAccName = "PAYROLL_IA_KREDIT";
            String _debetCurr = "IDR";
            String _creditAcc = o.RemittanceNumber;
            String _creditCurr = "IDR";
            String _creditAccName = o.Name;
            int clientId = py.ClientID;
            double fee = 0;

            if (o.InstructionCode.Equals("LLG"))
            {
                fee = py.FeeLLG;
            }
            else
            {
                fee = py.FeeRTG;
            }

            Double insAmtBook1 = (o.Amount / 100) + fee;
            String insAmtBook1Curr = "IDR";
            String bookingOffice = "0374";
            String OutMsg = "";
            int bookingId = 0;
            String remark = o.Id + " PY " + o.InstructionCode + " " + o.RemittanceNumber;
            int booktype = 1;


            if (BookingHelper.InsertBookingNew(session, o.Id.ToString(), fid, TotalOBHelper.Payroll,
                clientId, _debetAcc, _debetAccName, _debetCurr, _creditAcc, _creditAccName, _creditCurr, insAmtBook1,
                insAmtBook1Curr, "", remark, "", DateTime.Now, booktype, bookingOffice, fee,
                out bookingId, out OutMsg))
            {
                o.TrxRemark = remark;
                o.IdBooking = bookingId;
                o.Status = ParameterHelper.TRXSTATUS_PAYROLL_WAITINGRESPONSE_CHECK;
                o.Description = ParameterHelper.TRXDESCRIPTION_PAYROLL_CHECK_BOOK;
                log.Info(SchCode + " === (TrxID " + o.Id + ") === Success Post to Booking");
            }
            else
            {
                if (bookingId != 0)
                {
                    o.TrxRemark = remark;
                    o.IdBooking = bookingId;
                    o.Status = ParameterHelper.TRXSTATUS_PAYROLL_WAITINGRESPONSE_CHECK;
                    o.Description = ParameterHelper.TRXDESCRIPTION_PAYROLL_CHECK_BOOK;
                    log.Info(SchCode + " === (TrxID " + o.Id + ") === BookingId Already exist, continue to check");
                }
                else
                {
                    log.Error(SchCode + " === (TrxID " + o.Id + ") === Failed Post to Booking, retry in 2 min:  " + OutMsg);
                    o.LastUpdate = DateTime.Now.AddMinutes(2);
                }
            }


        }
        catch (Exception ex)
        {
            log.Error(SchCode + " === (TrxID " + o.Id + ") Exception Message:: " + ex.Message);
            log.Error(SchCode + " === (TrxID " + o.Id + ") Exception InnerException:: " + ex.InnerException);
            log.Error(SchCode + " === (TrxID " + o.Id + ") Exception StackTrace:: " + ex.StackTrace);
            o.LastUpdate = DateTime.Now.AddMinutes(5);
        }

        session.Update(o);
        session.Flush();
        log.Info(SchCode + " === (TrxID " + o.Id + ") === End Posting to Booking Process");

    }

    public static String isHoliday(ISession session, Int32 _fid, DateTime date)
    {
        String result = "[WEEKDAY]";
        try
        {
            if (date.DayOfWeek.ToString().Equals("Sunday") || date.DayOfWeek.ToString().Equals("Saturday") || date.DayOfWeek.ToString().Equals("Minggu") || date.DayOfWeek.ToString().Equals("Sabtu"))
            {
                return "[WEEKEND]";
            }

            Holidays o = session.CreateCriteria(typeof(Holidays))
                            .Add(Expression.Eq("Id", date.ToString("ddMMyyyy")))
                            .UniqueResult<Holidays>();

            if (o != null)
            {
                List<Int32> fid = new List<Int32>();
                String[] strfid = o.FunctionId.Trim().Split(new char[] { '|' });

                foreach (String s in strfid)
                {
                    try
                    {
                        fid.Add(Int32.Parse(s));
                    }
                    catch
                    { }
                }

                if (fid.Exists(p => p == _fid))
                {
                    return "[HOLIDAY]";
                }
            }
        }
        catch (Exception ex)
        {
            result = "[FAILED CHECK] >>> " + ex.Message + " >>> " + ex.StackTrace;
        }
        return result;
    }

    public static DateTime GetRealProcessTime(ISession session, DateTime date)
    {
        //ikut waktu single CN
        DateTime result = date;

        bool sudahKetemuProcessTime = false;
        string jamCutOff = ParameterHelper.GetString("LLG_CUTOFFTIME", session) + ":00";
        string jamStart = ParameterHelper.GetString("LLG_STARTTIME", session) + ":00";
        while (!sudahKetemuProcessTime)
        {
            string day = isHoliday(session, 1200, result);

            if (day.Equals("[WEEKDAY]"))
            {
                //cek uda lewat cutoff ato uda mule starttime

                DateTime ddCO = DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyyy") + " " + jamCutOff, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                DateTime ddST = DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyyy") + " " + jamStart, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                if (result.TimeOfDay > ddCO.TimeOfDay)
                {
                    //jamnya lewat dari waktu cutoff
                    result = DateTime.ParseExact(result.AddDays(1).ToString("dd/MM/yyyy") + " " + jamStart, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (result.TimeOfDay < ddST.TimeOfDay)
                {
                    //jamnya kurang dari waktu mulai
                    result = DateTime.ParseExact(result.ToString("dd/MM/yyyy") + " " + jamStart, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                    sudahKetemuProcessTime = true;
            }
            else
                result = DateTime.ParseExact(result.AddDays(1).ToString("dd/MM/yyyy") + " " + jamStart, "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

        }
        return result;
    }

    public static void ParseResponseFile(TrxPayroll payroll, String fileName, ISession session, ILog log, string _schCode)
    {

        log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Start parsing file " + fileName);

        try
        {

            //20170524 - sayedzul - handler file balikan dari host 0 KB
            FileInfo fl = new FileInfo(fileName);
            if (fl.Length == 0)
            {
                payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                payroll.Description = "Cannot Read File Response From Host, Plese contact Administrator||" + payroll.Description;
                payroll.ErrorDescription += "||FileResponse From Host NULL";
                log.Error(_schCode + " === (SeqNo " + payroll.SeqNumber + ") File Balikan dari Host 0 KB!");

                string email_content = _schCode + " === (SeqNo " + payroll.SeqNumber + ") File Balikan dari Host 0 KB!";

                string receiver = "cash_mgt@bri.cpayroll.id|cms_warrior@bri.cpayroll.id";
                int idEmailTransaction = 0;
                string msg = "";
                EmailHelperUniversal.AddEmailTransaction(session, 0, 100, "PAYROLL RESPONSE ERROR", 1705, receiver, "PAYROLL RESPONSE ERROR", email_content, "", 2, out idEmailTransaction, out msg);

                session.Update(payroll);
                session.Flush();
                return;
            }

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);


            StreamReader sr = new StreamReader(fs);


            Boolean _next = true;
            Boolean _confirm = true;
            double totamt = 0;
            double totfee = 0;
            double totaltrx = 0;
            String accfrom = "";
            while (_next && _confirm)
            {
                log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") si boolean next : " + _next + "isi boolean confirm : " + _confirm);

                String detail = sr.ReadLine();

                log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") isi detail : " + detail);

                if (null == detail) break;
                _confirm = false;
                switch (detail.Substring(0, 2))
                {
                    case "FH":
                        String _response = detail.Substring(37, 3);
                        if (!"000".Equals(_response))
                        {
                            log.Error(_schCode + " === (SeqNo " + payroll.SeqNumber + ") File header error, code : " + _response);

                            payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                            payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_REJECT);
                            payroll.LastUpdate = DateTime.Now;
                            payroll.ErrorDescription += "Status 20|File header (FH) error, code : " + _response;
                            session.Update(payroll);
                            _next = false;
                        }
                        else
                        {
                            log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") File header succesfully proceed");

                            _confirm = true;
                        }
                        break;

                    case "CH":
                        _response = detail.Substring(11, 3);
                        if (!"000".Equals(_response))
                        {
                            log.Error(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Client header error, code : " + _response);

                            payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                            payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_REJECT);
                            payroll.LastUpdate = DateTime.Now;
                            payroll.ErrorDescription += "||Status 20|Client Header(CH) error, code : " + _response;
                            session.Update(payroll);
                            _next = false;
                        }
                        else
                        {
                            log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Client header succesfully proceed");

                            _confirm = true;
                        }
                        break;

                    case "BH":
                        _response = detail.Substring(101, 3);
                        accfrom = detail.Substring(23, 15);
                        if (!"000".Equals(_response))
                        {
                            log.Error(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Batch header error, code : " + _response);

                            if (_response.Equals("005"))
                            {
                                payroll.StatusIFT = ParameterHelper.TRXSTATUS_REJECT;
                                payroll.Description = payroll.Description.Replace("PROCESSED", "REJECT - SALDO TIDAK CUKUP (Rekening tidak terdebet)");
                            }
                            else
                            {
                                payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                                payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_REJECT);
                            }

                            payroll.LastUpdate = DateTime.Now;
                            payroll.ErrorDescription += "||Status 20|Batch Header (BH) error, code : " + _response;
                            session.Update(payroll);
                            _next = false;
                        }
                        else
                        {
                            log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Batch header succesfully proceed");

                            _confirm = true;
                        }
                        break;

                    case "BD":
                        try
                        {
                            _response = detail.Substring(161, 3);
                            //log.Info("isi response : " + _response);

                            String account = detail.Substring(22, 16);
                            //log.Info("isi account : " + account);

                            String amount = detail.Substring(41, 15);
                            //log.Info("isi amount : " + amount);

                            String fee = detail.Substring(71, 15);
                            //log.Info("isi fee : " + fee);

                            String description = detail.Substring(164, 40);
                            //log.Info("isi description : " + description);

                            account = account.TrimStart('0');
                            //log.Info("isi account after trim : " + account);

                            //20170202 - sayedzul - add padding account for a better query
                            account = account.PadLeft(15, '0');
                            //log.Info("isi account after trim : " + account);

                            amount = amount.TrimStart('0');
                            //log.Info("isi amount after trim : " + amount);

                            fee = fee.TrimStart('0');
                            if (fee == "")
                            {
                                fee = "0";
                            }
                            //log.Info("isi fee after trim : " + fee);

                            log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Going into sql command");

                            //rfqconnection
                            Parameter dbConf = session.Load<Parameter>("CMS_DBCONNECTION");
                            MySqlConnection conn = new MySqlConnection(dbConf.Data);
                            //conn.ConnectionTimeout = 20000;
                            conn.Open();

                            MySqlCommand command = conn.CreateCommand();

                            if (!"000".Equals(_response))
                            {
                                command.CommandText = "update trxpayrolldetails set status=" +
                                    ParameterHelper.TRXSTATUS_REJECT + ", description='" + description + "' where pid='" + payroll.Id
                                    + "' and account = '" + account + "' and amount = " + amount;

                            }
                            else
                            {
                                totfee += double.Parse(fee);
                                totamt += double.Parse(amount);
                                totaltrx++;
                                command.CommandText = "update trxpayrolldetails set status=" +
                                    ParameterHelper.TRXSTATUS_SUCCESS + " where pid='" + payroll.Id
                                    + "' and account = '" + account + "' and amount = " + amount;

                            }
                            int i = command.ExecuteNonQuery();
                            if (i > 0)
                            {
                                log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Update detail succesfully proceed : " + account);

                                payroll.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_COMPLETE;
                                payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_SUCCESS);
                                _confirm = true;
                            }
                            else
                            {
                                log.Error(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Suspect detail detected, cannot execute query where account : " + account);
                                payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                                //payroll.Description = payroll.Description.Replace("PROCESSED", "SUSPECT");
                                payroll.ErrorDescription += "||Suspect detail detected, cannot execute query where account : " + account;
                                _confirm = false;
                            }
                            //tutup koneksi
                            conn.Close();
                        }
                        catch (Exception e)
                        {
                            log.Error(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Suspect detail detected, cannot process BD header");

                            payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                            payroll.ErrorDescription += "||Suspect detail detected, cannot process BD header >>>" + e.Message + "==" + e.InnerException + "==" + e.StackTrace;
                            //payroll.Description = payroll.Description.Replace("PROCESSED", "SUSPECT");
                            _confirm = false;
                        }
                        break;

                    default:
                        _confirm = true;
                        break;
                }
            }

            #region trxobhist
            //if (_confirm)
            //{
            //    TrxOBHist ob = new TrxOBHist();
            //    ob.ParentId = 0;
            //    Client cl = session.CreateCriteria(typeof(Client))
            //        .Add(Expression.Like("Handle", payroll.CreatedBy.Substring(0, payroll.CreatedBy.IndexOf("/")).Trim()))
            //        .UniqueResult<Client>();
            //    ob.ClientId = cl.Id;
            //    ob.TrxCode = TotalOBHelper.Payroll;
            //    ob.TrxType = 1;
            //    ob.TrxActId = "0";
            //    ob.ValueDate = DateTime.Now.ToString("yyMMdd");
            //    ob.CreditCur = "IDR";
            //    ob.CreditAmt = totamt / 100;
            //    ob.KursBeliTrx = "1";
            //    ob.KursJualTrx = "1";
            //    ob.DebitCur = "IDR";
            //    ob.DebitAmt = totamt / 100;
            //    ob.BaseCur = "IDR";
            //    ob.BaseAmt = totamt / 100;
            //    ob.KursJualBase = "1";
            //    ob.TrxRemark = payroll.Id; // pid payroll
            //    ob.VoucherCode = "";
            //    ob.ChargeCur = "IDR";
            //    ob.ChargeAmt = totfee / 100;
            //    ob.DebitAcc = accfrom;
            //    log.Info("Number : " + accfrom);
            //    log.Info("PID : " + cl.Id);
            //    ClientAccount cd = session.CreateCriteria(typeof(ClientAccount))
            //        .Add(Expression.Like("Number", accfrom))
            //        .Add(Expression.Eq("Pid", cl.Id))
            //        .UniqueResult<ClientAccount>();
            //    ob.DebitAccName = cd.Code;
            //    ob.BenAcc = totaltrx.ToString();
            //    ob.BenAccName = "-";
            //    ob.BenBankIdentifier = "0002";
            //    ob.BenBankName = "PT.BRI (PERSERO) TBK.";
            //    ob.MakerInfo = payroll.Maker;
            //    ob.CheckerInfo = payroll.Checker;
            //    ob.ApproverInfo = payroll.Approver;
            //    ob.LastUpdate = DateTime.Now;
            //    if (payroll.Description.Contains("SUCCESS"))
            //    {
            //        ob.Description = "Success";
            //        ob.Status = 3;
            //    }
            //    else
            //    {
            //        ob.Description = payroll.Description;
            //        ob.Status = 4;
            //    }
            //    session.Save(ob);
            //}
            #endregion

            //payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;

        }
        catch (Exception ex)
        {
            payroll.ErrorDescription += "||Status 20|Error : " + ex.Message + ">>" + ex.InnerException + ">>" + ex.StackTrace;
            payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
            log.Error(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Error : " + ex.Message + ">>" + ex.InnerException + ">>" + ex.StackTrace);
        }

        session.Update(payroll, payroll.Id);
        session.Flush();

        log.Info(_schCode + " === (SeqNo " + payroll.SeqNumber + ") Finish parsing file " + fileName);

    }



    /* New added by Mettaw : NEW MBASE EXPRESS */
    public static void ProcessMassLLG_POSTToExpress(ISession session, TrxPayrollDetail o, ILog log, int jobNumber)
    {
        ExpressProcess exprocess = new ExpressProcess();


        log.Info(SchCode + " === (TrxID " + o.Id + ") === Begin Posting to MBASE Express");

        try
        {
            #region Parameter Kudu ada
            Double _amt = 0;

            TrxPayroll p = o.Parent;
            ClientAccount clientAcc = new ClientAccount();

            Booking b = session.Load<Booking>(o.IdBooking);

            BankKliring bank = session.CreateCriteria(typeof(BankKliring))
                                       .Add(Expression.Eq("BankCode", o.Account))
                                       .UniqueResult<BankKliring>();

            ClientMatrix cm = session.Load<ClientMatrix>(p.ClientID);
            Client client = session.Load<Client>(p.ClientID);
            AuthorityHelper ah = new AuthorityHelper(cm.Matrix);

            //FEE
            if (o.InstructionCode.Equals(TotalOBHelper.RTGS))
                _amt = Double.Parse(ah.GetTransactionFee(session, 100, 2).ToString());
            else if (o.InstructionCode.Equals(TotalOBHelper.LLG))
                _amt = Double.Parse(ah.GetTransactionFee(session, 100, 3).ToString());


            int fid = 100;

            string fitur = TotalOBHelper.Payroll;
            string _debetAcc = "";
            string _debetAccName = "";
            string _benAcc = o.Account;
            string _benAccName = o.Name;
            string _benAccAddr = o.BenAddress;
            string _remark = o.TrxRemark.Trim();

            string bucketAmt = b.Bucket;
            string chargeType = "OUR";

            if (b.FeeBen > 0)
                chargeType = "BEN";

            string debitAccAddress = client.Address1;

            /*New added By Mettaw*/
            String _fitur = "PAYROLL LLG";
            String _outMsg = "";
            int outId = 0;

            double _instamount = o.Amount;
            string _benbankname = bank.Nama;
            string _instcur = "IDR";

            
            try
            {
                IList<ClientAccount> clAccs = session.CreateCriteria(typeof(ClientAccount))
                    .Add(Expression.Eq("Number", p.DebitAccount))
                    .Add(Expression.Eq("Pid", p.ClientID))
                    .Add(Expression.Eq("StatusDel", 0))
                    .SetMaxResults(1)
                    .List<ClientAccount>();

                clientAcc = clAccs[0];

                _debetAcc = p.DebitAccount;
                _debetAccName = clientAcc.Name;
            }
            catch { }


            #endregion

            #region Add ExpressProcess
            /*New added by Mettaw :  
                     * tampung data disini... untuk parameter WS Express 
                     * dan insert ke tbl expressprocess 
                     * 9 september 2019*/

            /*DATA MANDATORY*/
            exprocess.Tanggal_Transaksi = DateTime.Now; /*belum tau*/
            exprocess.Sarana_Transaksi = "2";
            exprocess.Kode_Transaksi = "50";
            exprocess.Peserta_Pengirim_Asal = "BRINIDJA";
            exprocess.Sandi_Kota_Asal = "0391";
            exprocess.Peserta_Pengirim_Penerus = "BRINIDJA";
            exprocess.Peserta_Penerima_Akhir = bank.BankCode;
            exprocess.Sandi_Kota_Tujuan = bank.SandiKota;
            exprocess.Peserta_Penerima_Penerus = bank.BankCode;
            exprocess.Jenis_Nasabah = "1";
            exprocess.No_Rekening_Pengirim = _debetAcc;
            exprocess.Nama_Pengirim = _debetAccName;
            exprocess.No_Identitas_Pengirim = "338489091092019201";
            exprocess.Jenis_Nasabah_Pengirim = "1";
            exprocess.Status_Kependudukan_Pengirim = "1";
            exprocess.Alamat_Pengirim = client.Address1;
            exprocess.No_Rekening_Tujuan = _benAcc;
            exprocess.Nama_Penerima = _benAccName;
            exprocess.Jenis_Nasabah_Penerima = "1";
            exprocess.Status_Kependudukan_Penerima = "1";
            exprocess.Alamat_Penerima = _benAccAddr;
            exprocess.Cara_Penyetoran = "DEBET_REK";
            exprocess.No_Rekening_Asal = _debetAcc;
            exprocess.Nama = _debetAccName;

            /*belum tau*/
            //exprocess.Jumlah_Dikirim = string.Format("{0:N}", o.InstAmount); 
            exprocess.Jumlah_Dikirim = (_instamount * 100).ToString("0000000000") + ".00"; /*Jika ikutin yg existing*/

            exprocess.Currency_Dikirim = _instcur;
            exprocess.Biaya = string.Format("{0:N}", _amt);

            /*belum tau*/
            //exprocess.Total = string.Format("{0:N}", o.InstAmount + o.FeeAmount); /*belum tau*/
            exprocess.Total = (double.Parse((_instamount * 100).ToString("0000000000")) + _amt) + ".00"; /*format Amountnya ikutin yg existing*/

            exprocess.Currency_Total = _instcur;
            exprocess.Kanca_Asal = "CMS";
            exprocess.User_Approve = "0374891";
            exprocess.Cabang = "0374";
            exprocess.No_Remitance = o.RemittanceNumber;

            exprocess.Jurnal_Seq = o.IdBooking.ToString();
            
            exprocess.Nama_Bank_Penerima = _benbankname;
            exprocess.Provinsi_Penerima = bank.SandiPropinsi;
            exprocess.Jenis_Badan_Usaha = bank.JenisUsaha.Trim();
            exprocess.Kode_Bank_Brinets_Penerima = bank.Code;
            exprocess.Channel = "CMS";
            
            exprocess.Booking_Id = o.IdBooking.ToString();


            /*DATA TIDAK MANDATORY*/
            exprocess.Telepon_Pengirim = "";
            exprocess.No_Identitas_Penerima = "";
            exprocess.Telepon_Penerima = "";
            exprocess.NoCekBG = "";
            exprocess.Berita = "";
            exprocess.Sumber_Dana = "";
            exprocess.Keperluan = "";
            exprocess.Pekerjaan = "";
            exprocess.Jabatan = "";
            exprocess.Ttl = "";
            exprocess.User_Id = "";
            exprocess.Cif = "";
            exprocess.Kode_Pos_Pengirim = "";
            exprocess.Kode_Pos_Penerima = "";
            exprocess.Remark = _remark;
            #endregion

            #region PROCESS
            if (MBASEHelper.InsertMBASEExpress(session, o.Id, _fitur, exprocess, out _outMsg, out outId))
            {
                #region UPDATE STATUS

                /*Express Process*/
                exprocess.Status = ParameterHelper.SCH_MBASE_WAITING; /*1: Waiting*/
                exprocess.Description = ParameterHelper.MBASEDESC_WAITINGMBASE;
                exprocess.LastUpdate = DateTime.Now;
                session.Update(exprocess);

                /*Trx Payroll*/
                o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG;
                o.Description = "proses express".ToUpper();
                
                log.Info(SchCode + " === (TrxID " + o.Id + ") === Posting to MBASEANDWS SUCCESS");
                #endregion
            }
            else
            {
                if (outId > 0)
                {
                    #region UPDATE STATUS
                    /*bila gagal validasi insert, tapi id sudah terbuat, update aja coy */

                    /*Express Process*/
                    ExpressProcess ex = session.Load<ExpressProcess>(outId);
                    ex.Status = ParameterHelper.SCH_MBASE_WAITING; /*1: Waiting MBASE*/
                    ex.Description = ParameterHelper.MBASEDESC_WAITINGMBASE;
                    ex.LastUpdate = DateTime.Now;
                    session.Update(ex);

                    /*Trx Payroll*/
                    o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG;
                    o.Description = "proses express".ToUpper();

                    log.Info(SchCode + " === (TrxID " + o.Id + ") === Already Exist, Posting to MBASEANDWS SUCCESS");
                    #endregion
                }
                else
                {
                    #region REVERSAL PROCESS
                    /*Gagal Insert to ExpressProcess, do Reversal.*/

                    /*Booking*/
                    Booking bookingAmt = session.Load<Booking>(o.IdBooking);
                    bookingAmt.StatusEC = 1;

                    ///*belum tau , tidak ada jurnal seq 2*/
                    //if (!string.IsNullOrEmpty(o.JournalSeq2))
                    //{
                    //    Booking bookingFee = session.Load<Booking>(int.Parse(o.JournalSeq2));
                    //    bookingFee.StatusEC = 1;
                    //    session.Update(bookingFee);
                    //}

                    /*Trx Payroll*/
                    o.Status = ParameterHelper.TRXSTATUS_REJECT; /*Rejected*/
                    o.Description = "Reject";

                    session.Update(bookingAmt);
                    session.Update(o);
                    session.Flush();

                    log.Info(SchCode + jobNumber + " ===  (TrxID " + o.Id + " ) Insert to ExpressProcess FAILED: " + _outMsg + " data Reject & REVERSAL");

                    #endregion
                }
            }

            session.Update(o);
            session.Flush();

            log.Info(SchCode + " === (TrxID " + o.Id + ") === End Posting to MBASEANDWS");
            #endregion

            
        }
        catch (Exception ex)
        {
            log.Error(SchCode + " === (TrxID " + o.Id + ") === Exception: " + ex.Message + " >>> " + ex.InnerException + " >>> " + ex.StackTrace);
        }
    }

    public static void ProcessMassLLG_CHECKToExpress(ISession session, TrxPayrollDetail o, ILog log, int jobNumber)
    {
        ExpressProcess exprocess = session.CreateCriteria(typeof(ExpressProcess))
                                                    .Add(Expression.Eq("TrxID", o.Id.ToString()))
                                                    .UniqueResult<ExpressProcess>();

        log.Info(SchCode + jobNumber + " ===  (TrxID " + o.Id + " ) Begin Check MBASE Process");

        if (exprocess.Status == ParameterHelper.SCH_MBASE_PROCESS) /*2: On Process*/
        {
            #region COMPLETE

            #endregion
        }
        else if (exprocess.Status == ParameterHelper.SCH_MBASE_FAILED) /*4: Rejected*/
        {
            #region REVERSAL

            #endregion
        }
        else
        {
            log.Info(SchCode + jobNumber + " ===  (TrxID " + o.Id + " ) Tidak ada transkasi");
        }

    }

    public static JObject postMBASEExpress(String data, String url)
    {
        JObject obj = null;
        try
        {
            #region postRestFul

            String sURL = "";
            if (data != null)
            {
                System.Net.ServicePointManager.DefaultConnectionLimit = 100;
                sURL = url;
                String sParam = data; //"object=" + data;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";

                    string HtmlResult = wc.UploadString(sURL, sParam);
                    Console.WriteLine(HtmlResult);
                    obj = JObject.Parse(HtmlResult);
                    wc.Dispose();
                }
            }
            #endregion
        }
        catch (Exception ex)
        {
        }

        return obj;
    }
    /* End added by Mettaw*/


    

    /* Process Generate File Payroll BRIS - DNY - 2020-01-16 */
    public static Boolean GenerateFileBRISPayment(ISession session, TrxPayroll o, ILog log, int jobNumber, String schInfo)
    {
        Boolean result = false;
        try
        {
            #region inisiasi
            Parameter generatePathFile = session.Load<Parameter>("PAYROLL_GENERATE_PATH_FILE");
            String generatePathfileData = generatePathFile.Data;
            
            #endregion inisiasi

            IList<TrxPayrollDetail> listPayDetail = session.CreateSQLQuery("select * from trxpayrolldetails where INSTRUCTIONCODE = ? and status = ? and pid = '" + o.Id + "'")
                    .AddEntity(typeof(TrxPayrollDetail))
                    .SetString(0, TotalOBHelper.BRIS)
                    .SetInt32(1, ParameterHelper.TRXSTATUS_PAYROLLDETAIL_AFTER_UPLOAD_BRIS)//1
                  .List<TrxPayrollDetail>();

            if (listPayDetail.Count > 0)
            {
                #region isi data detail
                int counter = 0;
                DateTime jamNow = DateTime.Now;

                FileInfo fi = new FileInfo(generatePathfileData);
                generatePathfileData = generatePathfileData + DateTime.Now.ToString("yyyyMMdd") + "\\";
                #region cek directory exist
                if (!Directory.Exists(generatePathfileData))
                {
                    System.IO.Directory.CreateDirectory(generatePathfileData);
                    log.Info(schInfo + SchCode + "SeqNum  = " + o.SeqNumber + " - Generate Directory hari ini : SUCCESS");
                }
                #endregion directori

                string filename = "payroll_" + o.ClientID + "_ " + jamNow.Month.ToString("MMM") + "_" + jamNow.ToString("ddMMyyyy") + "_" + o.SeqNumber;
                TextWriter tw1 = new StreamWriter(generatePathfileData + filename + ".csv");
                //localFile = pathReport + filename_error.Substring(0, filename_error.Length - 4) + ".ack";

                foreach (TrxPayrollDetail detail in listPayDetail)
                {

                    
                    string isiRow = "";
                    string text_status = "";
                    int no_urut = counter + 1;
                    String no_urut_pad = (no_urut.ToString()).PadLeft(4, '0');
                    if (counter == 0)
                    {
                        isiRow = "NO_URUT|MATA_UANG|NO_REKENING|NOMINAL|NARASI|TANGGALEKSEKUSI|REFFID";
                        tw1.WriteLine(isiRow);
                    }

                    //ini isi ke file
                    isiRow = no_urut_pad + "|IDR|" + detail.Account + "|" + detail.Amount + "|" + detail.TrxRemark + "|" + detail.LastUpdate + "|" + detail.CustomerReff;

                    tw1.WriteLine(isiRow);
                    counter++;
                    
                }
                tw1.Close();
                
                #endregion isi data


                //Sukses generate -  update agar sent FTP
                o.StatusBRIS = ParameterHelper.TRXSTATUS_PAYROLLNEW_WAITING_FTP_BRIS;//3 waiting send ftp
                o.FileNameBrisReq = generatePathfileData + "\\" + filename;
                o.FileNameBris = filename;
                o.LogFtpBris = DateTime.Now.ToString() + "- Gen File SUCCESS|";
                o.LastUpdate = DateTime.Now;
                session.Update(o);
                session.Flush();
                result = true;
                
            }
            else
            {
                log.Error(schInfo + SchCode + "SeqNum  = " + o.SeqNumber + " - Generate FIle BriS : Tidak ada transaksi BRIS ditemukan");
            }
        }
        catch (Exception x)
        { 
            //exception ketika generate file
            log.Error(schInfo + SchCode + "SeqNum  = " + o.SeqNumber + " - Generate FIle BriS : Exception");
            log.Error(schInfo + SchCode + "SeqNum  = " + o.SeqNumber + " - Generate FIle BriS : Exception : " + x.Message);
            log.Error(schInfo + SchCode + "SeqNum  = " + o.SeqNumber + " - Generate FIle BriS : Exception : " + x.InnerException);
        }

        return result;
    }

    //Send FTP to BRIS - DNY - 2020-01-20
    public static Boolean SendFileBRIS(ISession session, TrxPayroll o, ILog log, String SchCode)
    {
        Console.WriteLine(SchCode + " Download FTP seq : " + o.SeqNumber + " - Start");
        bool result = true;

        if (!String.IsNullOrEmpty(o.FileNameBrisReq))
        {
            

            //Inisiasi
            String filename = o.FileNameBrisReq;
            Parameter server = session.Load<Parameter>("PAYROLL_FTP_BRIS_SERVER");
            Parameter dir = session.Load<Parameter>("PAYROLL_FTP_BRIS_DIRECTORY_OUT");
            Parameter FTPUser = session.Load<Parameter>("PAYROLL_FTP_BRIS_USER");
            Parameter FTPPassword = session.Load<Parameter>("PAYROLL_FTP_BRIS_PASSWORD");
            Parameter FTPPort = session.Load<Parameter>("PAYROLL_FTP_BRIS_PORT");

            
            //Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                #region sendFTP OLD
                //clientSocket.Connect(server.Data, FTPPort.Data);
                //FTPHelper ff = new FTPHelper();
                //ff.setDebug(false);

                //ff.setRemoteHost(server.Data);
                //ff.setRemotePort(FTPPort.Data);
                //ff.setRemoteUser(FTPUser.Data);
                //ff.setRemotePass(FTPPassword.Data);
                //ff.setSocket(clientSocket);
                //Thread.Sleep(500);
                //ff.SendUserPassword();
                //ff.chdir(dir.Data);

                //ff.setBinaryMode(false);
                //ff.upload(path);
                //ff.close();
                //log.Info(SchCode + " Send FTP seq : " + trx.SeqNumber + " - SUCCESS");
                //#endregion sendFTP

                //#region update status 
                //o.StatusBRIS = ParameterHelper.TRXSTATUS_PAYROLLNEW_WAITING_RESP_FTP_BRIS;//5
                //o.LastUpdate = DateTime.Now;
                //o.LogFtpBris += DateTime.Now.ToString() + "- Send File SUCCESS|";
                //session.Update(o);
                //session.Flush();
                #endregion end of update status

                #region sent FTP NEW
                result = UploadDataFTP(server.Data + dir.Data, FTPUser.Data, FTPPassword.Data, o.FileNameBris, o.FileNameBrisReq, log);
                #endregion sent FTP New

                if (result)
                {
                    #region update status
                    o.statusbris = parameterhelper.trxstatus_payrollnew_waiting_resp_ftp_bris;//5
                    o.lastupdate = datetime.now;
                    o.logftpbris += datetime.now.tostring() + "- send file success|";
                    session.update(o);
                    session.flush();
                    #endregion update parent

                }
            }

            catch (Exception ex)
            {
                log.Info(SchCode + " Send FTP seq : " + o.SeqNumber + " - GAGAL");
                log.Info(SchCode + " Send FTP seq : " + o.SeqNumber + " - EXCEPTION : " + ex.Message);
                log.Info(SchCode + " Send FTP seq : " + o.SeqNumber + " - EXCEPTION : " + ex.InnerException);
                result = false;
                o.LastUpdate = DateTime.Now;
                o.LogFtpBris += DateTime.Now.ToString() + "- Send File GAGAL on " + ex.InnerException + "|";
                session.Update(o);
                session.Flush();
            }
        }
        else
        {
            result = false;
            log.Info(SchCode + " Send FTP seq : " + o.SeqNumber + " - File BRIS tidak ditemukan di database");

        }
        Console.WriteLine(SchCode + " Download FTP seq : " + o.SeqNumber + " - Finish");
        return result;
    }

    //Download file -  DNY - 2020-01-21
    /*
     * 1. Cek data sudah ada atau belm dengan Innititae FTP,eh sblm set download pastiin aja blm ada di DB, jika filename sudah pernah di ekse skip aja drpda ribet
     * 2. JIka blm ada, kelaurkan return false dg output message : NOT EXIST
     * 3. JIka ada, lakukan download, jika sukses download return true dan message : SUCCESS
     * 4. Jika ada, lakukan download, jika gagal download return false dan message : FAILED, kemudian lakukan retry sesuai role depan
     * 5. JIka berhasil download, pindahkan file response BRIS ke suatu folder / hapus sesuai keseakatan. Ideaknya ada folder PROCESSED
     */
    public static Boolean DownloadFileBRIS(ISession session, TrxPayroll o, ILog log, String SchCode, out string message)
    {
        Boolean result = false;
        message = "NOT COMPLETE";
        Console.WriteLine(SchCode + " Download FTP seq : " + o.SeqNumber + " - Start");
        try
        {
            //Inisiasi
            String filename = o.FileNameBris;
            Parameter server = session.Load<Parameter>("PAYROLL_FTP_BRIS_SERVER");
            Parameter dir = session.Load<Parameter>("PAYROLL_FTP_BRIS_DIRECTORY_IN");
            Parameter FTPUser = session.Load<Parameter>("PAYROLL_FTP_BRIS_USER");
            Parameter FTPPassword = session.Load<Parameter>("PAYROLL_FTP_BRIS_PASSWORD");
            Parameter FTPPort = session.Load<Parameter>("PAYROLL_FTP_BRIS_PORT");
            Parameter FTPDestDownload = session.Load<Parameter>("PAYROLL_FTP_BRIS_DEST_DOWNLOAD");
            Boolean GetListBRIS = false;
            List<string> files = new List<string>();
            Boolean downloadsegera = false;
            Boolean movesegera = false;
            Boolean deletSegera = false;

            String IP = "FTP://" + server.Data + ":" + FTPPort.Data + dir.Data; 

            
            
            try
            {
                if (GetListFTP(IP, FTPUser.Data, FTPPassword.Data, out files, log, o.SeqNumber, SchCode))
                {
                    //berhasil get list
                    if (files.Count > 0)
                    {
                        foreach (string file in files)
                        {
                            //Sementara BANGET - denny
                            if (!(file.ToUpper().Equals("OUTGOING/PROCESSED")))
                            {
                                //CEk dlu udah pernah di download blm
                                
                                //dowload
                                if (DownloadDataFTP(IP, FTPUser.Data, FTPPassword.Data, o.FileNameBris, FTPDestDownload.Data, log, o.SeqNumber, SchCode))
                                {
                                    //path name resp / hasil download
                                    
                                    //Upload processed
                                    if (UploadDataFTP(IP + "/PROCESSED", FTPUser.Data, FTPPassword.Data, o.FileNameBris, "", log, o.SeqNumber, SchCode))
                                    {
                                        //Hapus
                                        if (DeleteDataFTP(IP, FTPUser.Data, FTPPassword.Data, o.FileNameBris, log))
                                        {
                                            //berghasil hapus
                                        }
                                        else
                                        { 
                                            
                                        }
                                    }
                                    else
                                    { 
                                        //gagal upload
                                    }
                                    
                                }
                                else
                                {
                                    //gagal ddowbload
                                }
                
                            }
                            else
                            {
                                //ini file processed, g usah di download
                            }
                        }
                    }
                }
                else
                {
                    //gagal getlist
                }
            }
            catch (Exception getlist)
            {
                result = false;
            }
            
        }
        catch (Exception xx)
        { 
            
        }

        Console.WriteLine(SchCode + " Download FTP seq : " + o.SeqNumber + " - Finish");
        return result;
    }

    //----------------------------------DUNIA FTp------------------------------------------------
    /*FTP functions - Denny */
    public static bool GetListFTP(string ip, string user, string pswd, out List<string> filterList, ILog log, int seqnumber, String schcode)
    {
        List<string> listData = new List<string>();
        filterList = new List<string>();
        bool result = true;

        try
        {
            String uri = ip;
            String uid = user;
            String pass = pswd;

            Uri serverUri = new Uri(uri);

            FtpWebRequest reqFTP;

            reqFTP = (FtpWebRequest)FtpWebRequest.Create(serverUri);
            reqFTP.Credentials = new NetworkCredential(uid, pass);
            reqFTP.KeepAlive = false;
            //reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
            reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
            //reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            reqFTP.UseBinary = true;
            reqFTP.Proxy = null;
            reqFTP.UsePassive = true;
            reqFTP.Timeout = -1;

            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                listData = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            }

            //filterList = listData.Where(x => !x.ToUpper().Equals("PROCESSED")).ToList<string>();
            filterList = listData.Where(x => !x.Equals("PROCESSED")).ToList<string>();



            response.Close();
        }
        catch (Exception getlist)
        {
            log.Error(SchCode + " Get List FTP seq : " + seqnumber + " - File BRIS gagal getlist");
            log.Error(SchCode + " Get List FTP seq : " + seqnumber + " - IP FTP : " + ip);
            log.Error(SchCode + " Get List FTP seq : " + seqnumber + " - MSG FTP : " + getlist.Message);
            log.Error(SchCode + " Get List FTP seq : " + seqnumber + " - MSG FTP : " + getlist.InnerException);
            result = false;
        }
        return result;
    }

    /*Download FTP - Denny*/
    public static bool DownloadDataFTP(string ip, string user, string pswd, string fileName, string localFile, ILog log, int seqnumber, string schcode)
    {
        bool result = true;
        try
        {
            WebClient webClient = new WebClient();
            webClient.UseDefaultCredentials = true;
            webClient.Credentials = new NetworkCredential(user, pswd);
            webClient.DownloadFile(new Uri(ip + "/" + fileName), localFile);

        }
        catch (Exception ex)
        {
            result = false;
            log.Error(SchCode + " Download FTP seq : " + seqnumber + " - File BRIS gagal download");
            log.Error(SchCode + " Download FTP seq : " + seqnumber + " - IP FTP : " + ip);
            log.Error(SchCode + " Download FTP seq : " + seqnumber + " - MSG FTP : " + ex.Message);
            log.Error(SchCode + " Download FTP seq : " + seqnumber + " - MSG FTP : " + ex.InnerException);
        }

        
        return result;
    }

    /*Upload data - Denny */
    public static bool UploadDataFTP(string ip, string user, string pswd, string fileName, string localFile, ILog log)
    {
        bool result = true;
        try
        {
            FileInfo fi = new FileInfo(localFile);
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ip + "/" + fileName);
            req.Credentials = new NetworkCredential(user, pswd);
            req.UsePassive = true;
            req.UseBinary = true;
            req.KeepAlive = false;
            req.ContentLength = fi.Length;
            req.ReadWriteTimeout = 600000;
            req.Method = WebRequestMethods.Ftp.UploadFile;
            int intBufferLength = 16 * 1024;
            byte[] objBuffer = new byte[intBufferLength];
            FileStream objFileStream = fi.OpenRead();

            try
            {
                Stream objStream = req.GetRequestStream();
                int len = 0;

                while ((len = objFileStream.Read(objBuffer, 0, intBufferLength)) != 0)
                {
                    objStream.Write(objBuffer, 0, len);

                }

                objStream.Close();
                objFileStream.Close();

                result = true;
            }
            catch (Exception ex)
            {
                result = false;

            }
        }

        catch (Exception ex)
        {
            result = false;

        }
        return result;
    }

    /*Delete file FTP - Denny */
    public static bool DeleteDataFTP(string ip, string user, string pswd, string fileName, ILog log)
    {
        bool result = true;
        try
        {

            FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(ip + "/" + fileName);
            reqFTP.Credentials = new NetworkCredential(user, pswd);

            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            reqFTP.UseBinary = true;
            reqFTP.Proxy = null;
            reqFTP.UsePassive = true;
            reqFTP.Timeout = -1;
            reqFTP.ReadWriteTimeout = 600000;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            response.Close();
        }
        catch (Exception ex)
        {
            result = false;
            log.Error("URL: " + ip + " >>> " + ex.Message + " >>> " + ex.InnerException + " >>> " + ex.StackTrace);
        }
        return result;
    }

    //----------------------------------end of DUNIA FTp------------------------------------------------


    //--------------------------------PARSING--------------------------------------
    //Parsing hasil download
    public static Boolean ReadFileBRIS(ISession session, TrxPayroll o, ILog log, String SchCode)
    {
        Boolean result = true;

        return result;
    }

    public static Boolean RejectBRIS(ISession session, TrxPayroll o, ILog log, String SchCode)
    {
        Boolean hasil = false;



        return hasil;
    }


    //Helper Upload FTP eaea - Denny
    public static bool UploadDataFTP(string ip, string user, string pswd, string fileName, string localFile, ILog log, int seqnumber, string schcode)
    {
        bool result = true;
        try
        {
            FileInfo fi = new FileInfo(localFile);
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ip + "/" + fileName);
            req.Credentials = new NetworkCredential(user, pswd);
            req.UsePassive = true;
            req.UseBinary = true;
            req.KeepAlive = false;
            req.ContentLength = fi.Length;
            req.ReadWriteTimeout = 600000;
            req.Method = WebRequestMethods.Ftp.UploadFile;
            int intBufferLength = 16 * 1024;
            byte[] objBuffer = new byte[intBufferLength];
            FileStream objFileStream = fi.OpenRead();

            try
            {
                Stream objStream = req.GetRequestStream();
                int len = 0;

                while ((len = objFileStream.Read(objBuffer, 0, intBufferLength)) != 0)
                {
                    objStream.Write(objBuffer, 0, len);

                }

                objStream.Close();
                objFileStream.Close();

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                log.Error(SchCode + " Send FTP seq : " + seqnumber + " - File BRIS gagal di kirim [proses 2]");
                log.Error(SchCode + " Send FTP seq : " + seqnumber + " - IP FTP : " + ip);
                log.Error(SchCode + " Send FTP seq : " + seqnumber + " - MSG FTP : " + ex.Message);
                log.Error(SchCode + " Send FTP seq : " + seqnumber + " - MSG FTP : " + ex.InnerException);
            }
        }

        catch (Exception ex)
        {
            result = false;
            log.Error(SchCode + " Send FTP seq : " + seqnumber + " - File BRIS gagal di kirim [proses 1]");
            log.Error(SchCode + " Send FTP seq : " + seqnumber + " - IP FTP : " + ip);
            log.Error(SchCode + " Send FTP seq : " + seqnumber + " - MSG FTP : " + ex.Message);
            log.Error(SchCode + " Send FTP seq : " + seqnumber + " - MSG FTP : " + ex.InnerException);

        }
        return result;
    }
    //---------------------------------------------------------------------------//
    /* End of BRIS */
}
