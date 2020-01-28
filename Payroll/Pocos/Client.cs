using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class Client : AbstractTransaction
    {
        private Int32 id;
        private String handle;
        private String name;
        private String description;
        private String bussines;
        private String address1;
        private String address2;
        private String address3;
        private String miscData;
        private String phone;
        private String fax;
        private String type;
        private String cp;
        private String cpDescription;
        private String cpEmail;
        private String cpPhone;
        private DateTime creationDate;
        private DateTime lastUpdate;
        private String approver;
        private String language;
        private String currency;
        private int recStatus;
        private String perekomendasi;
        private int needtoken;
        private String bookingoffice;
        private String jns_prhs;
        private String cpDescription2;
        private String voucherCode;
        private int eregStat;

        private ClientMatrix clientMatrix;
        private User user1;
        private User user2;
        private ClientsMiscData clMisc;
        
        private String cp2;
        //private String cpDescription2;
        private String cpEmail2;
        private String cpPhone2;

        private Double limitoutidr;
        private Double limitoutvalas;
        private Double limittotalob;

        private int maxuser;

        private String seller;
        private String seller2;
        private String keterangan;
        private int jenisclient;
        private int etaxDKI;
        private int etaxNonDKI;
        private string token;
        

        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; } 
        }

        [XmlAttribute("Code")]
        public virtual String Handle
        {
            get { return handle; }
            set { handle = value; }
        }

        [XmlAttribute("Name")]
        public virtual String Name
        {
            get { return name; }
            set { name = value; }
        }

        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        [XmlAttribute("Bussines")]
        public virtual String Bussines
        {
            get { return bussines; }
            set { bussines = value; }
        }

        public virtual String Address1
        {
            get { return address1; }
            set { address1 = value; }
        }

        public virtual String Address2
        {
            get { return address2; }
            set { address2 = value; }
        }

        public virtual String Address3
        {
            get { return address3; }
            set { address3 = value; }
        }

        [XmlAttribute("MiscData")]
        public virtual String MiscData
        {
            get { return miscData; }
            set { miscData = value; }
        }

        private String accList;
        [XmlAttribute("AccList")]
        public virtual String AccList
        {
            get { return accList; }
            set { accList = value; }
        }

        private String fiturList;
        [XmlAttribute("FiturList")]
        public virtual String FiturList
        {
            get { return fiturList; }
            set { fiturList = value; }
        }

        public virtual String Phone
        {
            get { return phone; }
            set { phone = value; }
        }

        public virtual String Fax
        {
            get { return fax; }
            set { fax = value; }
        }

        public virtual String Type
        {
            get { return type; }
            set { type = value; }
        }

        public virtual String Cp
        {
            get { return cp; }
            set { cp = value; }
        }

        public virtual String CpDescription
        {
            get { return cpDescription; }
            set { cpDescription = value; }
        }

        public virtual String CpEmail
        {
            get { return cpEmail; }
            set { cpEmail = value; }
        }

        public virtual String CpPhone
        {
            get { return cpPhone; }
            set { cpPhone = value; }
        }

        [XmlAttribute("Cdate")]
        public virtual DateTime CreationDate
        {
            get { return creationDate; }
            set { creationDate = value; }
        }

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

        [XmlAttribute("VoucherCode")]
        public virtual String VoucherCode
        {
            get { return voucherCode; }
            set { voucherCode = value; }
        }

        [XmlAttribute("EregStat")]
        public virtual int EregStat
        {
            get { return eregStat; }
            set { eregStat = value; }
        }

        public virtual String Language
        {
            get { return language; }
            set { language = value; }
        }

        public virtual String Currency
        {
            get { return currency; }
            set { currency = value; }
        }

        public virtual int RecStatus
        {
            get { return recStatus; }
            set { recStatus = value; }
        }

        public virtual String Perekomendasi
        {
            get { return perekomendasi; }
            set { perekomendasi = value; }
        }

        public virtual int NeedToken
        {
            get { return needtoken; }
            set { needtoken = value; }
        }

        public virtual int EtaxDKI
        {
            get { return etaxDKI; }
            set { etaxDKI = value; }
        }

        public virtual int EtaxNonDKI
        {
            get { return etaxNonDKI; }
            set { etaxNonDKI = value; }
        }

        [XmlAttribute("Token")]
        public virtual string Token
        {
            get { return token; }
            set { token = value; }
        }

        public virtual String BookingOffice
        {
            get { return bookingoffice; }
            set { bookingoffice = value; }
        }

        public virtual String Jenis_prhs
        {
            get { return jns_prhs; }
            set { jns_prhs = value; }
        }
       
        [XmlIgnore]
        public virtual ClientMatrix ClientMatrix
        {
            get { return clientMatrix; }
            set { clientMatrix = value; }
        }

        public virtual User User1
        {
            get { return user1; }
            set { user1 = value; }
        }

        public virtual User User2
        {
            get { return user2; }
            set { user2 = value; }
        }

        public virtual ClientsMiscData ClMisc
        {
            get { return clMisc; }
            set { clMisc = value; }
        }

        public virtual String Cp2
        {
            get { return cp2; }
            set { cp2 = value; }
        }

        //public virtual String CpDescription2
        //{
        //    get { return cpDescription2; }
        //    set { cpDescription2 = value; }
        //}

        public virtual String CpEmail2
        {
            get { return cpEmail2; }
            set { cpEmail2 = value; }
        }

        public virtual String CpPhone2
        {
            get { return cpPhone2; }
            set { cpPhone2 = value; }
        }

        public virtual String CpDescription2
        {
            get { return cpDescription2; }
            set { cpDescription2 = value; }
        }

        public virtual Double LimitOutIDR
        {
            get { return limitoutidr; }
            set { limitoutidr = value; }
        }

        public virtual Double LimitOutValas
        {
            get { return limitoutvalas; }
            set { limitoutvalas = value; }
        }

        public virtual Double LimitTotalOB
        {
            get { return limittotalob; }
            set { limittotalob = value; }
        }

        public virtual int MaxUser
        {
            get { return maxuser; }
            set { maxuser = value; }
        }

        [XmlAttribute("Seller")]
        public virtual String Seller
        {
            get { return seller; }
            set { seller = value; }
        }

        [XmlAttribute("Keterangan")]
        public virtual String Keterangan
        {
            get { return keterangan; }
            set { keterangan = value; }
        }

        [XmlAttribute("Seller2")]
        public virtual String Seller2
        {
            get { return seller2; }
            set { seller2 = value; }
        }

        [XmlAttribute("JenisClient")]
        public virtual int JenisClient
        {
            get { return jenisclient; }
            set { jenisclient = value; }
        }

    }
}
