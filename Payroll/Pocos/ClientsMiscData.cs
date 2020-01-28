using System;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class ClientsMiscData
    {
        [XmlAttribute("Bentuk_Prsh")]
        private String bentuk_Prsh;
        public virtual String Bentuk_Prsh
        {
            get { return bentuk_Prsh; }
            set { bentuk_Prsh = value; }
        }

        [XmlAttribute("Tempat_Pendrian")]
        private String tempat_Pendrian;
        public virtual String Tempat_Pendrian
        {
            get { return tempat_Pendrian; }
            set { tempat_Pendrian = value; }
        }

        [XmlAttribute("Legalitas")]
        private String legalitas;
        public virtual String Legalitas
        {
            get { return legalitas; }
            set { legalitas = value; }
        }

        [XmlAttribute("Legal_No")]
        private String legal_No;
        public virtual String Legal_No
        {
            get { return legal_No; }
            set { legal_No = value; }
        }

        [XmlAttribute("Legal_Date")]
        private DateTime legal_Date;
        public virtual DateTime Legal_Date
        {
            get { return legal_Date; }
            set { legal_Date = value; }
        }

        [XmlAttribute("Legal_Expired")]
        private DateTime legal_Expired;
        public virtual DateTime Legal_Expired
        {
            get { return legal_Expired; }
            set { legal_Expired = value; }
        }

        [XmlAttribute("Akta_No")]
        private String akta_No;
        public virtual String Akta_No
        {
            get { return akta_No; }
            set { akta_No = value; }
        }

        [XmlAttribute("Akta_Date")]
        private DateTime akta_Date;
        public virtual DateTime Akta_Date
        {
            get { return akta_Date; }
            set { akta_Date = value; }
        }

        [XmlAttribute("Akta_Ch_No")]
        private String akta_Ch_No;
        public virtual String Akta_Ch_No
        {
            get { return akta_Ch_No; }
            set { akta_Ch_No = value; }
        }

        [XmlAttribute("Akta_Ch_Date")]
        private DateTime akta_Ch_Date;
        public virtual DateTime Akta_Ch_Date
        {
            get { return akta_Ch_Date; }
            set { akta_Ch_Date = value; }
        }

        [XmlAttribute("Npwp")]
        private String npwp;
        public virtual String Npwp
        {
            get { return npwp; }
            set { npwp = value; }
        }

        [XmlAttribute("KodePos")]
        private int kodePos;
        public virtual int KodePos
        {
            get { return kodePos; }
            set { kodePos = value; }
        }

        [XmlAttribute("Kecamatan")]
        private String kecamatan;
        public virtual String Kecamatan
        {
            get { return kecamatan; }
            set { kecamatan = value; }
        }

        [XmlAttribute("Kota")]
        private String kota;
        public virtual String Kota
        {
            get { return kota; }
            set { kota = value; }
        }

        [XmlAttribute("Propinsi")]
        private String propinsi;
        public virtual String Propinsi
        {
            get { return propinsi; }
            set { propinsi = value; }
        }

        [XmlAttribute("Email")]
        private String email;
        public virtual String Email
        {
            get { return email; }
            set { email = value; }
        }

        [XmlAttribute("Allow")]
        private int allow;
        public virtual int Allow
        {
            get { return allow; }
            set { allow = value; }
        }
    }
}
