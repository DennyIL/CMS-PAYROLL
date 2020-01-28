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
using BRIChannelSchedulerNew.Payroll.Helper;
using System.Xml;
using System.Xml.Serialization;
using NHibernate;
using NHibernate.Expression;


namespace BRIChannelSchedulerNew.Payroll.Helper
{
    public abstract class EmailHelperUniversal
    {
        public static string EmailTemplateForReveicer(ISession session, String senderInstitution, String BenBankName, String BenAccount, String BenAccountName, String Curr, String Amount, String Remark, String TrxTime)
        {
            String body = @"Nasabah yang terhormat,<br/>
                            Transfer dana melalui Cash Management System BRI telah berhasil dikirim.<br/>
                            <table border=0>
                                <tr>
	                                <td>Pengirim</td>
	                                <td>:</td>
	                                <td>" + senderInstitution + @"</td>
                                </tr>
                                <tr>
	                                <td>Bank Penerima</td>
	                                <td>:</td>
	                                <td>" + BenBankName + @"</td>
                                </tr>
                                <tr>
	                                <td>Rekening Penerima</td>
	                                <td>:</td>
	                                <td>" + BenAccount + @"</td>
                                </tr>
                                <tr>
	                                <td>Nama Rekening Penerima</td>
	                                <td>:</td>
	                                <td>" + BenAccountName + @"</td>
                                </tr>
                                <tr>
	                                <td>Jumlah</td>
	                                <td>:</td>
	                                <td>" + Curr + " " + Amount + @"</td>
                                </tr>
                                <tr>
	                                <td>Keterangan</td>
	                                <td>:</td>
	                                <td>" + Remark + @"</td>
                                </tr>
                                <tr>
	                                <td>Waktu Proses</td>
	                                <td>:</td>
	                                <td>" + TrxTime + @" (GMT+7)</td>
                                </tr>
                            </table>
                            <br/>
                            <font size='2'><i>
                            Note:<br/>
                            * Dokumen ini memberikan informasi bahwa BANK BRI telah mengirimkan dana ke bank tujuan. Dana akan diterima jika bank tujuan telah membuat proses settlement ke rekening Anda.<br/>
                            * Dokumen ini secara otomatis dihasilkan oleh sistem. JANGAN MERESPON EMAIL INI.<br/>
                            </i></font>
                            <br/>
                            Hormat Kami,<br/>
                            <a href='https://ibank.bri.co.id/'>Cash Management System BRI</a>
                            <br/>
                            <br/>
                            <br/>
                            ====================================================================================
                            <br/>
                            <br/>
                            <br/>
                            Dear Customer,<br/>
                            Fund transfer through the Cash Management System BRI has been successfully sent.<br/>
                            <table border=0>
                                <tr>
	                                <td>Sender</td>
	                                <td>:</td>
	                                <td>" + senderInstitution + @"</td>
                                </tr>
                                <tr>
	                                <td>Beneficiary Bank Name</td>
	                                <td>:</td>
	                                <td>" + BenBankName + @"</td>
                                </tr>
                                <tr>
	                                <td>Beneficiary Account No</td>
	                                <td>:</td>
	                                <td>" + BenAccount + @"</td>
                                </tr>
                                <tr>
	                                <td>Beneficiary Account Name</td>
	                                <td>:</td>
	                                <td>" + BenAccountName + @"</td>
                                </tr>
                                <tr>
	                                <td>Amount</td>
	                                <td>:</td>
	                                <td>" + Curr + " " + Amount + @"</td>
                                </tr>
                                <tr>
	                                <td>Remark</td>
	                                <td>:</td>
	                                <td>" + Remark + @"</td>
                                </tr>
                                <tr>
	                                <td>Process Time</td>
	                                <td>:</td>
	                                <td>" + TrxTime + @" (GMT+7)</td>
                                </tr>
                            </table>
                            <br/>
                            <font size='2'><i>
                            Note:<br/>
                            * This document provides information that BANK BRI has sent funds to the destination bank. Funds will be accepted if the destination bank has made the process of completion to your account.<br/>
                            * This document is automatically generated by system. PLEASE DO NOT RESPOND TO THIS EMAIL.<br/>
                            </i></font>
                            <br/>
                            Sincerely,<br/>
                            <a href='https://ibank.bri.co.id/'>Cash Management System BRI</a><br/>";
            return body;
        }


