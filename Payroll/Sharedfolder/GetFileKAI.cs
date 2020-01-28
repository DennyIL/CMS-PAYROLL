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
using System.Globalization;
using System.Data;
using System.Data.SqlClient;
using BriInterfaceWrapper.BriInterface;
using System.Threading;
using System.Net;

namespace BRIChannelSchedulerNew.Payroll.SharedFolder
{
    /**
     * 
     * Get File 
     * Payroll KAI
     * 
     **/

    public class GetFileKAI : IStatefulJob
    {
        //variable
        private int cid = 430;
        //private int cid = 1705;
        //private int cid = 255;
        private static Configuration cfg;
        private static ISessionFactory factory;
        String SchedulllerCode = "GETFILEPAYROLLKAIJOB";
        private static String SchCode = "GetFileKAI";
        private static String SchName = "Sch GetFileKAI";
       
        #region IJob Members
        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(GetFileKAI));
            ISession session = null;
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

                //GetPathFiles
                String fullPathFrom = "";
                try
                {
                  Parameter par1=session.CreateCriteria(typeof(Parameter))
                        .Add(Expression.Eq("Id", "FOLDER_PAYROLL_KAI"))
                        .UniqueResult<Parameter>();

                    if (par1 != null)fullPathFrom = par1.Data;

                    Parameter par_client = session.CreateCriteria(typeof(Parameter))
                        .Add(Expression.Eq("Id", "CLIENT_KAI_PAYROLL"))
                        .UniqueResult<Parameter>();

                    cid = int.Parse(par_client.Data);

                }
                catch (Exception ex)
                {
                    log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                    log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                    log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
                }
            
