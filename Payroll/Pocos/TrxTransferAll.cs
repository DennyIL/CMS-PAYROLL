using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxTransferAll : AbstractTransaction
    {
        private String autoreffno;
        [XmlAttribute("AutoRefNo")]
        public virtual String AutoRefNo
        {
            get { return autoreffno; }
            set { autoreffno = value; }
        }

        private String trxcode;
        [XmlAttribute("TrxCode")]
        public virtual String TrxCode
        {
            get { return trxcode; }
            set { trxcode = value; }
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

        private String debitaccbrinetname;
        [XmlAttribute("DebitAccBrinetName")]
        public virtual String DebitAccBrinetName
        {
            get { return debitaccbrinetname; }
            set { debitaccbrinetname = value; }
        }


        private String debitaccaddress;
        [XmlAttribute("DebitAccAddress")]
        public virtual String DebitAccAddress
        {
            get { return debitaccaddress; }
            set { debitaccaddress = value; }
        }


        private String benacc;
        [XmlAttribute("BenAcc")]
        public virtual String BenAcc
        {
            get { return benacc; }
            set { benacc = value; }
        }

        private String benacccoded;
        [XmlAttribute("BenAccCoded")]
        public virtual String BenAccCoded
        {
            get { return benacccoded; }
            set { benacccoded = value; }
        }

        private String benCurr;
        [XmlAttribute("BenCurr")]
        public virtual String BenCurr
        {
            get { return benCurr; }
            set { benCurr = value; }
        }

        private String benAccType;
        [XmlAttribute("BenAccType")]
        public virtual String BenAccType
        {
            get { return benAccType; }
            set { benAccType = value; }
        }

        private String benaccname;
        [XmlAttribute("BenAccName")]
        public virtual String BenAccName
        {
            get { return benaccname; }
            set { benaccname = value; }
        }

        private String benaddress;
        [XmlAttribute("BenAccAddress")]
        public virtual String BenAccAddress
        {
            get { return benaddress; }
            set { benaddress = value; }
        }

        private String notification;
        [XmlAttribute("Notification")]
        public virtual String Notification
        {
            get { return notification; }
            set { notification = value; }
        }

        private String usebic;
        [XmlAttribute("UseBIC")]
        public virtual String UseBIC
        {
            get { return usebic; }
            set { usebic = value; }
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

        private String useib;
        [XmlAttribute("UseIB")]
        public virtual String UseIB
        {
            get { return useib; }
            set { useib = value; }
        }

        private String interbankidentifier;
        [XmlAttribute("InterBankIdentifier")]
        public virtual String InterBankIdentifier
        {
            get { return interbankidentifier; }
            set { interbankidentifier = value; }
        }

        private String interbankname;
        [XmlAttribute("InterBankName")]
        public virtual String InterBankName
        {
            get { return interbankname; }
            set { interbankname = value; }
        }

        private String benrel;
        [XmlAttribute("BenRelation")]
        public virtual String BenRelation
        {
            get { return benrel; }
            set { benrel = value; }
        }

        private String bencat;
        [XmlAttribute("BenCategory")]
        public virtual String BenCategory
        {
            get { return bencat; }
            set { bencat = value; }
        }

        private String currency;
        [XmlAttribute("Currency")]
        public virtual String Currency
        {
            get { return currency; }
            set { currency = value; }
        }

        private Double amount;
        [XmlAttribute("Amount")]
        public virtual Double Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        private String chargetype;
        [XmlAttribute("ChargeType")]
        public virtual String ChargeType
        {
            get { return chargetype; }
            set { chargetype = value; }
        }

        private String chargecurrency;
        [XmlAttribute("ChargeCurrency")]
        public virtual String ChargeCurrency
        {
            get { return chargecurrency; }
            set { chargecurrency = value; }
        }

        private Double chargeamount;
        [XmlAttribute("ChargeAmount")]
        public virtual Double ChargeAmount
        {
            get { return chargeamount; }
            set { chargeamount = value; }
        }

        private Double chargeouramount;
        [XmlAttribute("ChargeOurAmount")]
        public virtual Double ChargeOurAmount
        {
            get { return chargeouramount; }
            set { chargeouramount = value; }
        }

        private Double chargebenamount;
        [XmlAttribute("ChargeBenAmount")]
        public virtual Double ChargeBenAmount
        {
            get { return chargebenamount; }
            set { chargebenamount = value; }
        }

        private String vouchercode;
        [XmlAttribute("VoucherCode")]
        public virtual String VoucherCode
        {
            get { return vouchercode; }
            set { vouchercode = value; }
        }

        private String valuedate;
        [XmlAttribute("ValueDate")]
        public virtual String ValueDate
        {
            get { return valuedate; }
            set { valuedate = value; }
        }


        private String sanditrxbi;
        [XmlAttribute("SandiTrxBI")]
        public virtual String SandiTrxBI
        {
            get { return sanditrxbi; }
            set { sanditrxbi = value; }
        }

        private String trxremark;
        [XmlAttribute("TrxRemark")]
        public virtual String TrxRemark
        {
            get { return trxremark; }
            set { trxremark = value; }
        }

        private String authnum;
        [XmlAttribute("AuthorityNum")]
        public virtual String AuthorityNum
        {
            get { return authnum; }
            set { authnum = value; }
        }

        private String extaccid;
        [XmlAttribute("ExtAccId")]
        public virtual String ExtAccId
        {
            get { return extaccid; }
            set { extaccid = value; }
        }


        private String rejecter;
        [XmlAttribute("Rejecter")]
        public virtual String Rejecter
        {
            get { return rejecter; }
            set { rejecter = value; }
        }


        private String rejectermode;
        [XmlAttribute("RejecterMode")]
        public virtual String RejecterMode
        {
            get { return rejectermode; }
            set { rejectermode = value; }
        }


        private String rejecter_level;
        [XmlAttribute("RejecterLevel")]
        public virtual String RejecterLevel
        {
            get { return rejecter_level; }
            set { rejecter_level = value; }
        }

        private String comment;
        [XmlAttribute("Comment")]
        public virtual String Comment
        {
            get { return comment; }
            set { comment = value; }
        }
        
        private String rtgstrxref;
        [XmlAttribute("RtgsTrxRef")]
        public virtual String RtgsTrxRef
        {
            get { return rtgstrxref; }
            set { rtgstrxref = value; }
        }
    }
}
