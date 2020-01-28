using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("DefaultFee")]
	public class DefaultFee : AbstractTransaction
	{

		private int _fid;
		[XmlAttribute("Fid")]
		public virtual int Fid
		{
			get { return _fid; }
			set { _fid = value; }
		}

		private float _fee;
		[XmlAttribute("Fee")]
		public virtual float Fee
		{
			get { return _fee; }
			set { _fee = value; }
		}

		private DateTime _lastupdate;
		[XmlAttribute("Lastupdate")]
		public virtual DateTime Lastupdate
		{
			get { return _lastupdate; }
			set { _lastupdate = value; }
		}

		private int _status;
		[XmlAttribute("Status")]
		public virtual int Status
		{
			get { return _status; }
			set { _status = value; }
		}

		private float _fee2;
		[XmlAttribute("Fee2")]
		public virtual float Fee2
		{
			get { return _fee2; }
			set { _fee2 = value; }
		}

		private float _fee3;
		[XmlAttribute("Fee3")]
		public virtual float Fee3
		{
			get { return _fee3; }
			set { _fee3 = value; }
		}

		private float _fee4;
		[XmlAttribute("Fee4")]
		public virtual float Fee4
		{
			get { return _fee4; }
			set { _fee4 = value; }
		}

		private float _fee5;
		[XmlAttribute("Fee5")]
		public virtual float Fee5
		{
			get { return _fee5; }
			set { _fee5 = value; }
		}

        private float _fee7;
        [XmlAttribute("Fee7")]
        public virtual float Fee7
        {
            get { return _fee7; }
            set { _fee7 = value; }
        }

	}
}
