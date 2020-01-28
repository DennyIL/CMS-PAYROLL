using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxMassLLGDetail : AbstractTransaction
    {
        private Int32 id;
        private String pid;
        private Int32 parentid;
        private String debitAccount;
        private String debitName;
        private String debitType;
        private String debitCurrency;
        private String creditAccount;
        private String creditName;
        private Double creditAmount;
        private Int32 status;
        private String remarks;
        private String description;
        private Double feeAmount;
        private String feeCurrency;
        private String trxreff;
        private DateTime runningDate;
        private String creditaccountaddress;
        private String creditaccountbank;
        private String rtgstrxref;
        private String memberinfo;
        private String cmsrefno;
        private String debitaccountnamebrinets;
        private String debitaccountnamecode;
        private String creditaccountcoded;
        private String creditaccountbankname;
        private String creditaccountbranch;
        private double amount;
        private double feeour;
        private double feeben;
        private double debitamount;
        private String notification;
        private String bucketamt;
        private String bucketfee;
        private String jurnalseq1;
        private String jurnalseq2;
        private String nextprocessor;
        private String chargetype;
        private int client;
        private String _bookingoffice;
        private String dke;
        private String seq;

        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("ParentId")]
        public virtual Int32 ParentId
        {
            get { return parentid; }
            set { parentid = value; }
        }

        [XmlAttribute("DebitAccount")]
        public virtual String DebitAccount
        {
            get { return debitAccount; }
            set { debitAccount = value; }
        }

        [XmlAttribute("DebitAccountName")]
        public virtual String DebitAccountName
        {
            get { return debitName; }
            set { debitName = value; }
        }
        [XmlAttribute("DebitAccountType")]
        public virtual String DebitAccountType
        {
            get { return debitType; }
            set { debitType = value; }
        }

        [XmlAttribute("DebitCurrency")]
        public virtual String DebitCurrency
        {
            get { return debitCurrency; }
            set { debitCurrency = value; }
        }

        [XmlAttribute("CreditAccount")]
        public virtual String CreditAccount
        {
            get { return creditAccount; }
            set { creditAccount = value; }
        }

        [XmlAttribute("CreditAccountName")]
        public virtual String CreditAccountName
        {
            get { return creditName; }
            set { creditName = value; }
        }

        [XmlAttribute("CreditAccountAddress")]
        public virtual String CreditAccountAddress
        {
            get { return creditaccountaddress; }
            set { creditaccountaddress = value; }
        }

        [XmlAttribute("CreditAccountBank")]
        public virtual String CreditAccountBank
        {
            get { return creditaccountbank; }
            set { creditaccountbank = value; }
        }

        [XmlAttribute("MemberInfo")]
        public virtual String MemberInfo
        {
            get { return memberinfo; }
            set { memberinfo = value; }
        }

        [XmlAttribute("RtgsTrxRef")]
        public virtual String RtgsTrxRef
        {
            get { return rtgstrxref; }
            set { rtgstrxref = value; }
        }

        [XmlAttribute("CreditAmount")]
        public virtual Double CreditAmount
        {
            get { return creditAmount; }
            set { creditAmount = value; }
        }

        [XmlAttribute("Status")]
        public virtual Int32 Status
        {
            get { return status; }
            set { status = value; }
        }

        [XmlAttribute("TrxRemarks")]
        public virtual String TrxRemarks
        {
            get { return remarks; }
            set { remarks = value; }
        }

        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        [XmlAttribute("FeeAmount")]
        public virtual Double FeeAmount
        {
            get { return feeAmount; }
            set { feeAmount = value; }
        }

        [XmlAttribute("FeeCurrency")]
        public virtual String FeeCurrency
        {
            get { return feeCurrency; }
            set { feeCurrency = value; }
        }

        [XmlAttribute("TrxReff")]
        public virtual String TrxReff
        {
            get { return trxreff; }
            set { trxreff = value; }
        }
        [XmlAttribute("RunningDate")]
        public virtual DateTime RunningDate
        {
            get { return runningDate; }
            set { runningDate = value; }
        }

        [XmlAttribute("CmsRefNo")]
        public virtual String CmsRefNo
        {
            get { return cmsrefno; }
            set { cmsrefno = value; }
        }

        [XmlAttribute("DebitAccountNameBrinets")]
        public virtual String DebitAccountNameBrinets
        {
            get { return debitaccountnamebrinets; }
            set { debitaccountnamebrinets = value; }
        }

        [XmlAttribute("DebitAccountNameCode")]
        public virtual String DebitAccountNameCode
        {
            get { return debitaccountnamecode; }
            set { debitaccountnamecode = value; }
        }

        [XmlAttribute("CreditAccountCoded")]
        public virtual String CreditAccountCoded
        {
            get { return creditaccountcoded; }
            set { creditaccountcoded = value; }
        }

        [XmlAttribute("CreditAccountBankName")]
        public virtual String CreditAccountBankName
        {
            get { return creditaccountbankname; }
            set { creditaccountbankname = value; }
        }

        [XmlAttribute("CreditAccountBranch")]
        public virtual String CreditAccountBranch
        {
            get { return creditaccountbranch; }
            set { creditaccountbranch = value; }
        }

        [XmlAttribute("Amount")]
        public virtual double Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        [XmlAttribute("FeeOur")]
        public virtual double FeeOur
        {
            get { return feeour; }
            set { feeour = value; }
        }

        [XmlAttribute("FeeBen")]
        public virtual double FeeBen
        {
            get { return feeben; }
            set { feeben = value; }
        }

        [XmlAttribute("DebitAmount")]
        public virtual double DebitAmount
        {
            get { return debitamount; }
            set { debitamount = value; }
        }

        [XmlAttribute("Notification")]
        public virtual String Notification
        {
            get { return notification; }
            set { notification = value; }
        }

        [XmlAttribute("BucketAmt")]
        public virtual String BucketAmt
        {
            get { return bucketamt; }
            set { bucketamt = value; }
        }

        [XmlAttribute("BucketFee")]
        public virtual String BucketFee
        {
            get { return bucketfee; }
            set { bucketfee = value; }
        }

        [XmlAttribute("JurnalSeq1")]
        public virtual String JurnalSeq1
        {
            get { return jurnalseq1; }
            set { jurnalseq1 = value; }
        }

        [XmlAttribute("JurnalSeq2")]
        public virtual String JurnalSeq2
        {
            get { return jurnalseq2; }
            set { jurnalseq2 = value; }
        }

        [XmlAttribute("NextProcessor")]
        public virtual String NextProcessor
        {
            get { return nextprocessor; }
            set { nextprocessor = value; }
        }

        [XmlAttribute("ChargeType")]
        public virtual String ChargeType
        {
            get { return chargetype; }
            set { chargetype = value; }
        }

        [XmlAttribute("Client")]
        public virtual int Client
        {
            get { return client; }
            set { client = value; }
        }

        [XmlAttribute("BookingOffice")]
        public virtual String BookingOffice
        {
            get { return _bookingoffice; }
            set { _bookingoffice = value; }
        }

        [XmlAttribute("Dke")]
        public virtual String Dke
        {
            get { return dke; }
            set { dke = value; }
        }

        [XmlAttribute("Seq")]
        public virtual String Seq
        {
            get { return seq; }
            set { seq = value; }
        }
    }
}
