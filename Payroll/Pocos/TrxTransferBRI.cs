using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    [XmlRoot("TrxTransferBRI")]
    public class TrxTransferBRI : AbstractTransaction
    {

        private Int32 _id;
        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _clientid;
        [XmlAttribute("ClientID")]
        public virtual int ClientID
        {
            get { return _clientid; }
            set { _clientid = value; }
        }

        private String _trxid;
        [XmlAttribute("TrxID")]
        public virtual String TrxID
        {
            get { return _trxid; }
            set { _trxid = value; }
        }

        private DateTime _trxdate;
        [XmlAttribute("TrxDate")]
        public virtual DateTime TrxDate
        {
            get { return _trxdate; }
            set { _trxdate = value; }
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

        private String _debitaccountnamebrinets;
        [XmlAttribute("DebitAccountNameBrinets")]
        public virtual String DebitAccountNameBrinets
        {
            get { return _debitaccountnamebrinets; }
            set { _debitaccountnamebrinets = value; }
        }

        private String _debitaccounttype;
        [XmlAttribute("DebitAccountType")]
        public virtual String DebitAccountType
        {
            get { return _debitaccounttype; }
            set { _debitaccounttype = value; }
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

        private String _creditaccountnamebrinets;
        [XmlAttribute("CreditAccountNameBrinets")]
        public virtual String CreditAccountNameBrinets
        {
            get { return _creditaccountnamebrinets; }
            set { _creditaccountnamebrinets = value; }
        }

        private String _creditaccounttype;
        [XmlAttribute("CreditAccountType")]
        public virtual String CreditAccountType
        {
            get { return _creditaccounttype; }
            set { _creditaccounttype = value; }
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

        private String _trxremark;
        [XmlAttribute("TrxRemark")]
        public virtual String TrxRemark
        {
            get { return _trxremark; }
            set { _trxremark = value; }
        }

        private String _status;
        [XmlAttribute("Status")]
        public virtual String Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private DateTime _creationdate;
        [XmlAttribute("CreationDate")]
        public virtual DateTime CreationDate
        {
            get { return _creationdate; }
            set { _creationdate = value; }
        }

        private DateTime _lastupdate;
        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return _lastupdate; }
            set { _lastupdate = value; }
        }

        private String _maker;
        [XmlAttribute("Maker")]
        public virtual String Maker
        {
            get { return _maker; }
            set { _maker = value; }
        }

        private String _cheker;
        [XmlAttribute("Cheker")]
        public virtual String Cheker
        {
            get { return _cheker; }
            set { _cheker = value; }
        }

        private String _approver;
        [XmlAttribute("Approver")]
        public virtual String Approver
        {
            get { return _approver; }
            set { _approver = value; }
        }

        private String _rejecter;
        [XmlAttribute("Rejecter")]
        public virtual String Rejecter
        {
            get { return _rejecter; }
            set { _rejecter = value; }
        }

        private String _checkerworkflow;
        [XmlAttribute("CheckerWorkflow")]
        public virtual String CheckerWorkflow
        {
            get { return _checkerworkflow; }
            set { _checkerworkflow = value; }
        }

        private String _approverworkflow;
        [XmlAttribute("ApproverWorkflow")]
        public virtual String ApproverWorkflow
        {
            get { return _approverworkflow; }
            set { _approverworkflow = value; }
        }

        private Double _feeamount;
        [XmlAttribute("FeeAmount")]
        public virtual Double FeeAmount
        {
            get { return _feeamount; }
            set { _feeamount = value; }
        }

        private String _feecurrency;
        [XmlAttribute("FeeCurrency")]
        public virtual String FeeCurrency
        {
            get { return _feecurrency; }
            set { _feecurrency = value; }
        }

        private Double _feeour;
        [XmlAttribute("FeeOur")]
        public virtual Double FeeOur
        {
            get { return _feeour; }
            set { _feeour = value; }
        }

        private Double _feedebet;
        [XmlAttribute("FeeDebet")]
        public virtual Double FeeDebet
        {
            get { return _feedebet; }
            set { _feedebet = value; }
        }

        private Double _feeben;
        [XmlAttribute("FeeBen")]
        public virtual Double FeeBen
        {
            get { return _feeben; }
            set { _feeben = value; }
        }

        private Double _feecredit;
        [XmlAttribute("FeeCredit")]
        public virtual Double FeeCredit
        {
            get { return _feecredit; }
            set { _feecredit = value; }
        }

        private Double _instamt;
        [XmlAttribute("InstAmount")]
        public virtual Double InstAmount
        {
            get { return _instamt; }
            set { _instamt = value; }
        }

        private String _instcur;
        [XmlAttribute("InstCur")]
        public virtual String InstCur
        {
            get { return _instcur; }
            set { _instcur = value; }
        }

        private String _vouchercode;
        [XmlAttribute("VoucherCode")]
        public virtual String VoucherCode
        {
            get { return _vouchercode; }
            set { _vouchercode = value; }
        }

        private String _notification;
        [XmlAttribute("Notification")]
        public virtual String Notification
        {
            get { return _notification; }
            set { _notification = value; }
        }

        private String _sanditrxbi;
        [XmlAttribute("SandiTrxBI")]
        public virtual String SandiTrxBI
        {
            get { return _sanditrxbi; }
            set { _sanditrxbi = value; }
        }

        private String _bucketamt;
        [XmlAttribute("BucketAmt")]
        public virtual String BucketAmt
        {
            get { return _bucketamt; }
            set { _bucketamt = value; }
        }

        private String _bucketfee;
        [XmlAttribute("BucketFee")]
        public virtual String BucketFee
        {
            get { return _bucketfee; }
            set { _bucketfee = value; }
        }

        private String _jurnalseq1;
        [XmlAttribute("JournalSeq1")]
        public virtual String JournalSeq1
        {
            get { return _jurnalseq1; }
            set { _jurnalseq1 = value; }
        }

        private String _jurnalseq2;
        [XmlAttribute("JournalSeq2")]
        public virtual String JournalSeq2
        {
            get { return _jurnalseq2; }
            set { _jurnalseq2 = value; }
        }

        private String _trxrate;
        [XmlAttribute("TrxRate")]
        public virtual String TrxRate
        {
            get { return _trxrate; }
            set { _trxrate = value; }
        }

        private int _isBrivaKredit;
        [XmlAttribute("IsBrivaKredit")]
        public virtual int IsBrivaKredit
        {
            get { return _isBrivaKredit; }
            set { _isBrivaKredit = value; }
        }

        private int _batchid;
        [XmlAttribute("BatchId")]
        public virtual int BatchId
        {
            get { return _batchid; }
            set { _batchid = value; }
        }

        private String processor;
        [XmlAttribute("Processor")]
        public virtual String Processor
        {
            get { return processor; }
            set { processor = value; }
        }

        private String schedule;
        [XmlAttribute("Schedule")]
        public virtual String Schedule
        {
            get { return schedule; }
            set { schedule = value; }
        }

        private String _bookingOffice;
        [XmlAttribute("BookingOffice")]
        public virtual String BookingOffice
        {
            get { return _bookingOffice; }
            set { _bookingOffice = value; }
        }

        private string reffId;
        [XmlAttribute("ReffId")]
        public virtual string ReffId
        {
            get { return reffId; }
            set { reffId = value; }
        }

    }
}
