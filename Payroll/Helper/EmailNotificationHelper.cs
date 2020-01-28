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
    public abstract class EmailNotificationHelper
    {
        public static void AuthorizationNotification(ISession session, String email, int CID, int idTrx, String fitur, String authorityINA, String authorityEng, String DebAcc, String DebAccName, String Curr, String Amount, String BenAcc, String BenAccName, String Valdate)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.To.Add(email);
                BRIChannelSchedulerNew.Payroll.Pocos.Parameter p1 = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("EMAIL_ADMINISTRATOR");

                message.From = new MailAddress(p1.Data, "CMS BRI Administrator");
                message.Subject = "" + fitur + " Need " + authorityEng + " eMail Notification";//"Cash Management BRI - Fund Transfer";
                
                String body = "Terdapat transaksi "+ fitur + " yang membutuhkan\n";
                body += authorityINA + ", sebagai berikut :\n\n";

                body += "ID Transaksi   :   " + idTrx.ToString() + "\n";
                body += "Pengirim       :   " + DebAcc+"\n";
                body += "                   " + DebAccName + "\n";
                body += "Penerima       :   " + BenAcc +"\n";
                body += "                   " + BenAccName + "\n";
                body += "Jumlah         :   " + Amount + " " + Curr + "\n";
                if (Valdate.ToLower().Equals("immediate"))
                {
                    body += "Jadwal         :   Segera\n\n";//Segera [atau]  30/01/2013 - 07:00 WIB\n\n";
                }
                else
                {
                    body += "Jadwal         :   " + Valdate + "\n\n";//Segera [atau]  30/01/2013 - 07:00 WIB\n\n";
                }

                body += "Semoga informasi ini dapat bermanfaat bagi anda. Untuk informasi\n";
                body += "lebih lanjut, silakan menghubungi Help Desk CMS BRI di nomor 5758965/45/46/64.\n\n";

                body += "Hormat Kami,\n";
                body += "PT. Bank Rakyat Indonesia (Persero), Tbk.\n\n";

                body += "***eMail ini dihasilkan oleh komputer dan tidak perlu dijawab kembali.***\n\n";

                body += "==============================================================\n\n";

                body += "There’s " + fitur + " transaction that need\n";
                body += authorityEng + ", as follow :\n\n";

                body += "Transaction ID :   " + idTrx.ToString() + "\n";
                body += "Sender         :   " + DebAcc + "\n";
                body += "                   " + DebAccName + "\n";
                body += "Beneficiary    :   " + BenAcc + "\n";
                body += "                   " + BenAccName + "\n";
                body += "Amount         :   " + Amount + " " + Curr + "\n";
                body += "Schedule       :   " + Valdate + "\n\n";//Segera [atau]  30/01/2013 - 07:00 WIB\n\n";

                body += "Hopefully this information can be useful for you. For  further\n";
                body += "information, please contact Help Desk CMS BRI at 5758965/45/46/64.\n\n";

                body += "Best Regards,\n";
                body += "PT. Bank Rakyat Indonesia (Persero) Tbk. \n\n";

                body += "***This is a computer-generated eMail, please do not reply.***\n";

                message.Body = body;
                message.IsBodyHtml = false;
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
                }
                catch (Exception ex)
                {
                    ////EvtLogger.Write("Exception on Sending Authority Email " + CID + " (" + email + ") " + fitur + " : " + idTrx.ToString() + " :: " + ex.Message + " => " + ex.InnerException + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
                }
            }
            catch (Exception ex)
            {
                ////EvtLogger.Write("Exception on Authority Email :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public static void MassNotification(ISession session, String email, int CID, int idTrx, String fitur, int state, String descTrx, String countTrx, String amountTrx, DateTime crTime, String maker, DateTime runTime, String status)
        {
            //state=0->inProcess verify; state=1->inProc Approve; state=2->Succed; state=3->reject
            #region sourcecode lama
            //try
            //            {
            //                MailMessage message = new MailMessage();
            //                message.To.Add(email);
            //                BRIChannelSchedulerNew.Payroll.Pocos.Parameter p1 = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("EMAIL_ADMINISTRATOR");

            //                message.From = new MailAddress(p1.Data, "CMS BRI Administrator");


            //                String varIna = "";
            //                String varEng = "";
            //                String TypeSubject = "";
            //                String typeIna = "";
            //                String typeEng = "";
            //                if (state == 0 || state == 1)
            //                {
            //                    varIna = " membutuhkan ";
            //                    varEng = " need to be ";
            //                    TypeSubject = "Verify";
            //                    if (state == 0)
            //                    {
            //                        typeIna = "verifikasi";
            //                        typeEng = "verified";
            //                    }
            //                    else
            //                    {
            //                        typeIna = "pengesahan";
            //                        typeEng = "approved";
            //                    }
            //                }
            //                else
            //                {
            //                    varIna = " telah ";
            //                    varEng = " has been ";
            //                    if (state == 2)
            //                    {
            //                        TypeSubject = "Success";
            //                        typeIna = "sukses";
            //                        typeEng = "succeed";
            //                    }
            //                    else
            //                    {
            //                        TypeSubject = "Reject";
            //                        typeIna = "ditolak";
            //                        typeEng = "rejected";
            //                    }
            //                }

            //                message.Subject = fitur + " " + TypeSubject + " eMail Notification";

            //                String body = "Nasabah Yth.\nTerdapat transaksi " + fitur + " yang" + varIna + typeIna + ",\n";
            //                body += "sebagai berikut :\n\n";
            //                body += "ID Transaksi            : " + idTrx.ToString() + "\n";
            //                body += "Deskripsi File          : " + descTrx + "\n";
            //                body += "Banyaknya Transaksi     : " + countTrx + "\n";
            //                body += "Total Nilai Transaksi   : " + amountTrx + "\n";
            //                body += "Waktu Pembuatan         : " + crTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
            //                body += "Pembuat                 : " + maker + "\n";

            //                if (runTime.ToString("dd/MM/yyyy").Equals("01/01/0001"))
            //                {
            //                    body += "Jadwal                  : Segera\n";
            //                }
            //                else
            //                {
            //                    body += "Jadwal                  : " + runTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
            //                }
            //                body += "Status                  : " + status + "\n\n";

            //                body += "Semoga informasi ini dapat bermanfaat bagi anda. Untuk informasi\n";
            //                body += "lebih lanjut, silakan menghubungi Help Desk CMS BRI di nomor 5758965/45/46/64.\n\n";

            //                body += "Hormat Kami,\n";
            //                body += "PT. Bank Rakyat Indonesia (Persero), Tbk.\n\n";

            //                body += "***eMail ini dihasilkan oleh komputer dan tidak perlu dijawab kembali.***\n\n";

            //                body += "==============================================================\n\n";

            //                body += "Dear Customer,\nThere’s " + fitur + " transaction that" + varEng + typeEng + ",\n";
            //                body += "as follow :\n\n";

            //                body += "Transaction ID          : " + idTrx.ToString() + "\n";
            //                body += "File Description        : " + descTrx + "\n";
            //                body += "Number of Transactions  : " + countTrx + "\n";
            //                body += "Total Transaction Value : " + amountTrx + "\n";
            //                body += "Created Time            : " + crTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
            //                body += "Creator                 : " + maker + "\n";

            //                if (runTime.ToString("dd/MM/yyyy").Equals("01/01/0001"))
            //                {
            //                    body += "Running Time            : Immediate\n";
            //                }
            //                else
            //                {
            //                    body += "Running Time            : " + runTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
            //                }
            //                body += "Status                  : " + status + "\n\n";

            //                body += "Hopefully this information can be useful for you. For  further\n";
            //                body += "information, please contact Help Desk CMS BRI at 5758965/45/46/64.\n\n";

            //                body += "Best Regards,\n";
            //                body += "PT. Bank Rakyat Indonesia (Persero) Tbk. \n\n";

            //                body += "***This is a computer-generated eMail, please do not reply.***\n";

            //                message.Body = body;
            //                message.IsBodyHtml = false;
            //                BRIChannelSchedulerNew.Payroll.Pocos.Parameter p2 = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("EMAIL_SMTP_SERVER");
            //                p1 = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("EMAIL_SMTP_PORT");
            //                SmtpClient smtp = new SmtpClient(p2.Data, int.Parse(p1.Data));
            //                smtp.Timeout = 180000;
            //                smtp.UseDefaultCredentials = false;
            //                smtp.Credentials = new NetworkCredential(p1.Data, p2.Data);
            //                smtp.EnableSsl = false;
            //                try
            //                {
            //                    smtp.Send(message);
            //                }
            //                catch (Exception ex)
            //                {
            //                    EvtLogger.Write("Exception on Sending " + TypeSubject + " Email " + fitur + " " + CID + " (" + email + ") :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                EvtLogger.Write("Exception on Done Sender Email :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
            //            }
            #endregion


            //state=0->inProcess verify; state=1->inProc Approve; state=2->Succed; state=3->reject
            try
            {
                MailMessage message = new MailMessage();
                message.To.Add(email);
                BRIChannelSchedulerNew.Payroll.Pocos.Parameter p1 = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("EMAIL_ADMINISTRATOR");

                message.From = new MailAddress(p1.Data, "CMS BRI Administrator");


                String varIna = "";
                String varEng = "";
                String TypeSubject = "";
                String typeIna = "";
                String typeEng = "";

                //rfq add
                string countTotal = "-";
                string amountTotal = "-";
                string countSuccess = "-";
                string amountSuccess = "-";
                string countFail = "-";
                string amountFail = "-";
                //end

                if (state == 0 || state == 1)
                {
                    varIna = " membutuhkan ";
                    varEng = " need to be ";
                    TypeSubject = "Verify";
                    if (state == 0)
                    {
                        typeIna = "verifikasi";
                        typeEng = "verified";
                    }
                    else
                    {
                        typeIna = "pengesahan";
                        typeEng = "approved";
                    }
                }
                else
                {
                    varIna = " telah ";
                    varEng = " has been ";
                    if (state == 2)
                    {
                        TypeSubject = "Success";
                        typeIna = "sukses";
                        typeEng = "succeed";
                    }
                    else
                    {
                        TypeSubject = "Reject";
                        typeIna = "ditolak";
                        typeEng = "rejected";
                    }
                }

                message.Subject = fitur + " " + TypeSubject + " eMail Notification";

                String body = "Nasabah Yth.\nTerdapat transaksi " + fitur + " yang" + varIna + typeIna + ",\n";
                body += "sebagai berikut :\n\n";
                body += "ID Transaksi            : " + idTrx.ToString() + "\n";
                body += "Deskripsi File          : " + descTrx + "\n";

                //Edited by rofiq, 04072014
                if (fitur.ToUpper().Contains("PAYROLL"))
                {
                    if (state == 2)//Done Payroll Email
                    {
                        if (!string.IsNullOrEmpty(countTrx))
                        {
                            string[] total = countTrx.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (total.Length == 3)
                            {
                                //|susccess|fail|total| ==> |1:200|:|2:300|
                                //total
                                string[] pecahan = total[2].Split(':');
                                countTotal = pecahan[0];
                                amountTotal = pecahan[1];
                                //success
                                string[] pecahanSuccess = total[0].Split(':');
                                countSuccess = pecahanSuccess[0];
                                amountSuccess = pecahanSuccess[1];
                                //faild
                                string[] pecahanFail = total[1].Split(':');
                                countFail = pecahanFail[0];
                                amountFail = pecahanFail[1];
                            }
                        }

                        amountTotal = double.Parse(amountTotal).ToString("N", CultureInfo.CreateSpecificCulture("EN-us"));
                        amountSuccess = double.Parse(amountSuccess).ToString("N", CultureInfo.CreateSpecificCulture("EN-us"));
                        amountFail = double.Parse(amountFail).ToString("N", CultureInfo.CreateSpecificCulture("EN-us"));

                        body += "Total Transaksi         : " + countTotal + " Transactions (" + amountTotal + " IDR)\n";
                        body += "Total Transaksi Sukses  : " + countSuccess + " Transactions (" + amountSuccess + " IDR)\n";
                        body += "Total Transaksi Gagal   : " + countFail + " Transactions (" + amountFail + " IDR)\n";
                    }
                    else
                    {
                        amountTrx = double.Parse(amountTrx).ToString("N", CultureInfo.CreateSpecificCulture("EN-us"));
                        body += "Banyaknya Payroll       : " + countTrx + " Records.\n";
                        body += "Total Nilai Payroll     : " + amountTrx + " IDR\n";
                    }
                }
                else
                {
                    body += "Banyaknya Transaksi     : " + countTrx + "\n";
                    body += "Total Nilai Transaksi   : " + amountTrx + "\n";
                }

                if (!fitur.ToUpper().Contains("PAYROLL"))
                {
                    body += "Waktu Pembuatan         : " + crTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
                }
                //end rfq

                body += "Pembuat                 : " + maker + "\n";

                if (runTime.ToString("dd/MM/yyyy").Equals("01/01/0001"))
                {
                    body += "Jadwal                  : Segera\n";
                }
                else
                {
                    body += "Jadwal                  : " + runTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
                }
                body += "Status                  : " + status + "\n\n";

                body += "Semoga informasi ini dapat bermanfaat bagi anda. Untuk informasi\n";
                body += "lebih lanjut, silakan menghubungi Help Desk CMS BRI di nomor 5758965/45/46/64.\n\n";

                body += "Hormat Kami,\n";
                body += "PT. Bank Rakyat Indonesia (Persero), Tbk.\n\n";

                body += "***eMail ini dihasilkan oleh komputer dan tidak perlu dijawab kembali.***\n\n";

                body += "==============================================================\n\n";

                body += "Dear Customer,\nThere’s " + fitur + " transaction that" + varEng + typeEng + ",\n";
                body += "as follow :\n\n";

                body += "Transaction ID          : " + idTrx.ToString() + "\n";
                body += "File Description        : " + descTrx + "\n";

                //Edited by rofiq, 04072014
                if (fitur.ToUpper().Contains("PAYROLL"))
                {
                    if (state == 2)//Done Payroll Email
                    {
                        body += "Total Transaction       : " + countTotal + " Transactions (Rp. " + amountTotal + ")\n";
                        body += "Total Trans. Success    : " + countSuccess + " Transactions (Rp. " + amountSuccess + ")\n";
                        body += "Total Transaction Fail  : " + countFail + " Transactions (Rp. " + amountFail + ")\n";
                    }
                    else
                    {
                        body += "Number of Payroll       : " + countTrx + " Records.\n";
                        body += "Total Transaction Value : Rp. " + amountTrx + ",-\n";
                    }
                }
                else
                {
                    body += "Number of Transactions  : " + countTrx + "\n";
                    body += "Total Transaction Value : " + amountTrx + "\n";
                }

                if (!fitur.ToUpper().Contains("PAYROLL"))
                {
                    body += "Created Time            : " + crTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
                }
                //end rofiq


                body += "Creator                 : " + maker + "\n";

                if (runTime.ToString("dd/MM/yyyy").Equals("01/01/0001"))
                {
                    body += "Running Time            : Immediate\n";
                }
                else
                {
                    body += "Running Time            : " + runTime.ToString("dd/MM/yyyy HH:mm:ss") + "\n";
                }
                body += "Status                  : " + status + "\n\n";

                body += "Hopefully this information can be useful for you. For  further\n";
                body += "information, please contact Help Desk CMS BRI at 5758965/45/46/64.\n\n";

                body += "Best Regards,\n";
                body += "PT. Bank Rakyat Indonesia (Persero) Tbk. \n\n";

                body += "***This is a computer-generated eMail, please do not reply.***\n";

                message.Body = body;
                message.IsBodyHtml = false;
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
                }
                catch (Exception ex)
                {
                    //EvtLogger.Write("Exception on Sending " + TypeSubject + " Email " + fitur + " " + CID + " (" + email + ") :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
                }
            }
            catch (Exception ex)
            {
                //EvtLogger.Write("Exception on Done Sender Email :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
            }
        }

    }
}