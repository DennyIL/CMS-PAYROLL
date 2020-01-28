using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("Email")]
	public class Email : AbstractTransaction
	{
		private Int32 _id;
		[XmlAttribute("Id")]
		public virtual Int32 Id
		{
			get { return _id; }
			set { _id = value; }
		}

        private int _transactionid;
        [XmlAttribute("TransactionID")]
        public virtual int TransactionID
        {
            get { return _transactionid; }
            set { _transactionid = value; }
        }

        private int _fid;
        [XmlAttribute("FID")]
        public virtual int FID
        {
            get { return _fid; }
            set { _fid = value; }
        }

        private string _fname;
        [XmlAttribute("FNAME")]
        public virtual string FNAME
        {
            get { return _fname; }
            set { _fname = value; }
        }

        private int _clientid;
        [XmlAttribute("ClientID")]
        public virtual int ClientID
        {
            get { return _clientid; }
            set { _clientid = value; }
        }
        
        private DateTime _createdtime;
		[XmlAttribute("CreatedTime")]
		public virtual DateTime CreatedTime
		{
			get { return _createdtime; }
			set { _createdtime = value; }
		}
		
		private DateTime _lastupdate;
		[XmlAttribute("LastUpdate")]
		public virtual DateTime LastUpdate
		{
			get { return _lastupdate; }
			set { _lastupdate = value; }
		}
		
		private String _receiver;
		[XmlAttribute("Receiver")]
		public virtual String Receiver
		{
			get { return _receiver; }
			set { _receiver = value; }
		}

        private String _subject;
        [XmlAttribute("Subject")]
        public virtual String Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        private String _content;
        [XmlAttribute("Content")]
        public virtual String Content
        {
            get { return _content; }
            set { _content = value; }
        }

        private Boolean _isbodyhtml;
        [XmlAttribute("IsBodyHtml")]
        public virtual Boolean IsBodyHtml
        {
            get { return _isbodyhtml; }
            set { _isbodyhtml = value; }
        }

		private Int32 _status;
		[XmlAttribute("Status")]
        public virtual Int32 Status
		{
			get { return _status; }
			set { _status = value; }
		}

		private String _errordescription;
		[XmlAttribute("ErrorDescription")]
        public virtual String ErrorDescription
		{
			get { return _errordescription; }
            set { _errordescription = value; }
		}
		
	}
}
