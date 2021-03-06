﻿using System;
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
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Xml;

namespace BRIChannelSchedulerNew.Payroll.SharedFolder
{
    class GetFile : IStatefulJob
    {
        private static Configuration cfg;
        private static ISessionFactory factory;
        private static String serverId = "20.8";
        private static String SchCode = "SharedFolder";
        private static String SchName = "SCH Share Folder";
        private static String schInfo = " " + DateTime.Now + "-";

        String url = "";
        String uid = "";
        String pass = "";
        String pid = "";
        String maker = "";
        String kodeformatfile = "";
        bool isSpecialFormat = false;
        int fid = 100;
        string fname = "Payroll";


        void IJob.Execute(JobExecutionContext context)
        {
            cfg = new Configuration();
            cfg.AddAssembly("BRIChannelSchedulerNew");
            ILog log = LogManager.GetLogger(typeof(GetFile));
            Console.WriteLine(schInfo + "Start Get FILE ===");
            try
            {
                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }

                ISession session = factory.OpenSession();
                Parameter paramClientId = session.Load<Parameter>("CLIENT_SHARED_FOLDER_PAYROLL");
                String[] CId = paramClientId.Data.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                DateTime schedulle = DateTime.Parse("0001-01-01 00:00:00");
                IList<ClientMatrix> cm = session.CreateCriteria(typeof(ClientMatrix))
                    .Add(Expression.In("Id", CId))
                    .List<ClientMatrix>();

                foreach (ClientMatrix cm1 in cm)
                {
                    try
                    {
                        //Sing rapi to cah
                        #region 1. Baca Temp Authority
                        AuthorityHelper ah = null;
                        ah = new AuthorityHelper(cm1.Matrix);
                        url = ah.GetTransactionURL(100);
                        uid = ah.GetTransactionUID(100);
                        pass = ah.GetTransactionPASS(100);
                        pid = ah.GetTransactionMAKER(100);
                        kodeformatfile = ah.GetTransactionKodeFormatFile(100);

                        if (!string.IsNullOrEmpty(kodeformatfile)) isSpecialFormat = true;
                        string path = "";
                        if (isSpecialFormat)//bukan format BRI
                        {
                            Parameter param2 = session.Load<Parameter>("FOLDER_PAYROLL_ORIGINAL_FILE");
                            path = param2.Data;
                        }
                        else
                        {
                            Parameter param = session.Load<Parameter>("FOLDER_PAYROLL_INPUT");
                            path = param.Data;
                        }

                        #endregion Baca Temporary

                        #region 2. cek directory exist
                        path = path + DateTime.Now.ToString("yyyyMMdd") + "\\";

                        if (!Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path);
                            log.Info("Create directory success. mkdir : " + path);
                        }


                        #endregion Cek direktori Path + now

                        #region 3. get input url
                        if (url.Contains("|"))
                        {
                            string[] url_temp = url.Split('|');
                            url = url_temp[0];
                        }

                        Console.WriteLine("Url : " + url + "Id maker : " + pid + "Kode Format File : " + kodeformatfile);
                        #endregion end of get input url

                        #region 4. Get user
                        if (!string.IsNullOrEmpty(pid))
                        {
                            User us = session.Load<User>(Convert.ToInt32(pid));
                            maker = us.Handle.Trim();

                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Maker : " + maker);
                            if (string.IsNullOrEmpty(maker))
                            {
                                log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Maker not found.");
                                break;
                            }
                        }
                        else
                        {
                            log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Maker not exist on Setting Client.");
                            break;
                        }
                        #endregion Get user


                        if (url.ToUpper().StartsWith("HTTP"))
                        {
                            #region Using HTTPWebRequest
                            log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Using HTTPWebRequest ===");

                            try
                            {
                                CookieContainer cookies;
                                HttpWebResponse response;
                                HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(url);

                                req.Credentials = new NetworkCredential(uid, pass);

                                //---
                                //YANG G BUTUH PROXY
                                string[] idUserProxyPayroll = ParameterHelper.GetString("PAYROLL_PROXY_CLIENT", session).Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                WebProxy myproxy = new WebProxy();
                                if (!(idUserProxyPayroll.Contains(cm1.Id.ToString())))
                                {
                                    Parameter prmProxServ = session.Load<Parameter>("PAYROLL_PROXY_SERVER");
                                    String proxyServer = prmProxServ.Data;

                                    Parameter prmProxPort = session.Load<Parameter>("PAYROLL_PROXY_PORT");
                                    int proxyPort = int.Parse(prmProxPort.Data);

                                    myproxy = new WebProxy(proxyServer, proxyPort);
                                    myproxy.BypassProxyOnLocal = false;
                                    req.Proxy = myproxy;
                                }
                                //---

                                //proxy
                                //Parameter prmProxServ = session.Load<Parameter>("PAYROLL_PROXY_SERVER");
                                //String proxyServer = prmProxServ.Data;

                                //Parameter prmProxPort = session.Load<Parameter>("PAYROLL_PROXY_PORT");
                                //int proxyPort = int.Parse(prmProxPort.Data);

                                //WebProxy myproxy = new WebProxy(proxyServer, proxyPort);
                                //myproxy.BypassProxyOnLocal = false;
                                //req.Proxy = myproxy;

                                //allow all certificate, not secure bro
                                ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;

                                req.CookieContainer = new CookieContainer();
                                req.AllowAutoRedirect = true;
                                cookies = req.CookieContainer;


                                #region download file
                                log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + "Start download file..");
                                response = (HttpWebResponse)req.GetResponse();

                                if (response.StatusCode != HttpStatusCode.OK)
                                {
                                    log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + "Url Not Responding");
                                }

                                using (response)
                                {
                                    log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + "Using response..");
                                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                    {
                                        string html = reader.ReadToEnd();
                                        Regex regEx = null;
                                        //log.Info("Isi HTML : " + html);
                                        //<a href="ST-BRI20150513094810.txt">ST-BRI20150513094810..&gt;</a>
                                        if (html.Contains("<a href"))
                                        {
                                            //regEx = new Regex("<a href=\".*?\">(?<filename>.*?)</a>");
                                            //<a href=\".*\">(?<filename>.*)</a>
                                            //
                                            //regEx = new Regex("<a href=(?<link>.*)>");
                                            log.Info("Masuk yang kecil");
                                            //regEx = new Regex("<a href=\".*?\">(?<filename>.*?)</a>");
                                            regEx = new Regex("<a href=\"(?<filename>.*?)\">(?<filename_display>.*?)</a>");
                                        }
                                        else regEx = new Regex("<A href=\"(?<filename>.*?)\">(?<filename_display>.*?)</A>");
                                        MatchCollection matches = regEx.Matches(html);


                                        if (matches.Count > 0)
                                        {
                                            foreach (Match match in matches)
                                            {
                                                log.Info("Masuk foreach...");
                                                log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + "Nama file : " + match.Groups["filename"].Value);
                                                //20180525 - sayedzul - handler .gpg file
                                                if (match.Success && (match.Groups["filename"].Value.EndsWith(".txt") || match.Groups["filename"].Value.EndsWith(".TXT") || match.Groups["filename"].Value.EndsWith(".csv") || match.Groups["filename"].Value.EndsWith(".CSV") || match.Groups["filename"].Value.EndsWith(".gpg") || match.Groups["filename"].Value.EndsWith(".GPG")))
                                                {
                                                    log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + "Connected and data match...");
                                                    String fname = match.Groups["filename"].Value;
                                                    String NamaFile = maker + "_" + match.Groups["filename"].Value;

                                                    //check if file exist on both format
                                                    string queryPath = "";
                                                    if (isSpecialFormat) queryPath = "FilePathOriginalFile";
                                                    else queryPath = "FilePath";

                                                    IList<TrxPayroll> o = session.CreateCriteria(typeof(TrxPayroll))
                                                        .Add(Expression.Like("FilePath", NamaFile, MatchMode.Anywhere))
                                                        //.Add(Expression.Not(Expression.In("Status", 4)))
                                                        .Add(Expression.Eq("ClientID", cm1.Id))
                                                        .List<TrxPayroll>();

                                                    IList<TrxPayroll> opx = session.CreateCriteria(typeof(TrxPayroll))
                                                        .Add(Expression.Like("OriginalFilePath", NamaFile, MatchMode.Anywhere))
                                                        //.Add(Expression.Not(Expression.In("Status", 4)))
                                                        .Add(Expression.Eq("ClientID", cm1.Id))
                                                        .List<TrxPayroll>();


                                                    if (o.Count < 1 && opx.Count < 1)
                                                    {
                                                        //download file
                                                        //Request.Method = "MOVE";
                                                        log.Info("downloading file...");
                                                        WebClient wc = new WebClient();
                                                        wc.Credentials = new NetworkCredential(uid, pass);
                                                        //using proxy
                                                        wc.Proxy = myproxy;
                                                        //download
                                                        wc.DownloadFile(url + fname, path + NamaFile);

                                                        string pathSavedFile = path + NamaFile;
                                                        log.Info("Download complete, file save as : " + pathSavedFile);

                                                        #region inser to DB



                                                        //String fname = DateTime.Now.ToString("ddMMyyhhmmss") + "_" + pathSavedFile;
                                                        TrxPayroll trx = new TrxPayroll();
                                                        trx.CreatedTime = DateTime.Now;
                                                        trx.LastUpdate = DateTime.Now;
                                                        //ex : dirgantara/admin

                                                        Client theClients = session.Load<Client>(cm1.Id);

                                                        trx.CreatedBy = theClients.Handle.Trim() + "/" + maker;
                                                        trx.Maker = maker + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                                        trx.Approver = "";
                                                        
                                                        //20180525 - sayedzul - handler .gpg
                                                        if (NamaFile.ToLower().EndsWith(".gpg"))
                                                        {
                                                            trx.FileName = pathSavedFile;
                                                            trx.Status = ParameterHelper.PAYROLL_DECRYPT_PROCESS;//waiting decrypt
                                                        }
                                                        else
                                                        {
                                                            if (isSpecialFormat)//bukan format BRI
                                                            {
                                                                trx.OriginalFilePath = pathSavedFile;
                                                                trx.FileFormatCode = int.Parse(kodeformatfile.Trim());
                                                                trx.Status = ParameterHelper.PAYROLL_WAITING_CONVERT;//waiting file convert
                                                            }
                                                            else
                                                            {
                                                                trx.FileName = pathSavedFile;
                                                                trx.Status = ParameterHelper.PAYROLL_WAITINGSEQNUM;//waiting seq number
                                                            }
                                                        }

                                                        trx.FileDescription = NamaFile.Substring(0, NamaFile.Length - 4) + "_SHARED_FOLDER";

                                                        //Client Acc
                                                        //Parameter accParam = session.Load<Parameter>("PAYROLL_ACC_SEMEN_TONASA");//50298
                                                        //int maxResult = int.Parse(maxTrx.Data);

                                                        trx.DebitAccount = "000000000000000";//isi di upload file
                                                        trx.Description = "WAITING||0||";//isi di upload file
                                                        trx.ProcessTime = schedulle;
                                                        trx.FilePath = pathSavedFile;
                                                        trx.ClientID = cm1.Id;
                                                        trx.Fee = GetTransactionFee(session, 100, cm1);
                                                        trx.PassKey = "";
                                                        trx.SharedFolderStatus = "YES|";

                                                        //FileUpload.PostedFile.SaveAs(path + fname);
                                                        AuditHelper.Log(session, maker, "Payroll Upload File Host to Host", 101, trx.ToString());
                                                        session.Save(trx);
                                                        session.Flush();


                                                        /*-------------------------------------------------*/

                                                        Transaction tx = new Transaction();
                                                        tx.Handle = "ADD PAYROLLBRI";
                                                        tx.TransactionId = 101;
                                                        tx.CreatedTime = DateTime.Now;
                                                        tx.VerifiedTime = DateTime.Now;
                                                        tx.ApprovedTime = DateTime.Now;
                                                        tx.Maker = maker + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                                        tx.TObject = trx.ToString();
                                                        tx.ClientId = cm1.Id;
                                                        tx.Status = 1;
                                                        tx.Action = "ADD";
                                                        tx.Checker = "";
                                                        tx.Approver = "";
                                                        tx.Checker = "";
                                                        tx.Approver = "";
                                                        tx.CheckWork = 0;
                                                        tx.CheckTotal = 0;
                                                        tx.ApproveWork = 0;
                                                        tx.CheckTotal = 0;



                                                        ClientWorkflow cw = session.CreateCriteria(typeof(ClientWorkflow))
                                                            .Add(Expression.Eq("Id", cm1.Id))
                                                            .UniqueResult<ClientWorkflow>();

                                                        WorkflowHelper wh = new WorkflowHelper(cw.Workflow);
                                                        if (!(null == wh))
                                                        {
                                                            Workflow flow = wh.GetWorkflow(100);
                                                            if (!(null == flow))
                                                            {
                                                                //tx.Maker = "";
                                                                //direct payroll
                                                                tx.Approver = "";
                                                                tx.CheckTotal = 0;//flow.TotalVerifier;
                                                                tx.ApproveTotal = 0;// flow.TotalApprover;

                                                                if (!ah.isDirectTransaction(100))
                                                                {
                                                                    tx.CheckTotal = flow.TotalVerifier;
                                                                    tx.ApproveTotal = flow.TotalApprover;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("Matrix PAYROLL is Null for this Client");
                                                            }
                                                        }

                                                        session.Save(tx);
                                                        session.Flush();

                                                        #endregion


                                                        #region meninggalkan jejak
                                                        log.Info("Start upload = " + url + fname.Substring(0, fname.Length - 4) + ".ACK");
                                                        wc.UploadFile(url + fname.Substring(0, fname.Length - 4) + ".ACK", "PUT", path + NamaFile);
                                                        log.Info("Upload response sukses.");

                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        log.Info(schInfo + " No New File ===");
                                                    }
                                                }
                                                else
                                                {
                                                    log.Info("Tidak ada file sesuai kriteria.");
                                                }
                                            }
                                        }
                                    }
                                }

