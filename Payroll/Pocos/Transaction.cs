using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
    public class Transaction : AbstractTransaction
    {
        private int id;
        private String handle;
        private int transactionId;
        private DateTime createdTime;
        private DateTime verifiedTime;
        private DateTime approvedTime;
        private String checker;
        private String maker;
        private String approver;
        private int checkWork;
        private int checkTotal;
        private int approveWork;
        private int approveTotal;
        private String tobject;
        private int status;
        private int clientId;
        private String action;
        private String nextprocessor;
        private String rejectDescription;
        private String rejecter;
        private String processor;

		[XmlAttribute("Id")]
		public virtual int Id
        {
            get { return id; }
            set { id = value; }
        }

		[XmlAttribute("Handle")]
        public virtual String Handle
        {
            get { return handle; }
            set { handle = value; }
        }
		
		[XmlAttribute("TransactionId")]
        public virtual int TransactionId
        {
            get { return transactionId; }
            set { transactionId = value; }
        }
		
		[XmlAttribute("CreatedTime")]
        public virtual DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }
		
		[XmlAttribute("VerifiedTime")]
        public virtual DateTime VerifiedTime
        {
            get { return verifiedTime; }
            set { verifiedTime = value; }
        }

		[XmlAttribute("ApprovedTime")]
        public virtual DateTime ApprovedTime
        {
            get { return approvedTime; }
            set { approvedTime = value; }
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

		[XmlAttribute("CheckWork")]
        public virtual int CheckWork
        {
            get { return checkWork; }
            set { checkWork = value; }
        }

		[XmlAttribute("CheckTotal")]
        public virtual int CheckTotal
        {
            get { return checkTotal; }
            set { checkTotal = value; }
        }

		[XmlAttribute("ApproveWork")]
        public virtual int ApproveWork
        {
            get { return approveWork; }
            set { approveWork = value; }
        }

		[XmlAttribute("ApproveTotal")]
        public virtual int ApproveTotal
        {
            get { return approveTotal; }
            set { approveTotal = value; }
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

		[XmlAttribute("ClientId")]
        public virtual int ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }

		[XmlAttribute("Action")]
        public virtual String Action
        {
            get { return action; }
            set { action = value; }
        }

		[XmlAttribute("NextProcessor")]
        public virtual String NextProcessor
        {
            get { return nextprocessor; }
            set { nextprocessor = value; }
        }

        [XmlAttribute("RejectDescription")]
        public virtual String RejectDescription
        {
            get { return rejectDescription; }
            set { rejectDescription = value; }
        }

        [XmlAttribute("Processor")]
        public virtual String Processor
        {
            get { return processor; }
            set { processor = value; }
        }

    }
}
