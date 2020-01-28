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
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Data;
using System.Threading;
using System.Net;
using System.Collections.Specialized;
using System.Net.Mail;
using BRIChannelSchedulerNew.Payroll.Helper;

namespace BRIChannelSchedulerNew.Payroll.SharedFolder
{
    class UploadFileResponse : IStatefulJob
    {
        private static Configuration cfg;
        private static ISessionFactory factory;
        private static String SchCode = " Response_SharedFolder ";
        private static String schInfo = " " + DateTime.Now + "-";

        #region Innitiate variable
        String url = "";
        String uid = "";
        String pass = "";
        String pid = "0";
        String maker = "";
        String localFile = "";
        String Email = "";
        int kodeFormatFile = 0;
        String Ext = "";
        int recordGagal = 0;
        int recordAck = 0;
        int recordNAPR = 0;
        int totalRecord = 0;
        int compareTime = 0;
        #endregion Innitiate

        void IJob.Execute(JobExecutionContext context)
        {
            cfg = new Configuration();
            cfg.AddAssembly("BRIChannelSchedulerNew");
            ILog log = LogManager.GetLogger(typeof(UploadFileResponse));
            Console.WriteLine(schInfo + SchCode + "STARTING SCHEDULLER COPY FILE RESPONSE PAYROLL TO SHARED FOLDER ==");

            try
            {

                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }
                ISession session = factory.OpenSession();
                Parameter paramReport = session.Load<Parameter>("FOLDER_PAYROLL_OUTPUT");
                String pathReport = paramReport.Data;


                Parameter paramClientId = session.Load<Parameter>("CLIENT_SHARED_FOLDER_PAYROLL");
                String[] CId = paramClientId.Data.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                IList<ClientMatrix> cm = session.CreateCriteria(typeof(ClientMatrix))
                    .Add(Expression.In("Id", CId))
                    .List<ClientMatrix>();

                foreach (ClientMatrix cm1 in cm)
                {

                    #region ger URL and kawan2
                    log.Info(schInfo + SchCode + "ClientID : " + cm1.Id + " - Start Checking URL");
                    AuthorityHelper ah = new AuthorityHelper(cm1.Matrix);
                    url = ah.GetTransactionURL(100); //log.Info(url);
                    uid = ah.GetTransactionUID(100);
                    pass = ah.GetTransactionPASS(100);
                    pid = ah.GetTransactionMAKER(100); //log.Info(maker);
                    Email = ah.GetTransactionEmail(100);


                    //get input url
                    if (url.Contains("|"))
                    {
                        string[] url_temp = url.Split('|');
                        url = url_temp[1];
                    }

                    IList<User> us = session.CreateCriteria(typeof(User))
                        .Add(Expression.Eq("Id", Convert.ToInt32(pid)))
                        .List<User>();
                    foreach (User u in us)
                    {
                        maker = u.Handle;
                    }
                    log.Info(schInfo + SchCode + "ClientID : " + cm1.Id + " URL found : " + url + " for user : " + maker);

                    #endregion end of get URL dkk


                    //NACK FILE
                    IList<TrxPayroll> ohno = session.CreateCriteria(typeof(TrxPayroll))
                           .Add(Expression.Eq("ClientID", cm1.Id))
                           .Add(Expression.Eq("SharedFolderStatus", "YES|"))
                           .Add(Expression.Like("FileDescription", maker, MatchMode.Anywhere))
                           .Add(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT))
                           .List<TrxPayroll>();

                    if (ohno.Count > 0)
                    {
                        #region Inquiry NACK
                        log.Info(schInfo + SchCode + "Client Id = " + cm1.Id + ". Total File Error = " + ohno.Count.ToString());

                        foreach (TrxPayroll ppp in ohno)
                        {
                            compareTime = DateTime.Compare(ppp.LastUpdate.AddMinutes(5), DateTime.Now);

                            if (compareTime < 0) //lASTUPDATE + 5 < NOW
                            {

                                FileInfo fi = new FileInfo(ppp.FilePath);
                                string filename_error = fi.Name;
                                localFile = pathReport + filename_error.Substring(0, filename_error.Length - 4) + ".nack";
                                TextWriter tw1 = new StreamWriter(pathReport + filename_error.Substring(0, filename_error.Length - 4) + ".nack");


                                //generate NACK
                                int counter = 0;
                                IList<TrxPayrollDetail> listPayDetail = ppp.TrxPayrollDetail;

                                foreach (TrxPayrollDetail detail in listPayDetail)
                                {
                                    string a = "";
                                    string text_status = "";

                                    //handling Exception (,) - Denny
                                    if (detail.Status == ParameterHelper.TRXSTATUS_SUCCESS)
                                    {
                                        if (text_status.Contains(","))
                                        {
                                            text_status = "SUCCESS - " + detail.Description.Replace(",", " ");
                                        }
                                        else
                                        {
                                            text_status = "SUCCESS - " + detail.Description;
                                        }

                                    }
                                    else
                                    {
                                        if (text_status.Contains(","))
                                        {
                                            text_status = "REJECTED - " + detail.Description.Replace(",", " ");
                                        }
                                        else
                                        {
                                            text_status = "REJECTED - " + detail.Description;
                                        }
                                    } 

                                    if (counter == 0)
                                    {
                                        a = "NO,NAMA,ACCOUNT,AMOUNT,EMAIL,CUSTOMERREFF,STATUS";
                                        tw1.WriteLine(a);
                                    }

                                    int no_urut = counter + 1;
                                    a = no_urut.ToString() + "," + detail.Name + "," + detail.Account + "," + detail.Amount + "," + detail.Email + "," + detail.CustomerReff + "," + text_status;
                                    tw1.WriteLine(a);

                                    counter++;
                                }
                                string[] batch_desc = ppp.Description.Split('|');//get batch desc
                                tw1.WriteLine(batch_desc[0]);
                                tw1.Close();
                                log.Info(schInfo + SchCode + "Client Id = " + cm1.Id + " File name : " + ppp.FileName + "Generate NACK Success");
                                Ext = ".nack";
                                string outUpload = "";
                                if (UploadToSharedFolder("3", filename_error.Substring(0, filename_error.Length - 4) + ".nack", pathReport, log, session, cm1, ppp.Id, SchCode, schInfo, url, uid, pass, localFile, ppp.SeqNumber, out outUpload))
                                {
                                    ppp.SharedFolderStatus += "1:NOT|";//a.k.a NACK
                                }
                                else
                                {
                                    ppp.SharedFolderStatus += "Exception on NACK||";
                                    ppp.ErrorDescription = "Exception on NACK||" + outUpload;
                                }
                                //update database
                                session.Update(ppp);
                                session.Flush();
                            }
                            else
                            {
                                log.Info(schInfo + SchCode + "ClientID : " + cm1.Id + " Waiting DateTime to Retry NACK File (5 minutes)");
                            }

                        }
                        #endregion
                    }
                    else
                    {

                        #region Inquiry Exception NACK
                        IList<TrxPayroll> ohnoex = session.CreateCriteria(typeof(TrxPayroll))
                           .Add(Expression.Eq("ClientID", cm1.Id))
                           .Add(Expression.Eq("SharedFolderStatus", "YES|Exception on NACK||"))
                           .Add(Expression.Like("FileDescription", maker, MatchMode.Anywhere))
                           .Add(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT))
                           .List<TrxPayroll>();
                        if (ohnoex.Count > 0)
                        {
                            //Update status to retry sent response NACK
                            foreach (TrxPayroll ohnoex2 in ohnoex)
                            {
                                ohnoex2.SharedFolderStatus = "YES|";
                                session.Update(ohnoex2);
                                log.Info(schInfo + SchCode + "ClientID  = " + cm1.Id + " Update Exception NACK to Retry again, SUCCESS ");
                            }
                        }
                        #endregion end of Inquiry Exception NACK
                        else
                        {
                            Console.WriteLine(schInfo + SchCode + "ClientID  = " + cm1.Id + " Tidak ada data untuk NACK file ");
                        }
                    }



