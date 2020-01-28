using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Quartz;
using Quartz.Impl;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using BRIChannelSchedulerNew.Payroll.Helper;
using BRIChannelSchedulerNew.Payroll.Pocos;
using System.Data.SqlClient;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.Sockets;

/// <summary>
/// Summary description for FundTransferHelper
/// </summary>
public class MassInquiryHelper
{
    public MassInquiryHelper() { }
    public static string schInfo = "=== " + DateTime.Now + " ";

    /* 
     * Untuk generate dan Penambahan pengecekan Instruction code hanya untuk IFT - Denny 
     * 13 Juni 2017 - Penambahan Mekanisme generate file agar lebih cefat - Denny
     */
    public static Boolean generate(TrxPayroll trx, ISession session, ILog log, string SchCode, out string outFileName)
    {
        DateTime awal = DateTime.Now;
        bool result = true;
        outFileName = "";
        Parameter paramIsDev = session.Load<Parameter>("IS_DEVELOPMENT");

        try
        {
        
            #region 1.Inisiasi
            //IList<TrxPayrollDetail> txList = trx.TrxPayrollDetail;
            String isiFile = "";

            String autonumber = "";
            if (!string.IsNullOrEmpty(trx.SeqNumber.ToString()) && trx.SeqNumber > 0)
            {
                autonumber = trx.SeqNumber.ToString().Substring(trx.SeqNumber.ToString().Length - 2, 2) + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + DateTime.Now.Millisecond.ToString().Substring(0, 1);
            }
            else//khusus pertamini yg Seq Number 0
            {
                autonumber = DateTime.Now.Year.ToString().Substring(2) + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + DateTime.Now.Second.ToString("00") + DateTime.Now.Millisecond.ToString().Substring(0, 1);
            }
            String filename = "PY" + autonumber;
            int countTotalData = 0;
            string head = "";
            string body = "";
            string foot = "";
            string temp_query = "";
            string msg = "";
            string dbResult = "";
            String fNameTemp = filename;
            Parameter input = session.Load<Parameter>("FOLDER_PAYROLL_INQ_INPUT");
            String path = input.Data;
            //ini kunci
            outFileName = filename;
            if (File.Exists(path + "\\" + filename))
            {
                File.Delete(path + "\\" + filename);
                log.Info(schInfo + "File Exist. File Deleted!");
            }

            /*Dev Only*/
            //Parameter tanggalHost = session.Load<Parameter>("DENNY_PAYROLL_HOSTDATE");
            //isiFile += "MH999." + filename + "." + tanggalHost.Data.ToString() + "\n";
            /*end of dev only*/

            /*Prod*/
            //isiFile += "MH999." + filename + "." + DateTime.Now.ToString("dd/MM/yyyy") + "\n";
            /*end of prod*/

            #endregion Inisiasi

            #region Isi file dengan data anak dengan foreach OLD
            /*
        foreach (TrxPayrollDetail item in txList)
        {
            try
            {
                if ((item.Status != 4) && (item.InstructionCode.Equals("IFT")))
                {
                    isiFile += "MD999." + item.Id.ToString().PadLeft(16, '0') + ".0" + item.Account + "\n";
                    countTotalData++;
                    Console.Write(schInfo + isiFile);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception on Scheduller Generate Mass Inq :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException);
                //EvtLogger.Write("Exception on Scheduller Converter Type :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
                trx.ErrorDescription += "Exception on Scheduller Generate Mass Inq " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException;
                trx.Status = ParameterHelper.PAYROLL_EXCEPTION;
                result = false;
                break;
            }
        }

        isiFile += "MF999." + countTotalData.ToString().PadLeft(16, '0');
         */
            #endregion Isi file dengan data dengan foreach OLD

            #region Isi file dengan data dengan Bulk

            #region 1. Pengecekan status bener2 waiting inquiry dan detailnya 55 tidak
            /*
        if (result)
        {
            temp_query = @"SELECT count(1)
                    from trxpayrolldetails where pid = '" + trx.Id + "' AND instructioncode = 'IFT';";//status 55

            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query 1: " + temp_query + " " + msg);
                result = false;
            }
            else
            {

                if (int.Parse(dbResult) == 0)
                {
                    result = false;
                }
                else
                {
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Pengecekan status bener detail siap Inquiry - Done");
                }
            }
        }
         * */
            #endregion end of 1

            #region 2. Set header dan body
            string angka = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            while (File.Exists(path + "\\" + fNameTemp) && countTotalData < 35)
            {
                countTotalData++;
                string tempel = angka.Substring(countTotalData, 1);
                fNameTemp = filename + tempel;
            }

            if (countTotalData < 35)
            {
                filename = fNameTemp;
                //PROD ONLY
                string tanggal = DateTime.Now.ToString("dd/MM/yyyy");

                //20200116
                #region cari aman koneksi 
                if (paramIsDev.Data == "1")//1 dev, 0 prod
                {
                    /*dev only*/
                    Parameter paramData = session.Load<Parameter>("DENNY_PAYROLL_HOSTDATE");
                    tanggal = paramData.Data;
                    /*end of dev onlu*/
                }
                #endregion cari aman tanggal

                //DEV ONLY
                //string tanggal = tanggalHost.Data;
                head = "MH999." + filename + "." + tanggal + "\n";

                if (result)
                {
                    temp_query = @"SELECT group_concat(concat('MD999.', lpad(id, 16, '0'), '.', lpad(ACCOUNT, 16, '0')) separator '\n')
                    from trxpayrolldetails where pid = '" + trx.Id + "' AND instructioncode = 'IFT' and status <> " + ParameterHelper.TRXSTATUS_REJECT + ";";

                    if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query 2: " + temp_query + " " + msg);
                        result = false;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(dbResult))
                        {
                            body = dbResult;
                        }
                        else
                            result = false;
                    }
                }
                else
                {
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Set Header & Body - Done");
                }
            }
            #endregion end of 2

