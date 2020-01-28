using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxMassFTDetail : AbstractTransaction
    {

        private Int32 id;
        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        private Int32 parentid;
        [XmlAttribute("ParentId")]
        public virtual Int32 ParentId
        {
            get { return parentid; }
            set { parentid = value; }
        }

        private String debitAccount;
        [XmlAttribute("DebitAccount")]
        public virtual String DebitAccount
        {
            get { return debitAccount; }
            set { debitAccount = value; }
        }

        private String debitName;
        [XmlAttribute("DebitName")]
        public virtual String DebitName
        {
            get { return debitName; }
            set { debitName = value; }
        }

        private String debitCurrency;
        [XmlAttribute("DebitCurrency")]
        public virtual String DebitCurrency
        {
            get { return debitCurrency; }
            set { debitCurrency = value; }
        }

        private String creditAccount;
        [XmlAttribute("CreditAccount")]
        public virtual String CreditAccount
        {
            get { return creditAccount; }
            set { creditAccount = value; }
        }

        private String creditName;
        [XmlAttribute("CreditName")]
        public virtual String CreditName
        {
            get { return creditName; }
            set { creditName = value; }
        }

        private String creditCurrency;
        [XmlAttribute("CreditCurrency")]
        public virtual String CreditCurrency
        {
            get { return creditCurrency; }
            set { creditCurrency = value; }
        }

        private Double amount;
        [XmlAttribute("Amount")]
        public virtual Double Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        private Int32 status;
        [XmlAttribute("Status")]
        public virtual Int32 Status
        {
            get { return status; }
            set { status = value; }
        }

        private String remarks;
        [XmlAttribute("Remarks")]
        public virtual String Remarks
        {
            get { return remarks; }
            set { remarks = value; }
        }

        private String description;
        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        private Double feeAmount;
        [XmlAttribute("FeeAmount")]
        public virtual Double FeeAmount
        {
            get { return feeAmount; }
            set { feeAmount = value; }
        }

        private String feeCurrency;
        [XmlAttribute("FeeCurrency")]
        public virtual String FeeCurrency
        {
            get { return feeCurrency; }
            set { feeCurrency = value; }
        }

        private Double feeOur;
        [XmlAttribute("FeeOur")]
        public virtual Double FeeOur
        {
            get { return feeOur; }
            set { feeOur = value; }
        }

        private Double feeDebet;
        [XmlAttribute("FeeDebet")]
        public virtual Double FeeDebet
        {
            get { return feeDebet; }
            set { feeDebet = value; }
        }

        private Double feeBen;
        [XmlAttribute("FeeBen")]
        public virtual Double FeeBen
        {
            get { return feeBen; }
            set { feeBen = value; }
        }

        private Double feeCredit;
        [XmlAttribute("FeeCredit")]
        public virtual Double FeeCredit
        {
            get { return feeCredit; }
            set { feeCredit = value; }
        }

        private String instCur;
        [XmlAttribute("InstCur")]
        public virtual String InstCur
        {
            get { return instCur; }
            set { instCur = value; }
        }

        private String voucherCode;
        [XmlAttribute("VoucherCode")]
        public virtual String VoucherCode
        {
            get { return voucherCode; }
            set { voucherCode = value; }
        }

        private String notification;
        [XmlAttribute("Notification")]
        public virtual String Notification
        {
            get { return notification; }
            set { notification = value; }
        }

        private String sandiTrxBi;
        [XmlAttribute("SandiTrxBi")]
        public virtual String SandiTrxBi
        {
            get { return sandiTrxBi; }
            set { sandiTrxBi = value; }
        }

        private String bucketAmt;
        [XmlAttribute("BucketAmt")]
        public virtual String BucketAmt
        {
            get { return bucketAmt; }
            set { bucketAmt = value; }
        }

        private String bucketFee;
        [XmlAttribute("BucketFee")]
        public virtual String BucketFee
        {
            get { return bucketFee; }
            set { bucketFee = value; }
        }

        private String jurnalseq1;
        [XmlAttribute("JurnalSeq1")]
        public virtual String JurnalSeq1
        {
            get { return jurnalseq1; }
            set { jurnalseq1 = value; }
        }

        private String jurnalseq2;
        [XmlAttribute("JurnalSeq2")]
        public virtual String JurnalSeq2
        {
            get { return jurnalseq2; }
            set { jurnalseq2 = value; }
        }

        private String trxRate;
        [XmlAttribute("TrxRate")]
        public virtual String TrxRate
        {
            get { return trxRate; }
            set { trxRate = value; }
        }

        private Int32 isBrivaKredit;
        [XmlAttribute("IsBrivaKredit")]
        public virtual Int32 IsBrivaKredit
        {
            get { return isBrivaKredit; }
            set { isBrivaKredit = value; }
        }

        private String trxReff;
        [XmlAttribute("TrxReff")]
        public virtual String TrxReff
        {
            get { return trxReff; }
            set { trxReff = value; }
        }

        private DateTime runningDate;
        [XmlAttribute("RunningDate")]
        public virtual DateTime RunningDate
        {
            get { return runningDate; }
            set { runningDate = value; }
        }

        private String debitAccountNameBrinets;
        [XmlAttribute("DebitAccountNameBrinets")]
        public virtual String DebitAccountNameBrinets
        {
            get { return debitAccountNameBrinets; }
            set { debitAccountNameBrinets = value; }
        }

        private String debitAccountNameCode;
        [XmlAttribute("DebitAccountNameCode")]
        public virtual String DebitAccountNameCode
        {
            get { return debitAccountNameCode; }
            set { debitAccountNameCode = value; }
        }

        private String debitAccountType;
        [XmlAttribute("DebitAccountType")]
        public virtual String DebitAccountType
        {
            get { return debitAccountType; }
            set { debitAccountType = value; }
        }

        private Double debitAmount;
        [XmlAttribute("DebitAmount")]
        public virtual Double DebitAmount
        {
            get { return debitAmount; }
            set { debitAmount = value; }
        }

        private String creditAccountNameBrinets;
        [XmlAttribute("CreditAccountNameBrinets")]
        public virtual String CreditAccountNameBrinets
        {
            get { return creditAccountNameBrinets; }
            set { creditAccountNameBrinets = value; }
        }

        private String creditAccountType;
        [XmlAttribute("CreditAccountType")]
        public virtual String CreditAccountType
        {
            get { return creditAccountType; }
            set { creditAccountType = value; }
        }

        private Double creditAmount;
        [XmlAttribute("CreditAmount")]
        public virtual Double CreditAmount
        {
            get { return creditAmount; }
            set { creditAmount = value; }
        }

        private String chargeType;
        [XmlAttribute("ChargeType")]
        public virtual String ChargeType
        {
            get { return chargeType; }
            set { chargeType = value; }
        }

        private String nextProcessor;
        [XmlAttribute("NextProcessor")]
        public virtual String NextProcessor
        {
            get { return nextProcessor; }
            set { nextProcessor = value; }
        }

        private int client;
        [XmlAttribute("Client")]
        public virtual int Client
        {
            get { return client; }
            set { client = value; }
        }

        private int isSuspectMinus;
        [XmlAttribute("IsSuspectMinus")]
        public virtual int IsSuspectMinus
        {
            get { return isSuspectMinus; }
            set { isSuspectMinus = value; }
        }
    }
}
