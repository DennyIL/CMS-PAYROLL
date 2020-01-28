using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("UserGroup")]
	public class UserGroup : AbstractTransaction
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

		private String _name;
		[XmlAttribute("Name")]
		public virtual String Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private int _status;
		[XmlAttribute("Status")]
		public virtual int Status
		{
			get { return _status; }
			set { _status = value; }
		}

	}
}
