/*
 * Author by Danar Widyantoro
 * 
 * change log:
 * 
 * Ardi Darmawan    03-06-2010  penambahan variable limit untuk fitur MOR pada line 112
 
 */

using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using BRIChannelSchedulerNew.Payroll.Pocos;
using NHibernate;

public abstract class ParameterHelper
{
	    /*for admin name*/
    public static String NAME_ADMIN_MAKER = "ADMIN";
    public static String NAME_ADMIN_VERIVIER = "SYSADMIN";


    public static String FORMAT_DATE = "dd/MM/yyyy";

    public static int UPLOADFILE_MAXSIZE = 1000000;
    public static String UPLOADFILE_DIRECTORY = "C:/Temp/";

    public static String SUCCESS = "[OK]";
    public static String FAILED = "[FAILED]";
    public static String ERROR = "[ERROR]";
    public static String UNKNOWN = "[UNKNOWN]";

    public static int TRXSTATUS_MAKER = 0;
    public static int TRXSTATUS_VERIFY = 1;
    public static int TRXSTATUS_APPROVE = 2;
    public static int TRXSTATUS_COMPLETE = 3;
    public static int TRXSTATUS_REJECT = 4;
    public static int TRXSTATUS_SUCCESS = 5;
    public static int TRXSTATUS_SUSPECT = 6;
    public static int TRXSTATUS_RUNNING_2 = 8;
    public static int TRXSTATUS_RUNNING = 9;
    public static int TRXSTATUS_VERIFY_PROCESS = 10;
    public static int TRXSTATUS_PROCESS = 11;
    public static int TRXSTATUS_FILE_PROCESS = 12;
    public static int TRXSTATUS_HOST_PROCESS = 13;
    public static int TRXSTATUS_WAITINGRESPONSE_PROCESS = 14;
    public static int TRXSTATUS_WAITINGRESPONSE_PROCESS_2 = 15;
    public static int TRXSTATUS_PENDING = 10;

    //Dikki :: MPN statushelper
    public static int TRXSTATUS_VERIFY_FINDNEXTPROCESSOR = 50;
    public static int TRXSTATUS_APPROVE_FINDNEXTPROCESSOR = 60;
    //END Dikki :: MPN statushelper

    /*purnomo punyas, status eventlicense*/
    public static int STATUS_WAITING_PROSESS = 53;
    public static int STATUS_INPROCESS_SCH = 54;
    public static int STATUS_EXP_SCH = 55;
    /*end purnomo punyas, status eventlicense*/

    /*Rofiq Creation, tabel transactions*/
    //add
    public static int TRXSTATUS_WAITING_SCHEDULLER = 15;
    public static int TRXSTATUS_INPROCESS_SCHEDULLER = 16;
    public static int TRXSTATUS_EXCEPTION_ONSCHEDULLER = 17;
    //rejected
    public static int TRXSTATUS_REJECTED_WAITING_SCHEDULLER = 25;
    public static int TRXSTATUS_REJECTED_INPROCESS_SCHEDULLER = 26;
    public static int TRXSTATUS_REJECTED_EXCEPTION_ONSCHEDULLER = 27;
    /*end Rofiq Creation, tabel transactions*/

    /*Denny Payroll - 12 Juni 2017*/
    public static int TRXSTATUS_GETFILE_DOWNLOAD = 0;
    public static int TRXSTATUS_GETFILE_UPLOAD = 1;
    public static int TRXSTATUS_GETFILE_CREATETRX = 2;
    public static int TRXSTATUS_GETFILE_DELETE = 3;

    /*Rofiq Creation, tabel Payment Priority*/
    public static int TRXPP_WAITING_CONVERT = 500;
    public static int TRXPP_PROCESS_CONVERT = 501;
    public static int TRXPP_PROCESS_CONVERT_EXCPTION = 502;
    public static int TRXPP_WAITING_FILE_CHECK = 20;

    /*end Rofiq Creation, tabel Payment Priority*/



    /*Rofiq Creation, tabel Payment Priority, refund ticket KAI*/
    public static int REFUND_KAI_WAITING = 1;
    public static int REFUND_KAI_PROCESS = 2;
    public static int REFUND_KAI_SUCCESS = 3;
    public static int REFUND_KAI_FAILED = 4;
    public static int REFUND_KAI_EXCEPTION = 5;
    public static int REFUND_KAI_NOT_PROCESS = 6;
    public static int REFUND_KAI_DONEUPDATE = 10;
    public static int REFUND_KAI_WAITING_SETTLE = 121;//tunggu debet rek KAI ke IA
    /*end Rofiq Creation, tabel Payment Priority*/

    public static int TRXSTATUS_WAITINGRESPONSE_FEE = 32;
    public static int TRXSTATUS_WAITINGRESPONSE_FEE_SUCCES = 33;
    
    public static string TRXSTATUS_SUCCES = "SUCCESS";
    public static string TRXSTATUS_FAILED = "FAILED";

    //PP pasca booking - Denny
    public static int TRX_BOOKED_SUCCESS = 3;
    public static int TRX_BOOKED_FAILED = 4;
    //DESCRIPTION
    public static string BOOKED_SUCCESS = "SUCCESS";
    public static string BOOKED_WAITING_FEE = "WAITING FEE";
    public static string BOOKED_FAILED = "FAILED";
    public static string BOOKED_FAILED_CUTOFFTIME = "ERROR - Transaksi Telah Melewati Cut Off Time";
    //End Denny 19 Maret 2016

    #region mass SWIFT
    public static int MSWIFT_WAITING_CHECK = 55;
    public static int MSWIFT_ONGOING_CHECK = 56;
    public static int MSWIFT_ERROR_ON_CHECK = 56;

    /*end Rofiq Creation, tabel transactions*/


    /*Rofiq Creation, tabel TRXSCV*/
    public static int TRXSCV_WAITING = 0;
    public static int TRXSCV_FILE_REJECT = 70;
    public static int TRXSCV_FILE_NOTVALID = 71;
    public static int TRXSCV_FILE_NOTVALID_LEVEL2 = 72;
    public static int TRXSCV_FILE_EXCEPTION = 73;
    public static int TRXSCV_FILE_VALID = 60;
    public static int TRXSCV_APPROVED = 63;
    public static int TRXSCV_INPROCESS = 64;
    public static int TRXSCV_SUCCESS = 80;
    /*end Rofiq Creation, tabel TRXSCV*/


    /*Rofiq Creation, tabel TRXSWIFT*/
    public static int TRXSWIFT_INVALID = 10;
    public static int TRXSWIFT_VALID = 11;
    public static int TRXSWIFT_APPROVED = 63;
    /*end Rofiq Creation, tabel TRXSWIFT*/
    #endregion



