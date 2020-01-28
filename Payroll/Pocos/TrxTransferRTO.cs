using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxTransferRTO : AbstractTransaction
    {
        private Int32 id;
        private int clientid;
        private String trxid;
        private DateTime trxdate;
        private String debitaccount;
        private String debitaccounttype;
        private String debitaccountname;
        private Double debitamount;
        private String debitcurrency;
        private String creditaccount;
        private String creditaccountname;
        private String beneficiarybankcode;
        private String beneficiarybankname;
        private Double creditamount;
        private String creditcurrency;
        private Double feeamount;
        private String feecurrency;
        private String trxremark;
        private String status;
        private DateTime creationdate;
        private DateTime lastupdate;
        private String maker;
        private String makedate;
        private String checker;
        private String checkdate;
        private String approver;
        private String approvedate;
        private String refnumber;
        private String trxseqnum;
        private String schedule;
        private String notification;
        private String chargeType;
        private String bucketfee;
        private String jseq1;
        private String jseq2;

        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("ClientID")]
        public virtual int ClientID
        {
            get { return clientid; }
            set { clientid = value; }
        }

        [XmlAttribute("TrxID")]
        public virtual String TrxID
        {
            get { return trxid; }
            set { trxid = value; }
        }

        [XmlAttribute("TrxDate")]
        public virtual DateTime TrxDate
        {
            get { return trxdate; }
            set { trxdate = value; }
        }

        [XmlAttribute("DebitAccount")]
        public virtual String DebitAccount
        {
            get { return debitaccount; }
            set { debitaccount = value; }
        }

        [XmlAttribute("DebitAccountType")]
        public virtual String DebitAccountType
        {
            get { return debitaccounttype; }
            set { debitaccounttype = value; }
        }

        [XmlAttribute("DebitAccountName")]
        public virtual String DebitAccountName
        {
            get { return debitaccountname; }
            set { debitaccountname = value; }
        }

        [XmlAttribute("DebitAmount")]
        public virtual Double DebitAmount
        {
            get { return debitamount; }
            set { debitamount = value; }
        }

        [XmlAttribute("DebitCurrency")]
        public virtual String DebitCurrency
        {
            get { return debitcurrency; }
            set { debitcurrency = value; }
        }

        [XmlAttribute("CreditAccount")]
        public virtual String CreditAccount
        {
            get { return creditaccount; }
            set { creditaccount = value; }
        }

        [XmlAttribute("CreditAccountName")]
        public virtual String CreditAccountName
        {
            get { return creditaccountname; }
            set { creditaccountname = value; }
        }

        [XmlAttribute("BeneficiaryBankCode")]
        public virtual String BeneficiaryBankCode
        {
            get { return beneficiarybankcode; }
            set { beneficiarybankcode = value; }
        }

        [XmlAttribute("BeneficiaryBankName")]
        public virtual String BeneficiaryBankName
        {
            get { return beneficiarybankname; }
            set { beneficiarybankname = value; }
        }

        [XmlAttribute("CreditAmount")]
        public virtual Double CreditAmount
        {
            get { return creditamount; }
            set { creditamount = value; }
        }

        [XmlAttribute("CreditCurrency")]
        public virtual String CreditCurrency
        {
            get { return creditcurrency; }
            set { creditcurrency = value; }
        }

        [XmlAttribute("FeeAmount")]
        public virtual Double FeeAmount
        {
            get { return feeamount; }
            set { feeamount = value; }
        }

        [XmlAttribute("FeeCurrency")]
        public virtual String FeeCurrency
        {
            get { return feecurrency; }
            set { feecurrency = value; }
        }

        [XmlAttribute("TrxRemark")]
        public virtual String TrxRemark
        {
            get { return trxremark; }
            set { trxremark = value; }
        }

        [XmlAttribute("Status")]
        public virtual String Status
        {
            get { return status; }
            set { status = value; }
        }

        [XmlAttribute("CreationDate")]
        public virtual DateTime CreationDate
        {
            get { return creationdate; }
            set { creationdate = value; }
        }

        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return lastupdate; }
            set { lastupdate = value; }
        }

        [XmlAttribute("Maker")]
        public virtual String Maker
        {
            get { return maker; }
            set { maker = value; }
        }

        [XmlAttribute("MakeDate")]
        public virtual String MakeDate
        {
            get { return makedate; }
            set { makedate = value; }
        }

        [XmlAttribute("Checker")]
        public virtual String Checker
        {
            get { return checker; }
            set { checker = value; }
        }

        [XmlAttribute("CheckDate")]
        public virtual String CheckDate
        {
            get { return checkdate; }
            set { checkdate = value; }
        }

        [XmlAttribute("Approver")]
        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        [XmlAttribute("ApproveDate")]
        public virtual String ApproveDate
        {
            get { return approvedate; }
            set { approvedate = value; }
        }

        [XmlAttribute("RefNumber")]
        public virtual String RefNumber
        {
            get { return refnumber; }
            set { refnumber = value; }
        }

        [XmlAttribute("TrxSeqNum")]
        public virtual String TrxSeqNum
        {
            get { return trxseqnum; }
            set { trxseqnum = value; }
        }

        [XmlAttribute("Schedule")]
        public virtual String Schedule
        {
            get { return schedule; }
            set { schedule = value; }
        }

        [XmlAttribute("Notification")]
        public virtual String Notification
        {
            get { return notification; }
            set { notification = value; }
        }

        [XmlAttribute("ChargeType")]
        public virtual String ChargeType
        {
            get { return chargeType; }
            set { chargeType = value; }
        }

        [XmlAttribute("BucketFee")]
        public virtual String BucketFee
        {
            get { return bucketfee; }
            set { bucketfee = value; }
        }

        [XmlAttribute("JournalSeq1")]
        public virtual String JournalSeq1
        {
            get { return jseq1; }
            set { jseq1 = value; }
        }

        [XmlAttribute("JournalSeq2")]
        public virtual String JournalSeq2
        {
            get { return jseq2; }
            set { jseq2 = value; }
        }
    }
}
