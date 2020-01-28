using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [XmlRoot("Payroll")]
    [Serializable]
    public class TrxPayroll : AbstractTransaction
    {
        private string id;
        private int seqnumber;
        private DateTime createdTime;
        private DateTime lastUpdate;
        private DateTime processTime;
        private String createdBy;
        private String maker;
        private String checker;
        private String approver;
        private String rejecter;
        private String originalfilepath;
        private int fileformatcode;
        private String fileName;
        private String fileDescription;
        private String debitaccount;
        private String description;
        private int status;        
        private String filePath;
        private int clientID;
        private String totaltrx;
        private float fee;
        private int statusemail;
        private String passKey;
        private String errordescription;
        private String sharedfolderstatus;
        private String filemassinq;

            
        private IList<TrxPayrollDetail> trxPayrollDetail;

        [XmlAttribute("Id")]
        public virtual string Id { 
            get { return id; }
            set { id = value; } 
        }

        [XmlAttribute("SeqNumber")]
        public virtual int SeqNumber
        {
            get { return seqnumber; }
            set { seqnumber = value; }
        }

        [XmlAttribute("CTime")]
        public virtual DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }

        [XmlAttribute("PTime")]
        public virtual DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        [XmlAttribute("Uid")]
        public virtual String CreatedBy
        {
            get { return createdBy; }
            set { createdBy = value; }
        }

        [XmlAttribute("Maker")]
        public virtual String Maker
        {
            get { return maker; }
            set { maker = value; }
        }

        [XmlAttribute("Checker")]
        public virtual String Checker
        {
            get { return checker; }
            set { checker = value; }
        }

        [XmlAttribute("Approver")]
        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        [XmlAttribute("Rejecter")]
        public virtual String Rejecter
        {
            get { return rejecter; }
            set { rejecter = value; }
        }

        [XmlAttribute("FName")]
        public virtual String FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        [XmlAttribute("OriginalFilePath")]
        public virtual String OriginalFilePath
        {
            get { return originalfilepath; }
            set { originalfilepath = value; }
        }

        [XmlAttribute("FileFormatCode")]
        public virtual int FileFormatCode
        {
            get { return fileformatcode; }
            set { fileformatcode = value; }
        }

        [XmlAttribute("FDesc")]
        public virtual String FileDescription
        {
            get { return fileDescription; }
            set { fileDescription = value; }
        }

        [XmlAttribute("DebitAccount")]
        public virtual String DebitAccount
        {
            get { return debitaccount; }
            set { debitaccount = value; }
        }

        [XmlAttribute("Desc")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        [XmlAttribute("Stat")]
        public virtual int Status
        {
            get { return status; }
            set { status = value; }
        }

        [XmlAttribute("ProcessTime")]
        public virtual DateTime ProcessTime
        {
            get { return processTime; }
            set { processTime = value; }
        }

        [XmlAttribute("FilePath")]
        public virtual String FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        [XmlAttribute("ClientID")]
        public virtual int ClientID
        {
            get { return clientID; }
            set { clientID = value; }
        }

        [XmlAttribute("PassKey")]
        public virtual String PassKey
        {
            get { return passKey; }
            set { passKey = value; }
        }

        [XmlAttribute("TotalTrx")]
        public virtual String TotalTrx
        {
            get { return totaltrx; }
            set { totaltrx = value; }
        }

        [XmlAttribute("Fee")]
        public virtual float Fee
        {
            get { return fee; }
            set { fee = value; }
        }

        [XmlAttribute("StatusEmail")]
        public virtual int StatusEmail
        {
            get { return statusemail; }
            set { statusemail = value; }
        }

        [XmlAttribute("ErrorDescription")]
        public virtual String ErrorDescription
        {
            get { return errordescription; }
            set { errordescription = value; }
        }

        [XmlAttribute("SharedFolderStatus")]
        public virtual String SharedFolderStatus
        {
            get { return sharedfolderstatus; }
            set { sharedfolderstatus = value; }
        }

        [XmlAttribute("FileMassInq")]
        public virtual String FileMassInq
        {
            get { return filemassinq; }
            set { filemassinq = value; }
        }

        private int isPayrollBankLain;
        [XmlAttribute("IsPayrollBankLain")]
        public virtual int IsPayrollBankLain
        {
            get { return isPayrollBankLain; }
            set { isPayrollBankLain = value; }
        }

        private double feeRTG;
        [XmlAttribute("FeeRTG")]
        public virtual double FeeRTG
        {
            get { return feeRTG; }
            set { feeRTG = value; }
        }

        private double feeLLG;
        [XmlAttribute("FeeLLG")]
        public virtual double FeeLLG
        {
            get { return feeLLG; }
            set { feeLLG = value; }
        }

        private double feeBRIS;
        [XmlAttribute("FeeBRIS")]
        public virtual double FeeBRIS
        {
            get { return feeBRIS; }
            set { feeBRIS = value; }
        }

        private int statusBRIS;
        [XmlAttribute("StatusBRIS")]
        public virtual int StatusBRIS
        {
            get { return statusBRIS; }
            set { statusBRIS = value; }
        }

        private double amountFromIADebet;
        [XmlAttribute("AmountFromIADebet")]
        public virtual double AmountFromIADebet
        {
            get { return amountFromIADebet; }
            set { amountFromIADebet = value; }
        }

        private double amountUsed;
        [XmlAttribute("AmountUsed")]
        public virtual double AmountUsed
        {
            get { return amountUsed; }
            set { amountUsed = value; }
        }

        private int idBooking1;
        [XmlAttribute("IdBooking1")]
        public virtual int IdBooking1
        {
            get { return idBooking1; }
            set { idBooking1 = value; }
        }

        private int idBooking2;
        [XmlAttribute("IdBooking2")]
        public virtual int IdBooking2
        {
            get { return idBooking2; }
            set { idBooking2 = value; }
        }

        private int idBooking3;
        [XmlAttribute("IdBooking3")]
        public virtual int IdBooking3
        {
            get { return idBooking3; }
            set { idBooking3 = value; }
        }

        private int idBooking4;
        [XmlAttribute("IdBooking4")]
        public virtual int IdBooking4
        {
            get { return idBooking4; }
            set { idBooking4 = value; }
        }

        private int statusIFT;
        [XmlAttribute("StatusIFT")]
        public virtual int StatusIFT
        {
            get { return statusIFT; }
            set { statusIFT = value; }
        }

        private string jSeqHold;
        [XmlAttribute("JSeqHold")]
        public virtual string JSeqHold
        {
            get { return jSeqHold; }
            set { jSeqHold = value; }
        }

        private int retryPosition;
        [XmlAttribute("RetryPosition")]
        public virtual int RetryPosition
        {
            get { return retryPosition; }
            set { retryPosition = value; }
        }

        //------------------------------------------------
        private String fileNameBris;
        [XmlAttribute("FileNameBris")]
        public virtual String FileNameBris
        {
            get { return fileNameBris; }
            set { fileNameBris = value; }
        }
        
        private String fileNameBrisReq;
        [XmlAttribute("FileNameBrisReq")]
        public virtual String FileNameBrisReq
        {
            get { return fileNameBrisReq; }
            set { fileNameBrisReq = value; }
        }

        private String fileNameBrisResp;
        [XmlAttribute("FileNameBrisResp")]
        public virtual String FileNameBrisResp
        {
            get { return fileNameBrisResp; }
            set { fileNameBrisResp = value; }
        }

        private String sentFtpBrisTime;
        [XmlAttribute("SentFtpBrisTime")]
        public virtual String SentFtpBrisTime
        {
            get { return sentFtpBrisTime; }
            set { sentFtpBrisTime = value; }
        }

        private String logFtpBris;
        [XmlAttribute("LogFtpBris")]
        public virtual String LogFtpBris
        {
            get { return logFtpBris; }
            set { logFtpBris = value; }
        }

        //-------------------------------------------------------------------

        [XmlIgnoreAttribute]
        public virtual IList<TrxPayrollDetail> TrxPayrollDetail 
        {
            get { return trxPayrollDetail; }
            set { trxPayrollDetail=value; } 
        }

        public virtual void AddDetail(TrxPayrollDetail detail)
        {
            if (null == trxPayrollDetail) trxPayrollDetail = new List<TrxPayrollDetail>();
            detail.Parent = this;
            trxPayrollDetail.Add(detail);
        }
    }
}