    /*Rofiq Creation, Booking Parameters*/
    //BOOK STATUS
    public static int SCH_WAITINGBOOK = 1;
    public static int SCH_BOOKPROCESS = 2;
    public static int SCH_BOOKSUCCESS = 3;
    public static int SCH_BOOKFAILED = 4;
    public static int SCH_BOOKCHECK = 5;

    //BOOK DESCRIPTION
    public static String BOOKINGDESC_WAITINGBOOKING = "WAITING BOOKING";
    public static String BOOKINGDESC_ONPROCESSBOOKING = "ON PROCESS BOOKING";
    public static String BOOKINGDESC_SUCCESSBOOKING = "SUCCESS";
    public static String BOOKINGDESC_FAILEDBOOKING = "ERROR";

    //BOOK TYPE
    public static int BOOKTYPE_AMOUNT = 1;
    public static int BOOKTYPE_FEE = 2;
    public static int BOOKTYPE_REVERSAL = 3;
    public static int BOOKTYPE_FEE_PENCADANGAN = 4;//khususon swift
    public static int BOOKTYPE_NOSTRO = 5;//nostrone swift
    public static int BOOKTYPE_BRIVAOUR = 6;//Briva our
    public static int BOOKTYPE_BRIVABEN = 7;//Briva ben
    public static int BOOKTYPE_IAKREDIT = 8; //khusus untuk debet IA Kredit dan Kredit ke IA Debet
    public static int BOOKTYPE_IADEBET = 9; //khusus untuk debet IA Debet dan Kredit ke IA Kredit


    //EC BOOK STATUS
    public static int SCH_ECWAITING = 1;
    public static int SCH_ECPROCESS = 2;
    public static int SCH_ECSUCCESS = 3;
    public static int SCH_ECFAILED = 4;


    /*end Rofiq Creation, tabel Booking*/

    /*Bhagas Creation*/
	public static int TRXSTATUS_RESPONSE_PROCESS = 15; //file response payroll sedang diproses
    public static int TRXSTATUS_INQUIRYNAMEPROCESS_START = 16; //proses check account payroll start
    public static int TRXSTATUS_INQUIRYNAMEPROCESS_FINISH = 17; //proses check account payroll selesai
    public static int TRXSTATUS_CHECKACCOUNTDONE_NEEDAPPROVE = 18; //after check account go for approver
    public static int TRXSTATUS_RUNNING_AFTERCHECKACCOUNT = 19;// running after check account
    public static int TRXSTATUS_PAYROLLTRX = 20;// satatus after send execute request
    public static int TRXSTATUS_PAYROLLTRX_READRESPONSE = 21;// satatus after get file output from host//rfq add, 20161202
    /*end Bhagas*/

    //sayed pur denny eksa - New Payroll
    
    /*Denny Payroll ALL bank*/
    public static int PAYROLL_STATUS_PARENT_FAILED = 4;
    public static string PAYROLL_DESC_PARENT_FAILED_DAILY = "Daily Transaction Over Limit";
    public static string PAYROLL_DESC_PARENT_FAILED_CORP = "Corporation Transactions Over Limit";
    public static string PAYROLL_DESC_PARENT_FAILED_USER = "User Transactions Over Limit";
    public static string PAYROLL_DESC_PARENT_FAILED_AUTH = "Checker / Approver invalid for this Transactions";
    public static string PAYROLL_DESC_PARENT_FAILED_FILE = "FIle Invalid";

    //status parent
    public static int TRXSTATUS_PAYROLLNEW_POST_BOOK_IADEBET = 20;
    public static int TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IADEBET = 21;
    public static int TRXSTATUS_PAYROLLNEW_READY_BOOK = 22;
    public static int TRXSTATUS_PAYROLLNEW_POST_BOOK_IAKREDIT = 23;
    public static int TRXSTATUS_PAYROLLNEW_CHECK_BOOK_IAKREDIT = 24;
    public static int TRXSTATUS_PAYROLLNEW_POST_BOOK_REK_NASABAH = 25;
    public static int TRXSTATUS_PAYROLLNEW_CHECK_BOOK_REK_NASABAH = 26;
    
    //statusIFT
    public static int TRXSTATUS_PAYROLLNEW_READY_BOOK_IFT = 1;
    public static int TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_GENERATENAME = 2;
    public static int TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_GENERATEFILE = 3;
    public static int TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_TRANSFERFILE = 5;
    public static int TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_MBASE = 6;
    public static int TRXSTATUS_PAYROLLNEW_BOOK_IFT_AFTER_FILEEXIST = 7;
    public static int TRXSTATUS_PAYROLLNEW_BOOK_IFT_COMPLETE = 9;

    //status book LLG/RTGS
    public static int TRXSTATUS_PAYROLLNEW_WAITINGRESPONSE_REMITNUMB = 14; //generate RemittanceNumber jika runtime siap
    public static int TRXSTATUS_PAYROLLNEW_POST_BOOK_RTGSANDLLG = 16; // POST ke booking u/ pembukuan
    public static int TRXSTATUS_PAYROLLNEW_CHECK_BOOK_RTGSANDLLG = 17; // CEK booking u/ pembukuan
    public static int TRXSTATUS_PAYROLLNEW_POST_MBASEANDWS_RTGSANDLLG = 32; //post ke mbase and ws
    public static int TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG = 33; //cek mbase and ws

    //status book BRIS Payroll - 2020-01-16
    public static int TRXSTATUS_PAYROLLNEW_AFTER_UPLOAD_BRIS = 1; //FIle Upload mengandung BRIS 
    public static int TRXSTATUS_PAYROLLNEW_WAITING_CREATEFILE_BRIS = 2; //Generate file BRIS
    public static int TRXSTATUS_PAYROLLNEW_WAITING_FTP_BRIS = 3; //Waitging file BRIS
    public static int TRXSTATUS_PAYROLLNEW_FTP_BRIS_ERROR = 4; //Gagal generate file
    public static int TRXSTATUS_PAYROLLNEW_WAITING_RESP_FTP_BRIS = 5; //WAITING GET LIST
    public static int TRXSTATUS_PAYROLLNEW_WAITING_DOWNLOAD_FTP_BRIS = 6; //WAITING DOWNLOAD file BRIS
    public static int TRXSTATUS_PAYROLLNEW_WAITING_MOVE_FTP_BRIS = 7; //WAITING MOVE file BRIS ke PROCESSED
    public static int TRXSTATUS_PAYROLLNEW_WAITING_DELETE_FTP_BRIS = 8; //WAITING DELETE file BRIS dr incoming
    public static int TRXSTATUS_PAYROLLNEW_FTP_BRIS_DONE = 9; //ACK/NACK file BRIS berhasil, tggal di parsing

    //status Book BRIS trxpayrolldetaiks - 2020-01-16
    public static int TRXSTATUS_PAYROLLDETAIL_AFTER_UPLOAD_BRIS = 1;
    public static int TRXSTATUS_PAYROLLDETAIL_ACK = 3;
    public static int TRXSTATUS_PAYROLLDETAIL_NACK = 4; 