                    //ACK FILE
                    IList<TrxPayroll> ohyes = session.CreateCriteria(typeof(TrxPayroll))
                           .Add(Expression.Eq("ClientID", cm1.Id))
                           .Add(Expression.Eq("SharedFolderStatus", "YES|"))
                           .Add(Expression.Like("FileDescription", maker, MatchMode.Anywhere))
                           .Add(Expression.Eq("Status", ParameterHelper.TRXSTATUS_COMPLETE))
                           .List<TrxPayroll>();
                    if (ohyes.Count > 0)
                    {
                        #region Inquiry ACk
                        log.Info(schInfo + SchCode + "Client Id = " + cm1.Id + ". Total File Sukses = " + ohyes.Count.ToString());

                        foreach (TrxPayroll ppp in ohyes)
                        {
                            compareTime = DateTime.Compare(ppp.LastUpdate.AddMinutes(5), DateTime.Now);

                            if (compareTime < 0) //lASTUPDATE + 5 < NOW
                            {

                                FileInfo fi = new FileInfo(ppp.FilePath);
                                string filename_error = fi.Name;
                                TextWriter tw1 = new StreamWriter(pathReport + filename_error.Substring(0, filename_error.Length - 4) + ".ack");
                                localFile = pathReport + filename_error.Substring(0, filename_error.Length - 4) + ".ack";


                                int counter = 0;
                                IList<TrxPayrollDetail> listPayDetail = ppp.TrxPayrollDetail;
                                totalRecord = listPayDetail.Count();

                                foreach (TrxPayrollDetail detail in listPayDetail)
                                {
                                    string a = "";
                                    string text_status = "";

                                    //handling Exception (,) - Denny
                                    if (detail.Status == ParameterHelper.TRXSTATUS_SUCCESS)
                                    {
                                        if (text_status.Contains(","))
                                        {
                                            text_status = "SUCCESS - " + detail.Description.Replace(",", " ");
                                            recordAck++;
                                        }
                                        else
                                        {
                                            text_status = "SUCCESS - " + detail.Description;
                                            recordAck++;
                                        }

                                    }
                                    else
                                    {
                                        if (text_status.Contains(","))
                                        {
                                            text_status = "REJECTED - " + detail.Description.Replace(",", " ");
                                            recordNAPR++;
                                        }
                                        else
                                        {
                                            text_status = "REJECTED - " + detail.Description;
                                            recordNAPR++;
                                        }
                                    } 


                                    if (counter == 0)
                                    {
                                        a = "NO,NAMA,ACCOUNT,AMOUNT,EMAIL,CUSTOMERREFF,STATUS";
                                        tw1.WriteLine(a);
                                    }

                                    int no_urut = counter + 1;
                                    a = no_urut.ToString() + "," + detail.Name + "," + detail.Account + "," + detail.Amount + "," + detail.Email + "," + detail.CustomerReff + "," + text_status;
                                    tw1.WriteLine(a);
                                    counter++;
                                }
                                tw1.Close();
                                log.Info(schInfo + SchCode + "ClientID  = " + cm1.Id + " File name : " + ppp.FileName + "Generate ACK Success");
                                Ext = ".ack";
                                string outUpload = "";
                                if (UploadToSharedFolder("", filename_error.Substring(0, filename_error.Length - 4) + ".ack", pathReport, log, session, cm1, ppp.Id, SchCode, schInfo, url, uid, pass, localFile, ppp.SeqNumber, out outUpload))
                                {
                                    ppp.SharedFolderStatus += "1:ACK|";//a.k.a NACK
                                }
                                else
                                {
                                    ppp.SharedFolderStatus += "Exception on ACK||";
                                    ppp.ErrorDescription = "Exception on ACK||" + outUpload;
                                }
                                //update database
                                session.Update(ppp);
                                session.Flush();
                            }
                            else
                            {
                                log.Info(schInfo + SchCode + "ClientID : " + cm1.Id + " Waiting DateTime to Retry ACK File (5 minutes)");
                            }
                        }

                    }

