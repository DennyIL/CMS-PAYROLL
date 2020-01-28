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
    public class Holidays : AbstractTransaction
    {
        private String id;
        private DateTime tanggal;
        private String keterangan;
        private String fids;

        [XmlAttribute("Id")]
        public virtual String Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("Tanggal")]
        public virtual DateTime Tanggal
        {
            get { return tanggal; }
            set { tanggal = value; }
        }

        [XmlAttribute("Keterangan")]
        public virtual String Keterangan
        {
            get { return keterangan; }
            set { keterangan = value; }
        }

        [XmlAttribute("FunctionId")]
        public virtual String FunctionId
        {
            get { return fids; }
            set { fids = value; }
        }
    }
}