    //PAYROLL EKSA
    public static int TRXPAYROLL_WAITING_BOOK_AMOUNT = 90;
    public static int TRXPAYROLL_BOOK_FEE_PROCESS = 91;
    public static int TRXPAYROLL_WAITING_BOOK_FEE = 92;
    public static int TRXPAYROLL_WAITING_BOOK_REVERSAL = 93;
    public static int TRXPAYROLL_WAITINGRESPONSE_SUCCES = 94;

    public static int TRXSTATUS_PAYROLL_WAITINGRESPONSE_REMITNUMB = 14;
    public static int TRXSTATUS_PAYROLL_PROCESS_CREATE_REMITNUMB = 15;
    public static string TRXDESCRIPTION_PAYROLL_WAITINGRESPONSE_PROCESS = "WAITING PROCESS BOOKING";
    public static int TRXSTATUS_PAYROLL_WAITINGRESPONSE_PROCESS = 16;
    public static int TRXSTATUS_PAYROLL_WAITINGRESPONSE_CHECK = 17;
    public static int TRXSTATUS_PAYROLL_POST_MBASEANDWS_RTGSANDLLG = 32;
    public static string TRXDESCRIPTION_PAYROLL_POST_MBASEANDWS_RTGSANDLLG = "PROCESS POST MBASE AND WS";
    public static string TRXDESCRIPTION_PAYROLL_SUCCESS = "TRANSACTION SUCCESS";
    public static string TRXDESCRIPTION_PAYROLL_CHECK_BOOK = "PROCESS CHECK BOOKING PAYROLL";
    //End PAYROLL EKSA

    public static int _needMBASE1 = 1;
    public static int _needMBASE2 = 2;
    public static int _needSentWS = 3;
    public static String SUCCESSpayroll = "Success";
    public static String FAILEDpayroll = "Failed";
    public static String RELOADpayroll = "Reload";


    /*Rofiq Creation, tabel Payroll*/
    public static int TRXPR_WAITING_CONVERT = 500;
    public static int TRXPR_PROCESS_CONVERT = 501;
    public static int TRXPR_PROCESS_CONVERT_EXCPTION = 502;
    public static int TRXPR_WAITING_FILE_CHECK = 20;
    /*end Rofiq Creation, tabel Payroll*/


    /*Rofiq Creation, payroll*/
    public static int PAYROLL_DECRYPT_PROCESS = 105;//Khususon ila musimas
    public static int PAYROLL_WAITINGSEQNUM = 100;//waiting seq number
    public static int PAYROLL_WAITING_CHECKACC = 11;
    public static int PAYROLL_SUCCESS_PARSING = 46;
    public static int PAYROLL_CHECKFILE = 516;

    public static int PAYROLLDETAILS_WAITING_CHECK = 55;//detail payroll

    public static int PAYROLL_EXCEPTION = 888;

    //email
    public static int PAYROLL_NEEDEMAIL = 1;
    public static int PAYROLL_PROCESSEMAIL = 2;
    public static int PAYROLL_DONEMAIL = 3;
    public static int PAYROLL_EXCEPTIONEMAIL = 888;

    //convert file if need
    public static int PAYROLL_WAITING_CONVERT = 500;
    public static int PAYROLL_PROCESS_CONVERT = 501;
    public static int PAYROLL_PROCESS_CONVERT_EXCPTION = 502;


    //generate file mass inq
    public static int PAYROLL_WAITING_GENERATE_MASS_INQ = 10;
    //public static int PAYROLL_WAITING_SEND_MASS_INQ = 10;
    // 11 tunggu balikan dari HOST
    /*end Rofiq*/
    /*end Rofiq*/


    /*novi start*/
    public static String ACCOUNT_TYPE = "ACCOUNT.TYPE";
    /*novi end*/

    public static int TRXSTATUS_MFT_VERIFY_RUNNING = 20;
    public static int TRXSTATUS_MFT_VERIFY_PROCESS = 21;

	
	/*Rofiq Creation, tabel trxJasaraharja*/
    public static int TRXJR_INPROCESS = 14;
    public static int TRXJR_EXCEPTION = 999;
    /*end Rofiq Creation, tabel transactions*/
	
    /*sayedzul start*/
    public static int TRXSTATUS_MRTGS_PENDING31 = 31; //exception di Pembukuan
    public static int TRXSTATUS_MRTGS_PENDING32 = 32; //resp null BOOK
    public static int TRXSTATUS_MRTGS_PENDING33 = 33; //timeout BOOK
    public static int TRXSTATUS_MRTGS_PENDING = 34;
    public static int TRXSTATUS_MRTGS_PENDING36 = 36;
    public static String TRXDESCRIPTION_MRTGS_PENDING = "PENDING";
    public static int TRXSTATUS_WAITINGRESPONSE_REMITNUMB = 25; //generate RemittanceNumber jika runtime siap
    public static int TRXSTATUS_MRTGS_APPROVE_RUNNING = 27; //file unggah belum diverifikasi oleh scheduler approver
    public static int TRXSTATUS_MRTGS_BOOK_PROCESS = 28; //data sedang proses JobMRTGSBook
    public static int TRXSTATUS_MRTGS_REG_PROCESS = 29; //data sedang proses JobMRTGSReg
    public static int TRXSTATUS_MRTGS_WAITING_REGISTER = 32;
    public static String TRXDESCRIPTION_MRTGS_VERIFY_RUNNING = "Finding Next Verifier";//"WAITING VERIFIED";
    public static String TRXDESCRIPTION_MRTGS_VERIFY = "Need Verify";//"WAITING VERIFIED";
    public static String TRXDESCRIPTION_MRTGS_APPROVE_RUNNING = "Finding Next Approver";//"WAITING APPROVED";
    public static String TRXDESCRIPTION_MRTGS_APPROVE = "Need Approve";//"WAITING APPROVED";
    public static String TRXDESCRIPTION_MRTGS_WAITING_RUN = "Waiting Schedule";
    public static String TRXDESCRIPTION_MRTGS_WAITINGRESPONSE_PROCESS = "Processing";
    public static String TRXDESCRIPTION_MRTGS_WAITING_REGISTER = "Registering";
    public static String TRXDESCRIPTION_MRTGS_REG_PROCESS = "Registering";
    public static String TRXDESCRIPTION_MRTGS_COMPLETE = "Transaction Complete";
    /*sayedzul end*/

