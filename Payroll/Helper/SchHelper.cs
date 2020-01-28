using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
//using BRIChannel.Manager;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
//using BRIChannel.Pocos;
//using BRIChannel.Helper;
using BRIChannelSchedulerNew.Payroll.Pocos;
//using BRIChannelSchedulerNew.GlobalPocos;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Mail;
using System.Net;
using BriInterfaceWrapper.BriInterface;
using System.Data.SqlClient;
using log4net;
using BRIVAWrapper.Briva;
/// <summary>
/// Summary description for TransferHelper
/// </summary>
public class SchHelper
{
    public static int DOREVERSAL = 1;
    public static int DOCANCELSPV = 2;
    public static int DOSPVTRX = 3;
	public static String PENDING = "Pending";
    public static String RELOAD = "Reload";
    public static String SUCCESS = "Success";
    public static String ERRORSCH = "Exception";
    public static String FAILED = "Failed";
    public static String NULL = "Null";
    public static String TIMEOUT = "Time Out";
    public static String CANCELSPVFAILED = "Suspect 1";
    public static String CANCELSPVTIMEOUT = "Suspect 2";
    public static String CANCELSPVTIMENULL = "Suspect 3";

    public SchHelper()
    {
    }

    public static String CekJenisAccount(ISession session,int jnsacc, String fid)
    {
        String result = "";
        try
        {
            //untuk account jenis credit only
            if (jnsacc == ParameterHelper.ACCOUNT_CREDITONLY)
            {
                result = "Account Credit Only.";
                return result;
            }

            // untuk fungsi account information and report
            if (jnsacc == ParameterHelper.ACCOUNT_INQUIRY)
            {
                Parameter paramfunc = session.Load<Parameter>("FUNCTION_ACCOUNT_INQUIRY");
                String accinq = paramfunc.Data;
                String[] fidlist = accinq.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (String ffid in fidlist)
                {
                    if (fid.Equals(ffid))
                    { }
                    else
                    {
                        result = "Account Inquiry Only.";
                        return result;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result = ex.StackTrace;
        }

        return result;
    }

    public static string ConvertDataTableToXML(DataTable dtBuildSQL)
    {
        DataSet dsBuildSQL = new DataSet();
        StringBuilder sbSQL;
        StringWriter swSQL;
        string XMLformat;

        sbSQL = new StringBuilder();
        swSQL = new StringWriter(sbSQL);
        dsBuildSQL.Merge(dtBuildSQL, true, MissingSchemaAction.AddWithKey);
        dsBuildSQL.Tables[0].TableName = "Table";
        foreach (DataColumn col in dsBuildSQL.Tables[0].Columns)
        {
            col.ColumnMapping = MappingType.Attribute;
        }
        dsBuildSQL.WriteXml(swSQL, XmlWriteMode.WriteSchema);
        XMLformat = sbSQL.ToString();
        return XMLformat;
    }
    
    public static String BufferCurrencyFormat(String input)
    {

        String output = "";
        String[] arrInput = input.Split('.');
        if (arrInput.Length > 1)
        {
            String temp = "";
            int length = 7 - arrInput[1].Length;
            for (int x = 0; x < length; x++)
            {
                temp += "0";
            }
            output = arrInput[0] + arrInput[1] + temp;
        }
        else if (arrInput.Length == 1)
        {

            output = arrInput[0] + "0000000";
        }
        return output;
    }

    public static String _CurrNo(String Currency)
    {
        switch (Currency)
        {
            case "IDR":
                return ("01");
            case "USD":
                return ("02");
            case "SGD":
                return ("03");
            case "JPY":
                return ("04");
            case "GBP":
                return ("05");
            case "HKD":
                return ("06");
            case "DEM":
                return ("07");
            case "MYR":
                return ("08");
            case "AUD":
                return ("09");
            case "CAD":
                return ("10");
            case "CHF":
                return ("11");
            case "EUR":
                return ("18");
            case "THB":
                return ("19");
            case "SEK":
                return ("20");
            case "SAR":
                return ("21");
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


    

    public static String _AccountType(String AccountNo)
    {
        switch (AccountNo.Substring(AccountNo.Length - 3, 1))
        {
            case "1":
                return ("L");
            case "5":
                return ("S");
            case "3":
                return ("D");
            case "9":
                return ("D");
            default:
                return (" ");
        }
    }

    

    

    public static Boolean GetKurs(String DebetCCY, String CreditCCY, String TellerID, out double buyrate, out double sellrate, out double bookrate,out double debetBuyRate,out double creditSelRate)
    {
        buyrate = 0;
        sellrate = 0;
        bookrate = 0;
        debetBuyRate = 0;
        creditSelRate = 0;

        MessageCurrencyRequest requestCurr = new MessageCurrencyRequest();
        MessageCurrencyResponse responseCurr = new MessageCurrencyResponse();
        briInterfaceService transaction2 = new briInterfaceService();
        try
        {
            requestCurr.creditcurrencycode = CreditCCY;
            requestCurr.debetcurrencycode = DebetCCY;            
            requestCurr.tellerid = TellerID;
            //transaction2.Timeout = 500000;
            transaction2.Timeout = 6000000;//milisecond
            responseCurr = transaction2.getCurrencyCounter(requestCurr);

            if (int.Parse(responseCurr.statuscode.Trim()) == 1)
            {
                buyrate = double.Parse(responseCurr.debetbuyrate) - double.Parse(responseCurr.dtSpreadDebet.Rows[0]["TTBuySpread"].ToString());
                sellrate = double.Parse(responseCurr.creditsellrate) + double.Parse(responseCurr.dtSpreadCredit.Rows[0]["TTSellSpread"].ToString());
                bookrate = double.Parse(responseCurr.debetbookrate);
                debetBuyRate = double.Parse(responseCurr.debetbuyrate);
                creditSelRate = double.Parse(responseCurr.creditsellrate);
                return true;
            }
            else 
            {
                return false;
            }
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static Boolean InquiryAcc(String NoRek, out double balance, out int statusrek) {
        balance = 0;
        statusrek = 0;

        try {
            String curr = _Currency(NoRek);
            String accType = _AccountType(NoRek);
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
            dAbcsMessage.Rows[0]["BUCKET1"] = NoRek;
            dAbcsMessage.Rows[0]["BUCKET7"] = "";
            dAbcsMessage.Rows[0]["BUCKET8"] = "";
            dAbcsMessage.Rows[0]["BUCKET19"] = DateTime.Now.ToString("ddMMyy");  //effective date
            dAbcsMessage.Rows[0]["BUCKET20"] = ""; //orig jseq for reverse
            dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";
            dAbcsMessage.Rows[0]["TELLERREMARK1"] = "Inquiry Account";
            dAbcsMessage.Rows[0]["TELLERREMARK2"] = "CMS";
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY1"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY2"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY3"] = "IDR";

            request.branchcode = "0374";
            request.tellerid = "0374891";
            request.supervisorid = "";
            request.origjournalseq = "";
            request.dSocketHeader = dSocketHeader;
            request.dMiddleWareHeader = dMiddleWareHeader;
            request.dAbcsHeader = dAbcsHeader;
            request.dAbcsMessage = dAbcsMessage;
            request.applicationname = "CMS";

            try
            {
                response = transaction.doABCSTransaction(request);
                if (response != null)
                {
                    if (int.Parse(response.statuscode.Trim()) == 1)
                    {
                        balance = double.Parse(response.msgabmessage[0][66].ToString());
                        statusrek = 1;
                        return true;
                        
                    }
                    else {
                        return false;
                    }
                }
                else {
                    return false;
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Write("Inquiry Acc BriInterface :: " + ex.Message, EventLogger.ERROR);
            }

            return true;
        } catch (Exception ex){
            return false;
		}
    }

    public static String InquiryAccName(String NoRek)
    {
        try
        {
            String curr = _Currency(NoRek);
            String accType = _AccountType(NoRek);
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
            dAbcsMessage.Rows[0]["BUCKET1"] = NoRek;
            dAbcsMessage.Rows[0]["BUCKET7"] = "";
            dAbcsMessage.Rows[0]["BUCKET8"] = "";
            dAbcsMessage.Rows[0]["BUCKET19"] = DateTime.Now.ToString("ddMMyy");  //effective date
            dAbcsMessage.Rows[0]["BUCKET20"] = ""; //orig jseq for reverse
            dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";
            dAbcsMessage.Rows[0]["TELLERREMARK1"] = "Inquiry Account";
            dAbcsMessage.Rows[0]["TELLERREMARK2"] = "CMS";
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY1"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY2"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY3"] = "IDR";

            request.branchcode = "0374";
            request.tellerid = "0374891";
            request.supervisorid = "";
            request.origjournalseq = "";
            request.dSocketHeader = dSocketHeader;
            request.dMiddleWareHeader = dMiddleWareHeader;
            request.dAbcsHeader = dAbcsHeader;
            request.dAbcsMessage = dAbcsMessage;
            request.applicationname = "CMS";

            try
            {
                response = transaction.doABCSTransaction(request);
                if (response != null)
                {
                    if (int.Parse(response.statuscode.Trim()) == 1)
                    {
                        if (accType.Equals("L"))
                        {
                            return response.msgabmessage[0][7].ToString().Trim().Replace("(","[").Replace(")","]");
                        } 
                        else if (accType.Equals("S"))
                        {
                            return response.msgabmessage[2][4].ToString().Trim().Replace("(", "[").Replace(")", "]");
                        } 
                        else if (accType.Equals("D"))
                        {
                            return response.msgabmessage[2][4].ToString().Trim().Replace("(", "[").Replace(")", "]");
                        }
                        else
                        {
                            return "";
                        } 
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Write("Inquiry Acc BriInterface :: " + ex.Message, EventLogger.ERROR);
            }

            return "";
        }
        catch (Exception ex)
        {
            return "";
        }
    }

    public static void doSuspectEmail(ISession session,int id,String process,String msg) {
        //TrxSwift o = session.Load<TrxSwift>(id);

        //#region trxobhist

        //int parentpp = 0;
        //if (o.Type == 3)
        //{
        //    IList<TrxPaymentPriorityBRIDetail> txListo2 = session.CreateCriteria(typeof(TrxPaymentPriorityBRIDetail))
        //       .Add(Expression.Eq("CmsRefNo", o.ReferenceNum))
        //       .AddOrder(Order.Desc("Id"))
        //       .List<TrxPaymentPriorityBRIDetail>();

        //    parentpp = txListo2[0].ParentId;
        //}

        //Currency curr2 = null;
        //TrxOBHist ob = new TrxOBHist();
        //ob.ApproverInfo = o.Approver;

        //ob.BaseCur = "IDR";
        //ob.BenAcc = o.CreditAccount;
        //ob.BenAccName = o.CreditAccountName;
        //ob.BenBankIdentifier = o.BeneficiaryBankBIC;
        //ob.BenBankName = o.BeneficiaryBankName;
        //ob.ChargeAmt = o.Charge;
        //ob.ChargeCur = o.ChargeCurrency;
        //ob.CheckerInfo = o.Checker;
        //ob.ClientId = int.Parse(o.ClientID);
        //ob.CreditAmt = o.CreditAmount;
        //ob.CreditCur = o.CreditCurrency;
        //ob.DebitAcc = o.DebitAccount;
        //if (o.BranchID.Equals("0374"))
        //{
        //    ClientAccount cd = session.CreateCriteria(typeof(ClientAccount))
        //        .Add(Expression.Like("Number", o.DebitAccount))
        //        .Add(Expression.Eq("Pid", int.Parse(o.ClientID)))
        //        .UniqueResult<ClientAccount>();
        //    ob.DebitAccName = cd.Code;
        //}
        //ob.DebitAmt = 0;
        //ob.BaseAmt = 0;        
        //ob.DebitCur = o.DebitCurrency;
        //ob.KursJualBase = "0";        
        //ob.KursBeliTrx = "1";
        //ob.KursJualTrx = "1"; 
        //ob.LastUpdate = DateTime.Now;
        //ob.MakerInfo = o.Maker;

        //ob.Description = msg + " On " + process;
        //ob.Status = 4;
        //ob.TrxActId = o.Id.ToString();
        //ob.TrxCode = TotalOBHelper.SWIFT;
        //ob.TrxRemark = o.TrxRemark;
        //ob.TrxType = o.Type;
        //if (o.Type == 2)
        //{
        //    ob.ParentId = o.ParentID;
        //}
        //else if (o.Type == 3)
        //{
        //    ob.ParentId = parentpp;
        //}
        //else
        //{
        //    ob.ParentId = 0;
        //}
        //ob.ValueDate = DateTime.Now.ToString("yyMMdd");
        //ob.VoucherCode = o.Rate.ToString();

        //session.Save(ob);
        //#endregion

    }

    public static String formatAmtBrinets(double amount) {
        return amount.ToString("0000000000.00").Replace(',', '.');
    }

    public static void trancode0808(){
        MessageABCSTransactionRequest request = new MessageABCSTransactionRequest();
        MessageABCSTransactionResponse[] response = new MessageABCSTransactionResponse[10];

        DataTable dSocketHeader = new DataTable("SOCKETHEADER");
        DataTable dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
        DataTable dAbcsHeader = new DataTable("ABCSHEADER");
        DataTable dAbcsMessage = new DataTable("ABCSMESSAGE");

        briInterfaceService transaction = new briInterfaceService();
        transaction.initiateABCS(ref dSocketHeader, ref dMiddleWareHeader, ref dAbcsHeader, ref dAbcsMessage);

        dMiddleWareHeader.Rows[0]["TRANCODE"] = "0808";

        dAbcsMessage.Rows[0]["TRANSACTIONCODE"] = "0808";
        dAbcsMessage.Rows[0]["BUCKET5"] = "10000000";
        dAbcsMessage.Rows[0]["BUCKET6"] = "10000000";

        request.applicationname = "CMS";
        request.branchcode = "0374";
        request.tellerid = "0374891";
        request.supervisorid = "";
        request.origjournalseq = "";
        request.dSocketHeader = dSocketHeader;
        request.dMiddleWareHeader = dMiddleWareHeader;
        request.dAbcsHeader = dAbcsHeader;
        request.dAbcsMessage = dAbcsMessage;

        int counter = 0;
        int status = 2;
        while (counter < 10)
        {
            response[counter] = new MessageABCSTransactionResponse();
            try
            {
                dAbcsMessage.Rows[0]["REBIDREQUEST"] = counter.ToString();
                response[counter] = transaction.doABCSTransaction(request);
                status = int.Parse(response[counter].statuscode);
                counter++;
            }
            catch
            {
                break;
            }
        }

        dSocketHeader.Clear();
        dMiddleWareHeader.Clear();
        dAbcsHeader.Clear();
        dAbcsMessage.Clear();

        dSocketHeader.Dispose();
        dMiddleWareHeader.Dispose();
        dAbcsHeader.Dispose();
        dAbcsMessage.Dispose();

        transaction.Dispose();
    }
    
    private String MD5(String text)
    {
        String result = "";

        UTF8Encoding encoder = new UTF8Encoding();
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();

        byte[] hash = provider.ComputeHash(encoder.GetBytes(text));
        System.Text.StringBuilder s = new System.Text.StringBuilder();
        foreach (byte b in hash)
        {
            s.Append(b.ToString("x2").ToLower());
        }
        result = s.ToString();

        return result;
    }

    public static bool isEmailAddress(String s)
    {
        System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
        _stringBuilder.Append(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}");
        _stringBuilder.Append(@"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\");
        _stringBuilder.Append(@".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");			

        System.Text.RegularExpressions.Match m;
        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(_stringBuilder.ToString());   //(@"^[a-zA-Z0-9 ]+$");
        try
        {
            m = r.Match(s);
            if (m.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (ArgumentException ex)
        {
            //Handle the validation failing here
            return false;
        }

    }

    public static Boolean InquirySaldoAcc(String NoRek, out double balance, out int statusrek)
    {
        balance = 0;
        statusrek = 0;

        try
        {
            String curr = _Currency(NoRek);
            String accType = _AccountType(NoRek);
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
            dAbcsMessage.Rows[0]["BUCKET1"] = NoRek;
            dAbcsMessage.Rows[0]["BUCKET7"] = "";
            dAbcsMessage.Rows[0]["BUCKET8"] = "";
            dAbcsMessage.Rows[0]["BUCKET19"] = DateTime.Now.ToString("ddMMyy");  //effective date
            dAbcsMessage.Rows[0]["BUCKET20"] = ""; //orig jseq for reverse
            dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";
            dAbcsMessage.Rows[0]["TELLERREMARK1"] = "Inquiry Account";
            dAbcsMessage.Rows[0]["TELLERREMARK2"] = "CMS";
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY1"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY2"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY3"] = "IDR";

            request.branchcode = "0374";
            request.tellerid = "0374891";
            request.supervisorid = "";
            request.origjournalseq = "";
            request.dSocketHeader = dSocketHeader;
            request.dMiddleWareHeader = dMiddleWareHeader;
            request.dAbcsHeader = dAbcsHeader;
            request.dAbcsMessage = dAbcsMessage;
            request.applicationname = "CMS";

            try
            {
                response = transaction.doABCSTransaction(request);
                if (response != null)
                {
                    if (int.Parse(response.statuscode.Trim()) == 1)
                    {
                        if (accType.Equals("L"))
                        {
                            balance = double.Parse(response.msgabmessage[0][21].ToString().Trim()) - double.Parse(response.msgabmessage[0][22].ToString());


                            if (balance < 0)
                                balance = balance * (-1);
                            statusrek = 1;
                            return true;
                        }
                        else
                        {
                            balance = double.Parse(response.msgabmessage[0][66].ToString());
                            statusrek = int.Parse(response.msgabmessage[0][8].ToString());
                            return true;
                        }
                    }
                    else {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
                //EventLogger.Write("Inquiry Acc BriInterface :: " + ex.Message, EventLogger.ERROR);
            }

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static String getRemittanceNumber(ISession session, int type, int rettyp, string trxcode)
    {
        //sayedzul 2016-03-07 Add sequence for mass and PP (RTGS & CN
        string param = "";
        Parameter parameter = new Parameter();
        if (trxcode.Equals(TotalOBHelper.RTGS))
        {
            if (type == 1) // single
                param = "LAST_REMITTANCE_SEQ";
            else if (type == 2) // Mass
                param = "LAST_REMITTANCE_SEQ_MASS";
            else if (type == 4) //payroll
                param = "LAST_REMITTANCE_SEQ_PAYROLL";
            else // PP
                param = "LAST_REMITTANCE_SEQ_PP";
        }
        else
        {
            if (type == 1) // single
                param = "LAST_REMITTANCE_SEQ_CN";
            else if (type == 2) // Mass
                param = "LAST_REMITTANCE_SEQ_CN_MASS";
            else if (type == 4) //payroll
                param = "LAST_REMITTANCE_SEQ_CN_PAYROLL";
            else // PP
                param = "LAST_REMITTANCE_SEQ_CN_PP";
        }

        parameter = session.Load(typeof(Parameter), param) as Parameter;

        string seq = (int.Parse(parameter.Data.Trim()) + 1).ToString();

        parameter.Data = seq;
        int seqnum = int.Parse(seq);
        String procode = "";

        session.Update(parameter, param);
        session.Flush();

        if (trxcode.Equals(TotalOBHelper.RTGS))
        {
            parameter = session.Load(typeof(Parameter), "GENERAL_CMS_BRANCH") as Parameter;

            Parameter procodeRTGS = session.Load(typeof(Parameter), "CMS_RTGS_PROCODE") as Parameter;
            procode = procodeRTGS.Data;
        }
        else
        {
            parameter = session.Load(typeof(Parameter), "GENERAL_CMS_CLEARING_BRANCH") as Parameter;

            Parameter procodeCLEARING = session.Load(typeof(Parameter), "CMS_CLEARING_PROCODE") as Parameter;
            procode = procodeCLEARING.Data;
        }
        String code_branch = parameter.Data;

        seq = (seqnum % 100000).ToString("00000");

        String ret = code_branch + "01" + type.ToString() + seq + procode;
        
        ret = ret + ComputeCheckSumBrinets(ret).ToString();

        if (rettyp == 1)
        {
            return ret;
        }
        else
        {
            return seq;
        }
    }


    public static int ComputeCheckSumBrinets(String Value)
    {
        int nSubTotal = 0;
        int nTotal = 0;
        string weight = "32765432765432";
        for (int b = 0; b < 14; b++)
        {
            nSubTotal = int.Parse(Value.Substring(b, 1)) * int.Parse(weight.Substring(b, 1));
            nTotal = nTotal + nSubTotal;
        }
        int nRem = nTotal % 10;
        int nRemMin = 10 - nRem;
        int cek = 0;
        if (nRemMin < 10)
        {
            cek = nRemMin;
        }
        return cek;
    }

    public static MessageMBASETransactionResponse getSaCaCIFNo(ISession session, String _debetAcc)
    {
        MessageMBASETransactionRequest request = new MessageMBASETransactionRequest();
        MessageMBASETransactionResponse response = new MessageMBASETransactionResponse();

        DataTable dSocketHeader = new DataTable("SOCKETHEADER");
        DataTable dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
        DataTable dMbaseHeader = new DataTable("MBASEHEADER");
        DataTable dMbaseMessage = new DataTable("MBASEMESSAGE");


        string trancode = "2618";
        briInterfaceService transaction = new briInterfaceService();
        transaction.initiateMBASE(trancode, ref dSocketHeader, ref dMiddleWareHeader, ref dMbaseHeader, ref dMbaseMessage);

        dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;

        dMbaseHeader.Rows[0]["TRANSACTIONCODE"] = trancode;
        dMbaseHeader.Rows[0]["ACTIONCODE"] = "A"; // "A";
        dMbaseHeader.Rows[0]["NOOFRECORD"] = "1";

        dMbaseHeader.Rows[0]["ACCOUNT"] = _debetAcc;
        dMbaseHeader.Rows[0]["ACCOUNTTYPE"] = SchHelper._AccountType(_debetAcc); // "D";
        dMbaseHeader.Rows[0]["CIF"] = "";

        Parameter acctno = session.Load<Parameter>("BRIIN_ACCTNO");
        Parameter actype = session.Load<Parameter>("BRIIN_ACTYPE");
        dMbaseMessage.Rows[0][acctno.Data] = _debetAcc;
        dMbaseMessage.Rows[0][actype.Data] = SchHelper._AccountType(_debetAcc);
        request.applicationname = "CMS";
        request.branchcode = "0206";//"0374";
        request.tellerid = "0206891";//"0374051";
        request.supervisorid = "";
        request.origjournalseq = "";
        request.dSocketHeader = dSocketHeader;
        request.dMiddleWareHeader = dMiddleWareHeader;
        request.dMbaseHeader = dMbaseHeader;
        request.dMbaseMessage = dMbaseMessage;

        response = transaction.doMBASETransaction(request);

        return response;
    }

    public static Boolean InquiryAccountName(String configbriva, String norek, out String name, out String curr, out String briva)
    {
        bool result = true; name = ""; curr = ""; briva = "";

        #region Cek ke Brinets
        MessageABCSTransactionResponse response = inqAccBRIInterface(norek);
        if (!_AccountType(norek).Equals("L"))
        {
            if (response == null) result = false;
            else
            {
                if (int.Parse(response.statuscode.Trim()) == 1)
                {
                    name = (int.Parse(response.msgabmessage[0][8].ToString()) == 1) ? response.msgabmessage[0][7].ToString().Trim() : "[NOTACTIVE]";
                    curr = _Currency(norek);
                }
                else result = false;
            }
        }
        else
        {
            if (response == null) result = false;
            else
            {
                if (int.Parse(response.statuscode.Trim()) == 1)
                {
                    name = response.msgabmessage[2][4].ToString().Trim();
                    curr = _Currency(norek);
                }
                else result = false;
            }
        }
        #endregion

        #region Cek ke Server Briva
        if (result == false)
        {
            SqlConnection conn = new SqlConnection(configbriva);
            try
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_brivainquiry", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@BrivaAccount", norek));
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    curr = _Currency(rdr["CorpRekNo"].ToString());
                    name = rdr["NamaCustomer"].ToString();
                    briva = rdr["CorpRekNo"].ToString();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                if (conn.State != ConnectionState.Closed) conn.Close();
            }
        }
        #endregion

        return result;
    }

    private static MessageABCSTransactionResponse inqAccBRIInterface(String AccNo)
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

        try
        {

            briInterfaceService transaction = new briInterfaceService();
            transaction.initiateABCS(ref dSocketHeader, ref dMiddleWareHeader, ref dAbcsHeader, ref dAbcsMessage);

            dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;
            dAbcsMessage.Rows[0]["TRANSACTIONCODE"] = trancode;
            dAbcsMessage.Rows[0]["BUCKET1"] = AccNo;
            dAbcsMessage.Rows[0]["BUCKET7"] = "";
            dAbcsMessage.Rows[0]["BUCKET8"] = "";
            dAbcsMessage.Rows[0]["BUCKET19"] = DateTime.Now.ToString("ddMMyy");  //effective date
            dAbcsMessage.Rows[0]["BUCKET20"] = ""; //orig jseq for reverse
            dAbcsMessage.Rows[0]["DEFAULTCURRENCY"] = "IDR";
            dAbcsMessage.Rows[0]["TELLERREMARK1"] = "Inquiry Account";
            dAbcsMessage.Rows[0]["TELLERREMARK2"] = "CMS";
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY1"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY2"] = curr;
            dAbcsMessage.Rows[0]["ALTERNATECURRENCY3"] = "IDR";

            request.branchcode = "0374";
            request.tellerid = "0374891";
            request.supervisorid = "";
            request.origjournalseq = "";
            request.dSocketHeader = dSocketHeader;
            request.dMiddleWareHeader = dMiddleWareHeader;
            request.dAbcsHeader = dAbcsHeader;
            request.dAbcsMessage = dAbcsMessage;
            request.applicationname = "CMS";

            response = transaction.doABCSTransaction(request);
        }
        catch (Exception ex)
        {
            response = null;
        }

        return response;
    }


    public static String formatAccountNum(Object an)
    {
        try
        {
            String fa;
            fa = Convert.ToString(an);

            if (fa.Length < 15)
            {
                int l = 15 - fa.Length;

                for (int i = 0; i < l; i++)
                {
                    fa = "0" + fa;
                }
            }
            String[] acnum = new String[5];
            acnum[0] = fa.Substring(0, 4);
            acnum[1] = fa.Substring(4, 2);
            acnum[2] = fa.Substring(6, 6);
            acnum[3] = fa.Substring(12, 2);
            acnum[4] = fa.Substring(14, 1);

            fa = "";
            for (int i = 0; i < 4; i++)
            {
                fa += acnum[i] + "-";
            }
            return (fa + acnum[4]);
        }
        catch (Exception ex)
        {
            return String.Empty;
        }
    }

    public static Boolean Mbase8632(string filename, ILog log)
    {
        Boolean hasil = false;
        try
        {
            MessageMBASETransactionRequest request = new MessageMBASETransactionRequest();
            MessageMBASETransactionResponse response = new MessageMBASETransactionResponse();

            DataTable dSocketHeader = new DataTable("SOCKETHEADER");
            DataTable dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
            DataTable dMbaseHeader = new DataTable("MBASEHEADER");
            DataTable dMbaseMessage = new DataTable("MBASEMESSAGE");


            string trancode = "8632";
            briInterfaceService transaction = new briInterfaceService();
            transaction.initiateMBASE(trancode, ref dSocketHeader, ref dMiddleWareHeader, ref dMbaseHeader, ref dMbaseMessage);

            dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;

            dMbaseHeader.Rows[0]["TRANSACTIONCODE"] = trancode;
            dMbaseHeader.Rows[0]["ACTIONCODE"] = "I";
            dMbaseHeader.Rows[0]["NOOFRECORD"] = "1";
            // dMbaseHeader.Rows[0]["ACCOUNT"] = account.Number;
            //dMbaseHeader.Rows[0]["ACCOUNTTYPE"] = _AccountType(account.Number.ToString());
            dMbaseHeader.Rows[0]["CIF"] = "";

            dMbaseMessage.Rows[0]["IFILEID"] = filename;//ACCOUNTNUMBER //ACCTNO
            //dMbaseMessage.Rows[0]["ACCOUNTTYPE"] = _AccountType(account.Number.ToString());//ACCOUNTTYPE //ACTYPE

            request.applicationname = "CMS";
            request.branchcode = "0374";
            request.tellerid = "0374891";
            request.supervisorid = "";
            request.origjournalseq = "";
            request.dSocketHeader = dSocketHeader;
            request.dMiddleWareHeader = dMiddleWareHeader;
            request.dMbaseHeader = dMbaseHeader;
            request.dMbaseMessage = dMbaseMessage;

            response = transaction.doMBASETransaction(request);
            if (int.Parse(response.statuscode.ToString()) == 1)
                hasil = true;
            else
                log.Error("Status Mbase8632: " + response.statuscode.ToString());


        }
        catch (Exception ex)
        {
            log.Error(ex.Message + " >> " + ex.InnerException + " >> " + ex.StackTrace);
        }

        return hasil;
    }

    public static DataRow doInqBrivaIFT(String _nobriva, out String _remark)
    {
        DataSet dataBriva = null;
        DataTable tabBriva = null;
        DataRow rowBriva = null;

        BrivaService briva = new BrivaService();
        try
        {
            _remark = "[NULL]";
            dataBriva = briva.brivaInquiry(_nobriva.Trim().Substring(0, 5), _nobriva.Trim().Substring(5, _nobriva.Trim().Length - 5));
            tabBriva = dataBriva.Tables[0];

            if (tabBriva.Rows.Count > 0)
            {
                rowBriva = tabBriva.Rows[0];

                if (rowBriva[12].ToString().Equals("N") && rowBriva[13].ToString().Equals("Y") && rowBriva[14].ToString().Equals("Y"))
                {
                    _remark = "[PAID]";
                }
                else { _remark = "[OK]"; }
            }
        }
        catch (Exception ex)
        {
            _remark = "[ERROR]|" + ex.Message + " ##stackTrace:: " + ex.StackTrace;
        }

        return rowBriva;
    }



    public static DataRow doInqBriva(String _nobriva, out String _remark)
    {
        DataSet dataBriva = null;
        DataTable tabBriva = null;
        DataRow rowBriva = null;

        BrivaService briva = new BrivaService();
        try
        {
            _remark = "[NULL]";
            dataBriva = briva.brivaInquiry(_nobriva.Trim().Substring(0, 5), _nobriva.Trim().Substring(5, _nobriva.Length - 5));
            tabBriva = dataBriva.Tables[0];

            if (tabBriva.Rows.Count > 0)
            {
                rowBriva = tabBriva.Rows[0];

                if (rowBriva[12].ToString().Equals("N") && rowBriva[13].ToString().Equals("Y") && rowBriva[14].ToString().Equals("Y"))
                {
                    _remark = "[PAID]";
                }
                else { _remark = "[OK]"; }
            }
        }
        catch (Exception ex)
        {
            _remark = "[ERROR]";
        }

        return rowBriva;
    }

    public static Boolean doPayBriva(ISession session, ILog log, int trxId, String creditAccount, String debitAccount, int tellerid, String schCode, String _journalseq, out String _status, out String _description, out String _message)
    {
        Boolean result = true;

        String _nobriva;
        String _custcode;
        String _statusbayar;
        String _tellerid;
        double _amount;
        int lastInsertId = 0;
        String _norekdebit;

        _status = "";
        _description = "";
        _message = "";

        DataRow _inqBriva;
        String _inq;
        String _pay;
        String[] _paytemp;

        try
        {
            _nobriva = creditAccount;

            _inqBriva = doInqBriva(_nobriva, out _inq);

            if (_inq.Equals("[OK]"))
            {
                _nobriva = _inqBriva[0].ToString();
                _custcode = _inqBriva[1].ToString();
                _statusbayar = "Y";
                _tellerid = "0" + tellerid.ToString();
                _amount = double.Parse(_inqBriva[3].ToString());
                _norekdebit = debitAccount;

                BrivaService briva = new BrivaService();
                try
                {
                    result = briva.payTransactionCMS(_nobriva, _custcode, _statusbayar, _tellerid, _amount, _norekdebit, _journalseq, out lastInsertId, out _pay);

                    if (result)
                    {
                        if (_pay.Equals("[OK]"))
                        { _status = "[OK]"; }
                        else
                        {
                            // Get Info
                            _paytemp = _pay.Split(new String[] { "|" }, StringSplitOptions.None);
                            _status = _paytemp[0];
                            _description = _paytemp[1];
                            _message = _paytemp[2];
                        }
                    }
                    else
                    {

                        _paytemp = _pay.Split(new String[] { "|" }, StringSplitOptions.None);
                        _status = _paytemp[0];
                        _description = _paytemp[1];
                        _message = _paytemp[2];

                        if (_description.ToLower().Contains("gagal update")
                            || _description.ToLower().Contains("tak ada briva yang diproses"))
                        {
                            _status = "[OK]";
                            result = true;
                        }
                    }
                }
                catch (Exception ee)
                {
                    log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ee.Message);
                    log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ee.InnerException);
                    log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ee.StackTrace);
                    result = false;
                }
            }
            else if (_inq.Equals("[PAID]"))
            {
                result = true;
                _status = "[OK]";
            }
            else
            {
                result = false;
            }
        }
        catch (Exception ex)
        {
            log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ex.Message);
            log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ex.InnerException);
            log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ex.StackTrace);
            result = false;
        }

        return result;
    }

    public static Boolean doPayBrivaUpdate(ISession session, ILog log, int trxId, String creditAccount, String debitAccount, int tellerid, String schCode, String _journalseq, out String _status, out String _description, out String _message, double creditAmount)
    {
        Boolean result = true;

        String _nobriva;
        String _custcode;
        String _statusbayar;
        String _tellerid;

        int lastInsertId = 0;
        String _norekdebit;

        _status = "";
        _description = "";
        _message = "";

        DataRow _inqBriva;
        String _inq;
        String _pay;
        String[] _paytemp;

        try
        {
            _nobriva = creditAccount;

            _inqBriva = doInqBriva(_nobriva, out _inq);

            if (_inq.Equals("[OK]"))
            {
                _nobriva = _inqBriva[0].ToString();
                _custcode = _inqBriva[1].ToString();
                _statusbayar = "Y";
                _tellerid = "0" + tellerid.ToString();

                _norekdebit = debitAccount;

                BrivaService briva = new BrivaService();
                try
                {
                    result = briva.payTransactionCMS(_nobriva, _custcode, _statusbayar, _tellerid, creditAmount, _norekdebit, _journalseq, out lastInsertId, out _pay);

                    if (result)
                    {
                        if (_pay.Equals("[OK]"))
                        { _status = "[OK]"; }
                        else
                        {
                            // Get Info
                            _paytemp = _pay.Split(new String[] { "|" }, StringSplitOptions.None);
                            _status = _paytemp[0];
                            _description = _paytemp[1];
                            _message = _paytemp[2];
                        }
                    }
                    else
                    {

                        _paytemp = _pay.Split(new String[] { "|" }, StringSplitOptions.None);
                        _status = _paytemp[0];
                        _description = _paytemp[1];
                        _message = _paytemp[2];

                        if (_description.ToLower().Contains("gagal update")
                            || _description.ToLower().Contains("tak ada briva yang diproses"))
                        {
                            _status = "[OK]";
                            result = true;
                        }
                    }
                }
                catch (Exception ee)
                {
                    log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ee.Message);
                    log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ee.InnerException);
                    log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ee.StackTrace);
                    result = false;
                }
            }
            else if (_inq.Equals("[PAID]"))
            {
                result = true;
                _status = "[OK]";
            }
            else
            {
                result = false;
            }
        }
        catch (Exception ex)
        {
            log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ex.Message);
            log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ex.InnerException);
            log.Error(schCode + " (TrxID " + trxId + " ) Pay Transaction BRIVA For: " + ex.StackTrace);
            result = false;
        }

        return result;
    }

    //sayedzul 20160226
    //inquiry MBASE general purpose
    public static int InquiryMBASE(ISession session, String _debetAcc, out MessageMBASETransactionResponse response)
    {
        MessageMBASETransactionRequest request = new MessageMBASETransactionRequest();
        response = new MessageMBASETransactionResponse();
        int result = 0;
        try
        {
            DataTable dSocketHeader = new DataTable("SOCKETHEADER");
            DataTable dMiddleWareHeader = new DataTable("MIDDLEWAREHEADER");
            DataTable dMbaseHeader = new DataTable("MBASEHEADER");
            DataTable dMbaseMessage = new DataTable("MBASEMESSAGE");


            string trancode = "2618";
            briInterfaceService transaction = new briInterfaceService();
            try
            {
                transaction.initiateMBASE(trancode, ref dSocketHeader, ref dMiddleWareHeader, ref dMbaseHeader, ref dMbaseMessage);
            }
            catch
            {
                return 99;
            }
            dMiddleWareHeader.Rows[0]["TRANCODE"] = trancode;

            dMbaseHeader.Rows[0]["TRANSACTIONCODE"] = trancode;
            dMbaseHeader.Rows[0]["ACTIONCODE"] = "A"; // "A";
            dMbaseHeader.Rows[0]["NOOFRECORD"] = "1";

            dMbaseHeader.Rows[0]["ACCOUNT"] = _debetAcc;
            dMbaseHeader.Rows[0]["ACCOUNTTYPE"] = SchHelper._AccountType(_debetAcc); // "D";
            dMbaseHeader.Rows[0]["CIF"] = "";

            Parameter acctno = session.Load<Parameter>("BRIIN_ACCTNO");
            Parameter actype = session.Load<Parameter>("BRIIN_ACTYPE");
            dMbaseMessage.Rows[0][acctno.Data] = _debetAcc;
            dMbaseMessage.Rows[0][actype.Data] = SchHelper._AccountType(_debetAcc);
            request.applicationname = "CMS";
            request.branchcode = "0374";
            request.tellerid = "0374891";
            request.supervisorid = "";
            request.origjournalseq = "";
            request.dSocketHeader = dSocketHeader;
            request.dMiddleWareHeader = dMiddleWareHeader;
            request.dMbaseHeader = dMbaseHeader;
            request.dMbaseMessage = dMbaseMessage;

            response = transaction.doMBASETransaction(request);

            result = int.Parse(response.statuscode.Trim());
        }
        catch (Exception ex)
        {
            result = 0;
        }

        return result;
    }

    //handler acc status
    public static string _AccStatus(int statRek)
    {
        switch (statRek)
        {
            case 1:
                return ("Account Active");
            case 2:
                return ("Account Closed");
            case 3:
                return ("Account Maturity");
            case 4:
                return ("Account Active");
            case 5:
                return ("Account Do Not Close");
            case 6:
                return ("Account Restricted");
            case 7:
                return ("Account Blocked");
            case 8:
                return ("Account Undefined");
            case 9:
                return ("Account Dormant");
            default:
                return ("Account Unknown");
        }
    }

    //sayedzul 20160226 end

    

    
}
