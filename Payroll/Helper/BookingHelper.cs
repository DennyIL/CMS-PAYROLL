using System;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Web.UI.WebControls;
using BRIChannelSchedulerNew.Payroll.Pocos;
using System.Xml;
using System.Xml.Serialization;
using NHibernate;
using NHibernate.Expression;
using System.IO;
using System.Text.RegularExpressions;

namespace BRIChannelSchedulerNew.Payroll.Helper
{
    public abstract class BookingHelper
    {
        public static bool InsertBooking(ISession session, string trxid, int fid, string fitur, int clientid, 
            string debitaccount, string debitaccountname, double debitamount, string debitcurrency, 
            string creditaccount, string creditaccountname, double creditamount, string creditcurrency,
            double instAmount, string instCurrency, string dealrate, string trxremark, DateTime trxdate, 
            int booktype, string bookingoffice, out int idbooking, out string OutMsg)
        {
            bool result = true;
            idbooking = 0;
            OutMsg = "";

            //20161122 - Add SayedZul Handler duplicate data
            try
            {
                string theQuery = "select id from booking where trxid = '" + trxid + "' and fid = " + fid + " and booktype = " + booktype + " and status <> 4 and not (status = 3 and statusec = 3) limit 1;";
                string theId = "";
                string msg = "";
                if (!PayrollHelper.ExecuteQueryValue(session, theQuery, out theId, out msg))
                {
                    result = false;
                    OutMsg = "Exception on checking duplicate trx - Message: " + msg;
                    Console.WriteLine("Exception Id: " + trxid + " === " + OutMsg);
                    return result;
                }
                else
                {
                    if (!String.IsNullOrEmpty(theId))
                    {
                        result = false;
                        idbooking = int.Parse(theId);
                        OutMsg = "Transaction already exist: " + idbooking;
                        return result;
                    }
                }

                
            }
            catch (Exception ee)
            {
                result = false;
                OutMsg = "Exception on checking duplicate trx - Message: " + ee.Message + " - InnerEx: " + ee.InnerException + " - StackTr: " + ee.StackTrace;
                Console.WriteLine("Exception Id: " + trxid + " === " + OutMsg);
                return result;
            }

            try
            {
                Booking bookrcd = new Booking();
                bookrcd.TrxId = trxid.ToString();
                bookrcd.FID = fid;
                bookrcd.Fitur = fitur;

                #region set jenis transaksi

                string JenisTransaksi = "";
                fitur = fitur.ToUpper();
                if (fitur.Contains("IFT"))
                {
                    JenisTransaksi = TotalOBHelper.Overbooking;
                }
                else if (fitur.Contains("LLG") || fitur.Contains("KLIRING"))
                {
                    JenisTransaksi = TotalOBHelper.LLG;
                }
                else if (fitur.Contains("RTG"))
                {
                    JenisTransaksi = TotalOBHelper.RTGS;
                }
                else if (fitur.Contains("SWIFT") || fitur.Contains("MOR"))
                {
                    JenisTransaksi = TotalOBHelper.SWIFT;
                }
                else if (fid == 100)
                {
                    JenisTransaksi = TotalOBHelper.Overbooking;
                }
                else
                {
                    JenisTransaksi = "OTHER";
                }
                bookrcd.JenisTransaksi = JenisTransaksi;
                #endregion

                bookrcd.ClientId = clientid;
                bookrcd.DebitAccount = debitaccount;
                bookrcd.DebitAccountName = debitaccountname;
                bookrcd.DebitAmount = debitamount;
                bookrcd.DebitCurrency = debitcurrency;
                bookrcd.CreditAccount = creditaccount;
                bookrcd.CreditAccountName = creditaccountname;
                bookrcd.CreditAmount = creditamount;
                bookrcd.CreditCurrency = creditcurrency;
                bookrcd.InstAmount = instAmount;
                bookrcd.InstCurrency = instCurrency;
                //bookrcd.TrxRate = trxrate;
                bookrcd.DealRate = dealrate;
                bookrcd.TrxDate = trxdate;
                bookrcd.BookType = booktype;
                bookrcd.TrxRemark = trxremark;
                if (booktype == ParameterHelper.BOOKTYPE_FEE)
                    bookrcd.TrxRemark = "Fee " + bookrcd.Fitur + " BRI CMS " + bookrcd.TrxId;
                bookrcd.BookingOffice = bookingoffice;

                bookrcd.Status = ParameterHelper.SCH_WAITINGBOOK;
                bookrcd.Description = ParameterHelper.BOOKINGDESC_WAITINGBOOKING;
                bookrcd.CreationDate = DateTime.Now;
                bookrcd.LastUpdate = DateTime.Now;

                session.Save(bookrcd);
                idbooking = bookrcd.Id;
                session.Flush();
                result = true;

            }
            catch (Exception e)
            {
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }

        public static bool InsertBookingNew(ISession session, string trxid, int fid, string fitur, int clientid,
           string debitaccount, string debitaccountname, string debitcurr,
           string creditaccount, string creditaccountname, string creditcurr,
           double instAmount, string instCurrency, string dealrate, string trxremark, string trxremark2, DateTime trxdate,
           int booktype, string bookingoffice, double feeBen, out int idbooking, out string OutMsg)
        {
            bool result = true;
            idbooking = 0;
            OutMsg = "";

            //20161122 - Add SayedZul Handler duplicate data
            try
            {
                string theQuery = "select id from booking where trxid = '" + trxid + "' and fid = " + fid + " and booktype = " + booktype + " and status <> 4 and not (status = 3 and statusec = 3) limit 1;";
                string theId = "";
                string msg = "";
                if (!PayrollHelper.ExecuteQueryValue(session, theQuery, out theId, out msg))
                {
                    result = false;
                    OutMsg = "Exception on checking duplicate trx - Message: " + msg;
                    Console.WriteLine("Exception Id: " + trxid + " === " + OutMsg);
                    return result;
                }
                else
                {
                    if (!String.IsNullOrEmpty(theId))
                    {
                        result = false;
                        idbooking = int.Parse(theId);
                        OutMsg = "Transaction already exist: " + idbooking;
                        return result;
                    }
                }
            }
            catch (Exception ee)
            {
                result = false;
                OutMsg = "Exception on checking duplicate trx - Message: " + ee.Message + " - InnerEx: " + ee.InnerException + " - StackTr: " + ee.StackTrace;
                Console.WriteLine("Exception Id: " + trxid + " === " + OutMsg);
                return result;
            }

            try
            {
                Booking bookrcd = new Booking();
                bookrcd.TrxId = trxid.ToString();
                bookrcd.FID = fid;
                bookrcd.Fitur = fitur;

                #region set jenis transaksi

                string JenisTransaksi = "";
                fitur = fitur.ToUpper();
                if (fitur.Equals("IFT"))
                {
                    JenisTransaksi = TotalOBHelper.Overbooking;
                }
                else if (fitur.Contains("LLG") || fitur.Contains("KLIRING"))
                {
                    JenisTransaksi = TotalOBHelper.LLG;
                }
                else if (fitur.Contains("RTG"))
                {
                    JenisTransaksi = TotalOBHelper.RTGS;
                }
                else if (fitur.Contains("SWIFT") || fitur.Contains("MOR"))
                {
                    JenisTransaksi = TotalOBHelper.SWIFT;
                }
                else if (fitur.Contains("PAYROLL"))
                {
                    string temp = trxremark;
                    string[] jenisTrx = temp.Trim().Split(' ');

                    if (jenisTrx.Length > 2)
                    {
                        if (jenisTrx[2].Equals("LLG"))
                        {
                            JenisTransaksi = TotalOBHelper.LLG;
                        }
                        else
                        {
                            JenisTransaksi = TotalOBHelper.RTGS;
                        }
                    }
                }
                else
                {
                    JenisTransaksi = "OTHER";
                }

                if (fid == 80 || fid == 85)
                    JenisTransaksi = TotalOBHelper.Overbooking;
                bookrcd.JenisTransaksi = JenisTransaksi;

                #endregion

                bookrcd.ClientId = clientid;
                bookrcd.DebitAccount = debitaccount;
                bookrcd.DebitAccountName = debitaccountname;
                bookrcd.CreditAccount = creditaccount;
                bookrcd.CreditAccountName = creditaccountname;
                bookrcd.InstAmount = instAmount;
                bookrcd.InstCurrency = instCurrency;
                bookrcd.DealRate = dealrate;
                bookrcd.TrxRemark = trxremark;
                bookrcd.TrxRemark2 = trxremark2;
                bookrcd.TrxDate = trxdate;
                bookrcd.BookType = booktype;
                bookrcd.BookingOffice = bookingoffice;

                bookrcd.Status = ParameterHelper.SCH_WAITINGBOOK;
                bookrcd.Description = ParameterHelper.BOOKINGDESC_WAITINGBOOKING;
                bookrcd.CreationDate = DateTime.Now;
                bookrcd.LastUpdate = DateTime.Now;
                bookrcd.FeeBen = Convert.ToInt32(feeBen);

                /* Denny IL - 27 Feb 17
                 * Add to get Currency from Account Number
                 */
                //bookrcd.CreditCurrency = GetCurrency(creditaccount);//skenariuo gagal insert
                bookrcd.CreditCurrency = creditcurr;
                bookrcd.DebitCurrency = SchHelper._Currency(debitaccount);


                session.Save(bookrcd);
                idbooking = bookrcd.Id;
                session.Flush();
                result = true;

            }
            catch (Exception e)
            {
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }


        public static bool StatusBooking(ISession session, Int32 idbooking, out Int32 status, out Double OutDebitAmount, out Double OutCreditAmount, out string OutMsg)
        {
            bool result = true;
            OutDebitAmount = 0;
            OutCreditAmount = 0;
            OutMsg = "";

            status = ParameterHelper.SCH_WAITINGBOOK;

            try
            {
                Booking bookstat = session.Load<Booking>(idbooking);
                
                if (bookstat.Status == ParameterHelper.SCH_BOOKSUCCESS)
                {
                    OutDebitAmount = bookstat.DebitAmount;
                    OutCreditAmount = bookstat.CreditAmount;
                    status = bookstat.Status;
                    OutMsg = bookstat.Description;
                    result = true;
                }
                else//selain sukses
                {
                    status = bookstat.Status;
                    OutMsg = bookstat.Description;
                    result = true;
                }
            }
            catch (Exception e)
            {
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }

        public static bool StatusBookingNew(ISession session, Int32 idbooking, out Int32 status, out Double OutDebitAmount, out Double OutCreditAmount, out string OutMsg, out string OutTrxRate, out string OutTellerID, out string OutJournalSeq)
        {
            bool result = true;
            OutDebitAmount = 0;
            OutCreditAmount = 0;
            OutMsg = "";
            OutTrxRate = "";
            status = ParameterHelper.SCH_WAITINGBOOK;
            OutTellerID = "";
            OutJournalSeq = "";
            try
            {
                Booking bookstat = session.Load<Booking>(idbooking);
                if (bookstat.Status != ParameterHelper.SCH_WAITINGBOOK)
                {
                    if (bookstat.Status == ParameterHelper.SCH_BOOKSUCCESS)
                    {
                        OutDebitAmount = bookstat.DebitAmount;
                        OutCreditAmount = bookstat.CreditAmount;
                        status = bookstat.Status;
                        OutMsg = bookstat.Description;
                        OutTrxRate = bookstat.TrxRate;
                        OutTellerID = bookstat.TellerId;
                        OutJournalSeq = bookstat.JournalSeq.ToString();
                        result = true;
                    }
                    else if (bookstat.Status == ParameterHelper.SCH_BOOKFAILED)
                    {
                        status = bookstat.Status;
                        OutMsg = bookstat.Description;
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception e)
            {
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }


        /*
         * Status
         * [REJECT] : tidak boleh di EC, tidak memenuhi S/K
         */
        public static bool ECRequest(ISession session, Int32 idbooking, out string OutMsg)
        {
            bool result = true;
            OutMsg = "";

            try
            {
                Booking bookstat = session.Load<Booking>(idbooking);
                //cek sudah sukses terbuku
                if (bookstat.Status == ParameterHelper.SCH_BOOKSUCCESS)
                {
                    //cek tanggal buku apakah today
                    if (bookstat.TrxDate.Date == DateTime.Now.Date)
                    {
                        bookstat.StatusEC = ParameterHelper.SCH_ECWAITING;//status 1
                        bookstat.LastUpdate = DateTime.Now;
                        OutMsg = "[SUKSES]";
                        session.Update(bookstat);

                        AddLog(session, bookstat, "Request EC");
                    }
                    else
                    {
                        OutMsg = "[REJECT]Transaksi beda hari tidak dapat di EC.";
                    }
                }
                else
                {
                    OutMsg = "[REJECT]Transaksi belum sukses tidak dapat di EC.";
                }
            }
            catch (Exception e)
            {
                OutMsg = "[EXCEPTION] " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }



        //add Log Function
        private static void AddLog(ISession session, Booking o, string message)
        {
            string LogTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " ";

            o.Log += LogTime + message + "|";
            session.Update(o);
            session.Flush();
        }

    }
}