    /*sayedzul start LLG*/
    public static int TRXSTATUS_MLLG_INQURY_DONE = 8; //selesai bulk insert
    public static int TRXSTATUS_MLLG_PENDING31 = 31; //exception di Pembukuan
    public static int TRXSTATUS_MLLG_PENDING32 = 32; //resp null BOOK
    public static int TRXSTATUS_MLLG_PENDING33 = 33; //timeout BOOK
    public static int TRXSTATUS_MLLG_PENDING34 = 34; //exception di MBASE1
    public static int TRXSTATUS_MLLG_PENDING35 = 35; //exception di MBASE2
    public static int TRXSTATUS_MLLG_PENDING36 = 36; //timeout MBASE 1
    public static int TRXSTATUS_MLLG_PENDING37 = 37; //timeout MBASE 2
    public static int TRXSTATUS_MLLG_PENDING38 = 38; //exception remmitance
    public static String TRXDESCRIPTION_MLLG_PENDING = "PENDING";
    public static int TRXSTATUS_MLLG_WAITINGREJECT = 20; //ready to execute for RejectHandler Job
    //public static int TRXSTATUS_WAITINGRESPONSE_REMITNUMB = 25; //generate RemittanceNumber jika runtime siap
    public static int TRXSTATUS_MLLG_APPROVE_RUNNING = 27; //file unggah belum diverifikasi oleh scheduler approver
    public static int TRXSTATUS_MLLG_BOOK_PROCESS = 28; //data sedang proses JobMLLGBook
    public static int TRXSTATUS_MLLG_REG1_PROCESS = 29; //data sedang proses JobMLLGReg
    public static int TRXSTATUS_MLLG_REG2_PROCESS = 30;
    public static int TRXSTATUS_MLLG_COM_PROCESS = 51; //sedang COMPLETE
    public static int TRXSTATUS_MLLG_WAITING_REGISTER1 = 41;
    public static int TRXSTATUS_MLLG_WAITING_REGISTER2 = 42;
    public static int TRXSTATUS_MLLG_WAITING_REGISTER3 = 43; //siap mlakukan COMPLETE
    public static String TRXDESCRIPTION_MLLG_VERIFY_RUNNING = "Need Verify";
    public static String TRXDESCRIPTION_MLLG_VERIFY = "Need Verify";
    public static String TRXDESCRIPTION_MLLG_APPROVE_RUNNING = "Need Approved";
    public static String TRXDESCRIPTION_MLLG_APPROVE = "Need Approve";
    public static String TRXDESCRIPTION_MLLG_WAITING_RUN = "Waiting Schedule";
    public static String TRXDESCRIPTION_MLLG_WAITING_PROC = "Waiting Process";
    public static String TRXDESCRIPTION_MLLG_WAITINGRESPONSE_PROCESS = "Processing";
    public static String TRXDESCRIPTION_MLLG_WAITING_REGISTER1 = "Registering1";
    public static String TRXDESCRIPTION_MLLG_WAITING_REGISTER2 = "Registering2";
    public static String TRXDESCRIPTION_MLLG_WAITING_COMPLETE = "Completing";
    public static String TRXDESCRIPTION_MLLG_REG_PROCESS = "Registering";
    public static String TRXDESCRIPTION_MLLG_COMPLETE = "Complete";
    public static String TRXDESCRIPTION_MLLG_CHECKACC = "Checking Account";
    /*sayedzul end*/

    /*sayedzul start MFT*/
    public static int TRXSTATUS_MFT_INQURY = 6;
    public static int TRXSTATUS_MFT_CREATEFILE = 7;
    public static int TRXSTATUS_MFT_INQURY_DONE = 8;
    public static int TRXSTATUS_MFT_RETRY = 11;
    public static String TRXDESCRIPTION_MFT_RETRY = "Waiting Retry";
    public static String TRXDESCRIPTION_MFT_CHECKACC = "Checking Account";
    public static int TRXSTATUS_MFT_ERRINQ = 30;
    public static int TRXSTATUS_MFT_FEE = 15; //Siap dibuku Fee
    public static int TRXSTATUS_MFT_PENDING31 = 31; //exception di Pembukuan
    public static int TRXSTATUS_MFT_PENDING32 = 32; //resp null BOOK
    public static int TRXSTATUS_MFT_PENDING33 = 33; //timeout BOOK
    public static int TRXSTATUS_MFT_PENDING34 = 34; //Gagal Mbuku Fee
    public static String TRXDESCRIPTION_MFT_PENDING = "PENDING";
    public static int TRXSTATUS_MFT_APPROVE_RUNNING = 27; //file unggah belum diverifikasi oleh scheduler approver
    public static int TRXSTATUS_MFT_BOOK_PROCESS = 28; //data sedang proses JobMFTBook
    public static int TRXSTATUS_MFT_BOOKFEE_PROCESS = 29; //data sedang proses JobMFTFee
    public static String TRXDESCRIPTION_MFT_VERIFY_RUNNING = "Need Verify";
    public static String TRXDESCRIPTION_MFT_VERIFY = "Need Verify";
    public static String TRXDESCRIPTION_MFT_APPROVE_RUNNING = "Need Approved";
    public static String TRXDESCRIPTION_MFT_APPROVE = "Need Approve";
    public static String TRXDESCRIPTION_MFT_WAITING_RUN = "Waiting Schedule";
    public static String TRXDESCRIPTION_MFT_WAITING_PROC = "Waiting Process";
    public static String TRXDESCRIPTION_MFT_WAITINGRESPONSE_PROCESS = "Processing";
    public static String TRXDESCRIPTION_MFT_COMPLETE = "Complete";
    public static String TRXDESCRIPTION_MFT_FEE = "Processing Fee";
    /*sayedzul end*/

    /*sayedzul start EReg CMS 17112014*/
    public const int EREG_retrive = 1; //data masuk dari eform (list FO) (soft) BO
    public const int EREG_checkedBO = 2; //data berada di list pending atasan FO (soft) / butuh approve pimpinan BO
    public const int EREG_approvedBO = 3; // data berada di list checker KP (soft) / butuh check KP
    public const int EREG_checkedKP = 4; // data berada di list Approver KP (soft) / butuh approv KP
    public const int EREG_approvedKP = 5; // data berada di list hard copy (hard) / list kelengkapan dokumen BO
    public const int EREG_hard_checkedBO = 6; //data berada di list pending atasan FO (hard) / butuh approved pimpinan BO
    public const int EREG_hard_checkedKP = 7; // data berada di list checker KP (hard) / butuh check KP
    public const int EREG_hard_checkedKP_pending = 71; // data berada di list PENDING checker KP (hard) / butuh check KP
    public const int EREG_hard_approvedKP = 8; //data berada di list approver KP (hard) / butuh approve KP
    public const int EREG_hard_approvedKP_pending = 81; //data berada di list PENDING approver KP (hard) / butuh approve KP
    public const int EREG_create_client = 9; //data berada di list client mgt KP
    public const int EREG_create_client_process = 31; //data berada di list client mgt KP
    public const int EREG_create_deliverytoBO = 10; //data berada di list Delivery EREG untuk proses pengiriman / create success KP
    public const int EREG_delivery_sudahdikirimkeBO = 11; //dokument suda diterima (berada di list delivered EREG) BO
    public const int EREG_delivery_kirimnasabah = 12; //dokument sudah dikirim ke nasabah BO
    public const int EREG_delivery_kirimdocumenkeCust = 13; //dokument sudah dikirim kembali ke KP BO
    public const int EREG_delivery_sudahditerimadiKP = 14; //dokument sudah diterima di KP
    public const int EREG_active_pendingapprove = 32; //aktivasi client pending approval
    public const int EREG_active = 15; //Aktif client KP
    public const int EREG_reject = 0; //Reject