        public static string EmailTemplateInternalTransfertoBenef(ISession session, String nama_transaksi, String senderInstitution, String BenAccount, String BenAccountName, String Curr, String Amount, String Remark, String TrxTime)
        {
            String body = @"Nasabah yang terhormat,<br/>
                            " + nama_transaksi + @" melalui Cash Management System BRI telah berhasil dikirim.<br/>
                            <table border=0>
                                <tr>
	                                <td>Pengirim</td>
	                                <td>:</td>
	                                <td>" + senderInstitution + @"</td>
                                </tr>
                                <tr>
	                                <td>Rekening Penerima</td>
	                                <td>:</td>
	                                <td>" + BenAccount + @"</td>
                                </tr>
                                <tr>
	                                <td>Nama Rekening Penerima</td>
	                                <td>:</td>
	                                <td>" + BenAccountName + @"</td>
                                </tr>
                                <tr>
	                                <td>Jumlah</td>
	                                <td>:</td>
	                                <td>" + Curr + " " + Amount + @"</td>
                                </tr>
                                <tr>
	                                <td>Berita</td>
	                                <td>:</td>
	                                <td>" + Remark + @"</td>
                                </tr>
                                <tr>
	                                <td>Waktu Proses</td>
	                                <td>:</td>
	                                <td>" + TrxTime + @" (GMT+7)</td>
                                </tr>
                            </table>
                            <br/>
                            Hormat Kami,<br/>
                            <a href='https://ibank.bri.co.id/'>Cash Management System BRI</a>
                            <br/>
                            <br/>
                            <br/>
                            ====================================================================================
                            <br/>
                            <br/>
                            <br/>
                            Dear Customer,<br/>
                            " + nama_transaksi + @" through the Cash Management System BRI has been successfully sent.<br/>
                            <table border=0>
                                <tr>
	                                <td>Sender</td>
	                                <td>:</td>
	                                <td>" + senderInstitution + @"</td>
                                </tr>
                                <tr>
	                                <td>Beneficiary Account No</td>
	                                <td>:</td>
	                                <td>" + BenAccount + @"</td>
                                </tr>
                                <tr>
	                                <td>Beneficiary Account Name</td>
	                                <td>:</td>
	                                <td>" + BenAccountName + @"</td>
                                </tr>
                                <tr>
	                                <td>Amount</td>
	                                <td>:</td>
	                                <td>" + Curr + " " + Amount + @"</td>
                                </tr>
                                <tr>
	                                <td>Remark</td>
	                                <td>:</td>
	                                <td>" + Remark + @"</td>
                                </tr>
                                <tr>
	                                <td>Process Time</td>
	                                <td>:</td>
	                                <td>" + TrxTime + @" (GMT+7)</td>
                                </tr>
                            </table>
                            <br/>
                            Sincerely,<br/>
                            <a href='https://ibank.bri.co.id/'>Cash Management System BRI</a><br/>";
            return body;
        }


        //denny 19 Maret 2016
        public static string EmailTemplateForSender(ISession session, int idTrx, String DebAcc, String DebAccName, String BenBankName, String BenAccount, String BenAccountName, String Curr, String Amount, String Remark, String TrxTime, String Status)
        {

            string body = "Pelanggan Yth, </br>Transfer dana dengan menggunakan Cash Management System BRI telah BERHASIL dikirim.</br>";

            body += "<table border=0>";

            body += "<tr>";
            body += "<td>Transaction ID</td>";
            body += "<td>:</td>";
            body += "<td>" + idTrx.ToString() + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Akun Pengirim</td>";
            body += "<td>:</td>";
            body += "<td>" + DebAcc + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Nama Pengirim</td>";
            body += "<td>:</td>";
            body += "<td>" + DebAccName + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Bank Penerima</td>";
            body += "<td>:</td>";
            body += "<td>" + BenBankName + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Akun Penerima No</td>";
            body += "<td>:</td>";
            body += "<td>" + BenAccount + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Nama Akun Penenrima</td>";
            body += "<td>:</td>";
            body += "<td>" + BenAccountName + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Jumlah</td>";
            body += "<td>:</td>";
            body += "<td>" + Curr + " " + Amount + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Keterangan</td>";
            body += "<td>:</td>";
            body += "<td>" + Remark + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Waktu Pemrosesan</td>";
            body += "<td>:</td>";
            body += "<td>" + TrxTime + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Status</td>";
            body += "<td>:</td>";
            body += "<td>" + Status + "</td>";
            body += "</tr>";

            body += "</table>";

            body += "</br>";
            body += "Dokumen ini otomatis di kirim oleh sistem.</br>Terimakasih tidak merespon email ini.</br></br>";
            body += "Terima kasih,</br>";
            body += "<a href='https://ibank.bri.co.id/'>Cash Management System BRI</a></br><br/><br/><br/>";
            body += "===============================================================================================";
            body += "<br/><br/><br/>";
            
            body += "Dear Customer, </br>Fund transfer through the Cash Management System BRI has been successfully sent.</br>";

            body += "<table border=0>";

            body += "<tr>";
            body += "<td>Transaction ID</td>";
            body += "<td>:</td>";
            body += "<td>" + idTrx.ToString() + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Sender Account</td>";
            body += "<td>:</td>";
            body += "<td>" + DebAcc + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Sender Name</td>";
            body += "<td>:</td>";
            body += "<td>" + DebAccName + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Benefeciery Bank Name</td>";
            body += "<td>:</td>";
            body += "<td>" + BenBankName + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Benefeciery Account</td>";
            body += "<td>:</td>";
            body += "<td>" + BenAccount + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Benefeciery Account Name</td>";
            body += "<td>:</td>";
            body += "<td>" + BenAccountName + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Amount</td>";
            body += "<td>:</td>";
            body += "<td>" + Curr + " " + Amount + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Remark</td>";
            body += "<td>:</td>";
            body += "<td>" + Remark + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Time Processing</td>";
            body += "<td>:</td>";
            body += "<td>" + TrxTime + "</td>";
            body += "</tr>";

            body += "<tr>";
            body += "<td>Status</td>";
            body += "<td>:</td>";
            body += "<td>" + Status + "</td>";
            body += "</tr>";

            body += "</table>";

            body += "</br>";
            body +=  "Note:<br/>";
            body +=  "* This document provides information that BANK BRI has sent funds to the destination bank. Funds will be accepted if the destination bank has made the process of completion to your account.<br/>";
            body +=  "* This document is automatically generated by system. PLEASE DO NOT RESPOND TO THIS EMAIL.<br/>";
            body += "<a href='https://ibank.bri.co.id/'>Cash Management System BRI</a></br>";



            return body;
        }


