using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("HistoryNCL")]
	public class HistoryNCL : AbstractTransaction
	{

		private int _id;
		[XmlAttribute("Id")]
		public virtual int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private int _clientid;
		[XmlAttribute("ClientId")]
		public virtual int ClientId
		{
			get { return _clientid; }
			set { _clientid = value; }
		}

		private String _applicantname;
		[XmlAttribute("ApplicantName")]
		public virtual String ApplicantName
		{
			get { return _applicantname; }
			set { _applicantname = value; }
		}

        private String _beneficiaryname;
        [XmlAttribute("BeneficiaryName")]
        public virtual String BeneficiaryName
        {
            get { return _beneficiaryname; }
            set { _beneficiaryname = value; }
        }

		private String _description;
		[XmlAttribute("Description")]
		public virtual String Description
		{
			get { return _description; }
			set { _description = value; }
		}

		private String _type;
		[XmlAttribute("Type")]
		public virtual String Type
		{
			get { return _type; }
			set { _type = value; }
		}

		private DateTime _createdtime;
		[XmlAttribute("CreatedTime")]
		public virtual DateTime CreatedTime
		{
			get { return _createdtime; }
			set { _createdtime = value; }
		}

		private String _currency;
		[XmlAttribute("Currency")]
		public virtual String Currency
		{
			get { return _currency; }
			set { _currency = value; }
		}

		private double _amount;
		[XmlAttribute("Amount")]
		public virtual double Amount
		{
			get { return _amount; }
			set { _amount = value; }
		}

		private String _debetkredit;
		[XmlAttribute("DebetKredit")]
		public virtual String DebetKredit
		{
			get { return _debetkredit; }
			set { _debetkredit = value; }
		}

		private int _status;
		[XmlAttribute("Status")]
		public virtual int Status
		{
			get { return _status; }
			set { _status = value; }
		}

        private double _convamount;
        [XmlAttribute("ConvAmount")]
        public virtual double ConvAmount
        {
            get { return _convamount; }
            set { _convamount = value; }
        }

        private double _kurs;
        [XmlAttribute("Kurs")]
        public virtual double Kurs
        {
            get { return _kurs; }
            set { _kurs = value; }
        }

        private DateTime _issuedate;
        [XmlAttribute("IssueDate")]
        public virtual DateTime IssueDate
        {
            get { return _issuedate; }
            set { _issuedate = value; }
        }

        private DateTime _expirydate;
        [XmlAttribute("ExpiryDate")]
        public virtual DateTime ExpiryDate
        {
            get { return _expirydate; }
            set { _expirydate = value; }
        }
	}
}