    public const int EREGSTAT_CREATE = 1; //Reject
    public const int EREGSTAT_RUNNING = 2; //Reject
    public const int EREGSTAT_COMPLETE = 3; //Reject

    public const String EREG_DESC_retrive = "Waiting Check SoftCopy";//1 
    public const String EREG_DESC_checkedBO = "Waiting Approve Pimpinan"; //2
    public const String EREG_DESC_approvedBO = "Waiting Check KP "; //3
    public const String EREG_DESC_checkedKP = "Waiting Approve KP"; //4
    public const String EREG_DESC_approvedKP = "Waiting Check HardCopy"; //5
    public const String EREG_DESC_hard_checkedBO = "Waiting Approve HardCopy"; //6
    public const String EREG_DESC_hard_checkedKP = "Waiting Check HardCopy KP"; //7
    public const String EREG_DESC_hard_approvedKP = "Waiting Approve HardCopy KP"; //8
    public const String EREG_DESC_create_client = "Waiting Create Client";//9
    public const String EREG_DESC_create_deliverytoBO = "Create Client Success";//10
    public const String EREG_DESC_delivery_sudahdikirimkeBO = "Delivered to Booking Office";//11
    public const String EREG_DESC_delivery_kirimnasabah = "Accepted Documents";//12
    public const String EREG_DESC_delivery_kirimdocumenkeCust = "Delivered Documents to Customer";//13
    public const String EREG_DESC_delivery_sudahditerimadiKP = "Activated to KP";//14
    public const String EREG_DESC_active = "Active";
    public const String EREG_DESC_reject = "Reject";

    /*sayedzul end*/

    /*Topan start MRTO*/
    public static int TRXSTATUS_WAITINGRESPONSE_RTO = 25; //generate RemittanceNumber jika runtime siap
    public static int TRXSTATUS_MRTO_APPROVE_RUNNING = 27; //file unggah belum diverifikasi oleh scheduler approver
    public static int TRXSTATUS_MRTO_WAITING_TRANSFER = 29; //data sedang proses sch Transfer
    public static String TRXDESCRIPTION_MRTO_VERIFY = "Need Verify";
    public static String TRXDESCRIPTION_MRTO_APPROVE = "Need Approve";
    public static String TRXDESCRIPTION_MRTO_WAITING_PROC = "Waiting Process";
    public static String TRXDESCRIPTION_MRTO_APPROVE_RUNNING = "Need Approved";
    public static String TRXDESCRIPTION_MRTO_WAITING_RUN = "Waiting Schedule";
    public static String TRXDESCRIPTION_MRTO_COMPLETE = "Complete";
    /*Topan end*/

    /*sayedzul Start Pooling*/
    public static int TRXSTATUS_POOLING_CREATE = 0;
    public static int TRXSTATUS_POOLING_ACTIVE = 1;
    public static int TRXSTATUS_POOLING_RUNNING = 2;
    public static int TRXSTATUS_POOLING_PROCESS_BOOK = 21;
    public static int TRXSTATUS_POOLING_PROCESS_RETRY_BOOK = 31;
    public static int TRXSTATUS_POOLING_PENDING_EX = 32; // exception
    public static int TRXSTATUS_POOLING_PENDING_EX3 = 33; // pending gegara createTrx tdak sempurna tapi ada trx terproses, investigasi manual 
    public static int TRXSTATUS_POOLING_PENDING_LIMITRETRY = 40;
    public static int TRXSTATUS_POOLING_PENDING_RETRY = 41; //Butuh Investigasi Manual cek rekening sudah terdebet/belum, jurnalseq/tellerid tidak ada
    public const String TRXDESC_POOLING_PENDING_RETRY = "PENDING";
    /*sayedzul end*/

    /*sayedzul start NotionalPooling*/
    public static int TRXSTATUS_NOTIONALPOOLINGREPORT_WAITINGPROCESS = 1;
    public static int TRXSTATUS_NOTIONALPOOLINGREPORT_COMPLETE = 3;
    /*sayedzul end */

    public static int TRXSTATUS_INQUIRY_PROCESS = 30;
    public static int TRXSTATUS_INQUIRY_WAITINGRESPONSE = 31; 
    public static int LOANACCOUNT_ACTIVE = 1; //SUDAH DICAIRKAN
    public static int LOANACCOUNT_CLOSED = 2; //SUDAH DILUNASIN
    public static int LOANACCOUNT_MATURED = 3; //SUDAH JATUH TEMPO
    public static int LOANACCOUNT_NEW = 4; //BARU
    public static int LOANACCOUNT_NONACCRUAL = 5; //BUNGA TIDAK DIHITUNG
    public static int LOANACCOUNT_FROZEN = 7; // DIBEKUKAN
    public static int LOANACCOUNT_PH = 8; //HAPUS BUKU
    

    public static String TRXDESCRIPTION_WAITING = "WAITING";
    public static String TRXDESCRIPTION_SUCCESS = "SUCCESS";
    public static String TRXDESCRIPTION_REJECT = "REJECT";
    public static String TRXDESCRIPTION_UNKNOWN = "UNKNOWN";
    public static String TRXDESCRIPTION_COMPLETE = "COMPLETE"; 
    public static String TRXDESCRIPTION_SUCCEED = "Transaction Succeed";
    public static String TRXDESCRIPTION_FAILED = "FAILED";

    public static int RECORD_NOTACTIVE = 0;
    public static int RECORD_ACTIVE = 1;
    public static int RECORD_DELETED = 2;
    public static int RECORD_INCOMPLETE = 96;

    public static int ACCOUNT_TRANSACTIONAL = 1;
    public static int ACCOUNT_INQUIRY = 2;
    public static int ACCOUNT_CREDITONLY = 3;
	public static int ACCOUNT_MASSDEBET = 4; //untuk fitur mass debet by Dikki

    public static int SERVICE_DELAY = 1800000;

    public static int TABLE_ROW_MAX = 5;

    /* Variable session parameter - key */
    public static String TABLE_LIST_INDEX = "LIST.INDEX";

