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
using BriInterfaceWrapper.BriInterface;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

public class CountCheck
{
    public int Result { get; set; }
}

namespace BRIChannelSchedulerNew.Payroll
{


    class RunPayroll : IStatefulJob
    {

        private const String _template_fh = "FHF-------------NV--------D          STS";
        private const String _template_ch = "CHC-------NSTS";
        private const String _template_bh = "BHR------------------FC--------------ACURV--------DR-----------------------------------------S999";
        private const String _template_bd = "BDR------------------NC--------------ACCRC-------------MD-------------MDH------------MCH------------MIBT----------BYIBT----------SLTRX----------BYTRX----------SLSTSR--------------------------------------S";
        private const String _template_bt = "BTRC------------TDB------------TCR------------T";
        private const String _template_ct = "CTP-------------T";
        private const String _template_ft = "FTDB------------TDB------------ACR------------TCR------------A";

        private static Configuration cfg;
        private static ISessionFactory factory;
        private static String schinfo = DateTime.Now + " Run Payroll ";

        #region IJob Members

        void IJob.Execute(JobExecutionContext context)
        {
            //cfg = new Configuration(); //NHib Configuration
            //cfg.AddAssembly("BRIChannel");
            ILog logs = LogManager.GetLogger(typeof(RunPayroll));
            logs.Info("==== Run Payroll Process ===");
            ISession session = null;
            try
            {
                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }

                //ISessionFactory factory = cfg.BuildSessionFactory();
                session = factory.OpenSession();



                TrxPayroll _payroll = null;
                Boolean _proceed = false;

                String _date_folder = DateTime.Now.ToString("yyyyMMdd");

                Parameter param = session.Load<Parameter>("FOLDER_PAYROLL_LOGS");
                String port = param.Data;
                String _payroll_Log_dir = port + "\\" + _date_folder;

                CekRunningFolder(_payroll_Log_dir);

                param = session.Load<Parameter>("FOLDER_PAYROLL_INPUT_HOST");
                port = param.Data;
                String _payroll_input_dir = port + "\\" + _date_folder;

                CekRunningFolder(_payroll_input_dir);

                param = session.Load<Parameter>("FOLDER_PAYROLL_OUTPUT_HOST");
                port = param.Data;
                String _payroll_output_dir = port + "\\" + _date_folder;

                CekRunningFolder(_payroll_output_dir);

                //CekRunningFolder(_payroll_Log_dir); //cek directory
                Parameter paramCMS = session.Load<Parameter>("CMS_OPERASIONAL_STATUS");
                String portCMS = paramCMS.Data;


                String _payrollFileName = "no_payroll_file";

                PayrollLog(session, "=== Run Payroll Process ===");

                if (portCMS.Equals("1"))
                {
                    PayrollLog(session, "-- CMS RUNNING --, status : " + portCMS);
                    logs.Info("CMS RUNNING, status = " + portCMS);

                    try
                    {
                        /* Query payroll waiting process - 11 */
                        PayrollLog(session, "Getting payroll process request....");

                        TrxPayroll payroll = null;

                        IList<TrxPayroll> lpayroll = session.CreateCriteria(typeof(TrxPayroll))
                           .Add(Expression.In("StatusIFT", new int[] { ParameterHelper.TRXSTATUS_PAYROLLNEW_READY_BOOK_IFT }))
                           .Add(Expression.Lt("ProcessTime", DateTime.Now)) 
                           .Add(Expression.Lt("LastUpdate", DateTime.Now))
                           .List<TrxPayroll>();
                        Console.Write(schinfo + "Data found : " + lpayroll.Count);
                        String piD = "";
                        foreach (TrxPayroll pay in lpayroll)
                        {
                            payroll = pay;

                            PayrollLog(session, "------------------ sebelum masyuk awal bgt bgt----------");
                            //foreach (TrxPayrollDetail xx in lpayroll)
                            //{
                            //    PayrollLog(session, xx.Id.ToString() + "--" + xx.Name + "-->" + xx.Account);
                            //}
                            PayrollLog(session, "pay ID sebelum : " + pay.Id);
                            piD = pay.Id;
                            PayrollLog(session, "pay ID sebelum : " + piD);
                            break;
                        }

                        PayrollLog(session, "------------------ sebelum masyuk awal bgt ubah ----------");
                        //rfq
                        IList<TrxPayrollDetail> lsdetailpayrollss = null;
                        if (lpayroll.Count > 0)
                        {
                            lsdetailpayrollss = payroll.TrxPayrollDetail;
                            foreach (TrxPayrollDetail xxr in lsdetailpayrollss)
                            {
                                PayrollLog(session, xxr.Id.ToString() + "--" + xxr.Name + "-->" + xxr.Account);
                            }
                        }
                        else 
                        {
                            IList<TrxPayroll> lpayroll12 = session.CreateCriteria(typeof(TrxPayroll))
                               .Add(Expression.Eq("StatusIFT", ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_GENERATENAME))
                               .Add(Expression.Lt("LastUpdate", DateTime.Now.AddMinutes(-30)))
                               .List<TrxPayroll>();


                            foreach (TrxPayroll gantung in lpayroll12)
                            {
                                logs.Info("Ada status gantung. ID : " + gantung.Id);
                                gantung.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_READY_BOOK_IFT;
                                gantung.LastUpdate = DateTime.Now;
                                gantung.ErrorDescription += DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + "|Status gantung cleanup by system|";
                                session.Update(gantung);
                                session.Flush();
                                logs.Info("End update status gantung. ID : " + gantung.Id);
                            }
                        }
                        

                        

                        logs.Info("List payroll : " + lpayroll.Count());
                        PayrollLog(session, "list payroll : " + lpayroll.Count());

                        if (payroll != null)
                        {
                            //sayedzul 20170411 - handler no IFT 
                            if (lsdetailpayrollss.Where(x => (x.InstructionCode == TotalOBHelper.IFT || String.IsNullOrEmpty(x.InstructionCode)) && (x.Status != ParameterHelper.TRXSTATUS_REJECT)).Count() > 0)
                            {
                                #region ada data IFT
                                PayrollLog(session, "------------------ sebelum masyuk awal bgt----------");
                                foreach (TrxPayrollDetail xx in lsdetailpayrollss)
                                {
                                    PayrollLog(session, xx.Id.ToString() + "--" + xx.Name + "-->" + xx.Account);
                                }

                                MessageABCSTransactionResponse response = null;
                                String[] descAcc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                                ClientAccount ca = session.Load<ClientAccount>(int.Parse(descAcc[1].Trim()));

                                String AccNo = ca.Number;
                                String TellerID = "0374891";

                                logs.Info("Acc Number for Checking Brinet : " + AccNo);
                                PayrollLog(session, "Acc Number for Checking Brinet : " + AccNo);
                                logs.Info("teller no : " + TellerID);

                                response = inqAccBRIInterface(session, AccNo, TellerID);
                                ca = null;

                                logs.Info("resp : " + response.statuscode.Trim());

                                if (int.Parse(response.statuscode.Trim()) == 1)
                                {
                                    PayrollLog(session, "-- BRINETS ONLINE --, status : " + response.statuscode.Trim());
                                    logs.Info("BRINETS ONLINE, status = " + response.statuscode.Trim());

                                    _payrollFileName = GeneratePayrollFileName(session, payroll, logs); //generate nama file buat dikirim ke host

                                    PayrollLog(session, "Payroll Process ID = " + payroll.Id.ToString() + " with payroll filename : " + _payrollFileName);

                                    #region send request to BRINETS
                                    if (payroll.StatusIFT == ParameterHelper.TRXSTATUS_PAYROLLNEW_READY_BOOK_IFT)
                                    {

                                        PayrollLog(session, "------------------ sebelum masyuk awal bgt2----------");
                                        foreach (TrxPayrollDetail xx in lsdetailpayrollss)
                                        {
                                            PayrollLog(session, xx.Id.ToString() + "--" + xx.Name + "-->" + xx.Account);
                                        }


                                        payroll.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_GENERATENAME; //processing after generate name
                                        payroll.LastUpdate = DateTime.Now;
                                        session.Update(payroll);
                                        session.Flush();

                                        PayrollLog(session, "------------------ sebelum masyuk ----------");
                                        foreach (TrxPayrollDetail xx in lsdetailpayrollss)
                                        {
                                            PayrollLog(session, xx.Id.ToString() + "--" + xx.Name + "-->" + xx.Account);
                                        }

                                        _proceed = ProcessPayroll(session, payroll, _payrollFileName, logs); // buat file

                                        if (!_proceed)
                                        {
                                            String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                                            payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                                            payroll.LastUpdate = DateTime.Now;
                                            //payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + ", File processing error" + "||" + desc[1];
                                            PayrollLog(session, "Error on processing detail");

                                        }
                                        else
                                        {
                                            payroll.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_GENERATEFILE;
                                        }

                                        payroll.LastUpdate = DateTime.Now;

                                        session.Update(payroll);
                                        session.Flush();

                                        /* Process Only One Id per Session */
                                        _payroll = payroll;
                                        //pfile.Flush();
                                        //pfile.Close();
                                        //break
                                        logs.Info(_proceed);
                                        PayrollLog(session, "telah parsing dan process payroll, status : " + _proceed);

                                        /* Upload File To Host */
                                        if (_proceed)
                                        {
                                            logs.Info("--transfer file to host--");
                                            PayrollLog(session, "--transfer file to host--");
                                            if (TransferFileToHost(_payroll_input_dir + "\\" + _payrollFileName, session))
                                            {

                                                PayrollLog(session, "Proses transfer file...");
                                                logs.Info("Proses transfer file...");


                                                _payroll.Description = _payroll.Description + "||" + _payrollFileName;
                                                _payroll.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_TRANSFERFILE;
                                            }
                                            else
                                            {
                                                String[] desc = _payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                                                _payroll.StatusIFT = ParameterHelper.TRXSTATUS_REJECT;
                                                _payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - File Transfer Error" + "||" + desc[1];
                                                logs.Info("File Upload Processing error");
                                            }
                                            _payroll.LastUpdate = DateTime.Now;
                                            session.Update(_payroll);
                                            session.Flush();


                                            PayrollLog(session, "==Run Payroll process end==");
                                            logs.Info("==Run Payroll Process End==");

                                        }
                                        //}
                                    }
                                    #endregion
                                }
                                //20170130 - sayedzul - handler gagal inquiry
                                else if (int.Parse(response.statuscode.Trim()) == 2)
                                {
                                    if (response.msgabmessage[0][4].ToLower().Contains("handler router"))
                                    {
                                        payroll.LastUpdate = DateTime.Now.AddMinutes(5);
                                        payroll.ErrorDescription += "Failed Inq Account. Response : " + response.statuscode.Trim() + "  === " + response.msgabmessage[0][4].Trim();
                                        session.Update(payroll);
                                        session.Flush();
                                        PayrollLog(session, "-- BRINETS OFFLINE --, status : " + response.statuscode.Trim() + "  === " + response.msgabmessage[0][4].Trim());
                                        logs.Info("--BRINETS OFFLINE, status : " + response.statuscode.Trim() + "  === " + response.msgabmessage[0][4].Trim());
                                    }
                                    else
                                    {
                                        payroll.StatusIFT = ParameterHelper.TRXSTATUS_REJECT;
                                        //payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                                        payroll.LastUpdate = DateTime.Now;
                                        //payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Failed Inquiry " + AccNo + " - " + response.msgabmessage[0][4].Trim();
                                        payroll.ErrorDescription += "Failed Inq Account. Response : " + response.statuscode.Trim() + "  === " + response.msgabmessage[0][4].Trim();

                                        //string outRejectChild = "";
                                        //if (rejectAllChild(session, payroll, out outRejectChild, logs))
                                        //{
                                        //    logs.Info("Success Update All Child.");
                                        //}
                                        //else
                                        //{
                                        //    payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                                        //    payroll.ErrorDescription = outRejectChild;
                                        //}

                                        session.Update(payroll);
                                        session.Flush();
                                        PayrollLog(session, "-- Failed Inq Account. Response : " + response.statuscode.Trim() + "  === " + response.msgabmessage[0][4].Trim());
                                        logs.Info("--Failed Inq Account. Response : " + response.statuscode.Trim() + "  === " + response.msgabmessage[0][4].Trim());
                                    }
                                }
                                else
                                {
                                    //rfq add, 20161215
                                    payroll.LastUpdate = DateTime.Now.AddMinutes(5);
                                    payroll.ErrorDescription += "Failed Inq Account. Response : " + response.statuscode.Trim();//rfq add, 20161215
                                    session.Update(payroll);
                                    session.Flush();
                                    //end rfq add, 20161215

                                    PayrollLog(session, "-- BRINETS OFFLINE --, status : " + response.statuscode.Trim());
                                    logs.Info("--BRINETS OFFLINE, status : " + response.statuscode.Trim());
                                }
                                #endregion
                            }
                            else
                            {
                                logs.Info("PAYROLL SEQ: " + payroll.SeqNumber + " No IFT, complete trx ift");
                                payroll.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_COMPLETE;
                                payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_SUCCESS);
                                session.Update(payroll);
                                session.Flush();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        logs.Error("Ooops..data not found because : " + ex.Message + "detail : " + ex.StackTrace + "Inner : " + ex.InnerException);
                        PayrollLog(session, "Ooops..data not found-check again : " + ex.Message + "detail : " + ex.StackTrace);
                    }
                }
                else
                {
                    PayrollLog(session, "-- CMS OFFLINE --, status : " + portCMS);
                    logs.Info(" CMS OFFLINE, status : " + portCMS);
                }
            }
            catch (Exception ee)
            {
                logs.Error("Scheduler Run Payroll failed to start : " + ee.Message + ee.StackTrace);
            }

            finally
            {
                session.Clear();
                session.Close();
                session.Dispose();
                factory.Close();
                cfg = null;
                GC.Collect();
            }
        }