                                #endregion



                            }
                            catch (Exception e)
                            {
                                log.Error("Ada Error, Message : " + e.Message + " Stack trace : " + e.StackTrace + " Ineer Exc : " + e.InnerException);
                            }
                            #endregion

                        }
                        else if (url.ToUpper().StartsWith("FTP"))
                        {
                            //ftp://10.107.10.150/kai-payment/OUTGOING/


                            #region Using OLD FTP
                            //string ipFTP = "";
                            //string subDir = "";

                            ////Get FTP IP
                            //ipFTP = url;
                            //string search = "0123456789";
                            //int posIP = ipFTP.LastIndexOfAny(search.ToCharArray());
                            //string tempIP = ipFTP.Remove(posIP + 1);
                            //log.Info("TempIP : " + tempIP);
                            //ipFTP = tempIP.Replace("ftp://", "");
                            //log.Info("IP FTP : " + ipFTP);


                            ////get FTP Directory
                            //string src = url;
                            //int pos = url.LastIndexOfAny(search.ToCharArray());
                            //subDir = src.Remove(0, pos + 1);
                            //log.Info("Dir FTP : " + subDir);

                            ////Inisiate FTP
                            //FTPHelper ff = new FTPHelper();
                            //ff.setRemoteHost(ipFTP);
                            //ff.setRemotePort(21021);
                            ////ff.setRemoteUser("");
                            ////ff.setRemotePass("");

                            ////Set Socket
                            //Socket clientSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            //clientSoc.Connect(ipFTP, 21021);
                            //ff.setSocket(clientSoc);
                            ////ff.SendUserPassword();

                            //bool connect = ff.login();
                            //ff.chdir(subDir);
                            ////string tes1 = "*" + util.getGlobalParam("MT100_PREFIX") + "*";
                            //string[] fileList = ff.getFileList("*"); //ff.getFileList("*" + util.getGlobalParam("MT100_PREFIX") + "*");
                            //ff.close();

                            //for (int i = 0; i < fileList.Length; i++)
                            //{
                            //    log.Info("File Name : " + fileList[i]);
                            //    string NamaFile = fileList[i].Trim();

                            //    ////check if file exist on both format
                            //    //string queryPath = "";
                            //    //if (isSpecialFormat) queryPath = "FilePathOriginalFile";
                            //    //else queryPath = "FilePath";

                            //    IList<TrxPaymentPriorityBRI> o = session.CreateCriteria(typeof(TrxPaymentPriorityBRI))
                            //        .Add(Expression.Like("FilePath", NamaFile, MatchMode.Anywhere))
                            //        .Add(Expression.Eq("ClientId", cm1.Id))
                            //        .List<TrxPaymentPriorityBRI>();

                            //    IList<TrxPaymentPriorityBRI> opx = session.CreateCriteria(typeof(TrxPaymentPriorityBRI))
                            //        .Add(Expression.Like("FilePathOriginalFile", NamaFile, MatchMode.Anywhere))
                            //        .Add(Expression.Eq("ClientId", cm1.Id))
                            //        .List<TrxPaymentPriorityBRI>();


                            //    if (o.Count < 1 && opx.Count < 1 && (NamaFile.ToUpper().Contains(".TXT") || NamaFile.ToUpper().Contains(".CSV")))
                            //    {
                            //        //download file
                            //        log.Info("Start Downloading file...");
                            //        //Inisiate FTP
                            //        ff = new FTPHelper();
                            //        ff.setRemoteHost(ipFTP);
                            //        ff.setRemotePort(21021);
                            //        //ff.setRemoteUser("");
                            //        //ff.setRemotePass("");

                            //        //Set Socket
                            //        clientSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            //        clientSoc.Connect(ipFTP, 21021);
                            //        ff.setSocket(clientSoc);
                            //        //ff.SendUserPassword();

                            //        connect = ff.login();
                            //        ff.chdir(subDir);
                            //        //string tes1 = "*" + util.getGlobalParam("MT100_PREFIX") + "*";
                            //        //Download
                            //        ff.download(NamaFile, path + NamaFile);
                            //        log.Info("Download Success..");

                            //        //Delete File
                            //        ff.deleteRemoteFile(NamaFile);
                            //        log.Info("DeleteSuccess..");

                            //        ff.close();


                            //        //inser to DB
                            //        TrxPaymentPriorityBRI trx = new TrxPaymentPriorityBRI();
                            //        if (isSpecialFormat)
                            //        {
                            //            trx.SharedFolder = 100 + int.Parse(kodeformatfile);
                            //            trx.FilePathOriginalFile = path + NamaFile;
                            //            trx.Status = ParameterHelper.TRXPP_WAITING_CONVERT;
                            //        }
                            //        else
                            //        {
                            //            trx.Status = ParameterHelper.TRXPP_WAITING_FILE_CHECK;
                            //            trx.FilePath = path + NamaFile;
                            //            trx.SharedFolder = 100;
                            //        }
                            //        trx.SharedFolderStatus = "";
                            //        trx.ClientId = cm1.Id;
                            //        trx.FileDescription = NamaFile.Substring(0, NamaFile.Length - 4) + "_SHARED_FOLDER";
                            //        trx.LastUpdate = DateTime.Now;
                            //        trx.Description = ParameterHelper.TRXDESCRIPTION_WAITING;
                            //        trx.ServerID = serverId;
                            //        trx.Fid = 1140;//apa beda???
                            //        trx.Pid = Convert.ToInt32(pid);
                            //        trx.CreatedTime = DateTime.Now;
                            //        session.Save(trx);
                            //        session.Flush();
                            //        log.Info("Save data Success to table TrxPaymentPriorityBRI");
                            //    }
                            //}
                            #endregion

                            #region Using FTP Payroll Develpment 1

                            //log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " URL Input FTP : " + url);

                            //string ipFTP = "";
                            //int port = 21;
                            //string subDir = "";
                            //string namaPembanding = "";
                            ////Get FTP IP
                            //ipFTP = url;
                            //string search = "0123456789";
                            //int posIP = ipFTP.LastIndexOfAny(search.ToCharArray());
                            //string tempIP = ipFTP.Remove(posIP + 1);
                            ////log.Info("TempIP : " + tempIP);
                            //string[] ipFTPandPort = tempIP.Replace("ftp://", "").Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                            //ipFTP = ipFTPandPort[0];
                            //port = int.Parse(ipFTPandPort[1]);
                            ////get FTP Directory
                            //string src = url;
                            //int pos = url.LastIndexOfAny(search.ToCharArray());
                            ////subDir = src.Remove(0, pos + 1);
                            //subDir = src.Remove(0, pos + 2);
                            //log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Dir FTP : " + subDir);


                            //#region Inisiate FTP
                            //log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Begin Inisiate FTP");
                            //List<string> fileList = new List<string>();

                            //if (GetListFTP(src, uid, pass, out fileList, log))
                            //{
                            //    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " SUCCESS Inisiate FTP");

                            //    #region mulai baca dan posting ke tabel
                            //    for (int i = 0; i < fileList.Count; i++)
                            //    {
                            //        log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " File Name : " + fileList[i]);
                            //        //log.Info("File Name : " + fileList[i]);
                            //        string NamaFile = fileList[i].Trim();

                            //        if (!NamaFile.Equals(""))
                            //        {
                            //            //penanda
                            //            namaPembanding = maker + "_" + NamaFile;
                            //            //Asli
                            //            IList<TrxPayroll> o = session.CreateCriteria(typeof(TrxPayroll))
                            //               .Add(Expression.Eq("FileDescription", namaPembanding))
                            //               .Add(Expression.Eq("ClientID", cm1.Id))
                            //               .Add(Expression.Not(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                            //               .List<TrxPayroll>();

                            //            //Convert
                            //            IList<TrxPayroll> oo = session.CreateCriteria(typeof(TrxPayroll))
                            //               .Add(Expression.Eq("OriginalFilePath", namaPembanding))
                            //               .Add(Expression.Eq("ClientID", cm1.Id))
                            //               .Add(Expression.Not(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                            //               .List<TrxPayroll>();

                            //            log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Jumlah dengan Nama File : " + namaPembanding + " , sejumlah = " + o.Count);
                            //            if ((o.Count < 1 || oo.Count < 1) && ((NamaFile.ToUpper().Contains(".TXT") || NamaFile.ToUpper().Contains(".CSV") || NamaFile.ToUpper().Contains(".ASC")) && (!NamaFile.ToUpper().Contains("PROCESSED"))))
                            //            {
                            //                #region new method
                            //                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Download file start");


                            //                /*Download ke local*/
                            //                if (DownloadDataFTP(src, uid, pass, NamaFile, path + NamaFile, log))
                            //                {
                            //                    log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Download file SUCCESS");
                            //                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Upload file start");


                            //                    /*Upload ke processed*/
                            //                    if (UploadDataFTP(src + "/PROCESSED", uid, pass, NamaFile, path + NamaFile, log))
                            //                    {
                            //                        log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Upload file SUCCESS");

                            //                        #region insertToDB
                            //                        DateTime schedulle = DateTime.Parse("0001-01-01 00:00:00");

                            //                        //String fname = DateTime.Now.ToString("ddMMyyhhmmss") + "_" + pathSavedFile;
                            //                        TrxPayroll trx = new TrxPayroll();
                            //                        trx.CreatedTime = DateTime.Now;
                            //                        trx.LastUpdate = DateTime.Now;
                            //                        //ex : dirgantara/admin

                            //                        Client theClients = session.Load<Client>(cm1.Id);

                            //                        trx.CreatedBy = theClients.Handle.Trim() + "/" + maker;
                            //                        trx.Maker = maker + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            //                        trx.Approver = "";

                            //                        if (isSpecialFormat)//bukan format BRI
                            //                        {
                            //                            trx.OriginalFilePath = path + NamaFile;
                            //                            trx.FileFormatCode = int.Parse(kodeformatfile.Trim());
                            //                            trx.Status = ParameterHelper.PAYROLL_WAITING_CONVERT;//waiting file convert
                            //                            log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Special Format Payroll");
                            //                        }
                            //                        else
                            //                        {
                            //                            trx.FileName = path + NamaFile;
                            //                            trx.Status = ParameterHelper.PAYROLL_WAITINGSEQNUM;//waiting seq number
                            //                            log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + "  Format Payroll BRI");
                            //                        }

                            //                        trx.FileDescription = NamaFile.Substring(0, NamaFile.Length - 4) + "_SHARED_FOLDER";

                            //                        //Client Acc
                            //                        //Parameter accParam = session.Load<Parameter>("PAYROLL_ACC_SEMEN_TONASA");//50298
                            //                        //int maxResult = int.Parse(maxTrx.Data);

                            //                        trx.DebitAccount = "000000000000000";//isi di upload file
                            //                        trx.Description = "WAITING||0||";//isi di upload file
                            //                        trx.ProcessTime = schedulle;
                            //                        trx.FilePath = path + NamaFile;
                            //                        trx.ClientID = cm1.Id;
                            //                        trx.Fee = GetTransactionFee(session, 100, cm1);
                            //                        trx.PassKey = "";
                            //                        trx.SharedFolderStatus = "YES|";

                            //                        //FileUpload.PostedFile.SaveAs(path + fname);
                            //                        AuditHelper.Log(session, maker, "Payroll Upload File Host to Host", 101, trx.ToString());
                            //                        session.Save(trx);
                            //                        session.Flush();


                            //                        /*-------------------------------------------------*/

                            //                        Transaction tx = new Transaction();
                            //                        tx.Handle = "ADD PAYROLLBRI";
                            //                        tx.TransactionId = 101;
                            //                        tx.CreatedTime = DateTime.Now;
                            //                        tx.VerifiedTime = DateTime.Now;
                            //                        tx.ApprovedTime = DateTime.Now;
                            //                        tx.Maker = maker + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            //                        tx.TObject = trx.ToString();
                            //                        tx.ClientId = cm1.Id;
                            //                        tx.Status = 1;
                            //                        tx.Action = "ADD";
                            //                        tx.Checker = "";
                            //                        tx.Approver = "";
                            //                        tx.Checker = "";
                            //                        tx.Approver = "";
                            //                        tx.CheckWork = 0;
                            //                        tx.CheckTotal = 0;
                            //                        tx.ApproveWork = 0;
                            //                        tx.CheckTotal = 0;



                            //                        ClientWorkflow cw = session.CreateCriteria(typeof(ClientWorkflow))
                            //                            .Add(Expression.Eq("Id", cm1.Id))
                            //                            .UniqueResult<ClientWorkflow>();

                            //                        WorkflowHelper wh = new WorkflowHelper(cw.Workflow);
                            //                        if (!(null == wh))
                            //                        {
                            //                            Workflow flow = wh.GetWorkflow(100);
                            //                            if (!(null == flow))
                            //                            {
                            //                                //tx.Maker = "";
                            //                                //direct payroll
                            //                                tx.Approver = "";
                            //                                tx.CheckTotal = 0;//flow.TotalVerifier;
                            //                                tx.ApproveTotal = 0;// flow.TotalApprover;

                            //                                if (!ah.isDirectTransaction(100))
                            //                                {
                            //                                    tx.CheckTotal = flow.TotalVerifier;
                            //                                    tx.ApproveTotal = flow.TotalApprover;
                            //                                    log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Format BRI - Payroll");
                            //                                }
                            //                                else
                            //                                {

                            //                                    log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Direct Transactions - Payroll");
                            //                                }
                            //                            }
                            //                            else
                            //                            {
                            //                                throw new Exception("Matrix PAYROLL is Null for this Client");
                            //                            }
                            //                        }

                            //                        session.Save(tx);
                            //                        session.Flush();
                            //                        log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Create Trx SUCCESS");
                            //                        #endregion insert

                            //                        #region delete dari folder sumber 

                            //                        Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Delete file start");

                            //                        if (DeleteDataFTP(src, uid, pass, NamaFile, log))
                            //                        {
                            //                            log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Delete file SUCCESS");
                            //                        }
                            //                        else
                            //                            log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Delete file FAILED");

                            //                        #endregion end of delete
                            //                    }
                            //                    else
                            //                    {
                            //                        log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Upload to PROCESSED - FAILED");

                            //                    }

                            //                }
                            //                else
                            //                {
                            //                    log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Download file FAILED");

                            //                }
                            //                #endregion end of new method
                            //            }
                            //            else log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " Nama file sama atau Format File (CSV/TXT/ASC/PROCESSED) tidak sesuai keriteria");
                            //        }
                            //        else log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " NamaFile kosong");

                            //    }
                            //    #endregion mulai baca dan posting ke tabel
                            //}
                            //else
                            //    log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " FAILED Inisiate FTP");

                            //#endregion Inisiate FTP 

                            #endregion end of using FTP

                            //Payroll FTP menggunakan mekanisme tempGetfile dan objek Getfile - Denny 11 Juni 2017
                            #region Using FTP Payroll Development 2
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") Using FTP Start");
                            log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") Start-URL : " + url + "-JUST FOR User : " + maker);

                            #region a. Get FTP IP
                            string ipFTP = url;
                            int port = 21;//default 21 ea ea
                            string subDir = "";
                            string search = "0123456789";
                            int posIP = ipFTP.LastIndexOfAny(search.ToCharArray());
                            string tempIP = ipFTP.Remove(posIP + 1);
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") TempIP : " + tempIP);
                            string[] ipFTPandPort = tempIP.Replace("ftp://", "").Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);

                            ipFTP = ipFTPandPort[0];
                            port = int.Parse(ipFTPandPort[1]);
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") IP FTP : " + ipFTP + " Port : " + port);

                            #endregion end of Get FTP IP

                            #region b. get Direktory
                            string src = url;
                            int pos = url.LastIndexOfAny(search.ToCharArray());
                            subDir = src.Remove(0, pos + 2);
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") Dir FTP : " + subDir);

                            #endregion get Direktory

                            #region c. Mulai Listing
                            TmpGetFilePayroll tmp = session.CreateCriteria(typeof(TmpGetFilePayroll))
                                .Add(Expression.Eq("ClientId", cm1.Id))
                                .Add(Expression.Eq("Fid", fid))
                                .UniqueResult<TmpGetFilePayroll>();

                            if (tmp != null)
                            {
                                if (!tmp.Alamat.Equals(url))//ada perubahan data pada License Seller pada url
                                {
                                    tmp.Alamat = url;
                                    tmp.Data = null;
                                }
                                else
                                {
                                    Parameter paramException = session.Load<Parameter>("FOLDER_EXCEPTION_FTP");
                                    if (tmp.Data == paramException.Data)
                                    {
                                        tmp.Data = null;
                                        session.Save(tmp);
                                        session.Flush();
                                    }
                                    
                                    //Data ada, tidak kosong,
                                    //if ((!(tmp.Data == null)) || (!(tmp.Data == "")))
                                    if(!string.IsNullOrEmpty(tmp.Data))
                                    {
                                        IList<DataGetFilePayroll> fileListAwal = new List<DataGetFilePayroll>();
                                        XmlDocument docAwal = new XmlDocument();
                                        docAwal.LoadXml(tmp.Data);
                                        XmlNodeReader readerAwal = new XmlNodeReader(docAwal.DocumentElement);
                                        XmlSerializer serAwal = new XmlSerializer(typeof(DataGetFilePayroll[]));
                                        object objAwal = serAwal.Deserialize(readerAwal);
                                        fileListAwal = ((DataGetFilePayroll[])objAwal).ToList<DataGetFilePayroll>();
                                        Boolean isRetryGetList = false;
                                        for (int i = 0; i < fileListAwal.Count; i++)
                                        {
                                            if (fileListAwal[i].Status == ParameterHelper.TRXSTATUS_GETFILE_DOWNLOAD)//0
                                            {
                                                //ulang aja
                                                tmp.Data = null;
                                            }
                                            if (fileListAwal[i].Status == ParameterHelper.TRXSTATUS_GETFILE_DELETE)//3
                                            {
                                                isRetryGetList = true;
                                            }
                                            else
                                            {
                                                isRetryGetList = false;
                                            }
                                        }

                                        if (isRetryGetList)
                                        {
                                            //ulang aja
                                            tmp.Data = null;
                                        }
                                    }
                                }

                                session.Update(tmp);
                                session.Flush();
                            }
                            else
                            {
                                Client theClient = session.Load<Client>(cm1.Id);
                                TmpGetFilePayroll tmpNew = new TmpGetFilePayroll();
                                tmpNew.ClientId = theClient.Id;
                                tmpNew.Fid = fid;
                                tmpNew.FName = fname;
                                tmpNew.ClientName = theClient.Handle;
                                tmpNew.Alamat = url;
                                tmpNew.User = uid;
                                tmpNew.Pass = pass;
                                tmp = tmpNew;
                                session.Save(tmp);
                                session.Flush();
                            }

                            

                            //Data file masih kosonmg, jika ada perubahan data license maka akan set kosong terlebih dahulu atau isi XML
                            Parameter paramException2 = session.Load<Parameter>("FOLDER_EXCEPTION_FTP");

                            if (String.IsNullOrEmpty(tmp.Data) || tmp.Data.Equals(paramException2.Data))
                            {

                                #region Inisiate FTP
                                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") Begin Inisiate FTP");
                                List<string> files = new List<string>();

                                if (GetListFTP(src, uid, pass, out files, log))
                                {
                                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") SUCCESS Inisiate FTP");
                                    IList<DataGetFilePayroll> datas = new List<DataGetFilePayroll>();
                                    if (files.Count > 0)
                                    {
                                        foreach (string file in files)
                                        {
                                            //Sementara BANGET - denny
                                            if (file.ToUpper().Equals("OUTGOING/PROCESSED"))
                                            {

                                            }
                                            else
                                            {
                                                DataGetFilePayroll data = new DataGetFilePayroll();
                                                if (file.ToUpper().Contains("OUTGOING"))
                                                {
                                                    string bb = Regex.Replace(file,"outgoing/","",RegexOptions.IgnoreCase);
                                                    //string aa = file.Replace("OUTGOING/", "");
                                                    data.NamaFile = bb;
                                                }
                                                else
                                                {
                                                    data.NamaFile = file;
                                                }
                                                data.Status = ParameterHelper.TRXSTATUS_GETFILE_DOWNLOAD;//0
                                                datas.Add(data);
                                            }
                                            //end of sementara banget
                                        }

                                        tmp.Data = QueryHelper.ArrayToXMLDataSource(typeof(DataGetFilePayroll[]), datas.ToArray<DataGetFilePayroll>());
                                        session.Update(tmp);
                                        session.Flush();
                                    }
                                    else
                                        log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") Success Initiate FTP but No Data Found");

                                }
                                else
                                    log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === (ClientID " + cm1.Id + ") FAILED Inisiate FTP");
                                #endregion
                            }
                            #endregion end of Listing

                            /*tmp data sudah ada atatu ada file count > 0*/

                            IList<DataGetFilePayroll> fileList = new List<DataGetFilePayroll>();
                            if (!String.IsNullOrEmpty(tmp.Data))
                            {
                                #region d. download, konsisi jika file name beda atau nama sama tapi status sudah gagal
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(tmp.Data);
                                XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
                                XmlSerializer ser = new XmlSerializer(typeof(DataGetFilePayroll[]));
                                object obj = ser.Deserialize(reader);
                                fileList = ((DataGetFilePayroll[])obj).ToList<DataGetFilePayroll>();
                                String NamaFile = "";
                                for (int i = 0; i < fileList.Count; i++)
                                {


                                    if (fileList[i].Status == ParameterHelper.TRXSTATUS_GETFILE_DOWNLOAD)//0
                                    {

                                        NamaFile = fileList[i].NamaFile.Trim();//nama harus unik
                                        log.Info(SchCode + " === (ClientID " + cm1.Id + ") File Name : " + NamaFile + " Found!!");
                                        //Sekect to handle duplikasi
                                        //Cek original untuk format bukan BRI dan Filepath untuk format BRI
                                        IList<TrxPayroll> o = session.CreateCriteria(typeof(TrxPayroll))
                                           .Add(Expression.Like("OriginalFilePath", "%" + NamaFile + "%"))
                                           .Add(Expression.Eq("ClientID", cm1.Id))
                                           .Add(Expression.Not(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                                           .List<TrxPayroll>();

                                        IList<TrxPayroll> oo = session.CreateCriteria(typeof(TrxPayroll))
                                           .Add(Expression.Like("FileName", "%" + NamaFile + "%"))
                                           .Add(Expression.Eq("ClientID", cm1.Id))
                                           .Add(Expression.Not(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                                           .List<TrxPayroll>();

                                        if (!NamaFile.Equals(""))
                                        {
                                            if (isSpecialFormat)
                                            {
                                                log.Info(SchCode + " === (ClientID " + cm1.Id + ") Jumlah File ( " + NamaFile + ") = " + o.Count + " FOUND-Special Format");
                                            }
                                            else
                                            {
                                                log.Info(SchCode + " === (ClientID " + cm1.Id + ") Jumlah File ( " + NamaFile + ") = " + oo.Count + " FOUND-Format BRI");
                                            }
                                            if (o.Count < 1 && oo.Count < 1)
                                            {
                                                if ((NamaFile.ToUpper().Contains(".TXT") || NamaFile.ToUpper().Contains(".CSV") || NamaFile.ToUpper().Contains(".ASC")) && (!NamaFile.ToUpper().Contains("PROCESSED")))
                                                {
                                                    Console.WriteLine(SchCode + " === (ClientID " + cm1.Id + ") Download file start");
                                                    if (DownloadDataFTP(src, uid, pass, NamaFile, path + NamaFile, log))
                                                    {
                                                        log.Info(SchCode + " === (ClientID " + cm1.Id + ") Download file SUCCESS");
                                                        fileList[i].Status = ParameterHelper.TRXSTATUS_GETFILE_UPLOAD;//1
                                                        //tmp.Data = fileList.ToString();
                                                        tmp.Data = QueryHelper.ArrayToXMLDataSource(typeof(DataGetFilePayroll[]), fileList.ToArray<DataGetFilePayroll>());
                                                        session.Update(tmp);
                                                        session.Flush();
                                                    }
                                                    else
                                                    {
                                                        log.Error(SchCode + " === (ClientID " + cm1.Id + ") Download file FAILED " + NamaFile);
                                                        log.Error(SchCode + " === (ClientID " + cm1.Id + ") Download file FAILED, SRC : " + src);
                                                        log.Error(SchCode + " === (ClientID " + cm1.Id + ") Download file FAILED, namafile : " + NamaFile);
                                                        log.Error(SchCode + " === (ClientID " + cm1.Id + ") Download file FAILED, path + nama  : " + path + NamaFile);
                                                    }
                                                }
                                                else
                                                {
                                                    log.Error(SchCode + " === (ClientID " + cm1.Id + ") File tidak sesuai keriteria ekstensi file (TXTX/CSV/ASC/PROCESSED)");
                                                }
                                            }
                                            else
                                            {
                                                log.Error(SchCode + " === (ClientID " + cm1.Id + ") Jumlah file dengan nama : " + NamaFile + " berjumlah > 0 DAN belum Reject");
                                            }
                                        }
                                        else
                                        {
                                            log.Error(SchCode + " === (ClientID " + cm1.Id + ") NamaFile kosong " + NamaFile);
                                        }
                                    }
                                }

                                #endregion

                                #region e. upload ke processed
                                //pindahin file ke processed, create transaksi status reject
                                for (int i = 0; i < fileList.Count; i++)
                                {
                                    if (fileList[i].Status == ParameterHelper.TRXSTATUS_GETFILE_UPLOAD)//1
                                    {
                                        NamaFile = fileList[i].NamaFile.Trim();

                                        if (UploadDataFTP(src + "/PROCESSED", uid, pass, NamaFile, path + NamaFile, log))
                                        {
                                            log.Info(SchCode + " === (ClientID " + cm1.Id + ") Upload file SUCCESS for file name : " + NamaFile);

                                            fileList[i].Status = ParameterHelper.TRXSTATUS_GETFILE_CREATETRX;//2
                                            tmp.Data = tmp.Data = QueryHelper.ArrayToXMLDataSource(typeof(DataGetFilePayroll[]), fileList.ToArray<DataGetFilePayroll>());
                                            session.Update(tmp);
                                            session.Flush();
                                        }
                                        else
                                            log.Error(SchCode + " === (ClientID " + cm1.Id + ") Upload file FAILED for file name : " + NamaFile);
                                    }
                                }

                                #endregion

                                #region f. create trx ke tabel Parent

                                for (int i = 0; i < fileList.Count; i++)
                                {
                                    if (fileList[i].Status == ParameterHelper.TRXSTATUS_GETFILE_CREATETRX)
                                    {
                                        Console.WriteLine(SchCode + " === (ClientID " + cm1.Id + ") File Name : " + fileList[i].NamaFile + " Insert Parent - Start");
                                        NamaFile = fileList[i].NamaFile.Trim();

                                        //Sekect to handle duplikasi
                                        IList<TrxPayroll> o = session.CreateCriteria(typeof(TrxPayroll))
                                           .Add(Expression.Like("OriginalFilePath", "%" + NamaFile + "%"))
                                           .Add(Expression.Eq("ClientID", cm1.Id))
                                           .Add(Expression.Not(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                                           .List<TrxPayroll>();

                                        IList<TrxPayroll> oo = session.CreateCriteria(typeof(TrxPayroll))
                                           .Add(Expression.Like("FileName", "%" + NamaFile + "%"))
                                           .Add(Expression.Eq("ClientID", cm1.Id))
                                           .Add(Expression.Not(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                                           .List<TrxPayroll>();
                                        if (o.Count < 1 && oo.Count < 1)
                                        {

                                            #region insert TrxPayrolls

                                            TrxPayroll trx = new TrxPayroll();
                                            if (isSpecialFormat)
                                            {
                                                trx.OriginalFilePath = path + NamaFile;
                                                trx.Status = ParameterHelper.PAYROLL_WAITING_CONVERT;//500
                                                trx.FileFormatCode = int.Parse(kodeformatfile.Trim());
                                            }
                                            else
                                            {
                                                trx.FileName = path + NamaFile;
                                                trx.Status = ParameterHelper.PAYROLL_WAITINGSEQNUM;//100
                                            }
                                            trx.CreatedTime = DateTime.Now;
                                            trx.LastUpdate = DateTime.Now;
                                            Client theClients = session.Load<Client>(cm1.Id);
                                            trx.CreatedBy = theClients.Handle.Trim() + "/" + maker;
                                            trx.Maker = maker + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                            trx.Approver = "";
                                            trx.FileDescription = maker + NamaFile.Substring(0, NamaFile.Length - 4) + "_SHARED_FOLDER";
                                            trx.DebitAccount = "000000000000000";//isi di upload file
                                            trx.Description = "WAITING||0||";//isi di upload file
                                            trx.ProcessTime = schedulle;
                                            trx.FilePath = path + NamaFile;
                                            trx.ClientID = cm1.Id;
                                            trx.Fee = GetTransactionFee(session, 100, cm1);
                                            trx.FeeRTG = GetTransactionRTGSFee(session, 100, cm1);
                                            trx.FeeLLG = GetTransactionLLGFee(session, 100, cm1);
                                            trx.PassKey = "";
                                            trx.SharedFolderStatus = "YES|";


                                            AuditHelper.Log(session, maker, "Payroll Upload File Host to Host", 101, trx.ToString());
                                            session.Save(trx);
                                            session.Flush();
                                            log.Info(SchCode + " === (ClientID " + cm1.Id + ") Insert TrxPayroll SUCCESS - ID :" + trx.Id.ToString());

                                            #endregion insert Trxpayroll

                                            #region Insert Transactions
                                            Transaction tx = new Transaction();
                                            tx.Handle = "ADD PAYROLLBRI";
                                            tx.TransactionId = 101;
                                            tx.CreatedTime = DateTime.Now;
                                            tx.VerifiedTime = DateTime.Now;
                                            tx.ApprovedTime = DateTime.Now;
                                            tx.Maker = maker + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                                            tx.TObject = trx.ToString();
                                            tx.ClientId = cm1.Id;
                                            tx.Status = 1;
                                            tx.Action = "ADD";
                                            tx.Checker = "";
                                            tx.Approver = "";
                                            tx.Checker = "";
                                            tx.Approver = "";
                                            tx.CheckWork = 0;
                                            tx.CheckTotal = 0;
                                            tx.ApproveWork = 0;
                                            tx.CheckTotal = 0;

                                            ClientWorkflow cw = session.CreateCriteria(typeof(ClientWorkflow))
                                                .Add(Expression.Eq("Id", cm1.Id))
                                                .UniqueResult<ClientWorkflow>();

                                            WorkflowHelper wh = new WorkflowHelper(cw.Workflow);
                                            if (!(null == wh))
                                            {
                                                Workflow flow = wh.GetWorkflow(100);
                                                if (!(null == flow))
                                                {
                                                    tx.Approver = "";
                                                    tx.CheckTotal = 0;//flow.TotalVerifier;
                                                    tx.ApproveTotal = 0;// flow.TotalApprover;

                                                    if (!ah.isDirectTransaction(100))
                                                    {
                                                        tx.CheckTotal = flow.TotalVerifier;
                                                        tx.ApproveTotal = flow.TotalApprover;
                                                    }
                                                }
                                                else
                                                {
                                                    throw new Exception("Matrix PAYROLL is Null for this Client");
                                                }
                                            }

                                            session.Save(tx);
                                            session.Flush();
                                            log.Info(SchCode + " === (ClientID " + cm1.Id + ") Insert Transactions, SUCCESS - ID : " + tx.Id.ToString());
                                            #endregion Insert Tranhsactions

                                        }
                                        else
                                        {
                                            //Diplikat
                                            log.Error(SchCode + " === (ClientID " + cm1.Id + ") Duplikasi File : " + NamaFile + ", Langsung DO DELETE aja");

                                        }
                                        //Set status Siap Hapus temporary
                                        fileList[i].Status = ParameterHelper.TRXSTATUS_GETFILE_DELETE;//3
                                        tmp.Data = tmp.Data = QueryHelper.ArrayToXMLDataSource(typeof(DataGetFilePayroll[]), fileList.ToArray<DataGetFilePayroll>());
                                        session.Update(tmp);
                                        session.Flush();
                                    }
                                }

                                #endregion

                                #region g. delete file di incoming

                                for (int i = 0; i < fileList.Count; i++)
                                {
                                    if (fileList[i].Status == ParameterHelper.TRXSTATUS_GETFILE_DELETE)
                                    {
                                        Console.WriteLine(SchCode + " === (ClientID " + cm1.Id + ") File Name : " + fileList[i].NamaFile + "-Ready to delete");
                                        NamaFile = fileList[i].NamaFile.Trim();

                                        #region delete

                                        Console.WriteLine(SchCode + " === (ClientID " + cm1.Id + ") Delete file start");

                                        if (DeleteDataFTP(src, uid, pass, NamaFile, log))
                                        {
                                            log.Info(SchCode + " === (ClientID " + cm1.Id + ") Delete file : " + NamaFile + " SUCCESS");

                                            fileList.Remove(fileList[i]);

                                            if (fileList.Count == 0)
                                                tmp.Data = null;
                                            else
                                                tmp.Data = tmp.Data = QueryHelper.ArrayToXMLDataSource(typeof(DataGetFilePayroll[]), fileList.ToArray<DataGetFilePayroll>());

                                            session.Update(tmp);
                                            session.Flush();

                                        }
                                        else
                                            log.Error(SchCode + " === (ClientID " + cm1.Id + ") Delete file FAILED" + NamaFile);

                                        #endregion
                                    }

                                }

                                #endregion
                            }

                            #endregion Using FTP Payroll Development 2
                            //===
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(SchCode + " === ERROR === (ClientID " + cm1.Id + ")" + "|| Message : " + ex.Message + "|| StackTrace : " + ex.StackTrace + "|| InnerException : " + ex.InnerException);
                    }
                }
            }
            catch (Exception ex)
            {
                //log.Error(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ClientID " + cm1.Id + " FAILED Inisiate FTP");
                log.Error(ex);
                factory.Close();
                factory.Dispose();
                factory = null;
                cfg = null;
                GC.Collect();
            }
            log.Info(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " =======End Process Get File =======");
        }



        /*FTP functions - Denny */
        public static bool GetListFTP(string ip, string user, string pswd, out List<string> filterList, ILog log)
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
            catch (Exception ex)
            {
                log.Error("URL: " + ip + " >>> " + ex.Message + " >>> " + ex.InnerException + " >>> " + ex.StackTrace);
                result = false;
            }
            return result;
        }

        /*Download FTP - Denny*/
        public static bool DownloadDataFTP(string ip, string user, string pswd, string fileName, string localFile, ILog log)
        {
            bool result = true;
            try
            {
                WebClient webClient = new WebClient();
                webClient.UseDefaultCredentials = true;
                webClient.Credentials = new NetworkCredential(user, pswd);
                //webClient.DownloadFile(new Uri(ip + "/" + fileName), localFile);
                //Sementara khusus airnav
                webClient.DownloadFile(new Uri(ip + "/" + fileName), localFile);
                


                /*
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(ip + "/" + fileName);
                reqFTP.Credentials = new NetworkCredential(user, pswd);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Proxy = null;
                reqFTP.UsePassive = true;
                reqFTP.Timeout = -1;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream responseStream = response.GetResponseStream();


                using (FileStream localFileStream = new FileStream(localFile, FileMode.Create))
                {
                    //Download the File.
                    CopyTo(response.GetResponseStream(), localFileStream);
                }
                response.Close();
                 * */
            }
            //catch (WebException xxx)
            //{
            //    String statusxx = ((FtpWebResponse)xxx.Response).StatusDescription;
            //    log.Error("URL: " + ip + " >>> " + statusxx);
            //    result = false;
            //}
            catch (Exception ex)
            {
                result = false;
                log.Error("URL: " + ip + " >>> " + ex.Message + " >>> " + ex.InnerException + " >>> " + ex.StackTrace);
            }

            /*
            FtpWebRequest ftpRequest = null;
           FtpWebResponse ftpResponse = null;
           Stream ftpStream = null;
           int bufferSize = 2048;

            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(ip + "/" + fileName);

                ftpRequest.Credentials = new NetworkCredential(user, pswd);

                ftpRequest.UseBinary = true;
                ftpRequest.UsePassive = true;
                ftpRequest.KeepAlive = true;
                ftpRequest.Timeout = -1;
                ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();

                FileStream localFileStream = new FileStream(localFile, FileMode.Create);

                byte[] byteBuffer = new byte[bufferSize];
                int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);

                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    }
                }

                catch (Exception ex) 
                {
                    result = false;
                    log.Error("URL: " + ip + " >>> " + ex.Message + " >>> " + ex.InnerException + " >>> " + ex.StackTrace);
            
                }

                localFileStream.Close();
            }

            catch (Exception ez) 
            {
                result = false;
                log.Error("URL: " + ip + " >>> " + ez.Message + " >>> " + ez.InnerException + " >>> " + ez.StackTrace);
            
            }
        
        
            if (ftpStream != null)
                ftpStream.Close();
            if (ftpResponse != null)
                ftpResponse.Close();
            ftpRequest = null;
             * */
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

        private static string SubstringBetween(string value, string a, string b)
        {
            int posA = value.IndexOf(a);
            int posB = value.LastIndexOf(b);
            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        }


        private float GetTransactionFee(ISession session, int fid, ClientMatrix cm)
        {
            float _result = 0;
            try
            {
                AuthorityHelper ah = new AuthorityHelper(cm.Matrix);
                _result = ah.GetTransactionFee(session, fid, 1);
            }
            catch
            {
                _result = 111;
            }
            return _result;
        }

        private float GetTransactionRTGSFee(ISession session, int fid, ClientMatrix cm)
        {
            float _result = 0;
            try
            {
                AuthorityHelper ah = new AuthorityHelper(cm.Matrix);
                _result = ah.GetTransactionFee(session, fid, 2);
            }
            catch { }
            return _result;
        }

        private float GetTransactionLLGFee(ISession session, int fid, ClientMatrix cm)
        {
            float _result = 0;
            try
            {
                AuthorityHelper ah = new AuthorityHelper(cm.Matrix);
                _result = ah.GetTransactionFee(session, fid, 3);
            }
            catch { }
            return _result;
        }
    }
}