    /* Variable to handle session_key */
    public static String PROCESS_ID = "PROCESS.ID";
    public static String PROCESS_ID_USER = "PROCESS.ID.USER";
    public static String PROCESS_ID_BACK = "PROCESS.ID.BACK";
    public static String SESSION_TXCODE = "PRODUCT.ID";
    public static String SESSION_FICODE = "PRODUCT.ID.PARENT";
    public static String SESSION_MATRIX = "MENU.AUTHORITY";
    public static String SESSION_DATEFROM = "DATE.FROM";
    public static String SESSION_DATEEND = "DATE.END";
    public static String SESSION_WORKFLOW = "MENU.WORKFLOW";
    public static String SESSION_TABLE_MAXROW = "TABLE.MAXROW";
    public static String SESSION_I8N = "I8N.LANGUAGE";
    public static String SESSION_OBJECT_TEMP = "WORKING.OBJECT";
    public static String SESSION_TRACE = "LOGGER.TRACE";
    public static String SESSION_SUPER = "ADMIN.SUPER";

    /*Begin SCV Parameter*/
    public static String APPLICATION_ID = "00009999";

    public static int TRXSTATUS_SCV_WAITING_BRANCH = 60;
    public static int TRXSTATUS_SCV_MAKE_BRANCH = 61;
    public static int TRXSTATUS_SCV_VERIFIED_BRANCH = 62;
    public static int TRXSTATUS_SCV_APPROVED_BRANCH = 63;
    public static int TRXSTATUS_SWIFT_WT8728 = 64;
    public static int TRXSTATUS_SCV_WAITING_HO = 65;
    public static int TRXSTATUS_SCV_MAKE_HO = 66;
    public static int TRXSTATUS_SCV_VERIFIED_HO = 67;
    public static int TRXSTATUS_SCV_APPROVED_HO = 68;
    public static int TRXSTATUS_SWIFT_WTSENDMT = 69;
    public static int TRXSTATUS_SCV_CANCELED = 70;
    public static int TRXSTATUS_SWIFT_SUSPECT = 72;
    public static int TRXSTATUS_SWIFT_PENDING = 73;
    public static int TRXSTATUS_SWIFT_PENDING_HO = 74;
    public static int TRXSTATUS_SWIFT_WAITINGACK = 75;
    public static int TRXSTATUS_SCV_ACK = 80;
    public static int TRXSTATUS_SCV_NACK = 81;
    public static int TRXSTATUS_SCV_WAITING_TO_REJECT = 85;
    public static int TRXSTATUS_SCV_WAITING_TO_BR_REJECT = 86;
    public static int TRXSTATUS_SCV_HO_REJECT = 87;
    public static int TRXSTATUS_SCV_BRANCH_REJECT = 88;
    public static int TRXSTATUS_SCV_MT202 = 202;
    
	/* eQ SSPCP */
    public static int TRXSTATUS_TAX_READYCOPY = 100;
    public static int TRXSTATUS_TAX = 101;
    public static int TRXSTATUS_TAX_INQUIRY_PROCESS = 102;
    public static int TRXREC_INQUIRY = 103;
    /* end eQ SSPCP */

    /* eQ Pajak DKI 12 Februari 2013 */
    public static int TRXSTATUS_READY_INQUIRY = 7;
    public static int TRXSTATUS_READY_PAYMENT = 8;
    /* end eQ Pajak DKI 12 Februari 2013 */

	/*19 juli 2012 Sari, untuk status dr shred folder pajak*/
    //public static int TRXSTATUS_TAX_CETAK_ACK_NACK = 103;
    /*end sari*/

	/*20072012, sari flag di table ceknpwp*/
    public static int FLAG_CEK_NPWP = 1;
    public static int FLAG_FINISH_CEK_NPWP = 2;
    public static int FLAG_COMPLETE_CEK_NPWP = 3;//sudah dibuat file
    /*end sari*/
	/*Status untuk Etax PLN*/
    public static int ETAX_PLN_READY = 100;
    public static int ETAX_PLN_ON_PROCESS = 101;
    public static int ETAX_PLN_SUCCESS_DETAIL_FILE = 102;
    public static int ETAX_PLN_SUCCESS_WRITE_FILE = 103;
    public static int ETAX_PLN_ERROR = 4;
    /*End Status eTax PLN*/
    public static String CLUIN_ID = "CLUIN.ID";

    public static String PARENT_PROCESS_ID = "PARENT.PROCESS.ID";
    public static String TRXDESCRIPTION_VERIFY = "VERIFIED";
    public static String TRXDESCRIPTION_NEED_VERIFY = "Need Verify";
    public static String TRXDESCRIPTION_APPROVE = "APPROVED";
    public static String TRXDESCRIPTION_NEED_APPROVE = "Need Approve";
    public static String TRXDESCRIPTION_OPEN = "OPEN";
    public static String SPOPEN = "SP.OPEN";
    public static String SPTICKET = "SP.TICKET";
    public static String SPLEVELID = "SP.LEVELID";
    public static String SPTRANLIMIT = "SP.TRANLIMIT";
    public static String SPPASSWORD = "SP.PASSWORD";
    /*MOR CORPORATE parameter*/
    public static String FILENAME = "DATA.FILENAME";
    public static String BATCHNAME = "DATA.BATCHNAME";
    public static String DATAUPLOAD = "DATA.UPLOAD";
    public static String PASSKEY = "DATA.PASSKEY";
    public static String INVALID_TAG = "Invalid Tag ";
    public static String VALID_TAG = "[VALID]";

    /*End of SCV Parameter*/

    /*Account Maintenance Parameter*/
    public static char FLAG_INTERNATIONAL = '1';
    public static char FLAG_BRI = '2';
    public static char FLAG_RTO = '3';
    public static char FLAG_RTGS = '4';


    /*Status for SPANfile*/
    public static int SPAN_READY = 100;
    public static int SPAN_ON_PROCESS = 101;
    public static int SPAN_NEED_VERIFY = 1;
    public static int SPAN_NEED_APPROVE = 2;
    public static int SPAN_READY_INSERT = 102;
    public static int SPAN_ON_PROCESS_READ = 103;
    public static int SPAN_ON_PROCESS_INSERT = 104;
    public static int SPAN_READY_FTP = 105;
    public static int SPAN_SUCCESS = 3;
    public static int SPAN_REJECT = 4;

    public static int SPANTRN_NEED_VERIFY = 1;
    public static int SPANTRN_NEED_APPROVE = 2;
    public static int SPANTRN_READY = 100;
    public static int SPANTRN_ON_PROCESS = 101;
    public static int SPANTRN_READY_WRITE = 102;
    public static int SPANTRN_SUCCESS = 3;
    public static int SPANTRN_REJECT = 4;
    /*end SPAN*/

    /*Status PP - wira */
    public static String TRXDESCRIPTION_PP_WAITINGRESPONSE_PROCESS = "WAITING PROCESS";
    public static int TRXSTATUS_PP_WAITINGRESPONSE_REMITNUMB = 14; //generate RemittanceNumber jika runtime siap
    public static int TRXSTATUS_PP_PROCESS_CREATE_REMITNUMB = 15; //status antara
    public static int TRXSTATUS_PP_WAITINGRESPONSE_PROCESS = 16; // siap untuk pembukuan

