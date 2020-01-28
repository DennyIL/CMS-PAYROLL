using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    [XmlRoot("TrxMassRTGSDetail")]
    public class TrxMassRTGSDetail : AbstractTransaction
    {

        private int _id;
        [XmlAttribute("Id")]
        public virtual int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _parentid;
        [XmlAttribute("ParentId")]
        public virtual int ParentId
        {
            get { return _parentid; }
            set { _parentid = value; }
        }

        private String _cmsrefno;
        [XmlAttribute("CmsRefNo")]
        public virtual String CmsRefNo
        {
            get { return _cmsrefno; }
            set { _cmsrefno = value; }
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

        private String _debitaccountnamecode;
        [XmlAttribute("DebitAccountNameCode")]
        public virtual String DebitAccountNameCode
        {
            get { return _debitaccountnamecode; }
            set { _debitaccountnamecode = value; }
        }

        private String _debitaccounttype;
        [XmlAttribute("DebitAccountType")]
        public virtual String DebitAccountType
        {
            get { return _debitaccounttype; }
            set { _debitaccounttype = value; }
        }

        private String _debitcurrency;
        [XmlAttribute("DebitCurrency")]
        public virtual String DebitCurrency
        {
            get { return _debitcurrency; }
            set { _debitcurrency = value; }
        }

        private String _rtgstrxref;
        [XmlAttribute("RtgsTrxRef")]
        public virtual String RtgsTrxRef
        {
            get { return _rtgstrxref; }
            set { _rtgstrxref = value; }
        }

        private String _creditaccount;
        [XmlAttribute("CreditAccount")]
        public virtual String CreditAccount
        {
            get { return _creditaccount; }
            set { _creditaccount = value; }
        }

        private String _creditaccountcoded;
        [XmlAttribute("CreditAccountCoded")]
        public virtual String CreditAccountCoded
        {
            get { return _creditaccountcoded; }
            set { _creditaccountcoded = value; }
        }

        private String _creditaccountname;
        [XmlAttribute("CreditAccountName")]
        public virtual String CreditAccountName
        {
            get { return _creditaccountname; }
            set { _creditaccountname = value; }
        }

        private String _creditaccountaddress;
        [XmlAttribute("CreditAccountAddress")]
        public virtual String CreditAccountAddress
        {
            get { return _creditaccountaddress; }
            set { _creditaccountaddress = value; }
        }

        private String _creditaccountbank;
        [XmlAttribute("CreditAccountBank")]
        public virtual String CreditAccountBank
        {
            get { return _creditaccountbank; }
            set { _creditaccountbank = value; }
        }

        private String _creditaccountbankname;
        [XmlAttribute("CreditAccountBankName")]
        public virtual String CreditAccountBankName
        {
            get { return _creditaccountbankname; }
            set { _creditaccountbankname = value; }
        }

        private String _creditaccountbranch;
        [XmlAttribute("CreditAccountBranch")]
        public virtual String CreditAccountBranch
        {
            get { return _creditaccountbranch; }
            set { _creditaccountbranch = value; }
        }

        private double _amount;
        [XmlAttribute("Amount")]
        public virtual double Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        private String _memberinfo;
        [XmlAttribute("MemberInfo")]
        public virtual String MemberInfo
        {
            get { return _memberinfo; }
            set { _memberinfo = value; }
        }

        private double _feeamount;
        [XmlAttribute("FeeAmount")]
        public virtual double FeeAmount
        {
            get { return _feeamount; }
            set { _feeamount = value; }
        }

        private double _feeour;
        [XmlAttribute("FeeOur")]
        public virtual double FeeOur
        {
            get { return _feeour; }
            set { _feeour = value; }
        }

        private double _feeben;
        [XmlAttribute("FeeBen")]
        public virtual double FeeBen
        {
            get { return _feeben; }
            set { _feeben = value; }
        }

        private double _debitamount;
        [XmlAttribute("DebitAmount")]
        public virtual double DebitAmount
        {
            get { return _debitamount; }
            set { _debitamount = value; }
        }

        private double _creditamount;
        [XmlAttribute("CreditAmount")]
        public virtual double CreditAmount
        {
            get { return _creditamount; }
            set { _creditamount = value; }
        }

        private String _feecurrency;
        [XmlAttribute("FeeCurrency")]
        public virtual String FeeCurrency
        {
            get { return _feecurrency; }
            set { _feecurrency = value; }
        }

        private DateTime _runningdate;
        [XmlAttribute("RunningDate")]
        public virtual DateTime RunningDate
        {
            get { return _runningdate; }
            set { _runningdate = value; }
        }

        private String _notification;
        [XmlAttribute("Notification")]
        public virtual String Notification
        {
            get { return _notification; }
            set { _notification = value; }
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
        [XmlAttribute("JurnalSeq1")]
        public virtual String JurnalSeq1
        {
            get { return _jurnalseq1; }
            set { _jurnalseq1 = value; }
        }

        private String _jurnalseq2;
        [XmlAttribute("JurnalSeq2")]
        public virtual String JurnalSeq2
        {
            get { return _jurnalseq2; }
            set { _jurnalseq2 = value; }
        }

        private String _nextprocessor;
        [XmlAttribute("NextProcessor")]
        public virtual String NextProcessor
        {
            get { return _nextprocessor; }
            set { _nextprocessor = value; }
        }

        private String _trxremark;
        [XmlAttribute("TrxRemark")]
        public virtual String TrxRemark
        {
            get { return _trxremark; }
            set { _trxremark = value; }
        }

        private String _trxreff;
        [XmlAttribute("TrxReff")]
        public virtual String TrxReff
        {
            get { return _trxreff; }
            set { _trxreff = value; }
        }

        private int _status;
        [XmlAttribute("Status")]
        public virtual int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private String _description;
        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private String _chargetype;
        [XmlAttribute("ChargeType")]
        public virtual String ChargeType
        {
            get { return _chargetype; }
            set { _chargetype = value; }
        }

        private int client;
        [XmlAttribute("Client")]
        public virtual int Client
        {
            get { return client; }
            set { client = value; }
        }
    }
}