        #endregion

        public static bool rejectAllChild(ISession session, TrxPayroll payroll, out string outMsgUpdate, ILog log)
        {
            outMsgUpdate = "";
            bool result = true;
            try
            {
                //update detail
                //Create Connection
                Parameter dbConf = session.Load<Parameter>("CMS_DBCONNECTION");
                MySqlConnection conn = new MySqlConnection(dbConf.Data);
                conn.Open();
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "update trxpayrolldetails set status=" +
                            ParameterHelper.TRXSTATUS_REJECT + ", description='Rejected by System' where pid='" + payroll.Id + "'";
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

        private Boolean PayrollParsingFile(TrxPayroll payroll, ISession session)
        {
            Boolean _resultparse = false;

            PayrollLog(session, "Start parsing file..." + payroll.Description);

            try
            {
                //UserMap umap = session[Context.User.Identity.Name] as UserMap; 
                ITransaction transaction = session.BeginTransaction();
                String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                ClientAccount ca = session.Load<ClientAccount>(int.Parse(desc[1]));

                ILog logs = LogManager.GetLogger(typeof(RunPayroll));

                /* Start parsing file */

                FileStream fs = new FileStream(payroll.FileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                String detail = "";
                int line = 0;
                int errColumn = 0;
                while (true)
                {
                    errColumn++;
                    line++;
                    detail = sr.ReadLine();
                    if (null == detail) break;

                    logs.Info("" + detail + "" + line);
                    PayrollLog(session, "" + detail + "" + line);
                    if (!detail.Equals(""))
                    {
                        logs.Info("masuk if");
                        TrxPayrollDetail trx = new TrxPayrollDetail();
                        try
                        {
                            String[] column = detail.Split(new String[] { "," }, StringSplitOptions.None);
                            switch (column[0])
                            {
                                case "NO":
                                    break;
                                case "COUNT":
                                    break;
                                case "TOTAL":
                                    break;
                                case "CHECK":
                                    break;
                                case "DEBACC":
                                    break;
                                default:
                                    TrxPayrollDetail trxDetail = new TrxPayrollDetail();
                                    trxDetail.Pid = payroll.Id;
                                    trxDetail.Name = column[1];
                                    trxDetail.Account = column[2];
                                    trxDetail.Amount = Double.Parse(column[3]);
                                    trxDetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                    trxDetail.Description = "";
                                    payroll.AddDetail(trxDetail);
                                    session.Save(trxDetail);
                                    session.Flush();
                                    _resultparse = true;
                                    trxDetail = null;
                                    break;
                            }
                            //logs.Info("result true");
                        }
                        catch (Exception ex)
                        {
                            PayrollLog(session, "Eror on detail line : " + line + "/" + detail);
                            logs.Info("Erorr on detail line " + line + "/" + detail);

                            _resultparse = false;
                            break;
                        }
                        trx = null;
                    }

                    //sr.Close();
                    //fs.Close();
                }
                logs.Info(_resultparse.ToString() + "commit");
                if (!_resultparse)
                {
                    PayrollLog(session, "result = " + _resultparse);
                    logs.Info("result :" + _resultparse);

                    transaction.Rollback();
                    transaction.Dispose();
                    payroll.Status = ParameterHelper.TRXSTATUS_REJECT;
                    payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_REJECT + "File Error, Error on detail line : " + line);
                }
                else
                {
                    PayrollLog(session, "result = false");
                    logs.Info("result false");

                    transaction.Commit();
                    transaction.Dispose();
                    payroll.Status = ParameterHelper.TRXSTATUS_PROCESS;
                }
                /* End parsing file */

                /* Update payroll status */
                payroll.LastUpdate = DateTime.Now;
                session.Update(payroll);
                session.Flush();

                fs.Dispose();
                sr.Dispose();

                //umap = null;
                transaction = null;
                ca = null;
                desc = null;
            }
            catch (Exception exc)
            {

                PayrollLog(session, "FATAL, " + exc.Message);
            }

            PayrollLog(session, "End Parsing File");

            return _resultparse;
        }

        //Transfer file to host
        private Boolean TransferFileToHost(string path, ISession session)
        {
            Boolean _result = true;

            #region comment
            //String[] file = path.Split(new String[] { "\\" }, StringSplitOptions.None);
            //ILog logs = LogManager.GetLogger(typeof(RunPayroll));


            //Parameter param = session.Load<Parameter>("SERVER_FTP_SNAP");
            //String port = param.Data;
            //String _serverUri = port;

            //param = session.Load<Parameter>("SERVER_FTP_SNAP_USER");
            //port = param.Data;
            //String _serverUser = port;

            //param = session.Load<Parameter>("SERVER_FTP_SNAP_PASS");
            //port = param.Data;
            //String _serverPassword = port;

            //param = session.Load<Parameter>("FOLDER_PAYROLL_REQUEST_HOST");
            //port = param.Data;
            //String _urlRemote = port;



            //Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //try
            //{

            //    clientSocket.Connect(_serverUri, 21);
            //    logs.Info("server uri --> " + _serverUri + " try to connect..");
            //    PayrollLog(session, "server uri --> " + _serverUri + " try to connect..");

            //    FTPHelper ff = new FTPHelper();
            //    ff.setDebug(false);
            //    ff.setRemoteHost(_serverUri);
            //    logs.Info(ff + _serverUri);
            //    PayrollLog(session, "" + ff + _serverUri);

            //    ff.setRemoteUser(_serverUser);
            //    logs.Info(ff + _serverUser);
            //    PayrollLog(session, "" + ff + _serverUser);

            //    ff.setRemotePass(_serverPassword);
            //    logs.Info(ff + _serverPassword);
            //    PayrollLog(session, "" + ff + _serverPassword);

            //    ff.setSocket(clientSocket);
            //    logs.Info(clientSocket);
            //    PayrollLog(session, "" + ff + clientSocket);

            //    Thread.Sleep(1000);
            //    ff.SendUserPassword();
            //    if (ff.isLogin())
            //    {
            //        ff.chdir(_urlRemote);
            //        logs.Info(_urlRemote);
            //        PayrollLog(session, " url remote : " + _urlRemote);

            //        ff.setBinaryMode(false);

            //        logs.Info(path);
            //        PayrollLog(session, "path : " + path);

            //        ff.upload(path);
            //        ff.close();

            //        _result = true;
            //        logs.Info("result login ftp" + _result);
            //        PayrollLog(session, "result login ftp " + _result);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    PayrollLog(session, "Error on connecting socket: " + _serverUri + ex.StackTrace);
            //    logs.Info("error on connecting socket : " + _serverUri + ex.StackTrace);
            //}
            //file = null;
            //clientSocket = null;
            #endregion

            try
            {
                Parameter param = session.Load<Parameter>("FOLDER_PAYROLL_IN_OUT_HOST");
                string FolderInOut = param.Data;
                
                FileInfo file = new FileInfo(path);
                file.CopyTo(FolderInOut + "\\" + file.Name);
            }
            catch(Exception e)
            {
                _result = false;
            }


            return _result;
        }


        private Boolean ProcessPayroll(ISession session, TrxPayroll payrolls, String _payrollFileName, ILog logs)
        {
            Boolean _result = true;
            try
            {
                //ILog logs = LogManager.GetLogger(typeof(RunPayroll));
                logs.Info("--Masuk Process Detail Payroll--");
                PayrollLog(session, "--Masuk Process Detail Payroll--");

                


                Parameter param = session.Load<Parameter>("FUNCTION_PAYROLL_ID");
                String port = param.Data;
                int fid = int.Parse(port);

                Parameter paramMusim = session.Load<Parameter>("MUSIMMAS_CLIENT_HANDLE");
                String ports = paramMusim.Data;

                /*tambahan after problem*/
                PayrollLog(session, "------------------ awal ----------");
                foreach (TrxPayrollDetail xx in payrolls.TrxPayrollDetail)
                {
                    PayrollLog(session, xx.Id.ToString() + "--" + xx.Name + "-->" + xx.Account);
                }

                //20170409 sayedzul add filter IFT Only
                IList<TrxPayrollDetail> lsdetailpayroll = payrolls.TrxPayrollDetail.Where(x => (x.InstructionCode.Equals(TotalOBHelper.IFT) || String.IsNullOrEmpty(x.InstructionCode)) && x.Status != ParameterHelper.TRXSTATUS_REJECT).ToList<TrxPayrollDetail>();
                    //session.CreateQuery("from TrxPayrollDetail WHERE PID = ? and INSTRUCTIONCODE = '"+TotalOBHelper.IFT+"'")
                    //    .SetString(0, payrolls.Id)
                    //    .List<TrxPayrollDetail>();


                PayrollLog(session, "------------------ ubah lg xxxx langsung ----------");
                foreach (TrxPayrollDetail xx in lsdetailpayroll)
                {
                    PayrollLog(session, xx.Id.ToString() + "--" + xx.Name + "-->" + xx.Account);
                }

                TrxPayroll payroll = session.Load(typeof(TrxPayroll), payrolls.Id) as TrxPayroll;
                PayrollLog(session, "Get new object with pid : " + payrolls.Id);

                //IList<TrxPayrollDetail> lpdetail = session.CreateCriteria(typeof(TrxPayrollDetail))
                //                .Add(Expression.Eq("PID", payroll.Id))
                //                .List<TrxPayrollDetail>();
                //rfq
                //lpdetail.Sum(a => a.Amount)
                IList<TrxPayrollDetail> lpdetail = payroll.TrxPayrollDetail.Where(x => (x.InstructionCode == TotalOBHelper.IFT || String.IsNullOrEmpty(x.InstructionCode)) && x.Status != ParameterHelper.TRXSTATUS_REJECT).ToList<TrxPayrollDetail>();
                
                //PayrollLog(session, "------------------ ubah lg ----------");
                //foreach (TrxPayrollDetail xx in payroll.TrxPayrollDetail)
                //{
                //    PayrollLog(session, xx.Id.ToString() + "--" + xx.Name + "-->" + xx.Account);
                //}
                


                PayrollLog(session, "Getting payroll detail list, generating file to host...");
                String[] info = payroll.CreatedBy.Split(new String[] { "/" }, StringSplitOptions.None);
                //String[] info = payroll.CreatedBy.Split(new String[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                String _client = info[0];

                String[] array = ports.Split(new String[] { "|" }, StringSplitOptions.None);

                if (Array.IndexOf(array, _client.ToUpper().Trim()) != -1)
                {
                    Parameter paramex = session.Load<Parameter>("FUNCTION_PAYROLL_ID_MUSIMAS");
                    String portz = paramex.Data;
                    fid = int.Parse(portz);
                    PayrollLog(session, "PAYROLL MUSIM MAS" + "client : " + _client);
                }

                PayrollLog(session, "payroll fid = " + fid);

                String _user = info[1];
                UserMap umap = session.CreateCriteria(typeof(UserMap))
                    .Add(Expression.Eq("ClientHandle", _client))
                    .Add(Expression.Eq("UserHandle", _user))
                    .UniqueResult<UserMap>();

                PayrollLog(session, "umap : " + umap + "with client : " + umap.Client + "clienthandle = " + umap.ClientHandle + "cliententity = " + umap.ClientEntity);

                //logs.Info("Transaction Fee >> " + GetTransactionFee(session, fid, umap).ToString());
                //logs.Info("Biaya ajah >> " + (double)GetTransactionFee(session, fid, umap));

                logs.Info("Transaction Fee >> " + payroll.Fee.ToString());

                double feeAmount = payroll.Fee * ParameterHelper.GetInteger("SYSTEM_DECIMAL_POINT", session);
                PayrollLog(session, "fee client ini per record : " + feeAmount);

                ClientAccount ca = session.Load<ClientAccount>(int.Parse(desc[1]));
                //20170411 - sayedzul ad handler rek debet
                string debetAccount = ca.Number;
                if (payroll.IsPayrollBankLain == 1)
                    debetAccount = ParameterHelper.GetString("PAYROLL_IA_KREDIT", session);

                String _fhead = _template_fh; //"FHF-------------NV--------D          STS";
                _fhead = _fhead.Replace("F-------------N", _payrollFileName.PadRight(15, ' '));
                _fhead = _fhead.Replace("V--------D", DateTime.Now.ToString("dd/MM/yyyy"));
                _fhead = _fhead.Replace("STS", "999");
                pfile(session, _fhead, _payrollFileName);
                //pfile.Close();

                String _rhead = _template_ch; // "CHC-------NSTS";
                _rhead = _rhead.Replace("C-------N", umap.ClientHandle.PadRight(40, ' ').Substring(0, 9));
                _rhead = _rhead.Replace("STS", "999");
                pfile(session, _rhead, _payrollFileName);
                //pfile.Close();

                long _totalHeaderAmount = long.Parse(lpdetail.Sum(a => a.Amount).ToString("##############"));

                String _chead = _template_bh; //"BHR------------------FC--------------ACURV--------DR-----------------------------------------S999";
                _chead = _chead.Replace("R------------------F", DateTime.Now.ToString("yyyyMMddhhmmss").PadRight(20, '0'));
                _chead = _chead.Replace("C--------------A", debetAccount.PadLeft(16, '0'));
                _chead = _chead.Replace("CUR", ca.Currency.Trim());
                _chead = _chead.Replace("V--------D", DateTime.Now.ToString("dd/MM/yyyy"));
                _chead = _chead.Replace("R-----------------------------------------S", payroll.FileDescription.PadRight(50, ' '));
                pfile(session, _chead, _payrollFileName);
                //pfile.Close();

                int _line = 1;
                long _totalDetailAmount = 0;
                //float _totalFeeAmount = 0;
                double _totalFeeAmount = 0;


                PayrollLog(session, "Getting payroll detail list, generating file to host...0");

                //IList<TrxPayrollDetail> lpdetail = payroll.TrxPayrollDetail;




                //PayrollLog(session, payroll.TrxPayrollDetail.ToString());


                foreach (TrxPayrollDetail pdetail in lpdetail)
                {
                    if (pdetail.Status != 4)//rfq add 24032015, (hanya kirim yg valid)
                    {
                        PayrollLog(session, "" + pdetail.Name + "" + pdetail.Account + "" + pdetail.Amount);
                        PayrollLog(session, "Getting payroll detail list, generating file to host...1");

                        String _ref = DateTime.Now.ToString("ddMMyyyyhhmmss") + _line++.ToString().PadLeft(6, '0');
                        String _detail = _template_bd; //"BDR------------------NC--------------ACC RC-------------MD-------------MDH------------MCH------------MIBT----------BYIBT----------SLTRX----------BYTRX----------SLSTSR--------------------------------------S";


                        PayrollLog(session, "Getting payroll detail list, generating file to host...2");

                        _detail = _detail.Replace("R------------------N", _ref);
                        _detail = _detail.Replace("C--------------A", pdetail.Account.PadLeft(16, '0'));


                        PayrollLog(session, "Getting payroll detail list, generating file to host...3");

                        _detail = _detail.Replace("CCR", ca.Currency.Trim());
                        _detail = _detail.Replace("C-------------M", pdetail.Amount.ToString("000000000000000"));


                        PayrollLog(session, "Getting payroll detail list, generating file to host...4");

                        _detail = _detail.Replace("D-------------M", (pdetail.Amount + (long)feeAmount).ToString("000000000000000"));

                        logs.Info("Fee+Amount" + _detail);

                        PayrollLog(session, "Getting payroll detail list, generating file to host...5");

                        _detail = _detail.Replace("DH------------M", feeAmount.ToString("000000000000000"));
                        _detail = _detail.Replace("CH------------M", "000000000000000");
                        _detail = _detail.Replace("IBT----------BY", "000000010000000");
                        _detail = _detail.Replace("IBT----------SL", "000000010000000");
                        _detail = _detail.Replace("TRX----------BY", "000000010000000");
                        _detail = _detail.Replace("TRX----------SL", "000000010000000");


                        PayrollLog(session, "Getting payroll detail list, generating file to host...6");


                        _detail = _detail.Replace("STS", "999");
                        _detail = _detail.Replace("R--------------------------------------S", payroll.FileDescription.PadRight(40, ' '));


                        PayrollLog(session, feeAmount.ToString("##############") + ">>> " + pdetail.Amount.ToString("##############"));


                        _totalDetailAmount = _totalDetailAmount + long.Parse(pdetail.Amount.ToString("##############"));
                        PayrollLog(session, "fee amount to string : " + feeAmount.ToString("##############"));
                        _totalFeeAmount = _totalFeeAmount + feeAmount;
                        PayrollLog(session, "jumlah fee (hitung per record) : " + _totalFeeAmount.ToString("##############") + "record no : " + (_line - 1));

                        pfile(session, _detail, _payrollFileName);
                    }
                }
                PayrollLog(session, "total fee batch ini : " + _totalFeeAmount.ToString("##############"));

                String _btrail = _template_bt;
                _btrail = _btrail.Replace("RC------------T", (_line - 1).ToString().PadLeft(15, '0'));
                _btrail = _btrail.Replace("DB------------T", (_totalDetailAmount + (long)_totalFeeAmount).ToString().PadLeft(15, '0'));
                _btrail = _btrail.Replace("CR------------T", _totalDetailAmount.ToString().PadLeft(15, '0'));
                pfile(session, _btrail, _payrollFileName);


                String _ctrail = _template_ct;
                _ctrail = _ctrail.Replace("P-------------T", "1".PadLeft(15, '0'));
                pfile(session, _ctrail, _payrollFileName);


                String _ftrail = _template_ft;
                _ftrail = _ftrail.Replace("DB------------T", "1".PadLeft(15, '0'));
                _ftrail = _ftrail.Replace("DB------------A", (_totalDetailAmount + (long)_totalFeeAmount).ToString().PadLeft(15, '0'));
                _ftrail = _ftrail.Replace("CR------------T", (_line - 1).ToString().PadLeft(15, '0'));
                _ftrail = _ftrail.Replace("CR------------A", _totalDetailAmount.ToString().PadLeft(15, '0'));
                pfile(session, _ftrail, _payrollFileName);

                //if (_totalDetailAmount == _totalHeaderAmount)
                //{
                //    PayrollLog(session, "Processing file detail and footer confirm.");
                //}
                //else
                //{
                //    PayrollLog(session, "FATAL, Processing file detail and footer not confirm. : {0} - {1}" + _totalHeaderAmount.ToString("###############") + _totalDetailAmount.ToString("###############"));
                //    _result = false;
                //}

                info = null;

                lpdetail = null;

                desc = null;
                umap = null;
                ca = null;

                //cfg = null;
                GC.Collect();

            }
            catch (Exception ex)
            {
                //PayrollLog(session, "Error on process id : " + payroll.Id + ex.Message + ">> " + ex.StackTrace);
                payrolls.ErrorDescription += "Error on process " + ex.Message + ">> " + ex.StackTrace + " >> " + ex.InnerException;
                logs.Error("Error on process " + ex.Message + ">> " + ex.StackTrace + " >> " + ex.InnerException);
                _result = false;
            }

            PayrollLog(session, "End payroll detail list");
            logs.Info("End Payroll detail list");
            return _result;
        }

        private Boolean CekRunningFolder(String path)
        {
            Boolean _result = true;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return _result;
        }

        private String GeneratePayrollFileName(ISession session, TrxPayroll pay, ILog log)
        {
            log.Info("Start Generate File");
            String _result = "";
            try
            {
                //String _result = "";
                //String _year = date.Year.ToString().Remove(0, 3);
                //String _date = date.DayOfYear.ToString();
                ////_result = "IPAY" + ToJulianDate(DateTime.Now);
                //_result = "IPAY" + _year + _date + "." + payrollSequence;
                //return _result;
                DateTime dt = DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyyy") + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                DateTime date = DateTime.Now;
                //String date2 = date.ToString("yyyy-MM-dd");
                PayrollLog(session, "date payroll : " + dt);
                
                String index;

                String _year = date.Year.ToString().Remove(0, 3);
                String _date = date.DayOfYear.ToString();

                //Set sequence on IPAY file by DB
                PayrollLog(session, "Get into generate payroll name");
                TrxPayrollSequence payseq = new TrxPayrollSequence();
                IList<TrxPayrollSequence> lpayrollSeq = session.CreateCriteria(typeof(TrxPayrollSequence))
                             .Add(Expression.Eq("ProcessDate", dt))
                             .AddOrder(Order.Desc("Sequence"))
                             .List<TrxPayrollSequence>();

                if (lpayrollSeq.Count > 0)
                {
                    PayrollLog(session, "Index continue");
                    index = (lpayrollSeq[0].Sequence + 1).ToString();
                    payseq.Pid = pay.Id;
                    payseq.ProcessDate = dt;
                    payseq.Sequence = int.Parse(index);
                    PayrollLog(session, "index :" + index);
                    session.Save(payseq);
                    session.Flush();
                }
                else
                {
                    PayrollLog(session, "First Sequence of the day");
                    index = "1";
                    payseq.Pid = pay.Id;
                    payseq.ProcessDate = dt;
                    payseq.Sequence = int.Parse(index);
                    session.Save(payseq);
                    session.Flush();

                }
                String filler = index.PadLeft(3, '0');
                _result = "IPAY" + _year + _date + "." + filler;
                PayrollLog(session, "Payroll filename = " + _result);
            }
            catch (Exception e)
            {
                log.Error("Message : " + e.Message + ">>> Inner : " + e.InnerException + ">>> Stack : " + e.StackTrace);
            }
            return _result;
        }


        private float GetTransactionFee(ISession session, int fid, UserMap umap)
        {
            float _result = 0;
            try
            {
                ClientMatrix cm = session.Load<ClientMatrix>(umap.ClientEntity);
                AuthorityHelper ah = new AuthorityHelper(cm.Matrix);
                _result = ah.GetTransactionFee(session, fid, 1);

                ah = null;
                cm = null;
            }
            catch { }
            return _result;
        }


        private void PayrollLog(ISession session, String Description)
        {
            ILog logs = LogManager.GetLogger(typeof(RunPayroll));
            try
            {
                String _date_folder = DateTime.Now.ToString("yyyyMMdd");

                Parameter param = session.Load<Parameter>("FOLDER_PAYROLL_LOGS");
                String port = param.Data;
                String _payroll_Log_dir = port + "\\" + _date_folder;

                String path = _payroll_Log_dir;
                String fName = "\\" + "RunPayroll " + DateTime.Now.ToString("ddMMyyyy") + ".log";
                path = path + fName;

                String time = DateTime.Now.ToString();

                StreamWriter log = new StreamWriter(path, true);
                log.WriteLine("" + time + " " + Description);
                log.Close();
            }
            catch (Exception e)
            {
                logs.Error(e.Message + ">>>" + e.InnerException + ">>>" + e.StackTrace);
            }

        }

        private void pfile(ISession session, String fileData, String _payrollFileName)
        {


            String _date_folder = DateTime.Now.ToString("yyyyMMdd");

            Parameter param = session.Load<Parameter>("FOLDER_PAYROLL_INPUT_HOST");
            String port = param.Data;
            String _payroll_input_dir = port + "\\" + _date_folder + "\\";

            String path = _payroll_input_dir + _payrollFileName;

            StreamWriter processFile = new StreamWriter(path, true);

            #region keperluan DEVONLY (di kommen)
            //Start Replace, khusus dev
            //rfq_sesuaikan dg tanggal brinets (khusus buat DEV)
            //19082014
            //19/08/2014
            //20140819
            //get dari DB
            //Parameter param1 = session.Load<Parameter>("PAYROLL_DEV_PARAM1");
            //String dataBuang1 = param1.Data;
            //Parameter param2 = session.Load<Parameter>("PAYROLL_DEV_PARAM2");
            //String dataBuang2 = param2.Data;
            //Parameter param3 = session.Load<Parameter>("PAYROLL_DEV_PARAM3");
            //String dataBuang3 = param3.Data;

            //string buang1 = DateTime.Now.ToString("ddMMyyyy");
            //string buang2 = DateTime.Now.ToString("dd/MM/yyyy");
            //string buang3 = DateTime.Now.ToString("yyyyMMdd");

            //fileData = fileData.Replace(buang1, dataBuang1);
            //fileData = fileData.Replace(buang2, dataBuang2);
            //fileData = fileData.Replace(buang3, dataBuang3);
            //end replace
            #endregion

            
            processFile.WriteLine(fileData);
            PayrollLog(session, "pfile = " + fileData);
            processFile.Close();

        }

        //private Boolean _Currency(TrxPayroll payroll, ISession session)
        //{
        //    ILog logs = LogManager.GetLogger(typeof(RunPayroll));
        //    PayrollLog(session, "--Currency Check--");
        //    logs.Info("--Currency Check--");

        //    Boolean _resultCurrency = false;
        //    Boolean _loopAccount = true;
        //    FileStream fs = new FileStream(payroll.FileName, FileMode.Open, FileAccess.Read);
        //    StreamReader sr = new StreamReader(fs);
        //    String detail = "";
        //    int line = 0;
        //    int errColumn = 0;

        //    while (_loopAccount)
        //    {
        //        PayrollLog(session, "while(true) -- Currency");
        //        logs.Info("while(true) -- Currency");
        //        errColumn++;
        //        line++;
        //        detail = sr.ReadLine();
        //        if (null == detail) break;

        //        if (!detail.Equals(""))
        //        {
        //            try 
        //            {
        //                String[] column = detail.Split(new String[] { "," }, StringSplitOptions.None);
        //                PayrollLog(session, "column : " + column);
        //                logs.Info("column : " + column);
        //                switch (column[0])
        //                {
        //                    case "NO":
        //                        break;
        //                    case "COUNT":
        //                        break;
        //                    case "TOTAL":
        //                        break;
        //                    case "CHECK":
        //                        break;
        //                    case "DEBACC":
        //                        break;
        //                    default:
        //                        PayrollLog(session,"check column");
        //                        logs.Info("check column");
        //                        if (!column[2].Equals("ACCOUNT"))
        //                        {
        //                            PayrollLog(session, "isi account after parse : " + column[2]);
        //                            logs.Info("isi account after parse: " + column[2]);

        //                            if (column[2].Substring(column[2].Length - 11, 2).Equals("01"))
        //                            {
        //                                PayrollLog(session, "isi account after check idr " + column[2]);
        //                                logs.Info("isi account after check idr: " + column[2]);
        //                                _resultCurrency = true;

        //                            }
        //                            else
        //                            {
        //                                PayrollLog(session, "account not compatible with idr : " + column[2]);
        //                                logs.Info("account not compatible with idr: " + column[2]);
        //                                _resultCurrency = false;
        //                                _loopAccount = false;
        //                                payroll.Status = ParameterHelper.TRXSTATUS_REJECT;
        //                                payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_REJECT + " - Invalid Currency, Error on record : " + (line - 1).ToString());
        //                                payroll.LastUpdate = DateTime.Now;
        //                                break;
        //                            }
        //                        }
        //                        break;

        //                }  
        //            }
        //            catch(Exception ex)
        //            {
        //                PayrollLog(session, ex.Message);
        //                _resultCurrency = false;
        //                break;
        //            }
        //        }

        //    }
        //    session.Update(payroll);
        //    session.Flush();
        //    fs.Dispose();
        //    sr.Dispose();
        //    PayrollLog(session, "result currency/proceed after proses : " + _resultCurrency);
        //    PayrollLog(session, "End Check IDR Currency");
        //    return _resultCurrency;

        //}

        private MessageMBASETransactionResponse SendTransaction(ISession session, String filename)
        {

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
            }
            catch (Exception ex)
            {
                PayrollLog(session, "exception : " + ex.Message + "--->" + ex.StackTrace);
            }

            PayrollLog(session, "response :" + response.statuscode.Trim() + "send trx response : " + response);
            return response;

        }

        private MessageABCSTransactionResponse inqAccBRIInterface(ISession session, String AccNo, String TellerID)
        {
            String curr = _Currency(AccNo);
            String accType = _AccountType(AccNo);
            String trancode = "1000";

            PayrollLog(session, "Inq with acc " + AccNo + "teller :" + TellerID);
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
            transaction.initiateABCS(ref dSocketHeader, ref dMiddleWareHeader, ref dAbcsHeader, ref dAbcsMessage);

            dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode.Trim();
            dAbcsMessage.Rows[0]["TRANSACTIONCODE"] = trancode.Trim();
            dAbcsMessage.Rows[0]["BUCKET1"] = AccNo.Trim();
            dAbcsMessage.Rows[0]["BUCKET7"] = "";
            dAbcsMessage.Rows[0]["BUCKET8"] = "";
            dAbcsMessage.Rows[0]["BUCKET19"] = DateTime.Now.ToString("ddMMyy");  //effective date
            dAbcsMessage.Rows[0]["BUCKET20"] = ""; //orig jseq for reverse
            dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";
            dAbcsMessage.Rows[0]["TELLERREMARK1"] = "Inquiry Account";
            dAbcsMessage.Rows[0]["TELLERREMARK2"] = "CMS";
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY1"] = curr.Trim();
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY2"] = curr.Trim();
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY3"] = "IDR";

            //dMiddleWareHeader.Rows[0]["TRANCODE"] = 7524;
            //dMiddleWareHeader.Rows[0]["SCENARIONO"] = "CASHMANAGEMENT00";
            //dAbcsMessage.Rows[0]["TRANSACTIONCODE"] = 7524;
            //dAbcsMessage.Rows[0]["CIFKEY"] = "54";
            //dAbcsMessage.Rows[0]["PAYEENAME"] = "3300000010";
            //dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";

            //dMiddleWareHeader.Rows[0]["TRANCODE"] = 7512;
            //dMiddleWareHeader.Rows[0]["SCENARIONO"] = "CASHMANAGEMENT00";
            //dAbcsMessage.Rows[0]["TRANSACTIONCODE"] = 7512;
            //dAbcsMessage.Rows[0]["CIFKEY"] = "021";
            //dAbcsMessage.Rows[0]["PAYEENAME"] = "5550001";
            //dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";

            request.branchcode = TellerID.Substring(0, 4);
            request.tellerid = TellerID;
            request.supervisorid = "";
            request.origjournalseq = "";
            request.dSocketHeader = dSocketHeader;
            request.dMiddleWareHeader = dMiddleWareHeader;
            request.dAbcsHeader = dAbcsHeader;
            request.dAbcsMessage = dAbcsMessage;
            request.applicationname = "CMS";

            PayrollLog(session, "before request");
            PayrollLog(session, "request" + request);
            try
            {
                response = transaction.doABCSTransaction(request);

            }
            catch (Exception ex)
            {
                //EventLogger.Write("Inquiry Acc BriInterface :: " + ex.Message, EventLogger.ERROR);
                PayrollLog(session, "exception : " + ex.Message + "--->" + ex.StackTrace);
            }
            PayrollLog(session, "response :" + response.statuscode.Trim() + "rep : " + response);
            return response;


        }

        private String _AccountType(String AccountNo)
        {
            switch (AccountNo.Substring(AccountNo.Length - 3, 1))
            {
                case "1":
                    return ("L");
                case "5":
                    return ("S");
                case "3":
                    return ("G");
                case "9":
                    return ("D");
                default:
                    return (" ");
            }
        }

        private String _Currency(String AccountNo)
        {
            switch (AccountNo.Substring(AccountNo.Length - 11, 2))
            {
                case "01":
                    return ("IDR");
                case "02":
                    return ("USD");
                case "03":
                    return ("SGD");
                case "04":
                    return ("JPY");
                case "05":
                    return ("GBP");
                case "06":
                    return ("HKD");
                case "07":
                    return ("DEM");
                case "08":
                    return ("MYR");
                case "09":
                    return ("AUD");
                case "10":
                    return ("CAD");
                case "11":
                    return ("CHF");
                case "18":
                    return ("EUR");
                case "19":
                    return ("THB");
                case "20":
                    return ("SEK");
                case "21":
                    return ("SAR");
                default:
                    return (" ");
            }
        }
    }
}