            #region 3. Tambah footer

            if (result)
            {
                temp_query = @"SELECT count(1)
                    from trxpayrolldetails where pid = '" + trx.Id + "' AND instructioncode = 'IFT' and status <> " + ParameterHelper.TRXSTATUS_REJECT + ";";

                if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                {
                    log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query 3: " + temp_query + " " + msg);
                    result = false;
                }
                else
                {
                    if (!String.IsNullOrEmpty(dbResult))
                    {
                        foot = "\nMF999." + dbResult.PadLeft(16, '0');
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Set Footer - Done");
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            #endregion end of tambah footer

            #region 4. merger
            if (result)
            {
                isiFile = head + body + foot;
                using (System.IO.StreamWriter tulis = new System.IO.StreamWriter(path + "\\" + filename))
                {
                    tulis.Write(isiFile);
                    tulis.Close();
                    tulis.Dispose();
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Merger Header+Body+Footer - Done");
                }

//                //Native update eaea
////                temp_query = @"update trxpayrolls p
////                                    set
////                                    p.filemassinq = '" + filename + "' where p.id = '" + trx.Id + "';";

////                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
////                {
////                    log.Error("Failed Execute query. " + msg.ToString());

////                }
//                trx.FileMassInq = filename;
//                session.Update(trx);
//                session.Flush();
            }
            #endregion end of merger

            #endregion Isi file dengan data dengan bulk



            //result = true;
            DateTime akhir = DateTime.Now;
            TimeSpan timeTotal = akhir.Subtract(awal);
            
            log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Finish Generating, Total Time : " + timeTotal.TotalSeconds + " seconds");
        }
        catch (Exception ex)
        {
            log.Error("Exception on Scheduller Generate Mass Inq :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException);
            trx.ErrorDescription += "Exception on Scheduller Generate Mass Inq " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException;
            trx.Status = ParameterHelper.PAYROLL_EXCEPTION;
            result = false;
        }
        return result;
    }

    //untuk Parsing File Output
    private struct ResultInquiry
    {
        public int id;
        public int statusRek;
        public String namaRek;
    }

    //Tanggal   : 2 Juni 2017 - Denny Indra L
    //Fungsi    : Bulk Insert Mass Inquiry Reader
    public static Boolean parsingFile(TrxPayroll trx, ISession session, ILog log, int jobnumber, string SchCode, out string msg)
    {
        bool result = true;
        msg = "";

        string NamafileMassInq = trx.FileMassInq;
        try
        {
            log.Info("for trx id : " + trx.SeqNumber);
            DateTime awal = DateTime.Now;

            Parameter paramoutput = session.Load<Parameter>("FOLDER_PAYROLL_INQ_OUTPUT");

            String fileName = paramoutput.Data + NamafileMassInq;
            FileInfo fi = new FileInfo(fileName);
            FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            String detail = "";
            IList<ResultInquiry> listOutput = new List<ResultInquiry>();

            //KOndisi jawaban host 0
            if (fi.Length == 0)
            {
               #region handler file balikan null dari host
                log.Error("SeqNumber: " + trx.SeqNumber + " Null Response from host, retry trx");
                result = false;
                msg = "[NULL]";
                return result;
                #endregion
            }
            else
            {
                #region update data OLD
                /*
                while (true)
                {
                    detail = sr.ReadLine();
                    if (null == detail) break;
                    else if (String.IsNullOrEmpty(detail.Trim())) break;
                    else
                    {
                        String temp = detail.Substring(0, 2);
                        #region switch
                        switch (temp)
                        {
                            case "PY": //footer
                                break;
                            case "MH": //header
                                if (!detail.Substring(2, 3).Equals("000"))//if batch error
                                {
                                    string outmsg = "";
                                    string _batcError = detail.Substring(33, detail.Length - 33);

                                    trx.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    trx.ErrorDescription += "Rejected - " + _batcError;
                                    string[] desc = trx.Description.Split('|');
                                    trx.Description = "Rejected - " + _batcError + "|" + desc[0];
                                    if (!CheckAccountHelper.rejectAllChild(session, trx, out outmsg, log))
                                    {
                                        result = false;
                                        trx.ErrorDescription += schInfo + outmsg;
                                    }
                                    break;
                                }
                                break;
                            case "MD": //detail
                                ResultInquiry res = new ResultInquiry();
                                res.id = int.Parse(detail.Substring(6, 16));
                                res.namaRek = detail.Substring(99, detail.Length - 99);
                                res.statusRek = 0;
                                if (detail.Substring(2, 3).Equals("000"))
                                    res.statusRek = int.Parse(detail.Substring(54, 1));

                                //add to list
                                listOutput.Add(res);
                                break;

                            default:
                                break;
                        }
                        #endregion
                    }
                }
                */
                
                /*
            try
            {
                IList<TrxPayrollDetail> detils = trx.TrxPayrollDetail.Where(x => x.Status == ParameterHelper.PAYROLLDETAILS_WAITING_CHECK).ToList<TrxPayrollDetail>();
                log.Info("Start Process batch id : " + trx.Id.ToString() + " --- " + detils.Count + " records");
                foreach (TrxPayrollDetail detil in detils)
                {
                    TrxPayrollDetail pdetail = session.Get<TrxPayrollDetail>(detil.Id);
                    ResultInquiry item = listOutput.SingleOrDefault(x => x.id == int.Parse(pdetail.Id.ToString()));
                    int _statusRek = item.statusRek;
                    string _namaBRINETS = item.namaRek;


                    #region status rekening
                    if (_statusRek == 1)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Account Matched" + "||" + "1";
                        }
                        else
                        {
                            pdetail.ErrorDescription = "";
                            pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                            pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "1";//nama
                        }
                    }
                    // if (_statusRek == 2) --> rekening tutup
                    else if (_statusRek == 2)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                            pdetail.ErrorDescription = "";
                            //pdetail.Description = "Account Matched" + "||" + "2";
                            pdetail.Description = "Rekening Tutup";
                        }
                        else
                        {
                            pdetail.ErrorDescription = "";
                            pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                            //pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "2";//nama
                            pdetail.Description = "Rekening Tutup";
                        }
                    }
                    //if (_statusRek == 3) --> rekening jatuh tempo
                    else if (_statusRek == 3)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Rekening Pinjaman tidak diperbolehkan sebagai Payroll" + "||" + "3";
                        }
                        else
                        {
                            pdetail.ErrorDescription = "";
                            pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                            pdetail.Description = "Nama Tidak Sesuai & Rekeknig Pinjaman tidak diperbolehakan sebagai Payroll" + "||" + "3";//nama
                        }
                    }
                    // if (_statusRek == 4) --> aktif (rekening baru)
                    else if (_statusRek == 4)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Account Matched | New today account" + "||" + "4";
                        }
                        else
                        {
                            pdetail.ErrorDescription = "";
                            pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                            pdetail.Description = "Account Matched | New today account||" + "4";//nama
                        }
                    }
                    // if (_statusRek == 5) --> aktif (do not close)
                    else if (_statusRek == 5)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Account Matched" + "||" + "5";
                        }
                        else
                        {
                            pdetail.ErrorDescription = "";
                            pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                            pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "5";//nama
                        }
                    }
                    // if (_statusRek == 6) --> aktif (restricted)
                    else if (_statusRek == 6)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Account Matched" + "||" + "6";
                        }
                        else
                        {
                            pdetail.ErrorDescription = "";
                            pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                            pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "6";//nama
                        }
                    }
                    // if ((_statusRek == 7)) --> blokir rekening
                    else if (_statusRek == 7)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Rekening Ter Blokir" + "||" + "7";
                        }
                        else
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Rekening Ter Blokir" + "||" + "7";
                        }
                    }
                    // if ((_statusRek == 8)) --> undefined account
                    else if (_statusRek == 8)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Account Matched" + "||" + "8";
                        }
                        else
                        {
                            pdetail.ErrorDescription = "";
                            pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                            pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "8";//nama
                        }
                    }
                    // if ((_statusRek == 9)) --> dormant
                    else if (_statusRek == 9)
                    {
                        if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                        {
                            pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                            pdetail.ErrorDescription = "";
                            pdetail.Description = "Account Matched" + "||" + "9";
                        }
                        else
                        {
                            pdetail.ErrorDescription = "";
                            pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                            pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "9";//nama
                        }
                    }
                    else
                    {
                        pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                        pdetail.Description = "Account not found";
                        pdetail.ErrorDescription += "masuk else " + _namaBRINETS;
                    }
                    #endregion

                    //update
                    session.Update(pdetail);
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - Success update detail id : " + pdetail.Id);

                }
                #region lawas
                /*
                    foreach (ResultInquiry item in listOutput)
                    {
                        double id = double.Parse((item.id.ToString()));
                        int _statusRek = item.statusRek;
                        string _namaBRINETS = item.namaRek;

                        TrxPayrollDetail pdetail = session.Load<TrxPayrollDetail>(id);
                        if (pdetail.Status == ParameterHelper.PAYROLLDETAILS_WAITING_CHECK)
                        {

                            #region status rekening
                            if (_statusRek == 1)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Account Matched" + "||" + "1";
                                }
                                else
                                {
                                    pdetail.ErrorDescription = "";
                                    pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                    pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "1";//nama
                                }
                            }
                            // if (_statusRek == 2) --> rekening tutup
                            else if (_statusRek == 2)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    pdetail.ErrorDescription = "";
                                    //pdetail.Description = "Account Matched" + "||" + "2";
                                    pdetail.Description = "Rekening Tutup";
                                }
                                else
                                {
                                    pdetail.ErrorDescription = "";
                                    pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    //pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "2";//nama
                                    pdetail.Description = "Rekening Tutup";
                                }
                            }
                            //if (_statusRek == 3) --> rekening jatuh tempo
                            else if (_statusRek == 3)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Rekening Pinjaman tidak diperbolehkan sebagai Payroll" + "||" + "3";
                                }
                                else
                                {
                                    pdetail.ErrorDescription = "";
                                    pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    pdetail.Description = "Nama Tidak Sesuai & Rekeknig Pinjaman tidak diperbolehakan sebagai Payroll" + "||" + "3";//nama
                                }
                            }
                            // if (_statusRek == 4) --> aktif (rekening baru)
                            else if (_statusRek == 4)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Account Matched | New today account" + "||" + "4";
                                }
                                else
                                {
                                    pdetail.ErrorDescription = "";
                                    pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    pdetail.Description = "Account Matched | New today account||" + "4";//nama
                                }
                            }
                            // if (_statusRek == 5) --> aktif (do not close)
                            else if (_statusRek == 5)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Account Matched" + "||" + "5";
                                }
                                else
                                {
                                    pdetail.ErrorDescription = "";
                                    pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                    pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "5";//nama
                                }
                            }
                            // if (_statusRek == 6) --> aktif (restricted)
                            else if (_statusRek == 6)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Account Matched" + "||" + "6";
                                }
                                else
                                {
                                    pdetail.ErrorDescription = "";
                                    pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                    pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "6";//nama
                                }
                            }
                            // if ((_statusRek == 7)) --> blokir rekening
                            else if (_statusRek == 7)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Rekening Ter Blokir" + "||" + "7";
                                }
                                else
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Rekening Ter Blokir" + "||" + "7";
                                }
                            }
                            // if ((_statusRek == 8)) --> undefined account
                            else if (_statusRek == 8)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Account Matched" + "||" + "8";
                                }
                                else
                                {
                                    pdetail.ErrorDescription = "";
                                    pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                    pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "8";//nama
                                }
                            }
                            // if ((_statusRek == 9)) --> dormant
                            else if (_statusRek == 9)
                            {
                                if (_namaBRINETS.ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                                {
                                    pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                    pdetail.ErrorDescription = "";
                                    pdetail.Description = "Account Matched" + "||" + "9";
                                }
                                else
                                {
                                    pdetail.ErrorDescription = "";
                                    pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                    pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + _namaBRINETS.Trim() + "||" + "9";//nama
                                }
                            }
                            else
                            {
                                pdetail.Status = ParameterHelper.TRXSTATUS_REJECT;
                                pdetail.Description = "Account not found";
                                pdetail.ErrorDescription += "masuk else " + _namaBRINETS;
                            }
                            #endregion

                            //update
                            session.Update(pdetail);
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - Success update detail id : " + pdetail.Id);
                        
                        }
                        else
                            Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - detail id : " + pdetail.Id);
                        
                    }
                    
                #endregion
                //flush
                session.Flush();
                log.Info("Success FLUSH batch id : " + trx.Id.ToString());
            }
            catch (Exception ex)
            {
                log.Error("Exception on Scheduller Read Output Mass Inq :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException);
                trx.ErrorDescription += "Exception on Scheduller Read Output Mass Inq :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException;
                trx.Status = ParameterHelper.PAYROLL_EXCEPTION;
                result = false;
            }
        */
                #endregion


                //Update BULK
                #region update data bulk

                log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Bulk Insert Start");

                #region Inisiasi Variable


                
                string msgErrorFile = "";
                string temp_query = "";
                string temp_table = "tmppayrollinq";//biar gampang kalo ganti2 :D
                string dbResult = "";
                string header = "";
                string filepath = fileName.Replace("\\", "\\\\"); ;
                
                int count = 0;
                bool isGenerateFileAgain = false;
                #endregion Inisiasi Variable

                #region SwitchTable
                int id_table = jobnumber;
                temp_table += id_table.ToString();
                #endregion

                #region 1. Prescreening

                //Truncate
                temp_query = "truncate table tmppayrollinq";
                temp_query = temp_query.Replace("tmppayrollinq", temp_table);
                log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Truncate table " + temp_table);
                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                {
                    log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                    msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                    result = false;
                }
                else
                {
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Trucate " + temp_table);
                }


                if (result)
                {
                    //bulk insert
                    temp_query = @"load data local infile '[FILEPATH]' into table tmppayrollinq;";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);
                    temp_query = temp_query.Replace("[FILEPATH]", filepath);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Bulk Insert " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + "Sukses Insert FILE " + temp_table);
                    }
                }


                //Bersih2
                if (result)
                {
                    temp_query = @"UPDATE tmppayrollinq SET
                            baris = trim(REPLACE(REPLACE(baris, '\r', ''), '\n', '')),
                            namabrinets = '' ;";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + "Sukses Cleansing File " + temp_table);
                    }
                }
                #endregion end of Prescreening

                #region 2.Cek Header
                if (result)
                {
                    temp_query = @"select baris from tmppayrollinq where baris like 'MH%' and baris not like 'MH000.%' limit 1;";

                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Header Check " + temp_table);
                    if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(dbResult))
                        {
                            //ada eror
                            header = dbResult;
                            msgErrorFile = header.Substring(33, header.Length - 33);
                            result = false;
                            isGenerateFileAgain = true; //Misal tanggal salah akan generate lagi
                        }
                        else
                        {
                            log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Cek Header = VALID " + temp_table);
                        }
                    }
                }
                #endregion end of cek header

