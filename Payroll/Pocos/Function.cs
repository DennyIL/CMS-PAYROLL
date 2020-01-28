using System;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class Function : AbstractTransaction
    {
        private int id;
        private int parent;
        private String name;
        private String link;
        private String description;
        private int type;
        private int pref;
        private int flow;
        private bool allowed;

        [XmlAttribute("Id")]
        public virtual int Id {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("Parent")]
        public virtual int Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        [XmlAttribute("Name")]
        public virtual String Name
        {
            get { return name; }
            set { name = value; }
        }

        [XmlAttribute("Link")]
        public virtual String Link
        {
            get { return link; }
            set { link = value; }
        }

        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        [XmlAttribute("Type")]
        public virtual int Type
        {
            get { return type; }
            set { type = value; }
        }

        [XmlAttribute("Pref")]
        public virtual int Pref
        {
            get { return pref; }
            set { pref = value; }
        }

        [XmlAttribute("Flow")]
        public virtual int Flow
        {
            get { return flow; }
            set { flow = value; }
        }

        [XmlAttribute("Allowed")]
        public virtual bool Allowed
        {
            set { allowed = value; }
            get { return allowed; }
        }
    }
}
