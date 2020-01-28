using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("UserGroupWorkflow")]
	public class UserGroupWorkflow : AbstractTransaction
	{

		private int _id;
		[XmlAttribute("Id")]
		public virtual int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private int _fid;
		[XmlAttribute("Fid")]
		public virtual int Fid
		{
			get { return _fid; }
			set { _fid = value; }
		}

        private int _cid;
        [XmlAttribute("Cid")]
        public virtual int Cid
        {
            get { return _cid; }
            set { _cid = value; }
        }

		private int _gid;
		[XmlAttribute("Gid")]
		public virtual int Gid
		{
			get { return _gid; }
			set { _gid = value; }
		}

		private int _seqv;
		[XmlAttribute("Sv")]
		public virtual int Sv
		{
			get { return _seqv; }
			set { _seqv = value; }
		}

		private int _totv;
		[XmlAttribute("Tv")]
		public virtual int Tv
		{
			get { return _totv; }
			set { _totv = value; }
		}

		private int _seqa;
		[XmlAttribute("Sa")]
		public virtual int Sa
		{
			get { return _seqa; }
			set { _seqa = value; }
		}

		private int _tota;
		[XmlAttribute("Ta")]
		public virtual int Ta
		{
			get { return _tota; }
			set { _tota = value; }
		}

	}
}
