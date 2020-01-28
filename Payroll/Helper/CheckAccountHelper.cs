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
using System.Diagnostics;
using System.Collections;
using System.Xml.Linq;
using BriInterfaceWrapper.BriInterface;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

public class CheckAccountHelper
{
    public CheckAccountHelper() { }
    public static int limitAccTimeout;



    /*Denny Indra L
     *Pengecekan untuk input ke deatil
     */
    public static Boolean PayrollParsingFile(TrxPayroll payroll, ISession session, ILog log, int jobNumber)
    {
        Boolean result = true;
        string msg = "";
        string msgErrorFile = "";
        string summaryError = "";
        string temp_query = "";
        string temp_table = "tmpPayrolls";
        bool isFileValid = true;
        string resultLimitHarian = "";
        string resultLimitKorporasi = "";
        int fid = 100;
        Boolean isLanjutValidasi = true;
        string dbResult = "";

        #region SwitchTable
        int id_table = jobNumber;//0;// payroll.SeqNumber % 10;
        temp_table += id_table.ToString();
        #endregion


        /*Truncate table temporari dengan mod*/
        #region Created Table, Insert Data and Clean Data
        temp_query = "truncate table tmpPayroll";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
        {
            log.Error("Gagal " + temp_query + " === "+ msg);
            summaryError += temp_query + "||" + msg;
            result = false;
        }
        #endregion Truncate

        #region delete existingfile
        if (result)
        {
            //delete duplicate record
            temp_query = @"select count(ID) from trxpayrolldetails where pid = '" + payroll.Id + "';";
            dbResult = "";
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
            else if (int.Parse(dbResult) > 0)
            {
                temp_query = @"delete from trxpayrolldetails where pid = '" + payroll.Id + "';";

                //log.Info(SchCode + " === (TrxID " + payroll.Id + ") DELETE duplicate data if exist");

                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    result = false;
                    isLanjutValidasi = false;
                }
            }
        }
        #endregion

        #region bulk insert dr file
        //bulk insert dari file
        temp_query = @"load data local infile '[FILEPATH]'
                              into table tmpPayroll
                              fields terminated by ','
                              ignore 1 lines
                              (BARIS, NAMA, @col3, AMOUNT, EMAIL, REFFNUM, BENBANK, BENADDRESS)
                               set ACCOUNT = replace(replace(replace(@col3, '-', ''),'.',''), ' ','');";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        string filepath = payroll.FileName.Replace("\\", "\\\\");

