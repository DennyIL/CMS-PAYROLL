using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class UserAccount
    {
        private Int32 id;
        private Int32 uid;
        private Int32 fid;
        private Int32 aid;

        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("Uid")]
        public virtual Int32 Uid
        {
            get { return uid; }
            set { uid = value; }
        }

        [XmlAttribute("Fid")]
        public virtual Int32 Fid
        {
            get { return fid; }
            set { fid = value; }
        }

        [XmlAttribute("Aid")]
        public virtual Int32 Aid
        {
            get { return aid; }
            set { aid = value; }
        }

    }
}