               List<FileInfo> listFile = new List<FileInfo>();
               if (fileIsExist(session, fullPathFrom, out listFile, log))
               {
                   for (int i = 0; i < listFile.Count; i++)
                   {
                       if (!listFile[i].FullName.Contains("COMPLETE"))
                       {
                           String fileName = Path.GetFileName(listFile[i].FullName);
                           log.Info("== " + SchedulllerCode + " = (START FILE NAME) :: " + fileName + " ==");
                           log.Info("== " + SchedulllerCode + " = (DATE): " + DateTime.Now.Date.ToString("yyyyMMdd") + " ==");
                           if (listFile[i].Extension.Equals(".csv"))
                           {
                               ProcessScheduler(fileName, session, log, listFile[i].FullName, cid);
                           }
                           else
                           {
                               Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Extension of " + fileName + " is not CSV. ===");
                           }
                           log.Info("== " + SchedulllerCode + " = END PROCESS FOR FILE "+ " " + DateTime.Now + " ==");
                       }
                       else
                       {
                           Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === File is COMPLETE, so not will be proceed ===");
                       }
                   }
               }
               else
               {
                   Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === No File to proceed ===");
               }
               Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === End " + SchName + " ===");
            
              
            }
            catch (Exception ex)
            {
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }
            finally
            {
                session.Clear();
                session.Close();
                session.Dispose();
                factory.Close();
                factory.Dispose();
                cfg = null;
                GC.Collect();
            }
        }
        #endregion

        #region (Generate MT100)
        private Boolean generateFileIsExist(ISession session, int cid, ILog log, out IList<TrxPayroll> trxGen)
        {
            Boolean exist = true;
            trxGen = null;
            try
            {
                trxGen = session.CreateSQLQuery("select * from trxpayrolls where DATE(LASTUPDATE) >= ? and STATUSEMAIL = ? and CLIENTID = ? and (STATUS = ? OR STATUS = ?)")
                            .AddEntity(typeof(TrxPayroll))
                            .SetDateTime(0, DateTime.Now.Date)
                            .SetInt32(1, int.Parse("3"))
                            .SetInt32(2, cid)
                            .SetInt32(3, ParameterHelper.TRXSTATUS_REJECT)
                            .SetInt32(4, ParameterHelper.TRXSTATUS_COMPLETE)
                            .List<TrxPayroll>();

                if (trxGen.Count < 1)
                {
                    exist = false;
                }
            }
            catch (Exception ex)
            {
                exist = false;
                log.Error("== " + SchedulllerCode + " = Scheduller GanerateMT100 Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GanerateMT100 Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GanerateMT100 Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return exist;
        }

        private Boolean job_GenerateMT100KAI(ISession session, String payrollID, String pathFile, ILog log, out String fName)
        {
            fName = "";
            Boolean generated = true;
            string[] tempLine = new string[24];
            try
            {
                TrxPayroll trxList = session.CreateCriteria(typeof(TrxPayroll))
                    .Add(Expression.Eq("Id", payrollID))
                    .UniqueResult<TrxPayroll>();

                IList<TrxPayrollDetail> trxResult = trxList.TrxPayrollDetail;
                String FName = Path.GetFileName(trxList.FileName);
                String[] splitFile = FName.Split('!');
                String fileName = splitFile[2];
                fName = fileName + ".txt";
                if (splitFile.Length == 6)
                {
                    if (trxResult.Count > 0)
                    {
                        using (StreamWriter sw = new StreamWriter(pathFile + "\\Output\\" + fileName + ".txt"))
                        {
                            int count = 0;
                            foreach (var item in trxResult)
                            {
                                count++;
                                String tempStatus = "";
                                tempStatus = item.Status != 5 ? "GAGAL" : "SUKSES";
                                #region(asignValue)
                                tempLine = new string[23];
                                tempLine[0] = "MT100";
                                tempLine[1] = count.ToString();
                                tempLine[2] = splitFile[2].Substring(0, 8);
                                tempLine[3] = "IDR";
                                tempLine[4] = item.Amount.ToString();
                                tempLine[5] = splitFile[2].Substring(14, 5);
                                tempLine[6] = splitFile[5].Substring(0,15);
                                tempLine[7] = "";
                                tempLine[8] = "";
                                tempLine[9] = "";
                                tempLine[10] = "";
                                tempLine[11] = "";
                                tempLine[12] = "";
                                tempLine[13] = "BRI";
                                tempLine[14] = "";
                                tempLine[15] = "";
                                tempLine[16] = "";
                                tempLine[17] = item.Account;
                                tempLine[18] = item.Name;
                                tempLine[19] = trxList.FileDescription;
                                tempLine[20] = "";
                                tempLine[21] = "OUR";
                                tempLine[22] = "";
                                tempLine[22] = tempStatus;
                                #endregion
                                String line = String.Join("|", tempLine);
                                sw.Write(line);
                                sw.Write("\r\n");
                            }
                        }

                        if (generated)
                        {
                            trxList.StatusEmail = 4;
                            session.Update(trxList);
                            session.Flush();
                        }
                    }
                    else
                    {
                        generated = false;
                        log.Error("== " + SchedulllerCode + " = Scheduller GanerateMT100 Failed to Start => " + " " + " Because Detailed File Is Null");
                    }
                }
                else
                {
                    generated = false;
                    log.Error("== " + SchedulllerCode + " = Scheduller GanerateMT100 Failed to Start => " + " " + " Because Incorect File Format");
                }
            }
            catch (Exception ex)
            {
               generated = false;
               log.Error("== " + SchedulllerCode + " = Scheduller GanerateMT100 Failed to Start => Exception Message =" + " " + ex.Message);
               log.Error("== " + SchedulllerCode + " = Scheduller GanerateMT100 Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
               log.Error("== " + SchedulllerCode + " = Scheduller GanerateMT100 Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return generated;
        }
        #endregion

        private Boolean fileIsExist(ISession session, String path, out List<FileInfo> listFile, ILog log) 
        {
            listFile = null;
            Boolean exist = true;
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] csvFile = di.GetFiles("*.csv").Where(x => x.LastWriteTime.Month == DateTime.Now.Month).ToArray();// kudu oleh BULAN ini saja
                if (csvFile.Length > 0)
                {
                    listFile = csvFile.OrderBy(x => x.LastWriteTime).ToList();
                    exist = true;
                }
                else
                {
                    exist = false;
                }
            }
            catch (Exception ex)
            {
                exist = false;
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return exist;
        }

        private void ProcessScheduler(String fileName, ISession session, ILog log, String filePathSource, int cid)
        {
            try
            {
                int IDTrx = 0;
                String IdTrxPayroll = "";
                String fileNames = Path.GetFileName(filePathSource);
                long size_file = new System.IO.FileInfo(filePathSource).Length;
                if (size_file > 0)
                {
                    if (saveTransactionPayroll(session, log, filePathSource, cid, out IdTrxPayroll, out IDTrx))
                    {
                        if (!RenameFile(session, filePathSource, log, "DONE!"))
                        {
                            //Jika salah hapus transaksi & trxPayroll
                            // TUlis Log kalau gagal rename
                            if (deleteTrxPayroll(session, IdTrxPayroll, log))
                            {
                                try
                                {
                                    Transaction trx = session.CreateCriteria(typeof(Transaction))
                                        .Add(Expression.Eq("Id", IDTrx))
                                        .UniqueResult<Transaction>();
                                    if (trx != null)
                                    {
                                        session.Delete(trx);
                                        session.Flush();
                                        log.Info("== " + SchedulllerCode + " = INSERT PAYROLLKAI IS [[FAILED]] FOR FILE " + fileNames + " " + DateTime.Now + " ==");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                                    log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                                    log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
                                }
                            }
                        }
                        else
                        {
                            log.Info("== " + SchedulllerCode + " = INSERT PAYROLLKAI IS [[SUCCESS]] FOR [ID TRXPAYROLL:: " + IdTrxPayroll.ToString() + "]" + " and [FILE NAME:: " + fileNames + "] [TIME:: " + DateTime.Now + "] ==");
                        }
                    }
                    else
                    {
                        if (RenameFile(session, filePathSource, log, "REJECT!"))
                        {
                            log.Info("== " + SchedulllerCode + " = INSERT PAYROLLKAI IS [[FAILED]] FOR FILE " + fileNames + " " + DateTime.Now + " ==");
                        }
                    }
                }
                else
                {
                    if (RenameFile(session, filePathSource, log, "REJECT!"))
                    {
                        log.Info("== " + SchedulllerCode + " = INSERT PAYROLLKAI IS [[FAILED]] FOR FILE " + fileNames + " " + DateTime.Now + " ==");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }
        }

        private bool deleteTrxPayroll(ISession session, String id, ILog log) 
        {
            bool isTrue = true;
            try
            {
                TrxPayroll trx=session.CreateCriteria(typeof(TrxPayroll))
                    .Add(Expression.Eq("Id", id))
                    .UniqueResult<TrxPayroll>();

                if (trx != null)
	            {
            		 session.Delete(trx);
                     session.Flush();
                     isTrue = true;
	            }
            }
            catch (Exception ex)
            {
                isTrue = false;
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return isTrue;
        }

        private int getIDClientAccount(ISession session, ILog log, String account, int cid) 
        {
            int id = 0;
            try
            {
                
                String[] tempAccount = account.Split('.');
                if (tempAccount[0].Length == 15)
                {
                    ClientAccount ca = session.CreateCriteria(typeof(ClientAccount))
                        .Add(Expression.Eq("Number", tempAccount[0]))
                        .Add(Expression.Eq("Pid", cid))
                        .UniqueResult<ClientAccount>();

                    if (ca != null)
                        id = ca.Id;
                }
                else
                {
                    throw new Exception("Digit this Debet Account:: [" + account + "] is not valid");
                }
            }
            catch (Exception ex)
            {
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return id;
        }

        private Boolean saveTransactionPayroll(ISession session, ILog log, String pathFileSource, int cid, out String IdTrxPayroll, out int IDTrx)
        {
            Boolean isTrue = true;
            String fileName = "";
            IdTrxPayroll = "";
            IDTrx = 0;
            try
            {
                log.Info("== " + SchedulllerCode + " = START Save Transactions PAYROLL ==");
                if (!fileIsInserted(session, log, pathFileSource, cid))
	            {
                    try
                    {
                        fileName = Path.GetFileName(pathFileSource);
                        String[] fileNameArray = fileName.Split('!');
                        if (fileNameArray.Length == 4)
                        {
                            UserMap umap = getUsermapsValues(session, log, cid);
                            String created = umap.ClientHandle.ToLower() + "/" + umap.UserHandle.ToLower();
                            String pathNoName = Path.GetDirectoryName(pathFileSource);

                            TrxPayroll trx = new TrxPayroll();
                            String createdTime = fileNameArray[0].Substring(0, 8);
                            //trx.CreatedTime = DateTime.ParseExact(createdTime, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                            trx.CreatedTime = DateTime.Now;
                            trx.SeqNumber = 0;
                            trx.Maker = umap.UserHandle + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            trx.DebitAccount = fileNameArray[3].Substring(0, 15).ToString();
                            trx.LastUpdate = DateTime.Now;
                            trx.CreatedBy = created;
                            trx.Approver = "";
                            //trx.FileName = path + fname;
                            trx.FileName = pathNoName + "\\" + "COMPLETE!DONE!" + fileName;
                            trx.FileDescription = fileNameArray[1] + "_" + DateTime.Now.ToString("ffff");//Baca dr Split
                            trx.Status = ParameterHelper.PAYROLL_WAITINGSEQNUM;
                            trx.StatusEmail = 0;
                            int idClientAccount = getIDClientAccount(session, log, fileNameArray[3], cid);
                            if (idClientAccount > 0)
                            {
                                trx.Description = ParameterHelper.TRXDESCRIPTION_WAITING + "||" + idClientAccount.ToString() + "||";//Baca dr split
                            }
                            else
                            {
                                throw new Exception("This Debet Account:: [" + fileNameArray[3] + "] is not registered");
                            }

                            DateTime tempDate = DateTime.ParseExact(fileNameArray[2], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);// Baca dr Split
                            DateTime dt = new DateTime();
                            if (tempDate.Date > DateTime.Now.Date)
                            {
                                dt = new DateTime(tempDate.Date.Year, tempDate.Date.Month, tempDate.Date.Day, 06, 00, 00);
                            }
                            else if (tempDate.Date < DateTime.Now.Date)
                            {
                                throw new Exception("Invalid Process Time, Date less than " + DateTime.Now.ToString("ddMMyyyy") + " with File Name:: " + fileName);
                            }
                            if (tempDate.Date == DateTime.Now.Date)
                            {
                                //dt = new DateTime(tempDate.Date.Year, tempDate.Date.Month, tempDate.Date.Day, DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);
                                dt = new DateTime(0001, 01, 01, 00, 00, 00);
                            }

                            trx.ProcessTime = dt;
                            trx.FilePath = pathFileSource;
                            trx.ClientID = cid;
                            trx.PassKey = "";
                            IdTrxPayroll = (String)session.Save(trx);
                            session.Flush();

                            int IDTrxs = 0;
                            if (saveTransaction(session, fileName, trx, cid, log, out IDTrxs, umap))
                            {
                                isTrue = true;
                                IDTrx = IDTrxs;
                            }
                            else
                            {
                                session.Delete(trx);
                                session.Flush();
                                isTrue = false;
                            }
                        }
                        else
                        {
                            throw new Exception("Thie File :: " + fileName + " can't be proceed, please check file format");
                        }
                    }
                    catch (Exception ex)
                    {
                        isTrue = false;
                        log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                        log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                        log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
                    }
                }
                else
                {
                    isTrue = false;
                    throw new Exception("File :: " +fileName+" is [[EXIST]]");
                }
            }
            catch (Exception ex)
            {
                isTrue = false;
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return isTrue;
        }

        private bool saveTransaction(ISession session, String filename, TrxPayroll trx, int cid, ILog log, out int IDTrxs, UserMap umap) 
        {
            IDTrxs = 0;
            Boolean isTrue = true;
            try
            {
                String[] tmpfileInfo = filename.Split('!');
                Transaction tx = new Transaction();
                tx.Handle = "ADD PAYROLLBRI";
                tx.TransactionId = 101;
                //tx.CreatedTime = DateTime.ParseExact(tmpfileInfo[0], "ddMMyyhhmmss", System.Globalization.CultureInfo.InvariantCulture);// Dari Split
                tx.CreatedTime = DateTime.Now;
                tx.VerifiedTime = DateTime.Now;
                tx.ApprovedTime = DateTime.Now;
                tx.TObject = trx.ToString();
                tx.ClientId = cid;
                tx.Status = 1;
                tx.Action = "ADD";
                tx.Maker = umap.UserHandle + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                Console.WriteLine("AA => " + umap.UserHandle + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                Console.WriteLine("BB => " + tx.Maker);
                tx.Checker = "";
                tx.Approver = "";
                tx.Checker = "";
                tx.Approver = "";
                tx.CheckWork = 0;
                tx.CheckTotal = 0;
                tx.ApproveWork = 0;
                tx.CheckTotal = 0;

                ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
                    .Add(Expression.Eq("Id", cid))
                    .UniqueResult<ClientWorkflow>();

                WorkflowHelper wh = new WorkflowHelper(cm.Workflow);
                if (!(null == wh))
                {
                    Workflow flow = wh.GetWorkflow(100);
                    if (!(null == flow))
                    {
                        //tx.Maker = "";
                        tx.Approver = "";
                        tx.CheckTotal = flow.TotalVerifier;
                        tx.ApproveTotal = flow.TotalApprover;
                    }
                    else
                    {
                        throw new Exception("Matrix PAYROLL is Null for this Client");
                    }
                }
                

                IDTrxs = (int)session.Save(tx);
                session.Flush();
                isTrue = true;
            }
            catch (Exception ex)
            {
                isTrue = false;
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }
            return isTrue;
        }

        private bool fileIsInserted(ISession session, ILog log, String pathFile, int cid)
        {
            bool isTrue = true;
            
            try
            {
                //String fileName = Path.GetFileName(pathFile);
                UserMap umap = getUsermapsValues(session, log, cid);
                String created = umap.ClientHandle.ToLower() + "/" + umap.UserHandle.ToLower();
                if (umap != null)
                {
                    IList<TrxPayroll> lpayroll = session.CreateCriteria(typeof(TrxPayroll))
                          .Add(Expression.Like("CreatedBy", created, MatchMode.Anywhere).IgnoreCase())
                          .Add(Expression.Eq("FilePath", pathFile))
                          .List<TrxPayroll>();

                    if (lpayroll.Count > 0)
                    {
                        isTrue = true;
                    }
                    else
                    {
                        isTrue = false;
                    }
                }
            }
            catch (Exception ex)
            {
                isTrue = false;
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return isTrue;
        }

        private UserMap getUsermapsValues(ISession session, ILog log , int cid)
        {
            UserMap result = new UserMap();
            try
            {
                string AsUserMaker = ParameterHelper.GetString("PAYROLL_KAI_USER_MAKER", session);

                //UserMap umap=session.CreateCriteria(typeof(UserMap))
                //    .Add(Expression.Eq("ClientEntity", cid))
                //    .Add(Expression.Eq("UserHandle", "ADMIN"))
                //    .UniqueResult<UserMap>();

                UserMap umap = session.CreateCriteria(typeof(UserMap))
                    .Add(Expression.Eq("ClientEntity", cid))
                    .Add(Expression.Eq("UserHandle", AsUserMaker))
                    .UniqueResult<UserMap>();

                if (umap != null)
                    result = umap;
            }
            catch (Exception ex)
            {
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return result;
        }

        private bool RenameFile(ISession session, String pathFileSource, ILog log, String flag)
        {
            Boolean isTrue = true;
            try
            {
                String fExt = "";
                String fFromName = "";
                String fToName = "";
                String fileName = Path.GetFileName(pathFileSource);
                String pathNoName = Path.GetDirectoryName(pathFileSource);
                String newFileName = pathNoName + "\\" + "COMPLETE!"+flag + fileName;

                log.Info("== " + SchedulllerCode + " = START RENAME FILE ==");
                if (System.IO.File.Exists(newFileName))
                {
                    int i = 1;
                    String nameNoExt = System.IO.Path.GetFileNameWithoutExtension(pathFileSource);
                    System.IO.FileInfo[] files = new System.IO.DirectoryInfo(pathNoName).GetFiles("*.csv").Where(x => x.FullName == newFileName).ToArray();
                    foreach (var item in files)
                    {

                        fExt = System.IO.Path.GetExtension(item.Name);

                        fFromName = pathFileSource;

                        fToName = string.Format("{0}\\{1}_{2}{3}", pathNoName, "COMPLETE!" + flag + nameNoExt, i.ToString(), fExt);

                        Boolean ceking = false;
                        do
                        {
                            fToName = string.Format("{0}\\{1}_{2}{3}", pathNoName, "COMPLETE!" + flag + nameNoExt, i.ToString(), fExt);
                            if (System.IO.File.Exists(fToName))
                            {
                                ceking = true;
                                i++;
                            }
                            else
                            {
                                ceking = false;
                                System.IO.File.Move(fFromName, fToName);
                            }

                        } while (ceking == true);                    

                        i++;
                    }
                }
                else
                {
                    System.IO.File.Move(pathFileSource, newFileName);
                }

                log.Info("== " + SchedulllerCode + " = File " + fileName + " is RENAMED ==");

                isTrue = true;
            }
            catch (Exception ex)
            {
                isTrue = false;
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception Message =" + " " + ex.Message);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception InnerMessage =" + " " + ex.InnerException);
                log.Error("== " + SchedulllerCode + " = Scheduller GetFilePayrollKai Failed to Start => Exception StackTrace =" + " " + ex.StackTrace);
            }

            return isTrue;
        }
    }

    public class FTP 
    {
        private String host;
        private String username;
        private String password;
        private String subFolder;

        public String HostFtp 
        {
            get { return host; }
            set { host = value; }
        }

        public String Username
        {
            get { return username; }
            set { username = value; }
        }

        public String Password
        {
            get { return password; }
            set { password = value; }
        }
        public String SubFolder 
        {
            get { return subFolder; }
            set { subFolder = value; }
        }
    }
}