        /* help AddEmailTransaction
             helper add data email agar di proses oleh Job
             * Input =
             * session : wajib
             * TransactionID : sunnah
             * FID : sunnah
             * ClientID : sunnah
             * Receiver : wajib
             * Subject : wajib
             * Content : wajib
             * attachment_path : sunnah
             * priority_level : wajib, lihat bawah ini
             * level kepentingan email, email2 yg harus segera diterima di set 1, agar dieksekusi lbh dulu [level = {1, 2}]
             * 
             * 
             * Output =
             * idEmailTransaction : untuk modal cek status email sudah terkirim atau tidak
             * msg : pesan jika data gagal ditambahkan
             */
        public static bool AddEmailTransaction(ISession session, int TransactionID, int FID, string FNAME, int ClientID, string Receiver, string Subject, string Content, string attachment_path, int priority_level, out int idEmailTransaction, out string msg)
        {
            bool result = true;
            msg = "";
            idEmailTransaction = 0;
            try
            {
                Email email = new Email();
                email.TransactionID = TransactionID;
                email.FID = FID;
                email.FNAME = FNAME;
                email.ClientID = ClientID;
                email.CreatedTime = DateTime.Now;
                email.LastUpdate = DateTime.Now;
                email.Receiver = Receiver.Trim();
                email.Subject = Subject.Trim();
                email.Content = Content.Trim();
                email.IsBodyHtml = true;
                email.Status = ParameterHelper.EMAIL_WAITING_SEND;
                email.ErrorDescription = "Waiting Send.";

                //save email transaction
                idEmailTransaction = (int)session.Save(email);
                session.Flush();

                msg = "SUKSES";
            }
            catch (Exception ex)
            {
                result = false;
                string eksep = "Exception on Done Sender Email :: " + ex.Message + " => " + ex.StackTrace;
                
                msg = eksep;
            }

            return result;
        }


