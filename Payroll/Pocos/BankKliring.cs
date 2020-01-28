using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class BankKliring
    {
        private Int32 id;
        private String code;
        private String kodeinduk;
        private String entityid;
        private String nama;
        private String address1;
        private String address2;
        private String address3;
        private String bankcode;

        private String sandikota;
        private String sandipropinsi;
        private String lastupdate;
        private String jenisusaha;


        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("Code")]
        public virtual String Code
        {
            get { return code; }
            set { code = value; }
        }

        [XmlAttribute("KodeInduk")]
        public virtual String KodeInduk
        {
            get { return kodeinduk; }
            set { kodeinduk = value; }
        }

        [XmlAttribute("EntityId")]
        public virtual String EntityId
        {
            get { return entityid; }
            set { entityid = value; }
        }

        [XmlAttribute("Nama")]
        public virtual String Nama
        {
            get { return nama; }
            set { nama = value; }
        }

        [XmlAttribute("Address1")]
        public virtual String Address1
        {
            get { return address1; }
            set { address1 = value; }
        }

        [XmlAttribute("Address2")]
        public virtual String Address2
        {
            get { return address2; }
            set { address2 = value; }
        }

        [XmlAttribute("Address3")]
        public virtual String Address3
        {
            get { return address3; }
            set { address3 = value; }
        }

        [XmlAttribute("BankCode")]
        public virtual String BankCode
        {
            get { return bankcode; }
            set { bankcode = value; }
        }

        [XmlAttribute("SandiKota")]
        public virtual String SandiKota
        {
            get { return sandikota; }
            set { sandikota = value; }
        }

        [XmlAttribute("SandiPropinsi")]
        public virtual String SandiPropinsi
        {
            get { return sandipropinsi; }
            set { sandipropinsi = value; }
        }

        [XmlAttribute("LastUpdate")]
        public virtual String LastUpdate
        {
            get { return lastupdate; }
            set { lastupdate = value; }
        }

        [XmlAttribute("JenisUsaha")]
        public virtual String JenisUsaha
        {
            get { return jenisusaha; }
            set { jenisusaha = value; }
        }
    }
}