                        #endregion Inquiry ACK

                    else
                    {
                        #region Inquiry Exception ACK
                        IList<TrxPayroll> ohyesex = session.CreateCriteria(typeof(TrxPayroll))
                           .Add(Expression.Eq("ClientID", cm1.Id))
                           .Add(Expression.Eq("SharedFolderStatus", "YES|Exception on ACK||"))
                           .Add(Expression.Like("FileDescription", maker, MatchMode.Anywhere))
                           .Add(Expression.Eq("Status", ParameterHelper.TRXSTATUS_COMPLETE))
                           .List<TrxPayroll>();
                        if (ohyesex.Count > 0)
                        {
                            //Update status to retry sent response ACK
                            foreach (TrxPayroll ohyesex2 in ohyesex)
                            {
                                ohyesex2.SharedFolderStatus = "YES|";
                                session.Update(ohyesex2);
                                log.Info(schInfo + SchCode + "ClientID  = " + cm1.Id + " Update Exception ACk to Retry again, SUCCESS ");
                            }
                        }
                        #endregion end of Inquiry Exception ACK

                        else
                        {
                            Console.WriteLine(schInfo + SchCode + "ClientID  = " + cm1.Id + " Tidak ada data untuk ACK file ");
                        }
                    }
                }

                /* Absensi SCHManager - Denny 20190207 
             * Absen dengan Message : RUNNING
             * SCHCODE harus sama dg SCHCODE di DB, sbg key
             * 
             * Ketika absen UPDATE data dengan cek lastRun VS Jeda:
             * - Update schlast run = Date now
             * - Last update = Date Now
             * - Set message 
             * 
             * Ketika autorestart, 
             * - melakukan rstart di semua job dalam 1 server
             * - Cek semua job support autorestart / tdk, jika tdk keluarkan message. 
             */
                #region Absensi job 1
                try
                {
                    Boolean absenSch = false;
                    String outMsgAbsen = "Belum dilakukan absen";
                    absenSch = SchManagerHelper.AbsensiSch(session, SchCode, "RUNNING", out outMsgAbsen);
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Absensi " + " :: " + outMsgAbsen);
                }
                catch (Exception ex)
                {
                    //Cetak log kalo gagal Absen
                    log.Error(SchCode + " === Gagal Absen Exception : " + ex.Message);
                    log.Error(SchCode + " === Gagal Absen Inner Exception : " + ex.InnerException);
                    log.Error(SchCode + " === Gagal Absen Stack Trace : " + ex.StackTrace);
                }

