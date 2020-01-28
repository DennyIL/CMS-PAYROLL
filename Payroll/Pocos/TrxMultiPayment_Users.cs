using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("TrxMultiPayment_Users")]
	public class TrxMultiPayment_Users : AbstractTransaction
	{

		private int _id;
		[XmlAttribute("Id")]
		public virtual int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private int _fitur_id;
		[XmlAttribute("Fitur_Id")]
		public virtual int Fitur_Id
		{
			get { return _fitur_id; }
			set { _fitur_id = value; }
		}

        private DateTime _createddate;
        [XmlAttribute("CreatedDate")]
        public virtual DateTime CreatedDate
        {
            get { return _createddate; }
            set { _createddate = value; }
        }

        private DateTime _lastupdate;
        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return _lastupdate; }
            set { _lastupdate = value; }
        }

		private int _client_id;
		[XmlAttribute("Client_Id")]
		public virtual int Client_Id
		{
			get { return _client_id; }
			set { _client_id = value; }
		}

		private int _user_id;
		[XmlAttribute("User_Id")]
		public virtual int User_Id
		{
			get { return _user_id; }
			set { _user_id = value; }
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

		private String _info1;
		[XmlAttribute("Info1")]
		public virtual String Info1
		{
			get { return _info1; }
			set { _info1 = value; }
		}

		private String _info2;
		[XmlAttribute("Info2")]
		public virtual String Info2
		{
			get { return _info2; }
			set { _info2 = value; }
		}

		private String _info3;
		[XmlAttribute("Info3")]
		public virtual String Info3
		{
			get { return _info3; }
			set { _info3 = value; }
		}

		private int _status;
		[XmlAttribute("Status")]
		public virtual int Status
		{
			get { return _status; }
			set { _status = value; }
		}

		private String _description;
		[XmlAttribute("Description")]
		public virtual String Description
		{
			get { return _description; }
			set { _description = value; }
		}

	}
}
