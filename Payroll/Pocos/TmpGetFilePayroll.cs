using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TmpGetFilePayroll
    {
        [XmlAttribute("Id")]
        private int id;
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("ClientId")]
        private int clientId;
        public virtual int ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }

        [XmlAttribute("ClientName")]
        private string clientName;
        public virtual string ClientName
        {
            get { return clientName; }
            set { clientName = value; }
        }

        [XmlAttribute("Fid")]
        private int fId;
        public virtual int Fid
        {
            get { return fId; }
            set { fId = value; }
        }

        [XmlAttribute("FName")]
        private string fName;
        public virtual string FName
        {
            get { return fName; }
            set { fName = value; }
        }

        [XmlAttribute("Alamat")]
        private string alamat;
        public virtual string Alamat
        {
            get { return alamat; }
            set { alamat = value; }
        }

        [XmlAttribute("User")]
        private string user;
        public virtual string User
        {
            get { return user; }
            set { user = value; }
        }

        [XmlAttribute("Pass")]
        private string pass;
        public virtual string Pass
        {
            get { return pass; }
            set { pass = value; }
        }

        [XmlAttribute("Data")]
        private string data;
        public virtual string Data
        {
            get { return data; }
            set { data = value; }
        }

    }
}
