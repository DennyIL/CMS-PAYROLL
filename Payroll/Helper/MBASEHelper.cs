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
using System.IO;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace BRIChannelSchedulerNew.Payroll.Helper
{
    public abstract class MBASEHelper
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

        public static bool InsertMBASEExpress(ISession session, double trxid, string fitur, ExpressProcess exprocess, out String OutMsg, out int idexpress)
        {
            bool result = true;
            OutMsg = "";
            idexpress = 0;

            try
            {
                #region Check is transaction valid
                string theQuery = "select id from expressprocess where trxid = '" + trxid.ToString() + "' and status not in (" + ParameterHelper.SCH_MBASE_FAILED + "," + ParameterHelper.SCH_MBASE_SUCCESS + ") limit 1;";
                string theId = "";
                string msg = "";
                if (!ExecuteQueryValue(session, theQuery, out theId, out msg))
                {
                    result = false;
                    OutMsg = "Exception on checking duplicate trx - Message: " + msg;
                    return result;
                }
                else
                {
                    if (!String.IsNullOrEmpty(theId))
                    {
                        result = false;
                        idexpress = int.Parse(theId);
                        OutMsg = "Transaction already exist: " + idexpress;
                        return result;
                    }
                }
                #endregion
            }
            catch (Exception ex) { }


            try
            {
                #region Mandatory
                
                /*Trx ID*/
                if (string.IsNullOrEmpty(trxid.ToString()))
                {
                    OutMsg = "trx id is null or empty";
                    result = false;
                    return result;
                }
                else
                    exprocess.TrxID = trxid.ToString();
                
                /*Trx Type*/
                if (string.IsNullOrEmpty(fitur))
                {
                    OutMsg = "trx type is null or empty";
                    result = false;
                    return result;
                }
                else
                    exprocess.TrxType = fitur;


                /*Tanggal Transaksi*/
                if (exprocess.Tanggal_Transaksi == DateTime.MinValue)
                {
                    OutMsg = "tanggal transaksi is null or empty";
                    result = false;
                    return result;
                }
                /*Sarana Transaksi*/
                if (string.IsNullOrEmpty(exprocess.Sarana_Transaksi))
                {
                    OutMsg = "sarana transaksi is null or empty";
                    result = false;
                    return result;
                }
                /*Kode Transaksi*/
                if (string.IsNullOrEmpty(exprocess.Kode_Transaksi))
                {
                    OutMsg = "kode transaksi is null or empty";
                    result = false;
                    return result;
                }

                /*Peserta Pengirim Asal*/
                if (string.IsNullOrEmpty(exprocess.Peserta_Pengirim_Asal))
                {
                    OutMsg = "peserta pengirim asal is null or empty";
                    result = false;
                    return result;
                }
                /*Sandi Kota Asal*/
                if (string.IsNullOrEmpty(exprocess.Sandi_Kota_Asal))
                {
                    OutMsg = "sandi kota asal is null or empty";
                    result = false;
                    return result;
                }
                /*Peserta Pengirim Penerus*/
                if (string.IsNullOrEmpty(exprocess.Peserta_Pengirim_Penerus))
                {
                    OutMsg = "peserta pengirim penerus is null or empty";
                    result = false;
                    return result;
                }
                /*Peserta Penerima Akhir*/
                if (string.IsNullOrEmpty(exprocess.Peserta_Penerima_Akhir))
                {
                    OutMsg = "peserta penerima akhir is null or empty";
                    result = false;
                    return result;
                }
                /*Sandi Kota Tujuan*/
                if (string.IsNullOrEmpty(exprocess.Sandi_Kota_Tujuan))
                {
                    OutMsg = "sandi kota tujuan is null or empty";
                    result = false;
                    return result;
                }
                /*Peserta Penerima Penerus*/
                if (string.IsNullOrEmpty(exprocess.Peserta_Penerima_Penerus))
                {
                    OutMsg = "peserta penerima penerus is null or empty";
                    result = false;
                    return result;
                }
                /*Jenis Nasabah*/
                if (string.IsNullOrEmpty(exprocess.Jenis_Nasabah))
                {
                    OutMsg = "jenis nasabah is null or empty";
                    result = false;
                    return result;
                }
                /*No Rekening Pengirim*/
                if (string.IsNullOrEmpty(exprocess.No_Rekening_Pengirim))
                {
                    OutMsg = "no rekening pengirim is null or empty";
                    result = false;
                    return result;
                }
                /*Nama Pengirim*/
                if (string.IsNullOrEmpty(exprocess.Nama_Pengirim))
                {
                    OutMsg = "nama pengirim is null or empty";
                    result = false;
                    return result;
                }
                /*No Identitas Pengirim*/
                if (string.IsNullOrEmpty(exprocess.No_Identitas_Pengirim))
                {
                    OutMsg = "no identitas pengirim is null or empty";
                    result = false;
                    return result;
                }
                /*Jenis Nasabah Pengirim*/
                if (string.IsNullOrEmpty(exprocess.Jenis_Nasabah_Pengirim))
                {
                    OutMsg = "jenis nasabah pengirim is null or empty";
                    result = false;
                    return result;
                }
                /*Status Kependudukan Pengirim*/
                if (string.IsNullOrEmpty(exprocess.Status_Kependudukan_Pengirim))
                {
                    OutMsg = "Status Kependudukan Pengirim is null or empty";
                    result = false;
                    return result;
                }
                /*Alamat Pengirim*/
                if (string.IsNullOrEmpty(exprocess.Alamat_Pengirim))
                {
                    OutMsg = "alamat pengirim is null or empty";
                    result = false;
                    return result;
                }
                /*No Rekening Tujuan*/
                if (string.IsNullOrEmpty(exprocess.No_Rekening_Tujuan))
                {
                    OutMsg = "no rekening tujuan is null or empty";
                    result = false;
                    return result;
                }
                /*Nama Penerima*/
                if (string.IsNullOrEmpty(exprocess.Nama_Penerima))
                {
                    OutMsg = "nama penerima is null or empty";
                    result = false;
                    return result;
                }
                /*Jenis Nasabah Penerima*/
                if (string.IsNullOrEmpty(exprocess.Jenis_Nasabah_Penerima))
                {
                    OutMsg = "jenis nasabah penerima is null or empty";
                    result = false;
                    return result;
                }
                /*Status Kependudukan Penerima*/
                if (string.IsNullOrEmpty(exprocess.Status_Kependudukan_Penerima))
                {
                    OutMsg = "status kependudukan penerima is null or empty";
                    result = false;
                    return result;
                }
                /*Alamat Penerima*/
                if (string.IsNullOrEmpty(exprocess.Alamat_Penerima))
                {
                    OutMsg = "alamat penerima is null or empty";
                    result = false;
                    return result;
                }
                /*Cara Penyetoran*/
                if (string.IsNullOrEmpty(exprocess.Cara_Penyetoran))
                {
                    OutMsg = "cara penyetoran is null or empty";
                    result = false;
                    return result;
                }
                /*No Rekening Asal*/
                if (string.IsNullOrEmpty(exprocess.No_Rekening_Asal))
                {
                    OutMsg = "no rekening asal is null or empty";
                    result = false;
                    return result;
                }
                /*Nama*/
                if (string.IsNullOrEmpty(exprocess.Nama))
                {
                    OutMsg = "nama transaksi is null or empty";
                    result = false;
                    return result;
                }
                /*Jumlah Dikirim*/
                if (string.IsNullOrEmpty(exprocess.Jumlah_Dikirim))
                {
                    OutMsg = "jumlah dikirim is null or empty";
                    result = false;
                    return result;
                }
                /*Currency Dikirim*/
                if (string.IsNullOrEmpty(exprocess.Currency_Dikirim))
                {
                    if (exprocess.Currency_Dikirim != "IDR")
                    {
                        OutMsg = "currency dikirim is not IDR";
                    }
                    else
                    {
                        OutMsg = "currency dikirim is null or empty";
                    }
                    
                    result = false;
                    return result;
                }
                /*Kode Transaksi*/
                if (string.IsNullOrEmpty(exprocess.Kode_Transaksi))
                {
                    OutMsg = "kode transaksi is null or empty";
                    result = false;
                    return result;
                }
                /*Biaya*/
                if (string.IsNullOrEmpty(exprocess.Biaya))
                {
                    OutMsg = "biaya is null or empty";
                    result = false;
                    return result;
                }
                /*Total*/
                if (string.IsNullOrEmpty(exprocess.Total))
                {
                    OutMsg = "total transaksi is null or empty";
                    result = false;
                    return result;
                }
                /*Currency Total*/
                if (string.IsNullOrEmpty(exprocess.Currency_Total))
                {
                    if (exprocess.Currency_Total != "IDR")
                    {
                        OutMsg = "currency total is not IDR";
                    }
                    else
                    {
                        OutMsg = "currency total is null or empty";
                    }

                    result = false;
                    return result;
                }
                /*Kanca Asal*/
                if (string.IsNullOrEmpty(exprocess.Kanca_Asal))
                {
                    OutMsg = "kanca asal is null or empty";
                    result = false;
                    return result;
                }
                /*User Approve*/
                if (string.IsNullOrEmpty(exprocess.User_Approve))
                {
                    OutMsg = "user approve is null or empty";
                    result = false;
                    return result;
                }
                /*Cabang*/
                if (string.IsNullOrEmpty(exprocess.Cabang))
                {
                    OutMsg = "cabang is null or empty";
                    result = false;
                    return result;
                }
                /*No Remittance*/
                if (string.IsNullOrEmpty(exprocess.No_Remitance))
                {
                    OutMsg = "no remittance is null or empty";
                    result = false;
                    return result;
                }
                /*Jurnal Seq*/
                if (string.IsNullOrEmpty(exprocess.Jurnal_Seq))
                {
                    OutMsg = "jurnal seq is null or empty";
                    result = false;
                    return result;
                }
                /*Nama Bank Penerima*/
                if (string.IsNullOrEmpty(exprocess.Nama_Bank_Penerima))
                {
                    OutMsg = "nama bank penerima is null or empty";
                    result = false;
                    return result;
                }
                /*Provinsi Penerima*/
                if (string.IsNullOrEmpty(exprocess.Provinsi_Penerima))
                {
                    OutMsg = "provinsi penerima is null or empty";
                    result = false;
                    return result;
                }
                /*Jenis Badan Usaha*/
                if (string.IsNullOrEmpty(exprocess.Jenis_Badan_Usaha))
                {
                    OutMsg = "jenis badan usaha is null or empty";
                    result = false;
                    return result;
                }
                /*Kode Bank Brinets Penerima*/
                if (string.IsNullOrEmpty(exprocess.Kode_Bank_Brinets_Penerima))
                {
                    OutMsg = "kode bank brinets penerima is null or empty";
                    result = false;
                    return result;
                }
                /*Channel*/
                if (string.IsNullOrEmpty(exprocess.Channel))
                {
                    OutMsg = "channel is null or empty";
                    result = false;
                    return result;
                }
                #endregion

                exprocess.CreatedTime = DateTime.Now;
                session.Save(exprocess);
                idexpress = exprocess.Id;
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
