using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class ClientWorkflow : AbstractTransaction
    {
        private int id;
        private DateTime createdTime;
        private DateTime lastUpdate;
        private String approver;
        private String workflow;

        [XmlAttribute("Id")]
        public virtual int Id
        {
            get { return id; }
            set { id = value; }
        }

        public virtual DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }

        [XmlAttribute("LUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        [XmlAttribute("Approver")]
        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        public virtual String Workflow
        {
            get { return workflow; }
            set { workflow = value; }
        }
    }
}