        temp_query = temp_query.Replace("[FILEPATH]", filepath);//replace with file name


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
            else
            {
                //sukses
                log.Info("Payroll ID :" + payroll.Id.ToString() + "Sukses Insert ke Tabel " + temp_table);
            }
        }
        #endregion bulk insert dr file

        #region bersih-bersih tb temporrari
        //bersih-bersih new line pada record
        temp_query = @"UPDATE tmpPayroll SET nama = REPLACE(REPLACE(nama, '\r', ''), '\n', ''),
                        account = REPLACE(REPLACE(account, '\r', ''), '\n', ''),
                        amount = REPLACE(REPLACE(amount, '\r', ''), '\n', ''),
                        description = REPLACE(REPLACE(description, '\r', ''), '\n', ''),
                        email = REPLACE(REPLACE(email, '\r', ''), '\n', ''),
                        benbank = REPLACE(REPLACE(benbank, '\r', ''), '\n', ''),
                        benaddress = REPLACE(REPLACE(benaddress, '\r', ''), '\n', ''),      
                        reffnum = REPLACE(REPLACE(reffnum, '\r', ''), '\n', '');";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
        }
        #endregion bersih-bersih tb temporrari

        #region sapu transaksi gegaran COUNT/TOTAL ada lebih dari 1
        //20170605 - sayedzul 
        temp_query = @"select count(1) from tmpPayroll where BARIS = 'COUNT';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        int total_record = 0;

        if (result)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === " + msg);
                summaryError += temp_query + "||" + msg;
                isLanjutValidasi = false;
                result = false;
            }
            else
            {
                //set total record
                total_record = int.Parse(dbResult);
                if (total_record != 1)
                {
                    //langsung cut disini sahaja
                    msgErrorFile = "No COUNT or Duplicate COUNT Param, please check file uploaded.";

                    log.Error("Payroll ID :" + payroll.Id.ToString() + " Param COUNT = " + dbResult + " BYE === " + temp_table);

                    string outRejectChild = "";
                    if (rejectAllChild(session, payroll, out outRejectChild, log))
                    {
                        log.Info("Success Update All Child.");
                        payroll.Status = ParameterHelper.TRXSTATUS_REJECT;//4
                        payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;//1
                        payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + "-" + msgErrorFile;
                    }
                    else
                    {
                        payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                        payroll.ErrorDescription = outRejectChild;
                    }

                    payroll.LastUpdate = DateTime.Now;
                    session.Update(payroll);
                    session.Flush();

                    return false;
                }
            }
        }

        temp_query = @"select count(1) from tmpPayroll where BARIS = 'TOTAL';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        total_record = 0;
        if (result)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === " + msg);
                summaryError += temp_query + "||" + msg;
                isLanjutValidasi = false;
                result = false;
            }
            else
            {
                //set total record
                total_record = int.Parse(dbResult);
                if (total_record != 1)
                {
                    //langsung cut disini sahaja
                    msgErrorFile = "No TOTAL or Duplicate TOTAL Param, please check file uploaded.";

                    log.Error("Payroll ID :" + payroll.Id.ToString() + " Param TOTAL = " + dbResult + " BYE === " + temp_table);

                    string outRejectChild = "";
                    if (rejectAllChild(session, payroll, out outRejectChild, log))
                    {
                        log.Info("Success Update All Child.");
                        payroll.Status = ParameterHelper.TRXSTATUS_REJECT;//4
                        payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;//1
                        payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + "-" + msgErrorFile;
                    }
                    else
                    {
                        payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                        payroll.ErrorDescription = outRejectChild;
                    }

                    payroll.LastUpdate = DateTime.Now;
                    session.Update(payroll);
                    session.Flush();

                    return false;
                }
            }
        }
        #endregion

        //edit by calvinzy 20200116 :: edit query support BRIS
        #region update Instruction
        string limitLLG = ParameterHelper.GetString("LLGX_CUTOFFAMOUNT", session) + "00";
        //temp_query = @"UPDATE  tmpPayroll SET instcode = IF((benbank = 'BRINIDJA' or benbank = '' or benbank is null), '" + TotalOBHelper.IFT + "', IF (amount > "+limitLLG+", '" + TotalOBHelper.RTGS + "', '" + TotalOBHelper.LLG + "')) WHERE   BARIS regexp '[0-9]+';";
        temp_query = @"UPDATE  tmpPayroll SET instcode = IF(benbank = 'DJARIDJ1', '" + ParameterHelper.INSTCODE_BRIS + "', IF((benbank = 'BRINIDJA' or benbank = '' or benbank is null), '" + TotalOBHelper.IFT + "', IF (amount > " + limitLLG + ", '" + TotalOBHelper.RTGS + "', '" + TotalOBHelper.LLG + "'))) WHERE   BARIS regexp '[0-9]+';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);

        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
            else
            {
                //sukses
                log.Info("Payroll ID :" + payroll.Id.ToString() + "Sukses update Instruction Code");
            }
        }
        #endregion update Instruction

        #region bankCode

        if (result)
        {
            //cek bank code 

//            temp_query = @"update tmpPayroll set DESCRIPTION = '[INVALID]#Invalid Bank Code.', STATUS=4 
//                    where benbank not in
//                    (select bankcode from bank) and instcode <> '"+TotalOBHelper.IFT+"' and BARIS regexp '[0-9]+';";
            temp_query = @"update tmpPayroll set DESCRIPTION = '[INVALID]#Invalid Bank Code.', STATUS=4 
                    where benbank not in
                    (select bankcode from bankkliring) and instcode <> '" + TotalOBHelper.IFT + "' and BARIS regexp '[0-9]+';";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);

            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
        }

        //Add by sayedzul 20160923 - handler bank tujuan BRI atau tidak terdaftar sebagai peserta SKNBI (where CODE <> 0)
        if (result)
        {
            //cek bank code 3

//            temp_query = @"update tmpPayroll set DESCRIPTION = '[INVALID]#Beneficiary Bank is not a member of SKNBI', STATUS=4 
//                    where benbank not in
//                    (select bankcode from bank where CODE <> 0 and bankcode <> 'BRINIDJA')
//                    and instcode = '" + TotalOBHelper.LLG + "' and BARIS regexp '[0-9]+';";
            temp_query = @"update tmpPayroll set DESCRIPTION = '[INVALID]#Beneficiary Bank is not a member of SKNBI', STATUS=4 
                    where benbank not in
                    (select bankcode from bankkliring where CODE <> 0 and bankcode <> 'BRINIDJA')
                    and instcode = '" + TotalOBHelper.LLG + "' and BARIS regexp '[0-9]+';";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);

            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
        }

        //End sayed 20160923 

        #endregion

        #region lengkapi rekening jika IFT dan Cek kelengkapan 15 karakter croot yg bank lain abaikan
        //lengkapi no rekening
        temp_query = @"update tmpPayroll
                        set ACCOUNT =  concat(REPEAT('0', 15-length(trim(ACCOUNT))), ACCOUNT)
                        where BARIS regexp '[0-9]+' and ACCOUNT regexp '^[0-9]' and length(trim(ACCOUNT)) < 15 
                        AND instcode = '" + TotalOBHelper.IFT + "';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
        }
        #endregion lengkapi rekening jika IFT croot

        /*Validasi 1 dan 2 terkait File, 3 terkait Client*/
        #region Validasi Tingkat 1

        //hitung jumlah record yang tersisa, jika kosong maka file tidak valid (reject)
        #region cek JUMLAH RECORD yg valid -- (VALIDASI 1)
        
        temp_query = @"select count(baris) from tmpPayroll where BARIS regexp '[0-9]+';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        total_record = 0;



        /*Lolos semua proses awal*/
        if (result == true)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                isLanjutValidasi = false;
                result = false;
            }
            else
            {
                //set total record
                total_record = int.Parse(dbResult);
                if (dbResult.Equals("0"))
                {
                    //add error message to table
                    string out_msg_insert_error = "";
                    if (!insertErrorDescription(session, temp_table, "No valid record found.", out out_msg_insert_error, log))
                    {
                        log.Error("Gagal " + out_msg_insert_error);
                        summaryError += temp_query + "||" + out_msg_insert_error;
                        result = false;
                        isLanjutValidasi = false;
                    }
                    else isFileValid = false;

                }
            }
        }
        #endregion


        #region validasi email
        temp_query = @"update tmpPayroll
                        set DESCRIPTION = '[INVALID]#Invalid Email Address.', STATUS=4
                        where trim(EMAIL) <> ''
                          and EMAIL not regexp '^[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$'
                          and BARIS regexp '[0-9]+';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
        }
        #endregion


        #region Validasi null or empty Account Name
        temp_query = @"update tmpPayroll
                        set DESCRIPTION = '[INVALID]#Invalid Account Name.', STATUS=4
                        where ((NAMA IS NULL or NAMA ='') and BARIS regexp '[0-9]+' );";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
        }
        #endregion Validasi Account Name


        #region Validasi null or empty Ben Address
        temp_query = @"update tmpPayroll
                                set DESCRIPTION = '[INVALID]#Invalid Benefeciary Address.', STATUS=4
                                where BARIS regexp '[0-9]+' and ((BENADDRESS IS NULL or BENADDRESS ='') and INSTCODE <> '" + TotalOBHelper.IFT + "');";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
            else
            {
                //sukses
                log.Info("Payroll ID :" + payroll.Id.ToString() + "Ben Address not Empty");
            }
        }
        #endregion Validasi null or empty BenBAnk


        #region validasi no rekening angka dan IDR khusus IFT
        temp_query = @"update tmpPayroll
                        set DESCRIPTION = '[INVALID]#Invalid Account or IDR Account only.', STATUS=4
                        where (BARIS regexp '[0-9]+' and ACCOUNT not regexp '^[0-9]+') or 
                        (BARIS regexp '[0-9]+' and ACCOUNT regexp '^[0-9]{15}$' and substring(ACCOUNT,5,2)<>'01' and INSTCODE = '"+TotalOBHelper.IFT+"');";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
            }
        }

        //20170622 - SayedZul - sikat account bukan angka, bugs jika ada karakter '
        temp_query = @"update ignore tmpPayroll
                        set DESCRIPTION = '[INVALID]#Invalid Account.', STATUS=4
                        where BARIS regexp '[0-9]+' and lpad(convert(cast(ACCOUNT as unsigned) using latin1), 15, '0') <> ACCOUNT and INSTCODE = '" + TotalOBHelper.IFT + "';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === " + msg);
                summaryError += temp_query + "||" + msg;
                result = false;
            }
        }
        #endregion


        #region validasi amount transaksi (harus > 0)
        temp_query = @"update tmpPayroll
                        set DESCRIPTION = '[INVALID]#Amount not valid.', STATUS=4
                        where ((AMOUNT IS NULL or AMOUNT = '' or AMOUNT <= '0') and BARIS regexp '[0-9]+' );";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
        }
        #endregion

        #endregion Validasi Tingkat 1


        /*VALIDASI TINGKAT 2*/
        #region VALIDASI TINGKAT 2

        #region cek jumlah record yg invalid (validasi 2)
        //hitung data yg invalid, jika sama dengan jumlah data, maka 1 batch tidak valid
        temp_query = @"select count(baris) from tmpPayroll where BARIS regexp '[0-9]+' and description like '%[INVALID]%'";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        int total_record_invalid = 0;
        //string valid_record_count = "";


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
            else
            {
                try
                {
                    total_record_invalid = int.Parse(dbResult);
                }
                catch (Exception e)
                {
                    isFileValid = false;
                }
            }

            //ITUNG
            if (total_record_invalid >= total_record)//tidak ada record yg valid
            {
                //add error message to table
                string out_msg_insert_error = "";
                if (!insertErrorDescription(session, temp_table, "No valid data found. Please review your file.", out out_msg_insert_error, log))
                {
                    log.Error("Gagal " + out_msg_insert_error);
                    summaryError += temp_query + "||" + out_msg_insert_error;
                    result = false;
                }
                else isFileValid = false;
            }
        }
        #endregion


        #region validasi COUNT text file
        temp_query = @"select count(baris) from tmpPayroll where BARIS regexp '[0-9]+';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        double systemDbCount = 0;


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
            }
            else
            {
                try
                {
                    systemDbCount = double.Parse(dbResult);
                }
                catch (Exception e)
                {
                    isFileValid = false;
                }
            }
        }


        //get inputed count, JUmlah yang kan diinput
        temp_query = @"select if(
	                            (select NAMA from tmpPayroll where BARIS='COUNT')!='',
	                            (select NAMA from tmpPayroll where BARIS='COUNT'),
		                            if(
			                            (select ACCOUNT from tmpPayroll where BARIS='COUNT')!='',
			                            (select ACCOUNT from tmpPayroll where BARIS='COUNT'),
			                            if(
				                            (select AMOUNT from tmpPayroll where BARIS='COUNT')!='',
				                            (select AMOUNT from tmpPayroll where BARIS='COUNT'),
				                            if(
					                            (select DESCRIPTION from tmpPayroll where BARIS='COUNT')!='',
					                            (select DESCRIPTION from tmpPayroll where BARIS='COUNT'),
					                            if(
						                            (select EMAIL from tmpPayroll where BARIS='COUNT')!='',
						                            (select EMAIL from tmpPayroll where BARIS='COUNT'),
						                            if(
							                            (select REFFNUM from tmpPayroll where BARIS='COUNT')!='',
							                            (select REFFNUM from tmpPayroll where BARIS='COUNT'),
                                                        if(
							                                (select BENBANK from tmpPayroll where BARIS='COUNT')!='',
							                                (select BENBANK from tmpPayroll where BARIS='COUNT'),
                                                            if(
							                                  (select BENADDRESS from tmpPayroll where BARIS='COUNT')!='',
							                                  (select BENADDRESS from tmpPayroll where BARIS='COUNT'),							                            
                                                                ''
                                                              )
                                                          )
						                            )
					                            )
				                            )
			                            )
		                            )
                            );";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        double inputCount = 0;


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
            }
            else
            {
                try
                {
                    inputCount = double.Parse(dbResult);
                }
                catch (Exception e)
                {
                    isFileValid = false;
                }
            }
        }

        //cek hitungan system (dr total baris) dengan jumlah baris yg tidak kosong per elemnnya 
        if (inputCount != systemDbCount)
        {
            //add error message to table
            string out_msg_insert_error = "";
            if (!insertErrorDescription(session, temp_table, "Total/Count Data not Valid", out out_msg_insert_error, log))
            {
                log.Error("Gagal " + out_msg_insert_error);
                summaryError += temp_query + "||" + out_msg_insert_error;
                result = false;
            }
        }
        #endregion


        #region validasi TOTAL AMOUNT
        temp_query = @"select sum(amount) from tmpPayroll where BARIS regexp '[0-9]+';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        dbResult = "";
        double systemDbSum = 0;


        if (result && isFileValid)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                result = false;
                isLanjutValidasi = false;
            }
            else
            {
                try
                {
                    systemDbSum = double.Parse(dbResult);
                }
                catch (Exception e)
                {
                    isFileValid = false;
                }
            }
        }


        //get inputed count
        temp_query = @"select if(
                                (select NAMA from tmpPayroll where BARIS='TOTAL')!='',
                                (select NAMA from tmpPayroll where BARIS='TOTAL'),
                                    if(
	                                    (select ACCOUNT from tmpPayroll where BARIS='TOTAL')!='',
	                                    (select ACCOUNT from tmpPayroll where BARIS='TOTAL'),
	                                    if(
		                                    (select AMOUNT from tmpPayroll where BARIS='TOTAL')!='',
		                                    (select AMOUNT from tmpPayroll where BARIS='TOTAL'),
		                                    if(
			                                    (select DESCRIPTION from tmpPayroll where BARIS='TOTAL')!='',
			                                    (select DESCRIPTION from tmpPayroll where BARIS='TOTAL'),
			                                    if(
				                                    (select EMAIL from tmpPayroll where BARIS='TOTAL')!='',
				                                    (select EMAIL from tmpPayroll where BARIS='TOTAL'),
				                                    if(
					                                    (select REFFNUM from tmpPayroll where BARIS='TOTAL')!='',
					                                    (select REFFNUM from tmpPayroll where BARIS='TOTAL'),
					                                    if(
							                                (select BENBANK from tmpPayroll where BARIS='TOTAL')!='',
							                                (select BENBANK from tmpPayroll where BARIS='TOTAL'),
                                                            if(
							                                  (select BENADDRESS from tmpPayroll where BARIS='TOTAL')!='',
							                                  (select BENADDRESS from tmpPayroll where BARIS='TOTAL'),							                            
                                                                ''
                                                              )
                                                          )
				                                    )
			                                    )
		                                    )
	                                    )
                                    )
                            );";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        double inputSum = 0;


        if (result == true)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
            else
            {
                try
                {
                    inputSum = double.Parse(dbResult);
                }
                catch (Exception e)
                {
                    isFileValid = false;
                }
            }
        }

        //cek hitungan system dengan input user
        if (inputSum != systemDbSum)
        {
            //add error message to table
            string out_msg_insert_error = "";
            if (!insertErrorDescription(session, temp_table, "Total Amount Data not Valid, inputSum = " + inputSum + " ::: systemDbSum = " + systemDbSum, out out_msg_insert_error, log))
            {
                log.Error("Gagal " + out_msg_insert_error);
                summaryError += temp_query + "||" + out_msg_insert_error;
                result = false;
                isLanjutValidasi = false;
            }
        }
        #endregion


        #region validasi CHECKSUM

        Parameter paramClientId = session.Load<Parameter>("CLIENT_PAYROLL_BAYPASS_CHECKSUM");
        string[] CId = paramClientId.Data.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        int[] theCID = Array.ConvertAll(CId, s => int.Parse(s));


        /*BYPASS ada di parameter--> jika BYPASS g ngapa2in */
        if (!theCID.Contains(payroll.ClientID) && result && isFileValid)
        {
            //get total 4 digit ahir rekening
            temp_query = @"select sum(RIGHT(account,4)) from tmpPayroll where BARIS regexp '[0-9]+';";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);
            dbResult = "";
            double systemDbTotAcc = 0;

            log.Info("Query BYPASS ada di parameter ");
            if (result == true)
            {
                if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    result = false;
                    isLanjutValidasi = false;
                }
                else
                {
                    try
                    {
                        systemDbTotAcc = double.Parse(dbResult);
                    }
                    catch (Exception e)
                    {
                        isFileValid = false;
                    }
                }
            }


            //hitung system checksum
            String systemCheckSum = systemDbCount.ToString().Trim() + systemDbSum.ToString().Trim() + systemDbTotAcc.ToString().Trim();
            systemCheckSum = PayrollHelper.MD5(systemCheckSum);


            //get inputed checksum
            temp_query = @"select if(
	                                (select NAMA from tmpPayroll where BARIS='CHECK')!='',
	                                (select NAMA from tmpPayroll where BARIS='CHECK'),
		                                if(
			                                (select ACCOUNT from tmpPayroll where BARIS='CHECK')!='',
			                                (select ACCOUNT from tmpPayroll where BARIS='CHECK'),
			                                if(
				                                (select AMOUNT from tmpPayroll where BARIS='CHECK')!='',
				                                (select AMOUNT from tmpPayroll where BARIS='CHECK'),
				                                if(
					                                (select DESCRIPTION from tmpPayroll where BARIS='CHECK')!='',
					                                (select DESCRIPTION from tmpPayroll where BARIS='CHECK'),
					                                if(
						                                (select EMAIL from tmpPayroll where BARIS='CHECK')!='',
						                                (select EMAIL from tmpPayroll where BARIS='CHECK'),
						                                if(
							                                (select REFFNUM from tmpPayroll where BARIS='CHECK')!='',
							                                (select REFFNUM from tmpPayroll where BARIS='CHECK'),
							                                if(
						                                        (select BENBANK from tmpPayroll where BARIS='CHECK')!='',
						                                        (select BENBANK from tmpPayroll where BARIS='CHECK'),
						                                        if(
							                                        (select BENADDRESS from tmpPayroll where BARIS='CHECK')!='',
							                                        (select BENADDRESS from tmpPayroll where BARIS='CHECK'),
							                                        ''
						                                            )
					                                            )
						                                )
					                                )
				                                )
			                                )
		                                )
                                );";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);
            string inputCheckSum = "";

            log.Info("Query get inputed checksum");
            if (result == true)
            {
                if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                {
                    summaryError += temp_query + "||" + msg;
                    isLanjutValidasi = false;
                }
                else
                {
                    inputCheckSum = dbResult;
                }
            }


            //Cek system checksum VS INput checksum alias dari inputan
            if (!systemCheckSum.Equals(inputCheckSum))
            {
                //add error message to table
                string out_msg_insert_error = "";
                if (!insertErrorDescription(session, temp_table, "Checksum Not Valid (System Checksum : " + systemCheckSum + ")", out out_msg_insert_error, log))
                {
                    log.Error("Gagal " + out_msg_insert_error);
                    summaryError += temp_query + "||" + out_msg_insert_error;
                    result = false;
                    isLanjutValidasi = false;
                }
            }
        }
        #endregion


        #region validasi rekening debet
        //get debit account
        temp_query = @"select if(
                                (select NAMA from tmpPayroll where BARIS='DEBITACCOUNT')!='',
                                (select NAMA from tmpPayroll where BARIS='DEBITACCOUNT'),
	                                if(
		                                (select ACCOUNT from tmpPayroll where BARIS='DEBITACCOUNT')!='',
		                                (select ACCOUNT from tmpPayroll where BARIS='DEBITACCOUNT'),
		                                if(
			                                (select AMOUNT from tmpPayroll where BARIS='DEBITACCOUNT')!='',
			                                (select AMOUNT from tmpPayroll where BARIS='DEBITACCOUNT'),
			                                if(
				                                (select DESCRIPTION from tmpPayroll where BARIS='DEBITACCOUNT')!='',
				                                (select DESCRIPTION from tmpPayroll where BARIS='DEBITACCOUNT'),
				                                if(
					                                (select EMAIL from tmpPayroll where BARIS='DEBITACCOUNT')!='',
					                                (select EMAIL from tmpPayroll where BARIS='DEBITACCOUNT'),
					                                if(
						                                (select REFFNUM from tmpPayroll where BARIS='DEBITACCOUNT')!='',
						                                (select REFFNUM from tmpPayroll where BARIS='DEBITACCOUNT'),
						                                ''
					                                )
				                                )
			                                )
		                                )
	                                )
                            );";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        string inputDebitAccount = "";

        log.Info("Query validasi rekening debet");
        if (result)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
            else
            {
                inputDebitAccount = dbResult;
            }
        }


        if (inputDebitAccount != "" && result && isFileValid)
        {
            String[] detmake = payroll.Maker.Split(new String[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
            UserMap umap = session.CreateCriteria(typeof(UserMap))
                .Add(Expression.Like("UserHandle", detmake[0].Trim()))
                .Add(Expression.Like("ClientEntity", payroll.ClientID))
                .UniqueResult<UserMap>();


            IList<ClientAccount> lca = session.CreateSQLQuery("select a.* from clientaccounts a, useraccounts b where a.Id = b.Aid and b.Uid = ? and b.Fid = ? and a.account = ?")
               .AddEntity("ClientAccounts", typeof(ClientAccount))
               .SetInt32(0, umap.UserEntity)
               .SetInt32(1, 100)
               .SetString(2, inputDebitAccount)
               .List<ClientAccount>();

            if (lca.Count <= 0)
            {
                //add error message to table
                string out_msg_insert_error = "";
                if (!insertErrorDescription(session, temp_table, "Debit Account Not Valid.", out out_msg_insert_error, log))
                {
                    log.Error("Gagal " + out_msg_insert_error);
                    summaryError += temp_query + "||" + out_msg_insert_error;
                    result = false;
                    isLanjutValidasi = false;
                }
            }
            else
            {
                //WAITING||168||73000000
                payroll.DebitAccount = lca[0].Number;
                payroll.Description = "WAITING||" + lca[0].Id + "||" + systemDbSum;
            }
        }
        #endregion


        #region validasi value date
        //get debit account
        temp_query = @"select if(
                                (select NAMA from tmpPayroll where BARIS='VALUEDATE')!='',
                                (select NAMA from tmpPayroll where BARIS='VALUEDATE'),
	                                if(
		                                (select ACCOUNT from tmpPayroll where BARIS='VALUEDATE')!='',
		                                (select ACCOUNT from tmpPayroll where BARIS='VALUEDATE'),
		                                if(
			                                (select AMOUNT from tmpPayroll where BARIS='VALUEDATE')!='',
			                                (select AMOUNT from tmpPayroll where BARIS='VALUEDATE'),
			                                if(
				                                (select DESCRIPTION from tmpPayroll where BARIS='VALUEDATE')!='',
				                                (select DESCRIPTION from tmpPayroll where BARIS='VALUEDATE'),
				                                if(
					                                (select EMAIL from tmpPayroll where BARIS='VALUEDATE')!='',
					                                (select EMAIL from tmpPayroll where BARIS='VALUEDATE'),
					                                if(
						                                (select REFFNUM from tmpPayroll where BARIS='VALUEDATE')!='',
						                                (select REFFNUM from tmpPayroll where BARIS='VALUEDATE'),
						                                if(
						                                (select BENBANK from tmpPayroll where BARIS='VALUEDATE')!='',
						                                (select BENBANK from tmpPayroll where BARIS='VALUEDATE'),
						                                if(
							                                (select BENADDRESS from tmpPayroll where BARIS='VALUEDATE')!='',
							                                (select BENADDRESS from tmpPayroll where BARIS='VALUEDATE'),
							                                ''
						                                )
					                                )
					                                )
				                                )
			                                )
		                                )
	                                )
                            );";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);
        string inputValueDate = "";

        log.Info("Query validasi value date");
        if (result)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;
            }
            else
            {
                inputValueDate = dbResult;
            }
        }


        if (inputValueDate != "" && result && isFileValid)
        {
            //validate date time
            string time_input = ParameterHelper.GetString("PAYROLL_STARTTIME", session).Replace(":", "");
            string inputValueDateOri = inputValueDate;
            DateTime tanggal = DateTime.Now;

            //format tanggal valid
            if (!DateTime.TryParseExact(inputValueDate, "ddMMyyyyHHmm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tanggal))
            {
                inputValueDate += time_input; 
                if (!DateTime.TryParseExact(inputValueDate, "ddMMyyyyHHmm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tanggal))
                {
                    //add error message to table
                    string out_msg_insert_error = "";
                    if (!insertErrorDescription(session, temp_table, "Valuedate not valid. (valuedate : " + inputValueDateOri + "), Should be in ddMMyyyyHHmm or ddMMyyyy Format", out out_msg_insert_error, log))
                    {
                        log.Error("Gagal " + out_msg_insert_error);
                        summaryError += temp_query + "||" + out_msg_insert_error;
                        result = false;
                        isLanjutValidasi = false;
                    }
                    else
                    {
                        isFileValid = false;
                        isLanjutValidasi = false;
                    }
                }
            }

            //apa backdate
            if (tanggal.Date < DateTime.Now.Date)
            {
                //add error message to table
                string out_msg_insert_error = "";
                isLanjutValidasi = false;
                if (!insertErrorDescription(session, temp_table, "Valuedate backdate.", out out_msg_insert_error, log))
                {
                    log.Error("Gagal " + out_msg_insert_error);
                    summaryError += temp_query + "||" + out_msg_insert_error;
                    result = false;
                }
            }

            //update tanggal
            if (isFileValid) payroll.ProcessTime = tanggal;
        }
        #endregion


        #region collect error jika ada error
        //get error
        temp_query = @"select NAMA from tmpPayroll where BARIS = 'INVALID';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);

        log.Info("Query collect error jika ada error");
        if (result == true)
        {
            if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
                isLanjutValidasi = false;

            }
            else
            {
                msgErrorFile = dbResult;
                if (!msgErrorFile.Equals("")) isFileValid = false;
            }
        }
        #endregion
        #endregion VALIDASI TINGKAT 2


        #region VALIDASI TINGKAT 3
        #region migrasi

        int countIFT = 0;
        int payrollBankLain = 0;
        #region isi default
        //delete record yg tidak dipakai sebelum migrasi
        temp_query = @"delete from tmpPayroll where BARIS not regexp '[0-9]+';";
        temp_query = temp_query.Replace("tmpPayroll", temp_table);

        if (result == true)
        {
            if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
            {
                log.Error("Gagal " + temp_query + " === "+ msg);
                summaryError += temp_query + "||" + msg;
                result = false;
            }
        }


        if ((isFileValid) && (result))
        {
            //isi data parent ID
            temp_query = @"update tmpPayroll set baris='" + payroll.Id + "';";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);


            if (result == true)
            {
                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    result = false;
                }
            }

            //isi status yg null
            temp_query = @"update tmpPayroll set status=55 where status is null;";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);


            if (result == true)
            {
                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    result = false;
                }
            }

            //isi status 1 buat payroll bank lain
            //temp_query = @"update tmpPayroll set status=1, description = 'Nasabah Bank Lain||CR' where status <> 4 and instcode in ('" + TotalOBHelper.RTGS + "', '" + TotalOBHelper.LLG + "');";
            temp_query = @"update tmpPayroll set status=1, description = 'Nasabah Bank Lain||CR' where status <> 4 and instcode in "
                + "('" + TotalOBHelper.RTGS + "', '" + TotalOBHelper.LLG + "','"+ ParameterHelper.INSTCODE_BRIS +"');";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);


            if (result == true)
            {
                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    result = false;
                }
            }

            //isi account yg null
            temp_query = @"update tmpPayroll set account='000000000000000' where account is null;";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);


            if (result == true)
            {
                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    result = false;
                }
            }

            //isi amount yg null
            temp_query = @"update tmpPayroll set amount='0' where amount is null;";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);


            if (result == true)
            {
                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    result = false;
                }
            }

            #region get ispayrollbanklain
            
            dbResult = "";
            //temp_query = @"select count(baris) from tmpPayroll where status <> 4 and instcode in ('"+TotalOBHelper.RTGS+"', '"+TotalOBHelper.LLG+"')";
            temp_query = @"select count(baris) from tmpPayroll where status <> 4 and instcode in "
                + "('" + TotalOBHelper.RTGS + "', '" + TotalOBHelper.LLG + "','"+ParameterHelper.INSTCODE_BRIS+"')";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);
            
            if (result == true)
            {
                if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    isLanjutValidasi = false;
                    result = false;
                }
                else
                {
                    //set total record
                    payrollBankLain = int.Parse(dbResult);
                }
            }

            dbResult = "";
            
            temp_query = @"select count(baris) from tmpPayroll where status <> 4 and instcode in ('"+TotalOBHelper.IFT+"')";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);
            
            if (result == true)
            {
                if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    isLanjutValidasi = false;
                    result = false;
                }
                else
                {
                    //set total record
                    countIFT = int.Parse(dbResult);
                }
            }
            #endregion

            #region pindah data

            //finally pindah data ===============
            temp_query = @"insert into trxpayrolldetails
                    (PID, NAME, ACCOUNT, AMOUNT, STATUS, DESCRIPTION, EMAIL, CUSTOMERREFF, BANKCODE, BENADDRESS, INSTRUCTIONCODE)
                    select * from tmpPayroll;";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);


            if (result == true)
            {
                if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                {
                    log.Error("Gagal " + temp_query + " === "+ msg);
                    summaryError += temp_query + "||" + msg;
                    result = false;
                }
            }
            #endregion
        }
        #endregion

        #endregion

        #endregion

        #region update payroll transaction
        if (result) //Sukses insert
        {
            if (isFileValid)
            {
                payroll.Status = ParameterHelper.PAYROLL_WAITING_GENERATE_MASS_INQ;//10
                if (countIFT == 0)
                    payroll.Status = ParameterHelper.TRXSTATUS_INQUIRYNAMEPROCESS_FINISH;

                String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                if (desc.Count() > 2)
                    payroll.Description = desc[0] + "||" + desc[1] + "||" + systemDbSum;
                else
                    payroll.Description += systemDbSum;
                if (payrollBankLain > 0)
                    payroll.IsPayrollBankLain = 1;
                log.Info(" =(TrxID " + payroll.Id + ") Success Add to DB");
            }
            else
            {
                if (msgErrorFile.Equals("")) msgErrorFile = "File not valid.";

                payroll.Status = ParameterHelper.TRXSTATUS_REJECT;//4
                payroll.StatusEmail = ParameterHelper.PAYROLL_NEEDEMAIL;//1
                payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + "-" + msgErrorFile;
                string outRejectChild = "";
                if (rejectAllChild(session, payroll, out outRejectChild, log))
                {
                    log.Info("Success Update All Child.");
                }
                else
                {
                    payroll.Status = ParameterHelper.PAYROLL_EXCEPTION;
                    payroll.ErrorDescription = outRejectChild;
                }
            }
        }

        if (payroll.Status == ParameterHelper.TRXSTATUS_INQUIRYNAMEPROCESS_FINISH)
        {
            //jika dalam batch hanya ada payroll bank lain, lansgung ke authority tanpa perlu Mass Acc Inquiry
            Transaction trans = session.CreateCriteria(typeof(Transaction))
                               .Add(Expression.In("TransactionId", new int[] { 101, 102, 103 }))
                               .Add(Expression.Like("TObject", payroll.Id, MatchMode.Anywhere))
                               .UniqueResult<Transaction>();
            if (trans != null)//avoid Payroll Musimas
            {
                trans.Status = ParameterHelper.TRXSTATUS_WAITING_SCHEDULLER;//15

                /*Ini nih kunci utnuk laporan query dll nyaaa*/
                //string feeIFT = PayrollHelper.getFeeTransaction(session, payroll.Id, TotalOBHelper.IFT, log);
                //string feeRTG = PayrollHelper.getFeeTransaction(session, payroll.Id, TotalOBHelper.RTGS, log);
                //string feeLLG = PayrollHelper.getFeeTransaction(session, payroll.Id, TotalOBHelper.LLG, log);
                //string reject = PayrollHelper.getCountRejectTransaction(session, payroll.Id, TotalOBHelper.IFT, log);
                //string totalTrx = PayrollHelper.getTotalTransaction(session, payroll.Id, log);

                //payroll.TotalTrx = "|0:0|0:0|0:0|0:0|0:0|";//"|" + feeIFT + "|" + reject + "|" + totalTrx + "|" + feeRTG + "|" + feeLLG + "|";
                payroll.TotalTrx = "|0:0|0:0|0:0|0:0|0:0|0:0|";//penambahan untuk BRIS;

                session.Update(trans);
                session.Flush();
                log.Info("Update transactions success");


            }
        }

        payroll.LastUpdate = DateTime.Now;
        session.Update(payroll);
        session.Flush();
        #endregion

        session.Update(payroll);
        session.Flush();
        return result;
    }

    /*Limit Total Harian aazaaa*/
    public static String limitTransfer(ISession session, String trxCurr, String trxAmt, out double amtIdr, String runDate, int clientId)
    {
        IList<String> curList = trxCurr.Split(new String[] { "|" }, StringSplitOptions.None).ToList<String>();
        IList<String> amtList = trxAmt.Split(new String[] { "|" }, StringSplitOptions.None).ToList<String>();
        String resultLimit = "";

        if (runDate.Equals("010101"))
            runDate = DateTime.Now.ToString("yyMMdd");
        amtIdr = 0;

        //asumsi cuma ada 1 mata uang


        resultLimit = TotalOBHelper.onTotalHarianLimit(session, clientId, TotalOBHelper.IFT, runDate, "IDR", amtIdr);
        amtIdr = double.Parse(amtList[0]);

        return resultLimit;
    }


    public static Boolean checkCurrency(String numAcc)
    {
        try
        {
            if (numAcc.Substring(numAcc.Length - 11, 2).Equals("01"))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }


    //Inquiry ke BRINETS//dulu ProcessPayroll
    public static Boolean CheckAccount(TrxPayrollDetail pdetail, ISession session, ILog log)
    {
        log.Info("Start Inquiry Account");
        //Payrolllog(session, "--Masuk Inquiry Account--");
        ITransaction transaction = session.BeginTransaction();
        MessageABCSTransactionResponse response = null;
        Boolean _result = true;

        try
        {
            //IList<TrxPayrollDetail> lpdetail = payroll.TrxPayrollDetail;
            int counterAccTimeout = 0;

            String AccNo = pdetail.Account;
            log.Info(AccNo);
            String TellerID = "0374051";
            log.Info("Cek acc.no: " + pdetail.Account + ", name: " + pdetail.Name.Trim());

            int timeout = 0;

            while (timeout < 3)
            {
                int counter = 0;
                do
                {
                    response = inqAccBRIInterface(AccNo, TellerID);
                    counter++;
                }
                while (response == null && counter < 3);

                #region read interface response
                if (response == null)
                {
                    log.Info("Ooooppsss.....something wrong with network connection");
                    pdetail.ErrorDescription = "Response null, Time Out Inquiry Account, something wrong with network connection. Try again in few minutes.";
                }
                else
                {
                    if ((int.Parse(response.statuscode.Trim()) == 1))//response success
                    {
                        //if (int.Parse(response.msgabmessage[0][8].ToString()) == 1)
                        if ((int.Parse(response.msgabmessage[0][8].ToString()) == 1))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "1";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "1";//nama
                            }
                        }
                        // if (int.Parse(response.msgabmessage[0][8].ToString()) == 2) --> rekening tutup
                        else if ((int.Parse(response.msgabmessage[0][8].ToString()) == 2))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "2";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "2";//nama
                            }
                        }
                        //if (int.Parse(response.msgabmessage[0][8].ToString()) == 3) --> rekening jatuh tempo
                        else if ((int.Parse(response.msgabmessage[0][8].ToString()) == 3))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "3";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "3";//nama
                            }
                        }
                        // if (int.Parse(response.msgabmessage[0][8].ToString()) == 4) --> aktif (rekening baru)
                        else if ((int.Parse(response.msgabmessage[0][8].ToString()) == 4))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "4";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "4";//nama
                            }
                        }
                        // if (int.Parse(response.msgabmessage[0][8].ToString()) == 5) --> aktif (do not close)
                        else if ((int.Parse(response.msgabmessage[0][8].ToString()) == 5))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "5";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "5";//nama
                            }
                        }
                        // if (int.Parse(response.msgabmessage[0][8].ToString()) == 6) --> aktif (restricted)
                        else if ((int.Parse(response.msgabmessage[0][8].ToString()) == 6))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "6";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "6";//nama
                            }
                        }
                        // if ((int.Parse(response.msgabmessage[0][8].ToString()) == 7)) --> blokir rekening
                        else if ((int.Parse(response.msgabmessage[0][8].ToString()) == 7))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "7";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "7";//nama
                            }
                        }
                        // if ((int.Parse(response.msgabmessage[0][8].ToString()) == 8)) --> undefined account
                        else if ((int.Parse(response.msgabmessage[0][8].ToString()) == 8))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "8";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "8";//nama
                            }
                        }
                        // if ((int.Parse(response.msgabmessage[0][8].ToString()) == 9)) --> dormant
                        else if ((int.Parse(response.msgabmessage[0][8].ToString()) == 9))
                        {
                            if (response.msgabmessage[2][4].ToUpper().Trim().ToString().Equals(pdetail.Name.ToUpper().Trim()))
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + "-- Matched!");
                                pdetail.Status = ParameterHelper.TRXSTATUS_VERIFY;
                                pdetail.ErrorDescription = "";
                                pdetail.Description = "Account Matched" + "||" + "9";
                            }
                            else
                            {
                                log.Info("Host name: " + response.msgabmessage[2][4].Trim().ToString() + ", Input name: " + pdetail.Name + "-- Not Matched!");
                                pdetail.ErrorDescription = "";
                                pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                                pdetail.Description = "Nama Tidak Sesuai,Seharusnya : " + response.msgabmessage[2][4].Trim() + "||" + "9";//nama
                            }
                        }
                        else
                        {
                            pdetail.Description = response.msgabmessage[2][4].Trim() + "||" + response.msgabmessage[0][8].ToString();
                            pdetail.ErrorDescription = "masuk else";
                        }
                        pdetail.LastUpdate = DateTime.Now;
                        break;
                    }
                    else if (int.Parse(response.statuscode.Trim()) == 2)//response failed
                    {
                        log.Info("response failed (2) : " + response.msgabmessage[0][4]);
                        pdetail.Description = response.msgabmessage[0][4];
                        pdetail.Status = ParameterHelper.TRXSTATUS_MAKER;
                        break;
                    }
                    else//response timeout
                    {
                        pdetail.ErrorDescription = "Response Timeout";
                        log.Info("Ooooopsss....response timeout");
                        timeout++;
                    }
                }
                pdetail.LastUpdate = DateTime.Now;
                session.Update(pdetail);
                session.Flush();
                #endregion

            }
            //end loop

            if (timeout >= 3)
            {
                Parameter paramJeda = session.Load<Parameter>("PAYROLL_CHECKACC_TIMETRYAGAIN");
                int jeda = int.Parse(paramJeda.Data);
                pdetail.LastUpdate = DateTime.Now.AddMinutes(jeda);
                log.Info("Time Out Inquiry Account, try again in " + jeda + " minutes");
                pdetail.Description = "";
                pdetail.ErrorDescription = "Time Out Inquiry Account, try again in " + DateTime.Now.AddMinutes(jeda).ToString("dd/MM/yyyy HH:mm:ss");
                //counterAccTimeout++;
                //log.Info("counter acc timeout = " + counterAccTimeout);
            }

            session.Update(pdetail);


            //log.Info("counterAccTimeout after flush and update : "+counterAccTimeout); 
            //log.Info("limitAccTimeout after flush and update : "+limitAccTimeout);

            //if (counterAccTimeout >= limitAccTimeout)
            //{
            //    //reject;
            //    //log.Info("Ooooops account inquiry error" + ex.Message + ex.StackTrace);
            //    log.Info("counterAccTimeout lebih besar sama dengan limitAccTimeouts");
            //    try
            //    {
            //        String[] desc = payroll.Description.Split(new String[] { "||" }, StringSplitOptions.None);
            //        log.Info("isi desc : "+ desc[0] + "," +desc[1] + ","+ ",payroll id : " + payroll.Id.ToString());
            //        payroll.Description = ParameterHelper.TRXDESCRIPTION_REJECT + " - Time Out Connection for Checking Account Please Try Again" + "||" + desc[1];
            //        payroll.Status = ParameterHelper.TRXSTATUS_REJECT;


            //        log.Info("status payroll : "+payroll.Description + "status : " +payroll.Status);

            //        session.Update(payroll);
            //         transaction.Commit();
            //         transaction.Dispose();
            //        session.Flush();
            //        _result = false;
            //        return _result;
            //    }
            //    catch(Exception d)
            //    {
            //        log.Info("errornya : "+ d.Message + " detailnya : " + d.StackTrace);
            //    }
            //}   

        }
        catch (Exception ex)
        {
            log.Info("Ooooops account inquiry error" + ex.Message + ex.StackTrace);
            pdetail.ErrorDescription = "Check Account Erro : " + ex.Message + ">>" + ex.StackTrace + ">>" + ex.InnerException + " .. Will try again in few minutes";
            Parameter paramJeda = session.Load<Parameter>("PAYROLL_CHECKACC_TIMETRYAGAIN");
            int jeda = int.Parse(paramJeda.Data);
            pdetail.LastUpdate = DateTime.Now.AddMinutes(jeda);
            //pdetail.Status = ParameterHelper.PAYROLL_EXCEPTION;
            _result = false;
        }

        //if (payroll.Status != ParameterHelper.TRXSTATUS_REJECT)
        //{
        //    payroll.Status = 17;
        //    payroll.Description = payroll.Description = payroll.Description.Replace(ParameterHelper.TRXDESCRIPTION_WAITING, "UPLOADED");
        //    _result = true;
        //}
        //session.Save(payroll);

        transaction.Commit();
        transaction.Dispose();
        log.Info("End Payroll account cek...");
        session.Flush();
        return _result;
    }

    //ke BRIInterface
    public static MessageABCSTransactionResponse inqAccBRIInterface(String AccNo, String TellerID)
    {
        String curr = _Currency(AccNo);
        String accType = _AccountType(AccNo);
        String trancode = "1000";

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

        dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;
        dAbcsMessage.Rows[0]["TRANSACTIONCODE"] = trancode;
        dAbcsMessage.Rows[0]["BUCKET1"] = AccNo;
        dAbcsMessage.Rows[0]["BUCKET7"] = "10000000";
        dAbcsMessage.Rows[0]["BUCKET8"] = "10000000";
        dAbcsMessage.Rows[0]["BUCKET19"] = DateTime.Now.ToString("ddMMyy");  //effective date
        dAbcsMessage.Rows[0]["BUCKET20"] = ""; //orig jseq for reverse
        dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";
        dAbcsMessage.Rows[0]["TELLERREMARK1"] = "Inquiry Account";
        dAbcsMessage.Rows[0]["TELLERREMARK2"] = "CMS";
        dAbcsMessage.Rows[0]["ALTERNATECURRENCY1"] = curr;
        dAbcsMessage.Rows[0]["ALTERNATECURRENCY2"] = curr;
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
        request.applicationname = "SEMFAX";

        try
        {
            response = transaction.doABCSTransaction(request);

        }
        catch (Exception ex)
        {
            //Eventlogger.Write("Inquiry Acc BriInterface :: " + ex.Message, Eventlogger.ERROR);
        }

        return response;
    }

    public static String _AccountType(String AccountNo)
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

    public static String _Currency(String AccountNo)
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

    public static void Payrolllog(ISession session, String Description)
    {

        String _date_folder = DateTime.Now.ToString("yyyyMMdd");

        Parameter param = session.Load<Parameter>("FOLDER_PAYROLL_log");
        String port = param.Data;
        String _payroll_log_dir = port + "\\" + _date_folder;

        String path = _payroll_log_dir;
        String fName = "\\" + "AccountChecking" + DateTime.Now.ToString("ddMMyyyy") + ".log";
        path = path + fName;

        String time = DateTime.Now.ToString();

        StreamWriter log = new StreamWriter(path, true);
        log.WriteLine("" + time + " " + Description);
        log.Close();

    }

    //public static int cekMaks(String jumlah)
    //{
    //    ILog log = logManager.Getlogger(typeof(CheckAccountHelper));

    //    int result = 0;
    //    int sum = 0;
    //    log.Info("jumlah count rekening : " + jumlah);

    //    if (int.TryParse(jumlah, out sum))
    //    {
    //        //result = Math.Floor(decimal.Divide(Convert.ToDecimal(sum), Convert.ToDecimal(4)));
    //        result = Convert.ToInt32(Math.Ceiling(decimal.Divide(Convert.ToDecimal(sum), Convert.ToDecimal(4))));
    //        log.Info("Jumlah Time Out Checking Maks : " + result);
    //    }
    //    return result;
    //}

    public static bool rejectAllChild(ISession session, TrxPayroll payroll, out string outMsgUpdate, ILog log)
    {
        outMsgUpdate = "";
        bool result = true;
        try
        {
            //delete transaction
            Transaction tx = session.CreateCriteria(typeof(Transaction))
               .Add(Expression.In("TransactionId", new int[] { 101, 102, 103, 1151, 1152, 1153 }))
               .Add(Expression.Like("TObject", payroll.Id, MatchMode.Anywhere))
               .UniqueResult<Transaction>();
            if (tx != null)
            {
                log.Info("delete transactions Id : " + tx.Id.ToString());
                session.Delete(tx);
                session.Flush();
            }

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

    public static bool isCustomerReffExist(ISession session, TrxPayroll payroll, string customer_reff, out string outMsgError, ILog log)
    {
        outMsgError = "";
        bool result = true;
        try
        {
            IList<TrxPayroll> listTrxPayroll = session.CreateCriteria(typeof(TrxPayroll))
                .Add(Expression.Not(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                .Add(Expression.Eq("ClientID", payroll.ClientID))
                .List<TrxPayroll>();

            foreach (TrxPayroll thepayroll in listTrxPayroll)
            {
                //IList<TrxPayrollDetail> listTrxPayrollDetail = session.CreateCriteria(typeof(TrxPayrollDetail))
                //    .Add(Expression.Not(Expression.Eq("Status", ParameterHelper.TRXSTATUS_REJECT)))
                //    .Add(Expression.Eq("Pid", thepayroll.Id))
                //    .Add(Expression.Eq("CustomerReff", customer_reff))
                //    .List<TrxPayrollDetail>();


                IList<TrxPayrollDetail> listTrxPayrollDetail = session.CreateSQLQuery("select * from trxpayrolldetails where pid<=? and status!=? and customerreff=? order by id ASC")
                    .AddEntity(typeof(TrxPayrollDetail))
                    .SetString(0, thepayroll.Id)
                    .SetInt32(1, ParameterHelper.TRXSTATUS_REJECT)
                    .SetString(2, customer_reff)
                    .List<TrxPayrollDetail>();


                if (listTrxPayrollDetail.Count > 0)
                {
                    result = false;
                    break;
                }
            }
        }
        catch (Exception e)
        {
            string eMsg = "Error check customer reff " + e.Message + "==" + e.InnerException + "==" + e.StackTrace;
            log.Error(eMsg);
            outMsgError = eMsg;
            result = false;
        }
        return result;
    }

    public static bool insertErrorDescription(ISession session, string temp_table, string errorMsg, out string out_msg, ILog log)
    {
        out_msg = "";
        string temp_query = "";
        string dbResult = "";
        string msg = "";

        bool result = true;
        try
        {
            //select apa sudah ada, jika sudah update, belum insert
            //get error
            temp_query = @"select NAMA from tmpPayroll where BARIS = 'INVALID';";
            temp_query = temp_query.Replace("tmpPayroll", temp_table);
            string isExist = "";


            if (result == true)
            {
                if (!PayrollHelper.ExecuteQueryValue(session, temp_query, out dbResult, out msg))
                {
                    log.Error("Gagal " + msg);
                    result = false;
                }
                else
                {
                    isExist = dbResult;
                }
            }


            if (isExist.Equals(""))
            {
                //add error message to table
                temp_query = @"insert into tmpPayroll values ('INVALID', '" + errorMsg + "', '', '', '', '', '', '', '', '', '');";
                temp_query = temp_query.Replace("tmpPayroll", temp_table);


                if (result == true)
                {
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error("Gagal " + msg);
                        result = false;
                    }
                }
            }
            else
            {
                //update error message to table
                errorMsg = "*" + errorMsg;
                temp_query = @"update tmpPayroll set NAMA = concat(NAMA, '" + errorMsg + "') where baris='INVALID';";
                temp_query = temp_query.Replace("tmpPayroll", temp_table);


                if (result == true)
                {
                    if (!PayrollHelper.ExecuteQuery(session, temp_query, out msg))
                    {
                        log.Error("Gagal " + msg);
                        result = false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            string eMsg = "Error on update child " + e.Message + "==" + e.InnerException + "==" + e.StackTrace;
            log.Error(eMsg);
            out_msg = eMsg;
            result = false;
        }
        return result;
    }

}