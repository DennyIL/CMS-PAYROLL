using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    public class ClientAccount : Account
    {
        private Int32 pid;
        private String approver;
        private DateTime createdTime;
        private DateTime lastUpdate;
        private String cif;
        private String name;

        [XmlAttribute("Pid")]
        public virtual Int32 Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        public virtual DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }

        public virtual DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }
        [XmlAttribute("Cif")]
        public virtual String Cif
        {
            get { return cif; }
            set { cif = value; }
        }

        [XmlAttribute("Name")]
        public virtual String Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