                #endregion Absensi job 1

            }
            catch (Exception ex)
            {
                log.Error(ex.Message + " = " + ex.InnerException + " = " + ex.StackTrace);
                factory.Close();
                factory.Dispose();
                factory = null;
                cfg = null;
                GC.Collect();
            }
            Console.WriteLine(schInfo + SchCode + "END OF SCHEDULLER FILE RESPONSE PAYROLL SHARED FOLDER ==");
        }


        private Boolean UploadToSharedFolder(String errorLevel, String filename, String pathReport, ILog log, ISession session, ClientMatrix cm1, String pid, string schcode, string schinfo, string url, string uid, string psw, string localfile, int idParent, out string outUpload)
        {
            outUpload = "";
            bool result = true;
            try
            {
                WebClient wcUpload = new WebClient();
                if (filename != "")
                {
                    String filepath = Path.GetFullPath(pathReport + filename);


                    if (url.ToUpper().StartsWith("HTTP"))
                    {
                        #region HTTPS
                        log.Info("== Ready send '===" + filepath + "===' to shared folder ==");

                        wcUpload.Credentials = new NetworkCredential(uid, pass);

                        //proxy
                        Parameter prmProxServ = session.Load<Parameter>("PAYROLL_PROXY_SERVER");
                        String proxyServer = prmProxServ.Data;

                        Parameter prmProxPort = session.Load<Parameter>("PAYROLL_PROXY_PORT");
                        int proxyPort = int.Parse(prmProxPort.Data);

                        WebProxy myproxy = new WebProxy(proxyServer, proxyPort);
                        myproxy.BypassProxyOnLocal = false;
                        wcUpload.Proxy = myproxy;

                        //allow all certificate, not secure bro
                        ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                        wcUpload.UploadFile(url + filename.Substring(0, filename.Length - 4).Replace(maker + "_", "") + Ext, "PUT", filepath);
                        log.Info("Upload Success");
                        #endregion HTTPS
                    }
                    else if (url.ToUpper().StartsWith("FTP"))
                    {
                        #region FTP
                        Console.WriteLine(SchCode + " = (ClientID " + cm1.Id + ") File Name : " + filename + " Upload FTP - Start");
                        try
                        {
                            if (UploadDataFTP(url, uid, pass, filename, localFile, log))
                            {
                                log.Info(SchCode + " = (ClientID " + cm1.Id + ") File Name : " + filename + " Upload FTP NACK/ACK File-Success");
                                result = true;
                            }
                            else
                            {
                                log.Error(SchCode + " = (ClientID " + cm1.Id + ") File Name : " + filename + " Upload FTP NACK/ACK File-Gagal-Retry 5 menit");
                                result = false;
                            }
                        }
                        catch (Exception ess)
                        {
                            log.Error(SchCode + " = (ClientID " + cm1.Id + ") File Name : " + filename + " exzception on : " + ess);
                            result = false;
                        }
                        #endregion FTP
                    }
                    else
                    {
                        log.Info(SchCode + " = (ClientID " + cm1.Id + ") File Name : KOSONG");
                        result = false;
                    }

                    #region email notification
                    //send email for nack transactions
                    if (!string.IsNullOrEmpty(Ext))
                    {
                        if (!string.IsNullOrEmpty(Email))
                        {
                            log.Info("=== Start generate email ===");
                            MailMessage message = new MailMessage();
                            String[] EmailRecipients = Email.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string to in EmailRecipients)
                            {
                                message.To.Add(to);
                            }

                            Parameter prm = session.Load<Parameter>("EMAIL_ADMINISTRATOR");
                            String prmEmailAdmin = prm.Data;
                            message.From = new MailAddress(prmEmailAdmin, "Administrator CMS BRI");
                            message.Subject = "Payroll Report Notification - Cash Management System BRI";


                            String body = "";
                            if (result)//sukses upload
                            {
                                #region Sukses upload kirim email ACK
                                //CSS Table
                                body += "<style>table {border-collapse: collapse;}table, td, th {border: 1px solid black; font-size: 10px}";
                                body += "td {padding: 5px;}th {background-color: grey;}</style>";

                                string status_trx = "";
                                string status_trx_en = "";
                                string text_jumlah_record = "";
                                string text_jumlah_record_en = "";
                                if (Ext.Equals(".nack"))
                                {
                                    status_trx = "gagal";
                                    status_trx_en = "failed";
                                    text_jumlah_record = "Jumlah record yang " + status_trx.ToUpper() + " = " + recordGagal.ToString() + " dari TOTAL record = " + totalRecord.ToString() + "<br /><br />";
                                    text_jumlah_record_en = "There are " + status_trx_en.ToUpper() + " transactions = " + recordGagal.ToString() + " from total transactions = " + totalRecord.ToString() + "<br /><br />";
                                }
                                else if (Ext.Equals(".ack"))
                                {
                                    status_trx = "berhasil";
                                    status_trx_en = "success";
                                    text_jumlah_record = "Jumlah record yang " + status_trx.ToUpper() + " = " + recordAck.ToString() + " dari TOTAL record = " + totalRecord.ToString() + "<br /><br />";
                                    text_jumlah_record_en = "There are " + status_trx_en.ToUpper() + " transactions = " + recordAck.ToString() + " from total transactions = " + totalRecord.ToString() + "<br /><br />";
                                }
                                else if (Ext.Equals(".napr"))
                                {
                                    status_trx = "tidak berhasil";
                                    status_trx_en = "not success";
                                    text_jumlah_record = "Jumlah record yang " + status_trx.ToUpper() + " = " + recordNAPR.ToString() + " dari TOTAL record = " + totalRecord.ToString() + "<br /><br />";
                                    text_jumlah_record_en = "There are " + status_trx_en.ToUpper() + " transactions = " + recordNAPR.ToString() + " from total transactions = " + totalRecord.ToString() + "<br /><br />";
                                }

                                body += "Terdapat transaksi yang " + status_trx + ".<br />";
                                body += "Dengan nama file " + filename.Substring(0, filename.Length - 4) + Ext + "<br />";
                                string bodytable = "";
                                if (!errorLevel.Equals("3"))
                                //if (1 == 2)
                                {
                                    body += text_jumlah_record;

                                    body += "<table border=1>";

                                    #region read file response to create table content
                                    FileInfo file = new FileInfo(filepath);
                                    FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                                    StreamReader sr = new StreamReader(fs);
                                    while (sr.Peek() > 0)
                                    {
                                        bodytable += "<tr>";
                                        string isi_table = sr.ReadLine();
                                        if (string.IsNullOrEmpty(isi_table)) break;
                                        string[] data_column = isi_table.Split(',');
                                        for (int xx = 0; xx < data_column.Length; xx++)
                                        {
                                            log.Info("Data kolumn ke-" + xx + data_column[xx]);
                                            if (string.IsNullOrEmpty(data_column[xx]))
                                            {
                                                bodytable += "<td>&nbsp;</td>";
                                            }
                                            else bodytable += "<td>" + data_column[xx] + "</td>";
                                        }
                                        bodytable += "</tr>";
                                    }
                                    body += bodytable;
                                    #endregion

                                    body += "</table><br/ >";
                                }
                                else body += "<br/ ><p style:'font-size:10px;'>***File tidak valid, file tidak dapat dipratinjau***</p><br /><br />";
                                body += "Semoga informasi ini dapat bermanfaat bagi anda. Untuk informasi lebih <br />";
                                body += "lanjut, silakan menghubungi Help Desk CMS BRI di nomor 5758964/65. <br /><br />";
                                body += "Hormat Kami,<br />";
                                body += "PT. Bank Rakyat Indonesia (Persero), Tbk. <br /><br />";
                                body += "***Email ini dihasilkan oleh komputer dan tidak perlu dijawab kembali.*** <br /><br />";
                                body += "======================================================== <br />";
                                body += "======================================================== <br /><br />";

                                body += "Transaction was " + status_trx_en + ".<br />";
                                body += "With file name " + filename.Substring(0, filename.Length - 4) + Ext + "<br />";
                                if (!errorLevel.Equals("3"))
                                {
                                    body += text_jumlah_record_en;

                                    body += "<table border=1>";

                                    #region header table
                                    body += "<tr>";
                                    body += "<th>No</th>";
                                    body += "<th>Name</th>";
                                    body += "<th>Account</th>";
                                    body += "<th>Amount</th>";
                                    body += "<th>Email</th>";
                                    body += "<th>Customer Reff</th>";
                                    body += "<th>Status</th>";
                                    body += "</tr>";
                                    #endregion

                                    //body table
                                    body += bodytable;

                                    body += "</table><br />";
                                }
                                else body += "<br/ ><p style:'font-size:10px;'>***File not valid, file can't be previewed***</p><br /><br />";

                                body += "Hopefully this information can be useful for you. For  further information,<br />";
                                body += "please contact Help Desk CMS BRI at 5758964/65. <br /><br />";
                                body += "Best Regards, <br />";
                                body += "PT. Bank Rakyat Indonesia (Persero) Tbk. <br /><br />";
                                body += "This is a computer-generated email, please do not reply. <br />";
                                #endregion kirim aemail jika sukses FTP
                            }
                            else
                            { 
                                body = "WARNING - Problem koneksi FTP ke : " + url + "<br />";
                                body += "Terdapat file transaksi Payroll : +" + filename + "<br />";
                                body += "Dalam 5 menit kami akan mengirim file FTP kembali. <br /><br /><br />";
                                body += "Semoga informasi ini dapat bermanfaat bagi anda. Untuk informasi lebih <br />";
                                body += "lanjut, silakan menghubungi Help Desk CMS BRI di nomor 5758964/65. <br /><br />";
                                body += "Hormat Kami,<br />";
                                body += "PT. Bank Rakyat Indonesia (Persero), Tbk. <br /><br />";
                                body += "***Email ini dihasilkan oleh komputer dan tidak perlu dijawab kembali.*** <br /><br />";
                                body += "======================================================== <br />";
                                body += "======================================================== <br /><br />";
                            }
                            #region New send email use table
                            /*int idEmail = 0;
                            string outMsg = "";
                            if (EmailHelperUniversal.AddEmailTransaction(session, idParent, 100, filename, cm1.Id, Email, "Notifikasi Payroll Host to Host - Cash Management System BRI", body, filepath, 1, out idEmail, out outMsg))
                            {
                                //sukses kirim email
                                log.Info(SchCode + " = (ClientID " + cm1.Id + ") File Name : " + filename + "Sukses Kirim Email-ID Email : " + outMsg);
                            }
                            else
                            { 
                                //gagal kirim email
                                log.Info(SchCode + " = (ClientID " + cm1.Id + ") File Name : " + filename + "Gagal Kirim Email-ID Email : " + outMsg);

                            }*/
                            #endregion 

                            //Old send email
                            #region email
                            
                            message.Body = body;
                            message.IsBodyHtml = true;
                            Parameter prm1 = session.Load<Parameter>("EMAIL_SMTP_SERVER");
                            String prmServer = prm1.Data;
                            Parameter prm2 = session.Load<Parameter>("EMAIL_SMTP_PORT");
                            Int32 prmPort = Convert.ToInt32(prm2.Data);
                            SmtpClient smtp = new SmtpClient(prmServer, prmPort);//(ParameterHelper.GetString("EMAIL_SMTP_SERVER"), ParameterHelper.GetInteger("EMAIL_SMTP_PORT"));
                            smtp.Timeout = 180000;
                            smtp.UseDefaultCredentials = false;
                            Parameter prm3 = session.Load<Parameter>("EMAIL_SMTP_USER");
                            String prmUser = prm3.Data;
                            Parameter prm4 = session.Load<Parameter>("EMAIL_SMTP_PASSWORD");
                            String prmPass = prm4.Data;
                            smtp.Credentials = new NetworkCredential(prmUser, prmPass);//(ParameterHelper.GetString("EMAIL_SMTP_USER"), ParameterHelper.GetString("EMAIL_SMTP_PASSWORD"));
                            smtp.EnableSsl = false;

                            Attachment attach = new Attachment(filepath);
                            attach.Name = filename;
                            //attach.Name = filename.Substring(0, filename.Length - 4).Replace(maker + "_", "") + Ext; 
                            message.Attachments.Add(attach);

                            //message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess; 
                            try
                            {
                                smtp.Send(message);
                                log.Info("Email Send Successfully");
                            }
                            catch (Exception ee)
                            {
                                log.Error("Error send email :: " + ee.Message + " = " + ee.InnerException.Message + " = " + ee.StackTrace);
                                result = false;
                            }
                            
                            #endregion email lama
                        }
                        else
                        {
                            log.Info("Email null");
                        }
                    }
                    #endregion email

                }
                else
                {
                    outUpload = "Tidak ada file yang di upload";
                    log.Info("Tidak ada file yang di upload");
                }
            }
            catch (Exception ee)
            {
                outUpload = ee.Message + " = " + ee.InnerException + " = " + ee.StackTrace;
                log.Error(ee.Message + " = " + ee.InnerException + " = " + ee.StackTrace);
                result = false;
            }
            return result;
        }

        //Helper Upload FTP eaea - Denny 13 Juni 2017
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

    }
}