    //add sayed
    public static int TRXPP_WAITING_BOOK_AMOUNT = 90;
    public static int TRXPP_BOOK_FEE_PROCESS = 91;
    public static int TRXPP_WAITING_BOOK_FEE = 92;
    public static int TRXPP_WAITING_BOOK_REVERSAL = 93;
    public static int TRXPP_WAITINGRESPONSE_SUCCES = 94;

    public static int TRXSTATUS_PP_WAITINGREGISTER = 32; // siap untuk MBASE1
    public static int TRXSTATUS_PP_REGISTERING = 33; // SEDANG MBASE1
    public static int TRXSTATUS_PP_WAITINGSENDING = 34; // siap untuk MBASE2
    public static int TRXSTATUS_PP_SENDING = 35; // SEDANG MBASE2
    /*Status PP - Al */
    public static Int16 TRXSTATUS_PP_WAITINGSENDING_SERVICESKNNG2 = 50; // data siap untuk dikirim ke webservice SKNNG2
    public static Int16 TRXSTATUS_PP_SENDING_SERVICESKNNG2 = 51; // data sedang dikirim ke webservice SKNNG2
    public static Int16 TRXSTATUS_PP_PENDINGSENDING_SERVICESKNNG2 = 53; // pending ketika kirim ke webservice SKNNG2
    

    //add sayed
    public static int TRXSTATUS_PP_COMPLETE_UPLOAD = 1;//PP Parent siap bulk insert
    public static string TRXDESCRIPTION_PP_COMPLETE_UPLOAD = "SUCCESS";//PP Parent siap bulk insert
    public static int TRXSTATUS_PP_UPLOAD_READY = 20;//PP Parent siap bulk insert
    public static int TRXSTATUS_PP_INQUIRY = 6;//PP Parent/Child Complete bulk Insert, Ready to inquiry
    public static string TRXDESCRIPTION_PP_INQUIRY = "Inquiry Process";
    public static int TRXSTATUS_PP_AUTH = 18;//PP Child Auth Process
    public static string TRXDESCRIPTION_PP_AUTH = "Finding Next Processor";//PP Child Auth Process
    public static int TRXSTATUS_PP_CREATEFILE = 7;//PP Parent/Child Complete bulk Insert, Ready to inquiry
    public static string TRXDESCRIPTION_PP_CREATEFILE = "Inquiry Process";
    public static int TRXSTATUS_PP_INQURY_DONE = 8;
    public static int TRXSTATUS_PP_BRIVAINQURY_PROCESS = 9;
    public static string TRXDESCRIPTION_PP_BRIVAINQURY_PROCESS = "Inquiry Process";
    

    /*Status Single RTGS & LLG - wira */
    public static String TRXSTATUS_SINGLERTGS_WAITING_SENDING_X = "WAITING SENDING X"; //Eksekusi via WS BRINET Express, no mbase
    public static string TRXSTATUS_SINGLERTGS_WAITING_SENDING = "WAITING SENDING"; //Eksekusi via WS SPAN, tetep MBASE sebelumnya
    public static String TRXSTATUS_SINGLERTGS_WAITINGRESPONSE_PROCESS = "WAITING PROCESS";
    public static String TRXSTATUS_SINGLERTGS_WAITING_REGISTER = "WAITING REGISTER";
    public static String TRXSTATUS_SINGLERTGS_WAITINGRESPONSE_REMITNUMB = "WAITING CREATE RN";
    public static String TRXSTATUS_SINGLERTGS_PROCESS_CREATE_REMITNUMB = "PROCESSING CREATE RN";
    public static String TRXSTATUS_SINGLELLG_WAITINGRESPONSE_PROCESS = "WAITING PROCESS";
    public static String TRXSTATUS_SINGLELLG_WAITING_REGISTER = "WAITING REGISTER";
    public static String TRXSTATUS_SINGLELLG_WAITINGRESPONSE_REMITNUMB = "WAITING CREATE RN";
    public static String TRXSTATUS_SINGLELLG_PROCESS_CREATE_REMITNUMB = "PROCESSING CREATE RN";
    public static string TRXSTATUS_SINGLELLG_WAITING_SENDING = "WAITING SENDING";
    public static string TRXSTATUS_SINGLELLG_WAITING_COMPLETE = "WAITING COMPLETE";

    /* Harry - Payment Status (TrxDoPayment) */
    public static int TRXPAY_MIGRATE = 1;
    public static int TRXPAY_BOOK = 2;
    public static int TRXPAY_FLAG = 3;
    public static int TRXPAY_ERROR = 4;
    public static int TRXPAY_SUCCESS = 5;
    public static int TRXPAY_PROCESSING = 10; // + Status Process
    public static int PAY_ORDER = 1;
    public static int PAY_NOORDER = 0;

    
	/*Dikki start MASSDEBET*/
    public static int TRXSTATUS_MASSDEBET_FEE = 15; //Siap dibuku Fee
    public static int TRXSTATUS_MASSDEBET_PENDING31 = 31; //exception di Pembukuan
    public static int TRXSTATUS_MASSDEBET_PENDING32 = 32; //resp null BOOK
    public static int TRXSTATUS_MASSDEBET_PENDING33 = 33; //timeout BOOK
    public static String TRXDESCRIPTION_MASSDEBET_PENDING = "PENDING";
    public static int TRXSTATUS_MASSDEBET_APPROVE_RUNNING = 27; //file unggah belum diverifikasi oleh scheduler approver
    public static int TRXSTATUS_MASSDEBET_BOOK_PROCESS = 28; //data sedang proses JobMFTBook
    public static int TRXSTATUS_MASSDEBET_BOOKFEE_PROCESS = 29; //data sedang proses JobMFTFee
    public static String TRXDESCRIPTION_MASSDEBET_VERIFY_RUNNING = "Need Verify";
    public static String TRXDESCRIPTION_MASSDEBET_VERIFY = "Need Verify";
    public static String TRXDESCRIPTION_MASSDEBET_APPROVE_RUNNING = "Need Approved";
    public static String TRXDESCRIPTION_MASSDEBET_APPROVE = "Need Approve";
    public static String TRXDESCRIPTION_MASSDEBET_WAITING_RUN = "Waiting Schedule";
    public static String TRXDESCRIPTION_MASSDEBET_WAITING_PROC = "Waiting Process";
    public static String TRXDESCRIPTION_MASSDEBET_WAITINGRESPONSE_PROCESS = "Processing";
    public static String TRXDESCRIPTION_MASSDEBET_COMPLETE = "Complete";
    /*Dikki end*/
    //Dikki :: MASSDEBET statushelper
    public static int TRXSTATUS_MASSDEBETDETAIL_ACTIVE = 100;
    public static int TRXSTATUS_MASSDEBETDETAIL_FINDNEXTSCHEDULEDDATE = 101;
    public static int TRXSTATUS_MASSDEBETDETAIL_PASIVE = 103;
    public static int TRXSTATUS_MASSDEBETDETAIL_REJECT = 104;
    public static String TRXSTATUS_MASSDEBETDETAIL_ACTIVE_DESC = "ACTIVE";
    public static String TRXSTATUS_MASSDEBETDETAIL_FINDNEXTSCHEDULEDDATE_DESC = "Preparing for booking process & finding next booking date";
    public static String TRXSTATUS_MASSDEBETDETAIL_PASIVE_DESC = "PASIVE";
    public static String TRXSTATUS_MASSDEBETDETAIL_REJECT_DESC = "REJECT";
    public static String TRXSTATUS_MASSDEBETBOOKING_REJECTRUNNINGEXCEED_DESC = "Running day melebihi grace period (Runnning day exceeded grace period)";
    public static int TRXSTATUS_MASSDEBET_FINDNEXTPROCESSDATE = 81;
    public static int TRXSTATUS_MASSDEBET_EDITED = 71;

