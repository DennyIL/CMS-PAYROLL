using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("SchManager")]
    public class SchManager : AbstractTransaction
	{
		private Int32 _id;
		[XmlAttribute("Id")]
		public virtual Int32 Id
		{
			get { return _id; }
			set { _id = value; }
		}

        private string _schcode;
        [XmlAttribute("SchCode")]
        public virtual string SchCode
        {
            get { return _schcode; }
            set { _schcode = value; }
        }

        private string _schdescription;
        [XmlAttribute("SchDescription")]
        public virtual string SchDescription
        {
            get { return _schdescription; }
            set { _schdescription = value; }
        }

        private string _fid;
        [XmlAttribute("FID")]
        public virtual string FID
        {
            get { return _fid; }
            set { _fid = value; }
        }

        private string _path;
        [XmlAttribute("Path")]
        public virtual string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private string _filename;
        [XmlAttribute("FileName")]
        public virtual string FileName
        {
            get { return _filename; }
            set { _filename = value; }
        }

        private string _serverid;
        [XmlAttribute("ServerId")]
        public virtual string ServerId
        {
            get { return _serverid; }
            set { _serverid = value; }
        }

        private string _starttime;
        [XmlAttribute("StartTime")]
        public virtual string StartTime
        {
            get { return _starttime; }
            set { _starttime = value; }
        }

        private string _endtime;
        [XmlAttribute("EndTime")]
        public virtual string EndTime
        {
            get { return _endtime; }
            set { _endtime = value; }
        }

        private int _runevery;
        [XmlAttribute("RunEvery")]
        public virtual int RunEvery
        {
            get { return _runevery; }
            set { _runevery = value; }
        }

        private DateTime _lastrun;
        [XmlAttribute("SchLastRun")]
        public virtual DateTime SchLastRun
        {
            get { return _lastrun; }
            set { _lastrun = value; }
        }

        private string _message;
        [XmlAttribute("Message")]
        public virtual string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        private int _restartstatus;
        [XmlAttribute("RestartStatus")]
        public virtual int RestartStatus
        {
            get { return _restartstatus; }
            set { _restartstatus = value; }
        }

        private bool _autorestart;
        [XmlAttribute("AutoRestart")]
        public virtual bool AutoRestart
        {
            get { return _autorestart; }
            set { _autorestart = value; }
        }

        private bool _uselog;
        [XmlAttribute("UseLog")]
        public virtual bool UseLog
        {
            get { return _uselog; }
            set { _uselog = value; }
        }
		
		private DateTime _loglastupdate;
        [XmlAttribute("SchLogLastUpdate")]
        public virtual DateTime SchLogLastUpdate
		{
            get { return _loglastupdate; }
            set { _loglastupdate = value; }
		}

        private DateTime _lastupdate;
        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return _lastupdate; }
            set { _lastupdate = value; }
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
