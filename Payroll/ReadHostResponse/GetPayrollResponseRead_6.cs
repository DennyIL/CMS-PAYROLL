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

namespace BRIChannelSchedulerNew.Payroll.ReadHostResponse
{
    class GetPayrollResponseRead_6 : IStatefulJob
    {
        private static Configuration cfg;
        private static ISessionFactory factory;

        private static int jobNumber = 6;
        private static String SchCode = "PayrollReadHostResponse_" + jobNumber.ToString();
        private static String SchName = "Sch PayrollReadHostResponse_" + jobNumber.ToString();
        
        ISession session;

        #region IJob Members

        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(GetPayrollResponseRead_6));
            

            try
            {
                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }
                
                session = factory.OpenSession();
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Starting " + SchName + " ===");

                try
                {
                    Commitjob_transaction(log);
                }
                catch (Exception ee)
                {
                    log.Error(SchCode + " === " + SchName + " Process Error Message:: " + ee.Message);
                    log.Error(SchCode + " === " + SchName + " Process Error Inner Exception:: " + ee.InnerException);
                    log.Error(SchCode + " === " + SchName + " Process Error StackTrace:: " + ee.StackTrace);
                }
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === End " + SchName + " ===");
            }
            catch (Exception ee)
            {
                log.Error("Scheduler Get Response Payroll failed to start : " + ee.Message + ee.StackTrace);
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


        private void Commitjob_transaction(ILog log)
        {
            Parameter maxJob = session.Load<Parameter>("PAYROLL_SCH_JOB_READFILEHOST");
            //Parameter maxTrx = session.Load<Parameter>("PAYROLL_SCH_MAX_TRX");
            int maxResult = 1;// int.Parse(maxTrx.Data);

            IList<TrxPayroll> trxList = session.CreateSQLQuery("select * from trxpayrolls where statusIFT=? and (seqnumber%?)=? and lastupdate < now() order by createdtime ASC")
                .AddEntity(typeof(TrxPayroll))
                .SetInt32(0, ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_FILEEXIST)
                .SetInt32(1, int.Parse(maxJob.Data))
                .SetInt32(2, jobNumber)
                .SetMaxResults(maxResult)
                .List<TrxPayroll>();

            if (trxList.Count > 0)
            {
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

                foreach (TrxPayroll payroll in trxList)
                {
                    String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
                    //payroll.Status = ParameterHelper.TRXSTATUS_RESPONSE_PROCESS;
                    payroll.ErrorDescription += "||File Response On Process..";
                    session.Update(payroll);
                    session.Flush();

                    log.Info(SchCode + " === (SeqNo " + payroll.SeqNumber + ") Start Processing response file " + desc[2]);
                    
                    String _fileName = desc[2].Replace("IPAY", "OPAY");
                    if (!desc[0].Equals("PROCESSED"))
                    {
                        break;
                        payroll.ErrorDescription += "||Description not PROCESSED";
                        session.Update(payroll);
                        session.Flush();
                    }

                    log.Info(SchCode + " === (SeqNo " + payroll.SeqNumber + ") " + _payroll_response_dir + "\\" + _fileName);
                    
                    string outErrorDesc = "";

                    try
                    {
                        if (payroll.StatusIFT == ParameterHelper.TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_FILEEXIST)
                        {
                            payroll.LastUpdate = DateTime.Now;
                            payroll.ErrorDescription += "Start read file response";
                            session.Update(payroll);
                            session.Flush();

                            //20170524 - SayedZul - Handler processtime tak sama dengan hari ini
                            string _dateProcess = payroll.ProcessTime.ToString("yyyyMMdd");
                            if (!_dateProcess.Equals(_date_folder))
                            {
                                Parameter paramDir = session.Load<Parameter>("FOLDER_PAYROLL_INPUT_HOST");
                                _payroll_output_dir = paramDir.Data + "\\" + _dateProcess;
                            }

                            PayrollHelper.ParseResponseFile(payroll, _payroll_output_dir + "\\" + _fileName, session, log, SchCode);
                        }
                    }
                    catch (Exception eek)
                    {
                        log.Error(eek.Message + eek.StackTrace);
                    }

                    log.Info(SchCode + " === (SeqNo " + payroll.SeqNumber + ") Finish Processing response file " + desc[2]);
                    
                }
            }
            else
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Tidak ada Trx yang diproses");

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