    public static int TRXSTATUS_MASSDEBETBOOKING_REJECT = 4;
    public static String TRXSTATUS_MASSDEBETBOOKING_REJECT_RETRY = "ERROR - Reject Retry";
    public static int TRXSTATUS_MASSDEBETBOOKING_SUCCESS = 3;
    public static int TRXSTATUS_MASSDEBETBOOKING_WAITING = 14;
    public static int TRXSTATUS_MASSDEBETBOOKING_ANTARA = 5;
    //END Dikki :: MASSDEBET statushelper


    //EMAIL (1 FOR ALL)
    public static int EMAIL_WAITING_SEND = 1;
    public static int EMAIL_PROCESSING = 2;
    public static int EMAIL_SUCCESS = 3;
    public static int EMAIL_EXCEPTION = 4;
    /*end Rofiq*/

    /*Rofiq Creation, tabel schmanager*/
    public static int SCH_NEEDRESTART = 1;
    public static int SCH_ON = 3;
    public static int SCH_NEEDSHUTDOWN = 4;
    public static int SCH_OFF = 5;
    /*end Rofiq Creation, tabel schmanager*/
	
	/* Multi Payment Status Terms */
    public static int MP_TERMS_ACTIVE = 1;
    public static int MP_TERMS_NONACTIVE = 0;

    /* Multi Payment Status Clients */
    public static int MP_CLIENTS_ACTIVE = 1;
    public static int MP_CLIENTS_ACTIVE_AUTH = 11;
    public static int MP_CLIENTS_NONACTIVE = 0;
    public static int MP_CLIENTS_NONACTIVE_AUTH = 10;
    public static int MP_CLIENTS_ACTIVE_TEMP = 91;
    public static int MP_CLIENTS_NONACTIVE_TEMP = 90;

    /* Multi Payment Status Users */
    public static int MP_USERS_ACTIVE = 1;
    public static int MP_USERS_ACTIVE_AUTH = 11;
    public static int MP_USERS_NONACTIVE = 0;
    public static int MP_USERS_NONACTIVE_AUTH = 10;
    public static int MP_USERS_ACTIVE_TEMP = 91;
    public static int MP_USERS_NONACTIVE_TEMP = 90;


    /*Mettaw, MBASE Parameters*/
    #region New MBASE Express
    //MBASE STATUS
    public static int SCH_MBASE_WAITING = 1;
    public static int SCH_MBASE_PROCESS = 2;
    public static int SCH_MBASE_SUCCESS = 3;
    public static int SCH_MBASE_FAILED = 4;
    public static int SCH_MBASE_CHECK = 5;
    //MBASE DESCRIPTION
    public static String MBASEDESC_WAITINGMBASE = "WAITING MBASE";
    public static String MBASEDESC_ONPROCESSMBASE = "ON PROCESS MBASE";
    public static String MBASEDESC_SUCCESSMBASE = "SUCCESS MBASE";
    public static String MBASEDESC_FAILEDMBASE = "REJECTED";

    //MBASE STATUS ANTARA
    public static int SCH_MBASE_ANTARA_POST = 310;
    public static int SCH_MBASE_ANTARA_SEND = 320;
    public static int SCH_MBASE_ANTARA_CHECK = 330;
    //MBASE DESCRIPTION ANTARA
    public static String SCH_MBASE_ANTARA_POST_DESC = "PROCESS POST";
    public static String SCH_MBASE_ANTARA_SEND_DESC = "PROCESS SEND";
    public static String SCH_MBASE_ANTARA_CHECK_DESC = "PROCESS CHECK";
    /*end, MBASE Parameters*/
    #endregion

    /* Add by calvinzy 20200116 */
    public static String INSTCODE_BRIS = "BRIS";

    public static String GetString(String key)
    {
        String result = "";
        ISession session = NHibernateHelper.GetCurrentSession();
        BRIChannelSchedulerNew.Payroll.Pocos.Parameter parameter = null;
        try
        {
            parameter = session.Load(typeof(BRIChannelSchedulerNew.Payroll.Pocos.Parameter), key) as BRIChannelSchedulerNew.Payroll.Pocos.Parameter;
            result = parameter.Data.Trim();
        }
        catch (HibernateException he)
        {
        }
        catch (Exception e)
        {
        }
        return result;
    }

    public static Int32 GetInteger(String key)
    {
        String result = "0";
        ISession session = NHibernateHelper.GetCurrentSession();
        BRIChannelSchedulerNew.Payroll.Pocos.Parameter parameter = null;
        try
        {
            parameter = session.Load(typeof(BRIChannelSchedulerNew.Payroll.Pocos.Parameter), key) as BRIChannelSchedulerNew.Payroll.Pocos.Parameter;
            result = parameter.Data.Trim();
        }
        catch (HibernateException he)
        {
        }
        catch (Exception e)
        {
        }
        return Int32.Parse(result);
    }

    public static String GetString(String key, ISession session)
    {
        String result = "";
        BRIChannelSchedulerNew.Payroll.Pocos.Parameter parameter = null;
        try
        {
            parameter = session.Load(typeof(BRIChannelSchedulerNew.Payroll.Pocos.Parameter), key) as BRIChannelSchedulerNew.Payroll.Pocos.Parameter;
            result = parameter.Data.Trim();
        }
        catch (HibernateException he)
        {
        }
        catch (Exception e)
        {
        }
        return result;
    }

    public static Int32 GetInteger(String key, ISession session)
    {
        String result = "0";
        BRIChannelSchedulerNew.Payroll.Pocos.Parameter parameter = null;
        try
        {
            parameter = session.Load(typeof(BRIChannelSchedulerNew.Payroll.Pocos.Parameter), key) as BRIChannelSchedulerNew.Payroll.Pocos.Parameter;
            result = parameter.Data.Trim();
        }
        catch (HibernateException he)
        {
        }
        catch (Exception e)
        {
        }
        return Int32.Parse(result);
    }

}
