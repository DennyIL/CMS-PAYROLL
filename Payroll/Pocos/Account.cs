using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    public class Account : AbstractTransaction
    {
        private Int32 id;
        private String code;
        private String number;
        private String type;
        private String currency;
        private String name;
        private Double ledger;
        private int jenis_account;
        private int kepemilikan;
        private DateTime deletetime;
        private int statusrek;
        private int statusdel;
        private String keterangan;
        private int isrdn;
        private int isnew;

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

        [XmlAttribute("Number")]
        public virtual String Number
        {
            get { return number; }
            set { number = value; }
        }

        [XmlAttribute("Type")]
        public virtual String Type
        {
            get { return type; }
            set { type = value; }
        }

        [XmlAttribute("Currency")]
        public virtual String Currency
        {
            get { return currency; }
            set { currency = value; }
        }

        [XmlAttribute("Name")]
        public virtual String Name
        {
            get { return name; }
            set { name = value; }
        }

        [XmlAttribute("Ledger")]
        public virtual Double Ledger
        {
            get { return ledger; }
            set { ledger = value; }
        }

        [XmlAttribute("Jenis_Account")]
        public virtual int Jenis_Account
        {
            get { return jenis_account; }
            set { jenis_account = value; }
        }

        [XmlAttribute("Kepemilikan")]
        public virtual int Kepemilikan
        {
            get { return kepemilikan; }
            set { kepemilikan = value; }
        }

        [XmlAttribute("DeleteTime")]
        public virtual DateTime DeleteTime
        {
            get { return deletetime; }
            set { deletetime = value; }
        }

        [XmlAttribute("StatusRek")]
        public virtual int StatusRek
        {
            get { return statusrek; }
            set { statusrek = value; }
        }

        [XmlAttribute("StatusDel")]
        public virtual int StatusDel
        {
            get { return statusdel; }
            set { statusdel = value; }
        }

        [XmlAttribute("Keterangan")]
        public virtual string Keterangan
        {
            get { return keterangan; }
            set { keterangan = value; }
        }

        [XmlAttribute("IsRDN")]
        public virtual int IsRDN
        {
            get { return isrdn; }
            set { isrdn = value; }
        }
        [XmlAttribute("IsNew")]
        public virtual int IsNew
        {
            get { return isnew; }
            set { isnew = value; }
        }
    }
}
