using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    [XmlRoot("Booking")]
    public class Booking : AbstractTransaction
    {

        private Int32 _id;
        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _trxid;
        [XmlAttribute("TrxId")]
        public virtual string TrxId
        {
            get { return _trxid; }
            set { _trxid = value; }
        }

        private int _fid;
        [XmlAttribute("FID")]
        public virtual int FID
        {
            get { return _fid; }
            set { _fid = value; }
        }

        private String _fitur;
        [XmlAttribute("Fitur")]
        public virtual String Fitur
        {
            get { return _fitur; }
            set { _fitur = value; }
        }

        private String _jenistransaksi;
        [XmlAttribute("JenisTransaksi")]
        public virtual String JenisTransaksi
        {
            get { return _jenistransaksi; }
            set { _jenistransaksi = value; }
        }

        private int _clientid;
        [XmlAttribute("ClientId")]
        public virtual int ClientId
        {
            get { return _clientid; }
            set { _clientid = value; }
        }

        private DateTime _creationdate;
        [XmlAttribute("CreationDate")]
        public virtual DateTime CreationDate
        {
            get { return _creationdate; }
            set { _creationdate = value; }
        }

        private String _debitaccount;
        [XmlAttribute("DebitAccount")]
        public virtual String DebitAccount
        {
            get { return _debitaccount; }
            set { _debitaccount = value; }
        }

        private String _debitaccountname;
        [XmlAttribute("DebitAccountName")]
        public virtual String DebitAccountName
        {
            get { return _debitaccountname; }
            set { _debitaccountname = value; }
        }

        private Double _debitamount;
        [XmlAttribute("DebitAmount")]
        public virtual Double DebitAmount
        {
            get { return _debitamount; }
            set { _debitamount = value; }
        }

        private String _debitcurrency;
        [XmlAttribute("DebitCurrency")]
        public virtual String DebitCurrency
        {
            get { return _debitcurrency; }
            set { _debitcurrency = value; }
        }

        private String _creditaccount;
        [XmlAttribute("CreditAccount")]
        public virtual String CreditAccount
        {
            get { return _creditaccount; }
            set { _creditaccount = value; }
        }

        private String _creditaccountname;
        [XmlAttribute("CreditAccountName")]
        public virtual String CreditAccountName
        {
            get { return _creditaccountname; }
            set { _creditaccountname = value; }
        }

        private Double _creditamount;
        [XmlAttribute("CreditAmount")]
        public virtual Double CreditAmount
        {
            get { return _creditamount; }
            set { _creditamount = value; }
        }

        private String _creditcurrency;
        [XmlAttribute("CreditCurrency")]
        public virtual String CreditCurrency
        {
            get { return _creditcurrency; }
            set { _creditcurrency = value; }
        }

        private Double _instamount;
        [XmlAttribute("InstAmount")]
        public virtual Double InstAmount
        {
            get { return _instamount; }
            set { _instamount = value; }
        }

        private String _instcurrency;
        [XmlAttribute("InstCurrency")]
        public virtual String InstCurrency
        {
            get { return _instcurrency; }
            set { _instcurrency = value; }
        }

        private String _trxrate;
        [XmlAttribute("TrxRate")]
        public virtual String TrxRate
        {
            get { return _trxrate; }
            set { _trxrate = value; }
        }

        private string _dealrate;
        [XmlAttribute("DealRate")]
        public virtual string DealRate
        {
            get { return _dealrate; }
            set { _dealrate = value; }
        }

        private String _trxremark;
        [XmlAttribute("TrxRemark")]
        public virtual String TrxRemark
        {
            get { return _trxremark; }
            set { _trxremark = value; }
        }

        private DateTime _trxdate;
        [XmlAttribute("TrxDate")]
        public virtual DateTime TrxDate
        {
            get { return _trxdate; }
            set { _trxdate = value; }
        }

        private int _booktype;
        [XmlAttribute("BookType")]
        public virtual int BookType
        {
            get { return _booktype; }
            set { _booktype = value; }
        }

        private int _status;
        [XmlAttribute("Status")]
        public virtual int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private int _statusec;
        [XmlAttribute("StatusEC")]
        public virtual int StatusEC
        {
            get { return _statusec; }
            set { _statusec = value; }
        }

        private string _description;
        [XmlAttribute("Description")]
        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private DateTime _lastupdate;
        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return _lastupdate; }
            set { _lastupdate = value; }
        }

        private String _trancode;
        [XmlAttribute("TranCode")]
        public virtual String TranCode
        {
            get { return _trancode; }
            set { _trancode = value; }
        }

        private String _tellerid;
        [XmlAttribute("TellerId")]
        public virtual String TellerId
        {
            get { return _tellerid; }
            set { _tellerid = value; }
        }

        private String _spvid;
        [XmlAttribute("SpvId")]
        public virtual String SpvId
        {
            get { return _spvid; }
            set { _spvid = value; }
        }

        private String _bucket;
        [XmlAttribute("Bucket")]
        public virtual String Bucket
        {
            get { return _bucket; }
            set { _bucket = value; }
        }

        private String _jurnalseq;
        [XmlAttribute("JournalSeq")]
        public virtual String JournalSeq
        {
            get { return _jurnalseq; }
            set { _jurnalseq = value; }
        }

        private String _bookingOffice;
        [XmlAttribute("BookingOffice")]
        public virtual String BookingOffice
        {
            get { return _bookingOffice; }
            set { _bookingOffice = value; }
        }

        private String _log;
        [XmlAttribute("Log")]
        public virtual String Log
        {
            get { return _log; }
            set { _log = value; }
        }

        private int isNotionalPooling;
        [XmlAttribute("IsNotionalPooling")]
        public virtual int IsNotionalPooling
        {
            get { return isNotionalPooling; }
            set { isNotionalPooling = value; }
        }

        private String _trxremark2;
        [XmlAttribute("TrxRemark2")]
        public virtual String TrxRemark2
        {
            get { return _trxremark2; }
            set { _trxremark2 = value; }
        }

        private double feeBen;
        [XmlAttribute("FeeBen")]
        public virtual double FeeBen
        {
            get { return feeBen; }
            set { feeBen = value; }
        }
    }
}
