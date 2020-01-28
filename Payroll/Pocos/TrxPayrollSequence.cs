using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxPayrollSequence : AbstractTransaction
    {
        private Int32 id;
        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        private String pid;
        [XmlAttribute("Pid")]
        public virtual String Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        private DateTime processDate;
        [XmlAttribute("ProcessDate")]
        public virtual DateTime ProcessDate
        {
            get { return processDate; }
            set { processDate = value; }
        }

        private Int32 sequence;
        [XmlAttribute("Sequence")]
        public virtual Int32 Sequence
        {
            get {return sequence;}
            set {sequence = value;}
        }

        
    }
        
}