                #region 3.Cek kelengkapan Header & Footer
                if (result)
                {
                    temp_query = @"select count(1) from tmppayrollinq where baris like 'MH000.%' or baris like 'MF000.%';";

                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cek Header" + temp_table);
                    if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(dbResult))
                        {
                            //add error message to table, COUNT input user tidak ketemu
                            msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                            result = false;
                        }
                        else if (!int.TryParse(dbResult, out count))
                        {
                            //add error message to table, gagal parse count
                            msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                            result = false;
                        }
                        else
                        {
                            if (count != 2)
                            {
                                result = false;
                                isGenerateFileAgain = true;
                                msgErrorFile = "isGenerateFileAgain";
                                //transaksi suram nih, createfile ulang
                            }
                            else
                            {
                                log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Cek Header & Footer = VALID " + temp_table);
                            }
                        }
                    }
                }
                #endregion kelengkapan Header & Footer

                #region 4.Bersihkan Header dan Footer
                if (result)
                {
                    temp_query = @"delete from tmppayrollinq where baris like 'MH000.%' or baris like 'MF000.%' or baris = '' or baris is null;";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Bersih - bersih Header & Footer " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Cleaning Header & Footer yang Valid  " + temp_table);
                    }
                }

                #endregion Bersihkan Header dan Footer

                #region 5.Parsing and Fixing
                if (result)
                {
                    temp_query = @"update tmppayrollinq set idtrx = substring(baris, 7, 16), 
                                namabrinets = substring(baris, 100, length(baris) - 99),benacc = substring(baris, 24, 16),
                                statusrek = substring(baris, 55, 1), creditcurr = substring(baris, 51, 3), 
                                ACCTYPE = substring(baris, 49, 1) where baris like 'MD000.%';";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Parsing Value " + temp_table);
                    }
                }
                //update yg ga ketemu
                if (result)
                {
                    temp_query = @"update tmppayrollinq set idtrx = substring(baris, 7, 16), status = 4, description = 'Account Not Found'
                        where baris not like '%MD000%';";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Update,selain respon : 'MD000' " + temp_table);
                    }
                }

                //DEV ONLY
                #region Dev only
                //Parameter pp = session.Load<Parameter>("Denny_respon_rekening");
                //if ((result) && (pp.Data != "0"))
                //{

                //    temp_query = @"update tmppayrollinq set statusrek = (select data from parameters where id = 'Denny_respon_rekening');";
                //    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                //    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data " + temp_table);
                //    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                //    {
                //        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                //        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                //        result = false;
                //    }

                //}
                #endregion 


                #region update status & description untuk statusrek yang akan direject host untuk  payroll 2/3/7/
                if (result)
                {
                    //Langusng Reject tanpa cek :D = 2/3/7/ else
                    temp_query = @"update tmppayrollinq set status = 4,
                        description = case
                        when statusrek = 2 then 'Rekening Tutup'
                        when statusrek = 3 then 'Rekening Pinjaman tidak diperbolehkan sebagai Payroll||3'
                        when statusrek = 7 then 'Rekening Ter Blokir||7'
                        else 'Account not found'
                        end
                        where (status <> 4 or status is null) and statusrek in (2, 3, 7);";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Update - Langsung Reject STATUSREK = 2/3/7 " + temp_table);
                    }
                }
                #endregion

                #region update yang jenis rek deposito
                if (result)
                {
                    temp_query = @"update tmppayrollinq set status = 4,
                        description = 'Rekenig Deposito tidak diijinkan'
                        where (status <> 4 or status is null) and ACCTYPE = 'T';";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data2 " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Update - ACCTYPE = T (deposito) " + temp_table);
                    }
                }
                #endregion

                #region Nama Sama dengan Brinets dan set need verify 1/4/5/6/8/9
                //Untuk status rek 1/4/5/6/8/9
                if (result)
                {
                    temp_query = @"update
                        tmppayrollinq t left join trxpayrolldetails p
                        on
                        p.id = t.idtrx
                        set t.status = 1,
                        t.description = case
                        when t.statusrek = 1 then concat('Account Matched||1')
                        when t.statusrek = 4 then concat('Account Matched||4')
                        when t.statusrek = 5 then concat('Account Matched||5')
                        when t.statusrek = 6 then concat('Account Matched||6')
                        when t.statusrek = 8 then concat('Account Matched||8')    
                        when t.statusrek = 9 then concat('Account Matched||9')
                        end
                        where (t.status <> 4 or t.status is null) and t.statusrek in (1,4,5,6,8,9) and t.namabrinets = p.name and p.pid = '" + trx.Id + "';";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data Nama" + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Upadte - Set NEED VERIFY respons : 1/4/5/6/8/9" + temp_table);
                    }
                }
                #endregion Nama Sama dengan Brinets


                #region Nama tidak Sama Brinets dan set Maker 1/5/6/8/9 + Nama Brinets
                if (result)
                {
                    temp_query = @"update
                        tmppayrollinq t left join trxpayrolldetails p
                        on
                        p.id = t.idtrx
                        set t.status = 0,
                        t.description = case
                        when t.statusrek = 1 then concat('Nama Tidak Sesuai,Seharusnya : ', t.namabrinets,'||1')
                        when t.statusrek = 5 then concat('Nama Tidak Sesuai,Seharusnya : ', t.namabrinets,'||5')
                        when t.statusrek = 6 then concat('Nama Tidak Sesuai,Seharusnya : ', t.namabrinets,'||6')
                        when t.statusrek = 8 then concat('Nama Tidak Sesuai,Seharusnya : ', t.namabrinets,'||8')
                        when t.statusrek = 9 then concat('Nama Tidak Sesuai,Seharusnya : ', t.namabrinets,'||9')
                        end
                        where (t.status <> 4 or t.status is null) and t.statusrek in (1,5,6,8,9) and t.namabrinets <> p.name and p.pid = '" + trx.Id + "';";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data Nama" + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Upadte - set MAKER respons : 1/5/6/8/9" + temp_table);
                    }
                }
                #endregion Nama tidak sama Brinets


                #region Nama tidak sama Brinets set Reject - 4 rekeknig Baru
                if (result)
                {
                    temp_query = @"update tmppayrollinq  t left join trxpayrolldetails p on
                                p.id = t.idtrx
                                set t.status = 4,
                                t.description = concat('Nama Tidak Sesuai,Seharusnya ', t.namabrinets, ': |New today account||4')
                                where (t.status <> 4 or t.status is null) and t.statusrek = 4 and t.namabrinets <> p.name and p.pid = '" + trx.Id + "';";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data2 " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Update - set REJECT respons : 4 (Rekening Baru)" + temp_table);
                    }
                }
                #endregion Nama tidak sama Brinets set Reject

                #region response status nou in 1-9
                if (result)
                {
                    temp_query = @"update tmppayrollinq set status = 4,
                        description = 'Rekenig tidak diketahui'
                        where (status <> 4 or status is null) and statusrek not in(1,2,3,4,5,6,7,8,9);";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data2 " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Update - ACCTYPE = T (deposito) " + temp_table);
                    }
                }
                #endregion response status nou in 1-9

                #endregion parsing & fixing

                #region 6.Posting
                if (result)
                {
                    temp_query = @"update trxpayrolldetails p left join tmppayrollinq t
                                    on 
                                    p.id = t.idtrx
                                    set
                                    p.status = t.status,
                                    p.description = t.description
                                    where p.pid = '" + trx.Id + "' and instructioncode = 'IFT' and t.status is not null;";
                    temp_query = temp_query.Replace("tmppayrollinq", temp_table);

                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Posting data " + temp_table);
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                        msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                        result = false;
                    }
                    else
                    {
                        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Posting trxpayrolldetails - set STATUS, DESC, NAME " + temp_table);
                    }
                }
                #endregion

                #region 7. Create ulang file
                if (isGenerateFileAgain)
                {
                    trx.Status = ParameterHelper.PAYROLL_WAITING_GENERATE_MASS_INQ;//10
                    trx.LastUpdate = trx.LastUpdate.AddMinutes(5);
                    trx.ErrorDescription += DateTime.Now + "=Create ulang file dan set status 10, karena file respone tidak lengkap";
                    session.Update(trx);
                    session.Flush();
                    result = false;
                    log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + "Create ulang file dan set status 10, karena file Header / Footer tidak lengkap");
                }
                #endregion End of Create ulang file

                //Finish
                if (result)
                {
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + "Bulk Insert Finish");
                }
                #endregion update data bulk

            }
            DateTime akhir = DateTime.Now;
            TimeSpan timeTotal = akhir.Subtract(awal);
            log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Finish Processing, Total Time : " + timeTotal.TotalSeconds + " seconds");
        }
        catch (Exception ex)
        {
            log.Error("Exception on Scheduller Read Output Mass Inq :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException);
            trx.ErrorDescription += "Exception on Scheduller Read Output Mass Inq :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException;
            trx.Status = ParameterHelper.PAYROLL_EXCEPTION;
            result = false;
        }

        return result;
    }

    //untuk send FTP and Do MBASE
    public static Boolean sendToHost(TrxPayroll trx, ISession session, ILog log, String fileNameInq)
    {
        bool result = true;

        if (!String.IsNullOrEmpty(fileNameInq))
        {
            log.Info("for trx id : " + trx.SeqNumber);

            String filename = fileNameInq;
            Parameter input = session.Load<Parameter>("FOLDER_PAYROLL_INQ_INPUT");
            string path = input.Data + "\\" + filename;
            Parameter server = session.Load<Parameter>("SERVER_FTP_MASS");
            Parameter dir = session.Load<Parameter>("FOLDER_FTP_MASS_IN");
            String _serverUri = server.Data;
            //String _serverUser = "";//ParameterHelper.GetString("SERVER_FTP_MASS_USER", session);
            //String _serverPassword = ""; //ParameterHelper.GetString("SERVER_FTP_MASS_PASSWORD", session);

            //add by calvinzy 20200116
            String _serverUser = "ftpmdb";
            String _serverPassword = "ftpmdb";

            String _urlRemote = dir.Data;// ParameterHelper.GetString("FOLDER_SCV_REQUEST_HOST", session);
            int _port = 21;// int.Parse(ParameterHelper.GetString("SERVER_FTP_MASS_PORT", session).ToString());
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                #region sendFTP
                clientSocket.Connect(_serverUri, _port);
                FTPHelper ff = new FTPHelper();
                ff.setDebug(false);

                ff.setRemoteHost(_serverUri);
                ff.setRemotePort(_port);
                ff.setRemoteUser(_serverUser);
                ff.setRemotePass(_serverPassword);
                ff.setSocket(clientSocket);
                Thread.Sleep(500);
                ff.SendUserPassword();
                ff.chdir(_urlRemote);

                ff.setBinaryMode(false);
                ff.upload(path);
                ff.close();
                log.Info("Success Upload.");
                #endregion

                #region send MBASE
                log.Info("Begin MBASE");

                bool mbb = false;
                bool markMbb = false;
                int count = 0;
                while (!mbb)
                {
                    mbb = SchHelper.Mbase8632(filename, log);
                    count++;
                    if (!mbb && count > 5)
                    {
                        mbb = true;
                        markMbb = true;
                    }
                }
                if (!markMbb)
                {
                    log.Info("=== (TrxID " + trx.SeqNumber + ") MBASE Command Success!");
                }
                else
                {
                    result = false;
                }

                #endregion
            }

            catch (Exception ex)
            {
                log.Error("Exception on Scheduller Generate Mass Inq :: " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException);
                trx.ErrorDescription += "Exception on Scheduller Generate Mass Inq " + ex.Message + " => " + ex.StackTrace + " => " + ex.InnerException;
                trx.Status = ParameterHelper.PAYROLL_EXCEPTION;
                result = false;
            }
        }
        else
        {
            result = false;
            log.Error("Ada problem dalam generate file. FileNameInq null atau kosong");
        }
        return result;
    }

    //untuk download file dari HOST
    public static Boolean downloadHostResponse(TrxPayroll trx, ISession session, out string outStatus, ILog log, int jobnumber)
    {
        bool result = true;
        outStatus = "";

        log.Info("for trx id : " + trx.SeqNumber);
        String filename = trx.FileMassInq;


        Parameter output = session.Load<Parameter>("FOLDER_PAYROLL_INQ_OUTPUT");
        string path = output.Data + filename;
        Parameter server = session.Load<Parameter>("SERVER_FTP_MASS");
        Parameter dir = session.Load<Parameter>("FOLDER_FTP_MASS_OUT");

        #region cek lokal dulu
        if (File.Exists(path))
        {
            log.Info("File " + filename + " allready exists on path " + path);
            trx.ErrorDescription = "File " + filename + " allready exists on path " + path;
            outStatus = "SUKSES";
            return true;
        }
        #endregion

        String[] file = path.Split(new String[] { "\\" }, StringSplitOptions.None);
        String _serverUri = server.Data;
        String _serverUser = ParameterHelper.GetString("SERVER_FTP_MASS_USER", session);
        String _serverPassword = ParameterHelper.GetString("SERVER_FTP_MASS_PASSWORD", session);
        String _urlRemote = dir.Data;// ParameterHelper.GetString("FOLDER_SCV_REQUEST_HOST", session);
        int _port = 21;// int.Parse(ParameterHelper.GetString("SERVER_FTP_MASS_PORT", session).ToString());
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            clientSocket.Connect(_serverUri, _port);
            FTPHelper ff = new FTPHelper();
            ff.setDebug(false);

            ff.setRemoteHost(_serverUri);
            ff.setRemotePort(_port);
            ff.setRemoteUser(_serverUser);
            ff.setRemotePass(_serverPassword);
            ff.setSocket(clientSocket);
            Thread.Sleep(500);
            ff.SendUserPassword();
            ff.chdir(_urlRemote);

            ff.setBinaryMode(false);

            try
            {
                ff.download(filename, path);
                Thread.Sleep(2000);
                log.Info("Download " + filename + " Success..");

                #region cek file komplit downlot
                bool njut = false;
                long sizeFileFTP = ff.getFileSize(filename);
                FileInfo fi = new FileInfo(path);
                long sizeFileLokal = fi.Length;

                if (sizeFileFTP == sizeFileLokal)
                    njut = true;
                #endregion
                

                if (njut)
                {
                    String outStatusFile = "";
                    if (cekFooterInquiry(trx, session, log, path, out outStatusFile, "", jobnumber))
                    {
                        if (outStatusFile == "[VALID]")
                        {
                            ff.deleteRemoteFile(filename);
                            log.Info("Delete Success..");
                            outStatus = "SUKSES";
                        }
                        else if (outStatusFile == "[ERROR]")//File nya daper response Host Error, 
                        {
                            outStatus = "ERROR";
                            fi.Delete();
                            log.Info("Delete Lokal Success..Because File is Error");
                        }
                        else if (outStatusFile == "[RETRY]")//Processing File is Not Complete, add 5 menit dan dlete file lokal
                        {
                            outStatus = "RETRY";
                            fi.Delete();
                            log.Info("Delete Lokal Success..Because File is Not Complete from Host");
                        }
                        else if (outStatusFile == "[UNKNOWN]")//Langsung Kill, sikat generate lagi
                        {
                            outStatus = "UNKNOWN";
                        }

                    }
                    else
                    {
                        outStatus = "INVALID";
                    }
                }
                else
                {
                    fi.Delete();
                    log.Info("Delete Lokal Success..");
                    result = false;
                }
            }
            catch (Exception e)
            {
                log.Info("File " + filename + " not exist. Try again in few moment.");
                trx.ErrorDescription += "File not exist. Try again in few moment.";
                
            }
            ff.close();

        }
        catch (Exception ex)
        {
            log.Info("=== (TrxID " + trx.SeqNumber + ") Download Exception!");
            log.Error("=== (TrxID " + trx.SeqNumber + ") Msg : " + ex.Message + "Stack  :" + ex.StackTrace + "Stack  :" + ex.InnerException);
            trx.ErrorDescription += "Download exception.";

            result = false;
        }

        return result;
    }

    //Cek footer dengan bulk insert - Denny 22 Sept 2017
    public static Boolean cekFooterInquiry(TrxPayroll trx, ISession session, ILog log, String path, out String outStatusFile, String SchCode, int jobnumber)
    {
        /*
         * 1. Insert table tmppayrollcekfooterinq
         * 2. Cek footer 
         * 3. JIka [VALID] maka delete original file, notal case
         * 4. JIka [ERROR] maka generate lagi, delete local eksisting
         * 5. Jika [RETRY] maka retry reading file
         * 6. Jika [UNKNOWN] maka generate lagi aja, langsung KILLLL
         */

        #region Inisisasi
        bool result = false;
        string msgErrorFile = "";
        string msg = "";
        string temp_query = "";
        string temp_table = "tmppayrollcekfooterinq";
        string dbResult = "";
        string filepath = path.Replace("\\", "\\\\"); ;
        outStatusFile = "";
        #endregion end of inisiasi

        #region SwitchTable
        int id_table = jobnumber;
        temp_table += id_table.ToString();
        #endregion

        #region Insert Processing
        //Truncate
        temp_query = "truncate table tmppayrollcekfooterinq";
        temp_query = temp_query.Replace("tmppayrollcekfooterinq", temp_table);
        log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Truncate table " + temp_table);
        if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
        {
            log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
            msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
            result = false;
        }
        else
        {
            log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Sukses Trucate " + temp_table);
            result = true;
        }

        //Do Insert
        if (result)
        {
            //bulk insert
            temp_query = @"load data local infile '[FILEPATH]' into table tmppayrollcekfooterinq;";
            temp_query = temp_query.Replace("[FILEPATH]", filepath);
            temp_query = temp_query.Replace("tmppayrollcekfooterinq", temp_table);

            log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Bulk Insert " + temp_table);
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                result = false;
            }
            else
            {
                log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + "Sukses Insert FILE " + temp_table);
            }
        }

        //Bersih-bersih
        if (result)
        {
            temp_query = @"UPDATE tmppayrollcekfooterinq SET
                            baris = trim(REPLACE(REPLACE(baris, '\r', ''), '\n', ''));";
            temp_query = temp_query.Replace("tmppayrollcekfooterinq", temp_table);
            log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Cleansing Data " + temp_table);
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error(SchCode + " === (SeqNum " + trx.SeqNumber + ") Failed Query: " + temp_query + " " + msg);
                msgErrorFile = "System Error, please try again in a few minutes or contact Help Desk CMS.";
                result = false;
            }
            else
            {
                log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + "Sukses Cleansing File " + temp_table);
            }
        }
        #endregion Insert

        #region Cheking File Processing
        if (result)
        {
            //delete baris kosong
            temp_query = @"delete from tmppayrollcekfooterinq where (baris = null or baris = '') and id is not null;";
            temp_query = temp_query.Replace("tmppayrollcekfooterinq", temp_table);
            dbResult = "";
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === " + msg);
                result = false;
            }
            else
            {

                //delete duplicate record
                temp_query = @"select BARIS as baris from tmppayrollcekfooterinq order by id desc limit 1;";
                temp_query = temp_query.Replace("tmppayrollcekfooterinq", temp_table);
                dbResult = "";
                if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                {
                    log.Error("Gagal " + temp_query + " === " + msg);
                    result = false;
                }
                else if (dbResult.Contains("MF000"))//Valid
                {
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") Footer is VAlid ");
                    outStatusFile = "[VALID]";
                }
                else if (dbResult.Contains("MH551"))//Error
                {
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") File Response Error - Invalid Header");
                    outStatusFile = "[ERROR]";
                }
                else if (dbResult.Contains("MD000"))//Not Completed
                {
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") File Response is NOt Completed");
                    outStatusFile = "[RETRY]";
                }
                else
                {
                    log.Info(SchCode + " === (SeqNum " + trx.SeqNumber + ") File Response is unknown");
                    outStatusFile = "[UNKNOWN]";
                }
            }
        }
        #endregion Cek File

        return result;

    }

    //untuk resend file mass inq jika balikan tak kunjung datang
    public static Boolean retryMassInq(TrxPayroll trx, ISession session, ILog log)
    {
        bool result = true;
        try
        {
            int menitBye = int.Parse(ParameterHelper.GetString("MASS_INQ_TIME_BYE", session));
            if (trx.CreatedTime.AddMinutes(menitBye) < DateTime.Now)
            {
                trx.Status = ParameterHelper.TRXSTATUS_REJECT;
                trx.Description = "ERROR - No Response From Host, please try again or contact administrator";
                trx.ErrorDescription += "|Reject Trx, sudah " + menitBye + " menit Broh.";
                log.Error("|Reject Trx, sudah " + menitBye + " menit Broh.");
                trx.LastUpdate = DateTime.Now;

                Transaction trans = session.CreateCriteria(typeof(Transaction))
                               .Add(Expression.In("TransactionId", new int[] { 101, 102, 103 }))
                               .Add(Expression.Like("TObject", trx.Id, MatchMode.Anywhere))
                               .UniqueResult<Transaction>();
                if (trans != null)//avoid Payroll Musimas
                {
                    session.Delete(trans);
                }
                string theQuery = @"delete from trxpayrolldetails where pid  = '" + trx.Id + "';";
                string msg = "";
                PayrollHelper.ExecuteQuery(session, theQuery, out msg);
            }
            else
            {
                int menit = int.Parse(ParameterHelper.GetString("MASS_INQ_TIME_RETRY", session));

                if (trx.LastUpdate.AddMinutes(menit) < DateTime.Now)
                {
                    trx.Status = ParameterHelper.PAYROLL_WAITING_GENERATE_MASS_INQ;
                    trx.ErrorDescription += "|Generate ulang mass inq, sudah " + menit + " menit Broh.";
                    log.Info("|Generate ulang mass inq, sudah " + menit + " menit Broh.");
                    trx.LastUpdate = DateTime.Now;
                }
            }

            session.Update(trx);
            session.Flush();
        }
        catch (Exception e)
        {
            result = false;
        }

        return result;

    }

    public static Boolean rejectPayroll(TrxPayroll trx, ISession session, ILog log, int perulangan)
    {
        Boolean hasil = false;
        bool result = true;

        try
        {
            trx.Status = ParameterHelper.TRXSTATUS_REJECT;
            trx.Description = "ERROR - Problem in Host BRI, please contact Administrator";
            trx.LastUpdate = DateTime.Now;

            Transaction trans = session.CreateCriteria(typeof(Transaction))
                           .Add(Expression.In("TransactionId", new int[] { 101, 102, 103 }))
                           .Add(Expression.Like("TObject", trx.Id, MatchMode.Anywhere))
                           .UniqueResult<Transaction>();
            if (trans != null)//avoid Payroll Musimas
            {
                session.Delete(trans);
            }
            string theQuery = @"delete from trxpayrolldetails where pid  = '" + trx.Id + "';";
            string msg = "";
            Boolean deleteDetail = PayrollHelper.ExecuteQuery(session, theQuery, out msg);
            trx.ErrorDescription += "|Reject Trx : Success to Delete Transactions, Set Reject Payroll and Delete Payroll detail";
            session.Update(trx);
            session.Flush();
            if (deleteDetail)
            {
                result = true;
            }
            else
                result = false;
        }
        catch (Exception ee)
        {
            result = false;
        }
        return result;
    }
}