        public static bool SendEmail(ISession session, Email email, out string msg)
        {
            bool result = true;
            msg = "";
            try
            {
                //update in process
                email.LastUpdate = DateTime.Now;
                email.Status = ParameterHelper.EMAIL_PROCESSING;
                session.Update(email);
                session.Flush();


                //Loop for each receiver
                string[] ArrReceiver = email.Receiver.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                for (int a = 0; a < ArrReceiver.Length; a++)
                {
                    MailMessage message = new MailMessage();
                    //Get Sender (Admin CMS Email)
                    BRIChannelSchedulerNew.Payroll.Pocos.Parameter p1 = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("EMAIL_ADMINISTRATOR");
                    message.From = new MailAddress(p1.Data, "CMS BRI Administrator");

                    //message.To.Add(ArrReceiver[a]);
                    message.To.Add(ArrReceiver[a]);

                    //Get Subject
                    message.Subject = email.Subject;

                    //Get Body
                    message.Body = email.Content;
                    message.IsBodyHtml = email.IsBodyHtml;

                    //Get SMPTP
                    BRIChannelSchedulerNew.Payroll.Pocos.Parameter p2 = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("EMAIL_SMTP_SERVER");
                    p1 = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("EMAIL_SMTP_PORT");
                    SmtpClient smtp = new SmtpClient(p2.Data, int.Parse(p1.Data));
                    smtp.Timeout = 180000;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(p1.Data, p2.Data);
                    smtp.EnableSsl = false;
                    try
                    {
                        smtp.Send(message);
                        email.ErrorDescription = "Send Email Success.";
                        email.Status = ParameterHelper.EMAIL_SUCCESS;
                        msg = "SUKSES";
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        string eksep = "Exception on Sender Email :: " + ex.Message + " => " + ex.StackTrace;
                        email.ErrorDescription += eksep;
                        email.Status = ParameterHelper.EMAIL_EXCEPTION;
                        //EvtLogger.Write(eksep, System.Diagnostics.EventLogEntryType.Error);
                        msg = eksep;
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                string eksep = "Exception on Sender Email :: " + ex.Message + " => " + ex.StackTrace;
                email.ErrorDescription += eksep;
                email.Status = ParameterHelper.EMAIL_EXCEPTION;
                //EvtLogger.Write(eksep, System.Diagnostics.EventLogEntryType.Error);
                msg = eksep;
            }

            //update email db
            email.LastUpdate = DateTime.Now;
            session.Update(email);
            session.Flush();

            return result;
        }

        public static string EmailTemplateIADebetKredit(string noRek1, string namaRek1, string jenisRek1, double aBal1, double hBal1, double cBal1, int statRek1,
            string noRek2, string namaRek2, string jenisRek2, double aBal2, double hBal2, double cBal2, int statRek2)
        {

            string body = "";
            if (aBal1 == 0 && aBal2 == 0) //aman ndan
                body = "IADebet/Kredit Payroll saldonya nihil";
            else
            body = @"Daily Warning IADebet/Credit,<br/>
                            ====================================================================================
                            <table border=0>
                                <tr>
	                                <td>Nomor Rekening</td>
	                                <td>:</td>
	                                <td>" + noRek1 + @"</td>
                                </tr>
                                <tr>
	                                <td>Nama Rekening</td>
	                                <td>:</td>
	                                <td>" + namaRek1 + @"</td>
                                </tr>
                                <tr>
	                                <td>Jenis Rekening</td>
	                                <td>:</td>
	                                <td>" + jenisRek1 + @"</td>
                                </tr>
                                <tr>
	                                <td>Available Balance</td>
	                                <td>:</td>
	                                <td>" + aBal1 + @"</td>
                                </tr>
                                <tr>
	                                <td>Hold Amount</td>
	                                <td>:</td>
	                                <td>" + hBal1 + @"</td>
                                </tr>
                                <tr>
	                                <td>Current Balance</td>
	                                <td>:</td>
	                                <td>" + cBal1 + @"</td>
                                </tr>
                                <tr>
	                                <td>Status Rekening</td>
	                                <td>:</td>
	                                <td>" + statRek1 + @"</td>
                                </tr>
                            </table>
                            ====================================================================================
                            <table border=0>
                                <tr>
	                                <td>Nomor Rekening</td>
	                                <td>:</td>
	                                <td>" + noRek2 + @"</td>
                                </tr>
                                <tr>
	                                <td>Nama Rekening</td>
	                                <td>:</td>
	                                <td>" + namaRek2 + @"</td>
                                </tr>
                                <tr>
	                                <td>Jenis Rekening</td>
	                                <td>:</td>
	                                <td>" + jenisRek2 + @"</td>
                                </tr>
                                <tr>
	                                <td>Available Balance</td>
	                                <td>:</td>
	                                <td>" + aBal2 + @"</td>
                                </tr>
                                <tr>
	                                <td>Hold Amount</td>
	                                <td>:</td>
	                                <td>" + hBal2 + @"</td>
                                </tr>
                                <tr>
	                                <td>Current Balance</td>
	                                <td>:</td>
	                                <td>" + cBal2 + @"</td>
                                </tr>
                                <tr>
	                                <td>Status Rekening</td>
	                                <td>:</td>
	                                <td>" + statRek2 + @"</td>
                                </tr>
                            </table>
                            <br/>
                            Mohon untuk dapat segera melakukan pengecekan ,<br/>
                            Terima Kasih";
            return body;
        }

    }
}