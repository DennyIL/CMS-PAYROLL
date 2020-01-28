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


namespace BRIChannelSchedulerNew.Payroll.Patching
{
    class jobAuthority_1 : IStatefulJob
    {
        private static String SchCode = "jobAuthority";
        private static String SchName = "Scheduller Get Authority Payroll";
        private static String schInfo = "=== " + DateTime.Now + " ";
        private static int jobNumber = 1;//trx yang diproses, ambil trxID kelipatan 10
        private static Configuration cfg;
        private static ISessionFactory factory;
        private String name = "";
        private String curr = "";
        protected ISession session;

        
        private DataTable dte = new DataTable("dte");
        #region IJob Members
        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(jobAuthority_1));
            try
            {
                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }
                session = factory.OpenSession();
                Console.WriteLine(schInfo + SchCode + " === Starting " + SchName + " ===");

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
                Console.WriteLine(schInfo + SchCode + " === End " + SchName + " ===");
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
            //IList<Transaction> trxList = session.CreateSQLQuery("select * from transactions where transactionid in (101, 102, 103, 1150, 1151, 1152) and status=? order by id ASC")
            //    .AddEntity(typeof(Transaction))
            //    .SetInt32(0, ParameterHelper.TRXSTATUS_WAITING_SCHEDULLER)//status 15
            //    .SetMaxResults(10)
            //    .List<Transaction>();

            IList<Transaction> trxList = session.CreateSQLQuery("select ID, HANDLE, TRANSACTIONID, CREATEDTIME, VERIFIEDTIME, "
            + " APPROVEDTIME, MAKER, CHECKER, APPROVER, CHECKWORK, CHECKTOTAL, APPROVEWORK, APPROVETOTAL,"
            + " TRANSACTIONOBJECT, `STATUS`, CLIENTID, `ACTION`, NEXTPROCESSOR, REJECTDESCRIPTION, PROCESSOR"
            + " from ("
            + " select *, case when createdtime = verifiedtime then createdtime else case when verifiedtime > approvedtime then verifiedtime"
            + " else approvedtime end end as lastupdate"
            + " from transactions"
            + " where transactionid in (101, 102, 103, 1150, 1151, 1152) and status = ?"
            + " and (ID%?)=?"
            + " ) a order by lastupdate asc;")
                .AddEntity(typeof(Transaction))
                .SetInt32(0, ParameterHelper.TRXSTATUS_WAITING_SCHEDULLER)//status 15
                .SetInt32(1, 5)//id mod(10)
                .SetInt32(2, jobNumber)
                .List<Transaction>();

            log.Info("Job Auth : " + jobNumber + " ada " + trxList.Count.ToString());
            //we've found some data
            foreach(Transaction tx in trxList)
            {
                Transaction trx = session.Load<Transaction>( tx.Id);

                log.Info("Trx Id Auth : " + trx.Id);
                if (FundTransferHelperSch.workflowCheckerPayroll(session, trx, log))
                {
                    log.Info("Cek MCS Payroll - Transaction.ID :" + trx.Id + "Sukses");
                    Boolean cekDebet = cekDebetAcc(session, trx, log);

                }
                else
                {
                    log.Error("Cek MCS Payroll - Transaction.ID :" + trx.Id + "Gagal cek Workflow");
                    trx.Status = ParameterHelper.TRXSTATUS_EXCEPTION_ONSCHEDULLER;//17
                    session.Update(trx, trx.Id);
                    session.Flush();
                }
            }
        }


        public static Boolean cekDebetAcc(ISession session, Transaction trx, ILog log)
        {
            Boolean result = false;
            //get trxpayrolls object
            XmlDocument doc = new XmlDocument();
            XmlSerializer ser = new XmlSerializer(typeof(TrxPayroll));
            doc.LoadXml(trx.TObject);
            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            object obj = ser.Deserialize(reader);

            TrxPayroll ObjPayroll = (TrxPayroll)obj;
            TrxPayroll payroll = session.Load<TrxPayroll>(ObjPayroll.Id);
            string dbAcc = payroll.DebitAccount;

            //dsini update Debet account jika beda (Khusu ftp jane) - denny
            #region update Debet Account tabel Transactions

            try
            {
                if (ObjPayroll.DebitAccount.Equals(payroll.DebitAccount))
                {
                    //Ya udah oke no problem
                    log.Info("Transaksi Payroll ID transactions : " + trx.Id + " :: Debet Transactions is Match ");
                    result = true;
                }
                else
                {
                    //Rubah biar samaan
                    log.Info("FTP Payroll ID transactions : " + trx.Id + " is Detected :: Debet Transactions is not Match ");
                    ObjPayroll.DebitAccount = dbAcc;
                    string hasilAkhir = ObjPayroll.ToString();
                    trx.TObject = hasilAkhir;
                    session.Update(trx);
                    session.Flush();
                    result = true;
                }
            #endregion update Debet Account tabel Transactions
            }
            catch (Exception ex)
            {
                log.Error("Ada exception, Trxid : " + trx.Id + "gagal cek atau update debit account");
                result = false;
            }
            return result;
        }
    }
}
