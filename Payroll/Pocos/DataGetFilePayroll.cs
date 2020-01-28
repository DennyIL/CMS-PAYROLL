using System;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class DataGetFilePayroll : AbstractTransaction
    {
        [XmlAttribute("NamaFile")]
        private string namaFile;
        public virtual string NamaFile
        {
            get { return namaFile; }
            set { namaFile = value; }
        }

        [XmlAttribute("Status")]
        private int status;
        public virtual int Status
        {
            get { return status; }
            set { status = value; }
        }

    }
}
