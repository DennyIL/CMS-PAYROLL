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
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace BRIChannelSchedulerNew.Payroll
{
    class GetPayrollResponse : IStatefulJob
    {
        private static Configuration cfg;
        private static ISessionFactory factory;

        #region IJob Members

        void IJob.Execute(JobExecutionContext context)
        {
            //cfg = new Configuration();
            //cfg.AddAssembly("BRIChannel");
            ILog logs = LogManager.GetLogger(typeof(GetPayrollResponse));
            logs.Info("==== Get Payroll Response Request ===");
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


                String _date_folder = DateTime.Now.ToString("yyyyMMdd");


                Parameter param = session.Load<Parameter>("FOLDER_PAYROLL_LOGS");
                string port = param.Data;
                String _payroll_Log_dir = port + "\\" + _date_folder;

                CekRunningFolder(_payroll_Log_dir);

                param = session.Load<Parameter>("FOLDER_PAYROLL_INPUT_HOST");
                port = param.Data;
                String _payroll_input_dir = port + "\\" + _date_folder;

                param = session.Load<Parameter>("FOLDER_PAYROLL_OUTPUT_HOST");
                port = param.Data;
                String _payroll_output_dir = port + "\\" + _date_folder;

                CekRunningFolder(_payroll_input_dir);

                param = session.Load<Parameter>("FOLDER_PAYROLL_RESPONSE_HOST");
                port = param.Data;
                String _payroll_response_dir = port;

                CekRunningFolder(_payroll_output_dir);

                PayrollLog(session, "==== Get Payroll Response Request ===");

                logs.Info("Response payroll scheduller process start...");
                PayrollLog(session, "Response payroll scheduller process start...");


                IList<TrxPayroll> lpayroll = session.CreateCriteria(typeof(TrxPayroll))
                    .Add(Expression.Eq("StatusIFT", ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_MBASE))
                    .Add(Expression.Lt("LastUpdate", DateTime.Now))
                    .AddOrder(Order.Asc("CreatedTime"))
                    .List<TrxPayroll>();
                logs.Info("Get Payroll Response :: Data to process = " + lpayroll.Count.ToString());

                foreach (TrxPayroll payroll in lpayroll)
                {
                    String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                    //payroll.Status = ParameterHelper.TRXSTATUS_RESPONSE_PROCESS;
                    payroll.ErrorDescription += "||File Response On Process..";
                    session.Update(payroll);
                    session.Flush();

                    logs.Info("Processing response file id : " + payroll.Id + " --> " + desc[2]);
                    PayrollLog(session, "Processing file id : " + payroll.Id + " --> " + desc[2]);

                    String _fileName = desc[2].Replace("IPAY", "OPAY");
                    if (!desc[0].Equals("PROCESSED"))
                    {
                        break;
                        payroll.ErrorDescription += "||Description not PROCESSED";
                        session.Update(payroll);
                        session.Flush();
                    }

                    logs.Info(" " + _payroll_response_dir + "\\" + _fileName);
                    PayrollLog(session, "" + _payroll_response_dir + "\\" + _fileName);

                    logs.Info(">>>>>Mau masuk GetResponseFile");
                    string outErrorDesc = "";

                    try
                    {
                        //rfq, 20161202, jika statusnya 20
                        if (payroll.StatusIFT == ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_MBASE)
                        {
                            if (GetResponseFile(_payroll_response_dir, _payroll_output_dir, _fileName, session, out outErrorDesc))
                            {
                                payroll.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_FILEEXIST;
                                payroll.LastUpdate = DateTime.Now;
                                payroll.ErrorDescription += "File respon found. Ready to read file response";
                                session.Update(payroll);
                                session.Flush();
                                //ParseResponseFile(payroll, _payroll_output_dir + "\\" + _fileName, session);
                                //logs.Info("No Response Payroll : " + _fileName);
                                //PayrollLog(session, "No response payroll : " + _fileName);
                                //break;
                            }
                            else
                            {
                                if (outErrorDesc.Contains("[RETRY]"))
                                {
                                    Parameter paramTryAgain = session.Load<Parameter>("PAYROLL_READRESPONSE_TIMETRYAGAIN");
                                    int tryAgain = int.Parse(paramTryAgain.Data);
                                    DateTime lUpdate = DateTime.Now.AddMinutes(tryAgain);
                                    payroll.StatusIFT = ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_MBASE;
                                    payroll.LastUpdate = lUpdate;
                                    payroll.ErrorDescription += "||" + outErrorDesc + "Try Again at " + lUpdate.ToString("dd/MM/yyyy HH:mm:ss");
                                }
                                else
                                {
                                    payroll.ErrorDescription += "||20|" + outErrorDesc;
                                    payroll.StatusIFT = ParameterHelper.PAYROLL_EXCEPTION;
                                }
                                session.Update(payroll);
                                session.Flush();
                                break;
                            }
                        }
                    }
                    catch (Exception eek)
                    {
                        logs.Error(eek.Message + eek.StackTrace);
                    }
                    

                    /* Only one payroll process */
                    break;
                }
            }
            catch (Exception ee)
            {
                logs.Error("Scheduler Get Response Payroll failed to start : " + ee.Message + ee.StackTrace);
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


        private void ParseResponseFile(TrxPayroll payroll, String fileName, ISession session)
        {
            ILog logs = LogManager.GetLogger(typeof(GetPayrollResponse));
            logs.Info(">>>>Start parsing file...." + fileName);
            PayrollLog(session, "Start parsing file...." + fileName);

            try
            {
                //ITransaction tx = session.BeginTransaction();
                //logs.Info("isi tx --> " + tx + "filename : " + fileName);
                //PayrollLog(session,"isi tx --> "+tx + "filename : "+fileName);

                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                logs.Info("isi fs : " + fs);
                PayrollLog(session, "isi fs : " + fs);

                StreamReader sr = new StreamReader(fs);
                logs.Info("isi sr : " + fs);
                PayrollLog(session, "isi sr : " + fs);

                Boolean _next = true;
                Boolean _confirm = true;
                double totamt = 0;
                double totfee = 0;
                double totaltrx = 0;
                String accfrom = "";
                while (_next && _confirm)
                {
                    logs.Info("isi boolean next : " + _next + "isi boolean confirm : " + _confirm);
                    PayrollLog(session, "isi boolean next : " + _next + "isi boolean confirm : " + _confirm);

                    String detail = sr.ReadLine();

                    logs.Info("isi detail : " + detail);
                    PayrollLog(session, "isi detail : " + detail);

                    if (null == detail) break;
                    _confirm = false;
                    switch (detail.Substring(0, 2))
                    {
                        case "FH":
                            String _response = detail.Substring(37, 3);
                            if (!"000".Equals(_response))
                            {
                                logs.Info("File header error, code : " + _response);
                                PayrollLog(session, "File header error , code : " + _response);

                                payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                                payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_REJECT);
                                payroll.LastUpdate = DateTime.Now;
                                payroll.ErrorDescription += "Status 20|File header (FH) error, code : " + _response;
                                session.Update(payroll);
                                _next = false;
                            }
                            else
                            {
                                logs.Info("File header succesfully proceed");
                                PayrollLog(session, "File header succesfully proceed");
                                _confirm = true;
                            }
                            break;

                        case "CH":
                            _response = detail.Substring(11, 3);
                            if (!"000".Equals(_response))
                            {
                                logs.Info("Client header error, code : " + _response);
                                PayrollLog(session, "Client header error, code : " + _response);

                                payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                                payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_REJECT);
                                payroll.LastUpdate = DateTime.Now;
                                payroll.ErrorDescription += "||Status 20|Client Header(CH) error, code : " + _response;
                                session.Update(payroll);
                                _next = false;
                            }
                            else
                            {
                                logs.Info("Client header succesfully proceed");
                                PayrollLog(session, "Client header succesfully proceed");
                                _confirm = true;
                            }
                            break;

                        case "BH":
                            _response = detail.Substring(101, 3);
                            accfrom = detail.Substring(23, 15);
                            if (!"000".Equals(_response))
                            {
                                logs.Info("Batch header error, code : " + _response);
                                PayrollLog(session, "Batch header error, code : " + _response);

                                if (_response.Equals("005"))
                                {
                                    payroll.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    payroll.Description = payroll.Description.Replace("PROCESSED", "REJECT - SALDO TIDAK CUKUP (Rekening tidak terdebet)");
                                }
                                else
                                {
                                    payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                                    payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_REJECT);
                                }

                                payroll.LastUpdate = DateTime.Now;
                                payroll.ErrorDescription += "||Status 20|Batch Header (BH) error, code : " + _response;
                                session.Update(payroll);
                                _next = false;
                            }
                            else
                            {
                                logs.Info("Batch header succesfully proceed");
                                PayrollLog(session, "Batch header succesfully proceed");
                                _confirm = true;
                            }
                            break;

                        case "BD":
                            try
                            {
                                _response = detail.Substring(161, 3);
                                logs.Info("isi response : " + _response);
                                PayrollLog(session, "isi response : " + _response);

                                String account = detail.Substring(22, 16);
                                logs.Info("isi account : " + account);
                                PayrollLog(session, "isi account : " + account);

                                String amount = detail.Substring(41, 15);
                                logs.Info("isi amount : " + amount);
                                PayrollLog(session, "isi amount : " + amount);

                                String fee = detail.Substring(71, 15);
                                logs.Info("isi fee : " + fee);
                                PayrollLog(session, "isi fee : " + fee);

                                String description = detail.Substring(164, 40);
                                logs.Info("isi description : " + description);
                                PayrollLog(session, "isi description : " + description);

                                account = account.TrimStart('0');
                                logs.Info("isi account after trim : " + account);
                                PayrollLog(session, "isi account after trim : " + account);

                                amount = amount.TrimStart('0');
                                logs.Info("isi amount after trim : " + amount);
                                PayrollLog(session, "isi amount after trim : " + amount);

                                fee = fee.TrimStart('0');
                                if (fee == "")
                                {
                                    fee = "0";
                                }
                                logs.Info("isi fee after trim : " + fee);
                                PayrollLog(session, "isi fee after trim : " + fee);

                                logs.Info("Going into sql command..");
                                PayrollLog(session, "Going into sql command..");

                                //rfqconnection
                                Parameter dbConf = session.Load<Parameter>("CMS_DBCONNECTION");
                                MySqlConnection conn = new MySqlConnection(dbConf.Data);
                                //conn.ConnectionTimeout = 20000;
                                conn.Open();
                                
                                MySqlCommand command = conn.CreateCommand();

                                PayrollLog(session, "Sql Connection status : " + command.Connection);
                                if (!"000".Equals(_response))
                                {
                                    command.CommandText = "update trxpayrolldetails set status=" +
                                        ParameterHelper.TRXSTATUS_REJECT + ", description='" + description + "' where pid='" + payroll.Id
                                        + "' and account like '%" + account + "' and amount = " + amount;
                                    PayrollLog(session, "Update trxpayrolldetails set status to reject, desc : " + description + "' where pid='" + payroll.Id
                                        + "' and account like '%" + account + "' and amount = " + amount);
                                }
                                else
                                {
                                    totfee += double.Parse(fee);
                                    totamt += double.Parse(amount);
                                    totaltrx++;
                                    command.CommandText = "update trxpayrolldetails set status=" +
                                        ParameterHelper.TRXSTATUS_SUCCESS + " where pid='" + payroll.Id
                                        + "' and account like '%" + account + "' and amount = " + amount;
                                    PayrollLog(session, "Update trxpayrolldetails set status to success, desc : " + description + "' where pid='" + payroll.Id
                                        + "' and account like '%" + account + "' and amount = " + amount);
                                }
                                int i = command.ExecuteNonQuery();
                                PayrollLog(session, "Command Execute Non Query status : " + i);
                                if (i > 0)
                                {
                                    logs.Info("Update detail succesfully proceed : " + account);
                                    PayrollLog(session, "Update detail succesfully proceed : " + account);

                                    payroll.Status = ParameterHelper.TRXSTATUS_COMPLETE;
                                    payroll.Description = payroll.Description.Replace("PROCESSED", ParameterHelper.TRXDESCRIPTION_SUCCESS);
                                    _confirm = true;
                                }
                                else
                                {
                                    logs.Error("Suspect detail detected, cannot execute query where account : " + account);
                                    PayrollLog(session, "Suscpect detail detected, cannot execute query where account : " + account);

                                    payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                                    //payroll.Description = payroll.Description.Replace("PROCESSED", "SUSPECT");
                                    payroll.ErrorDescription += "||Suspect detail detected, cannot execute query where account : " + account;
                                    _confirm = false;
                                }
                                //tutup koneksi
                                conn.Close();
                            }
                            catch(Exception e)
                            {
                                logs.Info("Suspect detail detected, cannot process BD header");
                                PayrollLog(session, "Suspect detail detected, cannot process BD header");

                                payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
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
                if (_confirm)
                {
                    TrxOBHist ob = new TrxOBHist();
                    ob.ParentId = 0;
                    Client cl = session.CreateCriteria(typeof(Client))
                        .Add(Expression.Like("Handle", payroll.CreatedBy.Substring(0, payroll.CreatedBy.IndexOf("/")).Trim()))
                        .UniqueResult<Client>();
                    ob.ClientId = cl.Id;
                    ob.TrxCode = TotalOBHelper.Payroll;
                    ob.TrxType = 1;
                    ob.TrxActId = "0";
                    ob.ValueDate = DateTime.Now.ToString("yyMMdd");
                    ob.CreditCur = "IDR";
                    ob.CreditAmt = totamt / 100;
                    ob.KursBeliTrx = "1";
                    ob.KursJualTrx = "1";
                    ob.DebitCur = "IDR";
                    ob.DebitAmt = totamt / 100;
                    ob.BaseCur = "IDR";
                    ob.BaseAmt = totamt / 100;
                    ob.KursJualBase = "1";
                    ob.TrxRemark = payroll.Id; // pid payroll
                    ob.VoucherCode = "";
                    ob.ChargeCur = "IDR";
                    ob.ChargeAmt = totfee / 100;
                    ob.DebitAcc = accfrom;
                    logs.Info("Number : " + accfrom);
                    logs.Info("PID : " + cl.Id);
                    ClientAccount cd = session.CreateCriteria(typeof(ClientAccount))
                        .Add(Expression.Like("Number", accfrom))
                        .Add(Expression.Eq("Pid", cl.Id))
                        .UniqueResult<ClientAccount>();
                    ob.DebitAccName = cd.Code;
                    ob.BenAcc = totaltrx.ToString();
                    ob.BenAccName = "-";
                    ob.BenBankIdentifier = "0002";
                    ob.BenBankName = "PT.BRI (PERSERO) TBK.";
                    ob.MakerInfo = payroll.Maker;
                    ob.CheckerInfo = payroll.Checker;
                    ob.ApproverInfo = payroll.Approver;
                    ob.LastUpdate = DateTime.Now;
                    if (payroll.Description.Contains("SUCCESS"))
                    {
                        ob.Description = "Success";
                        ob.Status = 3;
                    }
                    else
                    {
                        ob.Description = payroll.Description;
                        ob.Status = 4;
                    }
                    session.Save(ob);
                }
                #endregion

                payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                //session.Update(payroll, payroll.Id);
                //session.Flush();
                //tx.Commit();
                //tx.Dispose();
                //sr.Close(); sr.Dispose();
                //fs.Close(); fs.Dispose();
            }
            catch (Exception ex)
            {
                payroll.ErrorDescription += "||Status 20|Error : " + ex.Message + ">>" + ex.InnerException + ">>" + ex.StackTrace;
                payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                logs.Error("Error : " + ex.Message + ">>" + ex.InnerException + ">>" + ex.StackTrace);
            }

            session.Update(payroll, payroll.Id);
            session.Flush();

            logs.Info("=== End Parsing File ===");
            PayrollLog(session,"End parsing file..");
        }




        private bool GetResponseFile(string _payroll_response_dir, string _payroll_output_dir, string _file, ISession session, out string outResult)
        {
            outResult = "";
            bool _result = true;
            EventLog log = new EventLog("Cash Management");
            log.Source = "Cash Management";

            ILog logs = LogManager.GetLogger(typeof(GetPayrollResponse));
            logs.Info("--get response file proses--");
            PayrollLog(session,"--get response file proses--");

            FileInfo file = new FileInfo(_payroll_response_dir + "\\" + _file);
            PayrollLog(session,"filenya adalah : " + file.FullName);
            
            FileInfo file2 = new FileInfo(_payroll_output_dir + "\\" + _file);

            try
            {
                //if (file.Exists(_payroll_response_dir + "\\" + _file))
                if(file.Exists)
                {
                    logs.Info(">>>>>> Ok, File exist");
                    PayrollLog(session, "file exists in : "+_payroll_response_dir+"filename : "+ _file);

                    try
                    {
                        //File.Copy(_payroll_response_dir + "\\" + _file, _payroll_output_dir + "\\" + _file);
                        file.CopyTo(_payroll_output_dir + "\\" + _file, true);
                        PayrollLog(session, "Copy file : " + _file + "to :" + _payroll_output_dir);

                        try
                        {
                            //File.Delete(_payroll_response_dir + "\\" + _file);
                            file.Delete();
                            PayrollLog(session, "Delete file : " + _file + "from :" + _payroll_response_dir);
                            logs.Info(">>>>>> File exist");
                        }
                        catch (Exception e1)
                        {
                            _result = false;
                            outResult = "[RETRY]Error when deleting payroll response file : " + _file + "," + e1.Message;
                            logs.Error("Error when deleting payroll response file : " + _file + "," + e1.Message);
                            log.WriteEntry("Error when deleting payroll response file : " +_file + "," + e1.Message, EventLogEntryType.Error);
                            PayrollLog(session,"Error when deleting payroll response file : "+_file + ","+ e1.Message+e1.StackTrace);
                        }
                    }
                    catch (Exception e2)
                    {
                        _result = false;
                        outResult = "Error when copying payroll response file : " + _file + "," + e2.Message;
                        logs.Error("Error when copying payroll response file : " + _file + "," + e2.Message);
                        log.WriteEntry("Error when copying payroll response file : " + _file + "," + e2.Message, EventLogEntryType.Error);
                        PayrollLog(session, "Error when copying payroll response file : " + _file + "," + e2.Message + e2.StackTrace);
                    }
                }
                else if (file2.Exists)
                {
                    logs.Info(">>>>>> Ok, File exist in " + _payroll_output_dir);
                    PayrollLog(session, "file exists in : " + _payroll_output_dir + "filename : " + _file);
                }
                else
                {
                    _result = false;
                    outResult = "[RETRY]File Not Found on Directory...";
                    logs.Error("File Not Found on Directory...");
                    log.WriteEntry("File : " + _payroll_response_dir + "\\" + _file + " not exist.");
                    PayrollLog(session, "file : " + _payroll_response_dir + "\\" + _file + "not exist");
                }
            }
            catch (Exception e3)
            {
                _result = false;
                outResult = "Error when getting payroll response file : " + _file + "," + e3.Message;
                logs.Error("Error when getting payroll response file : " + _file + "," + e3.Message);
                log.WriteEntry("Error when getting payroll response file : " + _file + "," + e3.Message, EventLogEntryType.Error);
                PayrollLog(session, "Error when getting payroll response file : " + _file + "," + e3.Message + e3.StackTrace);
            }
            logs.Info("--get response file end--");
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


        private void PayrollLog(ISession session, String Description)
        {
            try
            {
                String _date_folder = DateTime.Now.ToString("yyyyMMdd");

                Parameter param = session.Load<Parameter>("FOLDER_PAYROLL_LOGS");
                String port = param.Data;
                String _payroll_Log_dir = port + "\\" + _date_folder;

                String path = _payroll_Log_dir;
                String fName = "\\" + "Get Payroll Response" + DateTime.Now.ToString("ddMMyyyy") + ".log";
                path = path + fName;

                String time = DateTime.Now.ToString();

                StreamWriter log = new StreamWriter(path, true);
                log.WriteLine("" + time + " " + Description);
                log.Close();
            }
            catch (Exception e)
            {

            }

        }
    }
}
