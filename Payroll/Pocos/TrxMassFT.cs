using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxMassFT : AbstractTransaction
    {
        private int id;
        [XmlAttribute("Id")]
        public virtual int Id
        {
            get { return id; }
            set { id = value; }
        }

        private DateTime createdTime;
        [XmlAttribute("CreatedTime")]
        public virtual DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }

        private DateTime lastUpdate;
        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        private String fileName;
        [XmlAttribute("FileName")]
        public virtual String FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private String fileDescription;
        [XmlAttribute("FileDescription")]
        public virtual String FileDescription
        {
            get { return fileDescription; }
            set { fileDescription = value; }
        }

        private int status;
        [XmlAttribute("Status")]
        public virtual int Status
        {
            get { return status; }
            set { status = value; }
        }

        private String approver;
        [XmlAttribute("Approver")]
        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        private int approverTotal;
        [XmlAttribute("ApproverTotal")]
        public virtual int ApproverTotal
        {
            get { return approverTotal; }
            set { approverTotal = value; }
        }

        private String rejecter;
        [XmlAttribute("Rejecter")]
        public virtual String Rejecter
        {
            get { return rejecter; }
            set { rejecter = value; }
        }

        private String description;
        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        private String passKey;
        [XmlAttribute("PassKey")]
        public virtual String PassKey
        {
            get { return passKey; }
            set { passKey = value; }
        }

        private int pid;
        [XmlAttribute("Pid")]
        public virtual int Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        private String maker;
        [XmlAttribute("Maker")]
        public virtual String Maker
        {
            get { return maker; }
            set { maker = value; }
        }

        private String checker;
        [XmlAttribute("Checker")]
        public virtual String Checker
        {
            get { return checker; }
            set { checker = value; }
        }

        private int checkerTotal;
        [XmlAttribute("CheckerTotal")]
        public virtual int CheckerTotal
        {
            get { return checkerTotal; }
            set { checkerTotal = value; }
        }

        private Double cp;
        [XmlAttribute("CP")]
        public virtual Double CP
        {
            get { return cp; }
            set { cp = value; }
        }

        private Double ct;
        [XmlAttribute("CT")]
        public virtual Double CT
        {
            get { return ct; }
            set { ct = value; }
        }

        private Double tp;
        [XmlAttribute("TP")]
        public virtual Double TP
        {
            get { return tp; }
            set { tp = value; }
        }

        private Double tt;
        [XmlAttribute("TT")]
        public virtual Double TT
        {
            get { return tt; }
            set { tt = value; }
        }

        private int fid;
        [XmlAttribute("Fid")]
        public virtual int Fid
        {
            get { return fid; }
            set { fid = value; }
        }

        private int client;
        [XmlAttribute("Client")]
        public virtual int Client
        {
            get { return client; }
            set { client = value; }
        }

        private String tobject;
        [XmlAttribute("TObject")]
        public virtual String TObject
        {
            get { return tobject; }
            set { tobject = value; }
        }

        private DateTime runningTime;
        [XmlAttribute("RunningTime")]
        public virtual DateTime RunningTime
        {
            get { return runningTime; }
            set { runningTime = value; }
        }

        private String alamatFtp;
        [XmlAttribute("AlamatFtp")]
        public virtual String AlamatFtp
        {
            get { return alamatFtp; }
            set { alamatFtp = value; }
        }

        private String trxAmt;
        [XmlAttribute("TrxAmt")]
        public virtual String TrxAmt
        {
            get { return trxAmt; }
            set { trxAmt = value; }
        }

        private String trxCnt;
        [XmlAttribute("TrxCnt")]
        public virtual String TrxCnt
        {
            get { return trxCnt; }
            set { trxCnt = value; }
        }

        private String trxCurr;
        [XmlAttribute("TrxCurr")]
        public virtual String TrxCurr
        {
            get { return trxCurr; }
            set { trxCurr = value; }
        }

        private String schedule;
        [XmlAttribute("Schedule")]
        public virtual String Schedule
        {
            get { return schedule; }
            set { schedule = value; }
        }

        private int uselessMark;
        [XmlAttribute("UselessMark")]
        public virtual int UselessMark
        {
            get { return uselessMark; }
            set { uselessMark = value; }
        }

        private String processor;
        [XmlAttribute("Processor")]
        public virtual String Processor
        {
            get { return processor; }
            set { processor = value; }
        }

        private String fileNameInq;
        [XmlAttribute("FileNameInq")]
        public virtual String FileNameInq
        {
            get { return fileNameInq; }
            set { fileNameInq = value; }
        }

        private int isBrivaBatch;
        [XmlAttribute("IsBrivaBatch")]
        public virtual int IsBrivaBatch
        {
            get { return isBrivaBatch; }
            set { isBrivaBatch = value; }
        }
    }
}
