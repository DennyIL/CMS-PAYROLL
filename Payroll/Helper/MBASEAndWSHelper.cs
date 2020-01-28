using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BRIChannelSchedulerNew.Payroll.Pocos;
using System.Xml;
using System.Xml.Serialization;
using NHibernate;
using NHibernate.Expression;
using MySql.Data.MySqlClient;

namespace BRIChannelSchedulerNew.Payroll.Helper
{
    public class MBASEAndWSHelper
    {
        public static Boolean ExecuteQueryValue(ISession session, string query, out string dbResutl, out string message)
        {
            Boolean result = false;
            dbResutl = "";
            message = "";
            MySqlConnection conn = new MySqlConnection();
            try
            {
                //open connection
                //BRIChannel.Pocos.Parameter dbConf = session.Load<Parameter>("CMS_DBCONNECTION");
                String DBConfig = ParameterHelper.GetString("CMS_DBCONNECTION", session);
                conn = new MySqlConnection(DBConfig);
                conn.Open();

                //create command
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = query;
                command.CommandTimeout = 6000;//dalam second

                //ekse query
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        dbResutl = reader.GetString(0);
                }
                reader.Close();
                message = "SUKSES";
                result = true;

            }
            catch (Exception e)
            {
                message = e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }
            finally
            {
                conn.Close();
            }

            return result;
        }

        public static bool insertMbaseandws(ISession session, int trxid, Int32 fid, String fitur, Int32 clientid, String instructioncode,
            String debitaccount, String debitaccountname, String creditaccount, String creditaccountname, String creditAddress,
            String trxremark, string bucketamt, int idbooking, String rmnumber, String BankCode, String Jseq,
            String chrgeType, Double chrgeAmt, string debitAccAddress, out Int32 idmbaseandws, out String OutMsg)
        {
            bool result = true;
            idmbaseandws = 0;
            OutMsg = "";
            int _gagalMbasews = 4;
            int _suksesMbasews = 5;

            try
            {
                string theQuery = "select id from mbaseandws where trxid = '" + trxid.ToString() + "' and fid = " + fid + " and status <> " + ParameterHelper.SCH_BOOKFAILED + " and status <> " + ParameterHelper.SCH_BOOKSUCCESS + " limit 1;";
                string theId = "";
                string msg = "";
                if (!MBASEAndWSHelper.ExecuteQueryValue(session, theQuery, out theId, out msg))
                {
                    result = false;
                    OutMsg = "Exception on checking duplicate trx - Message: " + msg;
                    //Console.WriteLine("Exception Id: " + trxid + " === " + OutMsg);
                    return result;
                }
                else
                {
                    if (!String.IsNullOrEmpty(theId))
                    {
                        result = false;
                        idbooking = int.Parse(theId);
                        OutMsg = "Transaction already exist: " + idbooking;
                        return result;
                    }
                }
            }
            catch (Exception ex) { }

            try
            {
                Mbaseandws mbws = new Mbaseandws();
                if (string.IsNullOrEmpty(trxid.ToString()))
                {
                    OutMsg = "trxid is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.TrxId = trxid.ToString();

                if (string.IsNullOrEmpty(fid.ToString()))
                {
                    OutMsg = "fid is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.FID = fid;

                if (string.IsNullOrEmpty(fitur))
                {
                    OutMsg = "fitur is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.Fitur = fitur;

                if (string.IsNullOrEmpty(debitAccAddress))
                {
                    OutMsg = "DebitAccAddress is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.DebitAccAddress = debitAccAddress;

                if (string.IsNullOrEmpty(instructioncode))
                {
                    OutMsg = "instructioncode is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.InstructionCode = instructioncode.Trim();


                if (string.IsNullOrEmpty(clientid.ToString()))
                {
                    OutMsg = "clientid is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.ClientId = clientid;


                mbws.CreatedTime = DateTime.Now;
                mbws.TrxDate = DateTime.Now;
                mbws.LastUpdate = DateTime.Now;

                if (string.IsNullOrEmpty(rmnumber))
                {
                    OutMsg = "rmnumber is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.RemittanceNumner = rmnumber;

                if (string.IsNullOrEmpty(debitaccount))
                {
                    OutMsg = "debitaccount is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.DebetAccount = debitaccount;

                if (string.IsNullOrEmpty(debitaccountname))
                {
                    OutMsg = "debitaccountname is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.DebetAccountName = debitaccountname;

                if (string.IsNullOrEmpty(creditaccount))
                {
                    OutMsg = "creditaccount is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.BenAcc = creditaccount;

                if (string.IsNullOrEmpty(creditaccountname))
                {
                    OutMsg = "Error creditaccountname is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.BenAccName = creditaccountname;

                if (string.IsNullOrEmpty(creditAddress))
                {
                    OutMsg = "creditAddress is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.BenAccAddress = creditAddress;

                if (string.IsNullOrEmpty(idbooking.ToString()))
                {
                    OutMsg = "idbooking is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.IdBooking = idbooking;

                if (string.IsNullOrEmpty(BankCode))
                {
                    OutMsg = "BankCode is null or empty";
                    result = false;
                    return result;
                }
                else
                    mbws.BankCode = BankCode;

                mbws.Status = ParameterHelper.SCH_WAITINGBOOK;
                mbws.ChargeAmount = chrgeAmt;
                mbws.JurnalSeq = Jseq;
                mbws.BucketAmount = bucketamt;
                mbws.Remark = trxremark;
                mbws.RtgsTrxReff = "IFT00000";
                //mbws.ChargeType = "OUR";  //default-nye 'BEN'
                //mbws.Log = null;
                //mbws.Description = "";
                //mbws.TicketNumber = null;
                //mbws.CnNumber = null;

                mbws.StatusData = ParameterHelper._needMBASE1;
                mbws.ChargeType = chrgeType;

                session.Save(mbws);
                idmbaseandws = mbws.Id;
                session.Flush();
                result = true;
            }
            catch (Exception e)
            {
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }
    }
}
