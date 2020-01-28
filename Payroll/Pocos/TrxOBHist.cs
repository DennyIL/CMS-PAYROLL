using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxOBHist : AbstractTransaction
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

        private Int32 clientid;
        [XmlAttribute("ClientId")]
        public virtual Int32 ClientId
        {
            get { return clientid; }
            set { clientid = value; }
        }

        private String trxcode;
        [XmlAttribute("TrxCode")]
        public virtual String TrxCode
        {
            get { return trxcode; }
            set { trxcode = value; }
        }

        private Int32 trxtype;
        [XmlAttribute("TrxType")]
        public virtual Int32 TrxType
        {
            get { return trxtype; }
            set { trxtype = value; }
        }

        private String trxactid;
        [XmlAttribute("TrxActId")]
        public virtual String TrxActId
        {
            get { return trxactid; }
            set { trxactid = value; }
        }

        private String valuedate;
        [XmlAttribute("ValueDate")]
        public virtual String ValueDate
        {
            get { return valuedate; }
            set { valuedate = value; }
        }

        private String creditcur;
        [XmlAttribute("CreditCur")]
        public virtual String CreditCur
        {
            get { return creditcur; }
            set { creditcur = value; }
        }

        private Double creditamt;
        [XmlAttribute("CreditAmt")]
        public virtual Double CreditAmt
        {
            get { return creditamt; }
            set { creditamt = value; }
        }

        private String kursbelitrx;
        [XmlAttribute("KursBeliTrx")]
        public virtual String KursBeliTrx
        {
            get { return kursbelitrx; }
            set { kursbelitrx = value; }
        }

        private String kursjualtrx;
        [XmlAttribute("KursJualTrx")]
        public virtual String KursJualTrx
        {
            get { return kursjualtrx; }
            set { kursjualtrx = value; }
        }

        private String debitcur;
        [XmlAttribute("DebitCur")]
        public virtual String DebitCur
        {
            get { return debitcur; }
            set { debitcur = value; }
        }

        private Double debitamt;
        [XmlAttribute("DebitAmt")]
        public virtual Double DebitAmt
        {
            get { return debitamt; }
            set { debitamt = value; }
        }

        private String basecur;
        [XmlAttribute("BaseCur")]
        public virtual String BaseCur
        {
            get { return basecur; }
            set { basecur = value; }
        }

        private Double baseamt;
        [XmlAttribute("BaseAmt")]
        public virtual Double BaseAmt
        {
            get { return baseamt; }
            set { baseamt = value; }
        }

        private String kursjualbase;
        [XmlAttribute("KursJualBase")]
        public virtual String KursJualBase
        {
            get { return kursjualbase; }
            set { kursjualbase = value; }
        }

        private String trxremark;
        [XmlAttribute("TrxRemark")]
        public virtual String TrxRemark
        {
            get { return trxremark; }
            set { trxremark = value; }
        }

        private String vouchercode;
        [XmlAttribute("VoucherCode")]
        public virtual String VoucherCode
        {
            get { return vouchercode; }
            set { vouchercode = value; }
        }

        private String chargecur;
        [XmlAttribute("ChargeCur")]
        public virtual String ChargeCur
        {
            get { return chargecur; }
            set { chargecur = value; }
        }

        private Double chargeamt;
        [XmlAttribute("ChargeAmt")]
        public virtual Double ChargeAmt
        {
            get { return chargeamt; }
            set { chargeamt = value; }
        }

        private String debitacc;
        [XmlAttribute("DebitAcc")]
        public virtual String DebitAcc
        {
            get { return debitacc; }
            set { debitacc = value; }
        }

        private String debitaccname;
        [XmlAttribute("DebitAccName")]
        public virtual String DebitAccName
        {
            get { return debitaccname; }
            set { debitaccname = value; }
        }

        private String benacc;
        [XmlAttribute("BenAcc")]
        public virtual String BenAcc
        {
            get { return benacc; }
            set { benacc = value; }
        }

        private String benaccname;
        [XmlAttribute("BenAccName")]
        public virtual String BenAccName
        {
            get { return benaccname; }
            set { benaccname = value; }
        }

        private String benbankidentifier;
        [XmlAttribute("BenBankIdentifier")]
        public virtual String BenBankIdentifier
        {
            get { return benbankidentifier; }
            set { benbankidentifier = value; }
        }

        private String benbankname;
        [XmlAttribute("BenBankName")]
        public virtual String BenBankName
        {
            get { return benbankname; }
            set { benbankname = value; }
        }

        private String makerinfo;
        [XmlAttribute("MakerInfo")]
        public virtual String MakerInfo
        {
            get { return makerinfo; }
            set { makerinfo = value; }
        }

        private String checkerinfo;
        [XmlAttribute("CheckerInfo")]
        public virtual String CheckerInfo
        {
            get { return checkerinfo; }
            set { checkerinfo = value; }
        }

        private String approverinfo;
        [XmlAttribute("ApproverInfo")]
        public virtual String ApproverInfo
        {
            get { return approverinfo; }
            set { approverinfo = value; }
        }
       
        private DateTime lastupdate;
        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return lastupdate; }
            set { lastupdate = value; }
        }

        private Int32 status;
        [XmlAttribute("Status")]
        public virtual Int32 Status
        {
            get { return status; }
            set { status = value; }
        }

        private String description;
        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }  
    }
}
