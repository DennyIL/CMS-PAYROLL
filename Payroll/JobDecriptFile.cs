using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Quartz;
using Quartz.Impl;
using log4net;
using BRIChannelSchedulerNew.Payroll.Helper;
using BRIChannelSchedulerNew.Payroll.Pocos;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using BriInterfaceWrapper.BriInterface;
using BRIVAWrapper.Briva;
using System.Threading;
using System.Data.SqlClient;
using System.Text.RegularExpressions;


namespace BRIChannelSchedulerNew.Payroll
{
    class JobDecriptFile : IStatefulJob
    {
        private static String SchCode = "JobDecriptFile";
        private static String SchName = "Scheduller Decript File Payroll";
        private static String schInfo = "";
        private static int jobNumber = 0;//trx yang diproses, ambil trxID kelipatan 10
        private static Configuration cfg;
        private static ISessionFactory factory;
        private String name = "";
        private String curr = "";
        protected ISession session;

        
        private DataTable dte = new DataTable("dte");
        #region IJob Members
        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(JobDecriptFile));
            try
            {
                //log
                schInfo = "=== " + DateTime.Now + " ";
                
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
            catch (Exception ex)
            {
                log.Error(SchCode + " === " + SchName + " Failed To Start Message:: " + ex.Message);
                log.Error(SchCode + " === " + SchName + " Failed To Start Inner Exception:: " + ex.InnerException);
                log.Error(SchCode + " === " + SchName + " Failed To Start StackTrace:: " + ex.StackTrace);
            }
            finally
            {
                if (session.IsOpen)
                {
                    session.Clear();
                    session.Close();
                    session.Dispose();
                }
                factory.Close();
                factory.Dispose();

                cfg = null;
                GC.Collect();
            }
        }

        #endregion

        private void Commitjob_transaction(ILog log)
        {
            IList<TrxPayroll> trxList = session.CreateCriteria(typeof(TrxPayroll))
                    .Add(Expression.Eq("Status", ParameterHelper.PAYROLL_DECRYPT_PROCESS))
                    .Add(Expression.Lt("LastUpdate", DateTime.Now))
                    .AddOrder(Order.Asc("CreatedTime"))
                    .List<TrxPayroll>();

            log.Info("Jumlah transaksi menunggu decript : " + trxList.Count);
                
            //we've found some data
            foreach (TrxPayroll payroll in trxList)
            {
                
                //get file
                //FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                string file = payroll.FileName;
                FileInfo fi = new FileInfo(file); //create,edit file
                ClientMatrix cm = session.Load<ClientMatrix>(payroll.ClientID); //lisensi client
                AuthorityHelper ah = new AuthorityHelper(cm.Matrix); //wewenang client

                FileStream fs = null; // read write file
                string cms_encrypt_key = ParameterHelper.GetString("CMS_ENCRYPT_KEY", session);
                log.Info("isi encrypt key :: " + cms_encrypt_key);

                try
                {
                    log.Info(":: Opening gpg file :: ");
                    string file_hasil_decript = DecryptFile(session, payroll, fi, cms_encrypt_key, log);

                    if (!file_hasil_decript.Equals(""))
                    {
                        fs = new FileStream(fi.DirectoryName + "\\" + file_hasil_decript, FileMode.Open, FileAccess.Read);
                        log.Info("filename :: " + fs.Name);
                        payroll.Status = ParameterHelper.PAYROLL_WAITINGSEQNUM;
                        payroll.LastUpdate = DateTime.Now;
                        payroll.Description += "||";
                        payroll.FileName = fs.Name;
                    }
                    else
                    {
                        payroll.LastUpdate = DateTime.Now.AddMinutes(5);
                        payroll.ErrorDescription += "Decript gagal. Coba lagi nanti Gan!!!";

                        //sudah terlalu lama menunggu
                        if (DateTime.Now > payroll.CreatedTime.AddHours(6))
                        {
                            payroll.ErrorDescription += "Reject Gan, sudah terlalu lama menunggu!!!";
                            payroll.Status = ParameterHelper.TRXSTATUS_REJECT;
                            payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;
                            payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + "-" + "Decript failed, encription key not valid.";
                        }
                    }
                    session.Update(payroll);
                    session.Flush();
                }
                catch (Exception exce)
                {
                    log.Error(":: Opening file error : " + exce.Message + "||" + exce.InnerException + "||" + exce.StackTrace);
                    payroll.LastUpdate = DateTime.Now;
                    payroll.ErrorDescription += ":: Opening file error : " + exce.Message + "||" + exce.InnerException + "||" + exce.StackTrace;
                    session.Update(payroll);
                    session.Flush();
                }
            }
        }



        //method decript
        private string DecryptFile(ISession session, TrxPayroll payroll, FileInfo encryptedFile, String key, ILog log)
        {
            // decrypts the file using GnuPG, saves it to the file system
            // and returns the new (decrypted) file name.

            // encrypted file: thefile.xml.gpg decrypted: thefile.xml

            //log.Info("Decrypt File:");

            string outputFileName = encryptedFile.Name.Substring(0, encryptedFile.Name.Length - 4) + ".csv";
            // whatever you want here - just a convention

            string path = encryptedFile.DirectoryName;
            string outputFileNameFullPath = path + "\\" + outputFileName;
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("cmd.exe");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.WorkingDirectory = "C:\\Program Files (x86)\\GNU\\GnuPG";

                //string sCommandLine = "gpg.exe --allow-secret-key-import --import cmsPubKey.asc";
                string sCommandLine = "\"C:/Program Files (x86)/GNU/GnuPG/gpg.exe\" --passphrase \"" + key + "\" -o \"" +
                outputFileNameFullPath + "\" --decrypt \"" +
                                         encryptedFile.FullName + "\"";
                //psi.FileName = CreateBat(session,"ita",sCommandLine);

                System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi);
                // actually NEED to set this as a local string variable
                // and pass it - bombs otherwise!


                log.Info("cmd :: " + sCommandLine);
                process.StandardInput.WriteLine(sCommandLine);
                process.StandardInput.Flush();
                process.StandardInput.Close();
                process.WaitForExit();

                using (StreamReader reader = process.StandardError)
                {
                    string result = reader.ReadToEnd();
                    log.Info("Result decrypt :: " + result);
                }

                process.Close();
            }
            catch (Exception exce)
            {
                log.Error(":: Opening file error : " + exce.Message + "||" + exce.InnerException + "||" + exce.StackTrace);
                payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                payroll.LastUpdate = DateTime.Now;
                payroll.ErrorDescription += ":: Opening file error : " + exce.Message + "||" + exce.InnerException + "||" + exce.StackTrace;
                session.Update(payroll);
                session.Flush();
                outputFileName = "";
            }
            return outputFileName;
        }


    }
}
