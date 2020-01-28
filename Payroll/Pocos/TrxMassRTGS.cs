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
    [XmlRoot("TrxMassRTGS")]
    public class TrxMassRTGS : AbstractTransaction
    {
        private int id;
        private int pid;
        private DateTime createdTime;
        private DateTime lastUpdate;
        private DateTime runningTime;
        private String fileName;
        private String fileDescription;
        private String description;
        private int status;
        private String maker;
        private String checker;
        private int checkerTotal;
        private String approver;
        private int approverTotal;
        private String tobject;
        private String passKey;
        private Double cp;
        private Double ct;
        private Double tp;
        private Double tt;
        private int fid;
        private int client;

        [XmlAttribute("Id")]
        public virtual int Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("Pid")]
        public virtual int Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        [XmlAttribute("Fid")]
        public virtual int Fid
        {
            get { return fid; }
            set { fid = value; }
        }

        [XmlAttribute("Client")]
        public virtual int Client
        {
            get { return client; }
            set { client = value; }
        }

        [XmlAttribute("CreatedTime")]
        public virtual DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }

        [XmlAttribute("RunningTime")]
        public virtual DateTime RunningTime
        {
            get { return runningTime; }
            set { runningTime = value; }
        }

        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        [XmlAttribute("Approver")]
        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        [XmlAttribute("ApproverTotal")]
        public virtual int ApproverTotal
        {
            get { return approverTotal; }
            set { approverTotal = value; }
        }

        [XmlAttribute("FileName")]
        public virtual String FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        [XmlAttribute("FileDescription")]
        public virtual String FileDescription
        {
            get { return fileDescription; }
            set { fileDescription = value; }
        }

        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        [XmlAttribute("PassKey")]
        public virtual String PassKey
        {
            get { return passKey; }
            set { passKey = value; }
        }

        [XmlAttribute("TObject")]
        public virtual String TObject
        {
            get { return tobject; }
            set { tobject = value; }
        }

        [XmlAttribute("Status")]
        public virtual int Status
        {
            get { return status; }
            set { status = value; }
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

        [XmlAttribute("CheckerTotal")]
        public virtual int CheckerTotal
        {
            get { return checkerTotal; }
            set { checkerTotal = value; }
        }

        [XmlAttribute("CP")]
        public virtual Double CP
        {
            get { return cp; }
            set { cp = value; }
        }

        [XmlAttribute("CT")]
        public virtual Double CT
        {
            get { return ct; }
            set { ct = value; }
        }

        [XmlAttribute("TP")]
        public virtual Double TP
        {
            get { return tp; }
            set { tp = value; }
        }

        [XmlAttribute("TT")]
        public virtual Double TT
        {
            get { return tt; }
            set { tt = value; }
        }
    }
}
