using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("UserGroupMaps")]
	public class UserGroupMaps : AbstractTransaction
	{

		private int _id;
		[XmlAttribute("Id")]
		public virtual int Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private int _cid;
		[XmlAttribute("Cid")]
		public virtual int Cid
		{
			get { return _cid; }
			set { _cid = value; }
		}

		private int _uid;
		[XmlAttribute("Uid")]
		public virtual int Uid
		{
			get { return _uid; }
			set { _uid = value; }
		}

        private String _handle;
        [XmlAttribute("Handle")]
        public virtual String Handle
        {
            get { return _handle; }
            set { _handle = value; }
        }

		private int _gid;
		[XmlAttribute("Gid")]
		public virtual int Gid
		{
			get { return _gid; }
			set { _gid = value; }
		}

        private int _sequ;
        [XmlAttribute("SU")]
        public virtual int SU
        {
            get { return _sequ; }
            set { _sequ = value; }
        }
	}
}
