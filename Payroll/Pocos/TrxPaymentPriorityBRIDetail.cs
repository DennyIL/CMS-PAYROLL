using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxPaymentPriorityBRIDetail : AbstractTransaction
    {


        private Int32 id;
        private String custrefno;
        private String cmsrefno;
        private String fxcode;
        private Int32 parentid;
        private Int32 clientid;
        private DateTime lastupdate;
        private Int32 makerwork;
        private Int32 makertotal;
        private Int32 checkwork;
        private Int32 checktotal;
        private Int32 approvework;
        private Int32 approvetotal;
        private String uploadinfo;
        private String checkerinfo;
        private String approverinfo;
        private String instructioncode;
        private String trxcode;
        private DateTime valuedate;
        private String currency;
        private Double amount;
        private Double amountdebet;
        private String kursjual;
        private String kursbeli;
        private String trxremark;
        private String misctobankinfo;
        private String chargetype;
        private String chargecurrency;
        private Double chargeamount;
        private String debitacc;
        private String debitaccname;
        private String debitaccaddress;
        private String debitacccountrycode;
        private String benacc;
        private String benaccname;
        private String benaccnamebrinets;
        private String benaccaddress;
        private String benacccountrycode;
        private String benbanktype;
        private String benbankidentifier;
        private String benbankname;
        private String benbankaddress;
        private String benbankcountrycode;
        private String interbanktype;
        private String interbankidentifier;
        private String interbankname;
        private String interbankaddress;
        private String interbankcountrycode;
        private String notification;
        private Int16 status;
        private String description;
        private String debitcurr;
        private String sanditrxbi;
        private String bencategory;
        private String benrelation;
        private String nextprocessor;
        private double feeour;
        private Int16 brivakreditis;
        private Int16 spesialworkflow;
        private Int16 swiflag;
        private int reportflag;
        private DateTime reportgeneratetime;
        private String _bookingoffice;
        private Int16 corvadebitis;

        [XmlAttribute("CorvaDebitIs")]
        public virtual Int16 CorvaDebitIs
        {
            get { return corvadebitis; }
            set { corvadebitis = value; }
        }

        [XmlAttribute("BrivaKreditis")]
        public virtual Int16 BrivaKreditis
        {
            get { return brivakreditis; }
            set { brivakreditis = value; }
        }


        private int brivatranid;
        [XmlAttribute("BrivaTranId")]
        public virtual int BrivaTranId
        {
            get { return brivatranid; }
            set { brivatranid = value; }
        }

        private String brivarekgiro;
        [XmlAttribute("BrivaRekGiro")]
        public virtual String BrivaRekGiro
        {
            get { return brivarekgiro; }
            set { brivarekgiro = value; }
        }

        private String brivacustname;
        [XmlAttribute("BrivaCustName")]
        public virtual String BrivaCustName
        {
            get { return brivacustname; }
            set { brivacustname = value; }
        }

        private String benaccpostalcode;
        [XmlAttribute("BenAccPostalCode")]
        public virtual String BenAccPostalCode
        {
            get { return benaccpostalcode; }
            set { benaccpostalcode = value; }
        }

        private String benaccphone;
        [XmlAttribute("BenAccPhone")]
        public virtual String BenAccPhone
        {
            get { return benaccphone; }
            set { benaccphone = value; }
        }

        private String bucketamt;
        [XmlAttribute("BucketAmt")]
        public virtual String BucketAmt
        {
            get { return bucketamt; }
            set { bucketamt = value; }
        }

        private String bucketfee;
        [XmlAttribute("BucketFee")]
        public virtual String BucketFee
        {
            get { return bucketfee; }
            set { bucketfee = value; }
        }

        private String oriError1;
        [XmlAttribute("ErrorCode1")]
        public virtual String ErrorCode1
        {
            get { return oriError1; }
            set { oriError1 = value; }
        }

        private String journalseq1;
        [XmlAttribute("JournalSeq1")]
        public virtual String JournalSeq1
        {
            get { return journalseq1; }
            set { journalseq1 = value; }
        }

        private String oriError2;
        [XmlAttribute("ErrorCode2")]
        public virtual String ErrorCode2
        {
            get { return oriError2; }
            set { oriError2 = value; }
        }

        private String journalseq2;
        [XmlAttribute("JournalSeq2")]
        public virtual String JournalSeq2
        {
            get { return journalseq2; }
            set { journalseq2 = value; }
        }

        [XmlAttribute("Id")]
        public virtual int Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return lastupdate; }
            set { lastupdate = value; }
        }

        [XmlAttribute("CustRefNo")]
        public virtual String CustRefNo
        {
            get { return custrefno; }
            set { custrefno = value; }
        }

        [XmlAttribute("CmsRefNo")]
        public virtual String CmsRefNo
        {
            get { return cmsrefno; }
            set { cmsrefno = value; }
        }
        [XmlAttribute("FxCode")]
        public virtual String FxCode
        {
            get { return fxcode; }
            set { fxcode = value; }
        }
        [XmlAttribute("ParentId")]
        public virtual int ParentId
        {
            get { return parentid; }
            set { parentid = value; }
        }

        [XmlAttribute("ClientId")]
        public virtual int ClientId
        {
            get { return clientid; }
            set { clientid = value; }
        }

        [XmlAttribute("MakerWork")]
        public virtual int MakerWork
        {
            get { return makerwork; }
            set { makerwork = value; }
        }
        [XmlAttribute("MakerTotal")]
        public virtual int MakerTotal
        {
            get { return makertotal; }
            set { makertotal = value; }
        }

        [XmlAttribute("CheckWork")]
        public virtual int CheckWork
        {
            get { return checkwork; }
            set { checkwork = value; }
        }
        [XmlAttribute("CheckTotal")]
        public virtual int CheckTotal
        {
            get { return checktotal; }
            set { checktotal = value; }
        }

        [XmlAttribute("ApproveWork")]
        public virtual int ApproveWork
        {
            get { return approvework; }
            set { approvework = value; }
        }
        [XmlAttribute("ApproveWTotal")]
        public virtual int ApproveTotal
        {
            get { return approvetotal; }
            set { approvetotal = value; }
        }
        [XmlAttribute("UploadInfo")]
        public virtual String UploadInfo
        {
            get { return uploadinfo; }
            set { uploadinfo = value; }
        }
        [XmlAttribute("CheckerInfo")]
        public virtual String CheckerInfo
        {
            get { return checkerinfo; }
            set { checkerinfo = value; }
        }
        [XmlAttribute("ApproverInfo")]
        public virtual String ApproverInfo
        {
            get { return approverinfo; }
            set { approverinfo = value; }
        }
        [XmlAttribute("InstructionCode")]
        public virtual String InstructionCode
        {
            get { return instructioncode; }
            set { instructioncode = value; }
        }
        [XmlAttribute("TrxCode")]
        public virtual String TrxCode
        {
            get { return trxcode; }
            set { trxcode = value; }
        }
        [XmlAttribute("ValueDate")]
        public virtual DateTime ValueDate
        {
            get { return valuedate; }
            set { valuedate = value; }
        }
        [XmlAttribute("Currency")]
        public virtual String Currency
        {
            get { return currency; }
            set { currency = value; }
        }
        [XmlAttribute("Amount")]
        public virtual Double Amount
        {
            get { return amount; }
            set { amount = value; }
        }
        [XmlAttribute("AmountDebet")]
        public virtual Double AmountDebet
        {
            get { return amountdebet; }
            set { amountdebet = value; }
        }
        [XmlAttribute("KursJual")]
        public virtual String KursJual
        {
            get { return kursjual; }
            set { kursjual = value; }
        }
        [XmlAttribute("KursBeli")]
        public virtual String KursBeli
        {
            get { return kursbeli; }
            set { kursbeli = value; }
        }
        [XmlAttribute("TrxRemark")]
        public virtual String TrxRemark
        {
            get { return trxremark; }
            set { trxremark = value; }
        }
        [XmlAttribute("MiscToBankInfo")]
        public virtual String MiscToBankInfo
        {
            get { return misctobankinfo; }
            set { misctobankinfo = value; }
        }
        [XmlAttribute("ChargeType")]
        public virtual String ChargeType
        {
            get { return chargetype; }
            set { chargetype = value; }
        }
        [XmlAttribute("ChargeCurrency")]
        public virtual String ChargeCurrency
        {
            get { return chargecurrency; }
            set { chargecurrency = value; }
        }
        [XmlAttribute("ChargeAmount")]
        public virtual Double ChargeAmount
        {
            get { return chargeamount; }
            set { chargeamount = value; }
        }
        [XmlAttribute("DebitAcc")]
        public virtual String DebitAcc
        {
            get { return debitacc; }
            set { debitacc = value; }
        }
        [XmlAttribute("DebitAccName")]
        public virtual String DebitAccName
        {
            get { return debitaccname; }
            set { debitaccname = value; }
        }
        [XmlAttribute("DebitAccAddress")]
        public virtual String DebitAccAddress
        {
            get { return debitaccaddress; }
            set { debitaccaddress = value; }
        }
        [XmlAttribute("DebitAccCountryCode")]
        public virtual String DebitAccCountryCode
        {
            get { return debitacccountrycode; }
            set { debitacccountrycode = value; }
        }
        [XmlAttribute("BenAcc")]
        public virtual String BenAcc
        {
            get { return benacc; }
            set { benacc = value; }
        }
        [XmlAttribute("BenAccName")]
        public virtual String BenAccName
        {
            get { return benaccname; }
            set { benaccname = value; }
        }
        [XmlAttribute("BenAccNameBrinets")]
        public virtual String BenAccNameBrinets
        {
            get { return benaccnamebrinets; }
            set { benaccnamebrinets = value; }
        }
        [XmlAttribute("BenAccAddress")]
        public virtual String BenAccAddress
        {
            get { return benaccaddress; }
            set { benaccaddress = value; }
        }
        [XmlAttribute("BenAccCountryCode")]
        public virtual String BenAccCountryCode
        {
            get { return benacccountrycode; }
            set { benacccountrycode = value; }
        }
        [XmlAttribute("BenBankType")]
        public virtual String BenBankType
        {
            get { return benbanktype; }
            set { benbanktype = value; }
        }
        [XmlAttribute("BenBankIdentifier")]
        public virtual String BenBankIdentifier
        {
            get { return benbankidentifier; }
            set { benbankidentifier = value; }
        }
        [XmlAttribute("BenBankName")]
        public virtual String BenBankName
        {
            get { return benbankname; }
            set { benbankname = value; }
        }
        [XmlAttribute("BenBankAddress")]
        public virtual String BenBankAddress
        {
            get { return benbankaddress; }
            set { benbankaddress = value; }
        }
        [XmlAttribute("BenBankCountryCode")]
        public virtual String BenBankCountryCode
        {
            get { return benbankcountrycode; }
            set { benbankcountrycode = value; }
        }
        [XmlAttribute("InterBankType")]
        public virtual String InterBankType
        {
            get { return interbanktype; }
            set { interbanktype = value; }
        }
        [XmlAttribute("InterBankIdentifier")]
        public virtual String InterBankIdentifier
        {
            get { return interbankidentifier; }
            set { interbankidentifier = value; }
        }
        [XmlAttribute("InterBankName")]
        public virtual String InterBankName
        {
            get { return interbankname; }
            set { interbankname = value; }
        }
        [XmlAttribute("InterBankAddress")]
        public virtual String InterBankAddress
        {
            get { return interbankaddress; }
            set { interbankaddress = value; }
        }
        [XmlAttribute("InterBankCountryCode")]
        public virtual String InterBankCountryCode
        {
            get { return interbankcountrycode; }
            set { interbankcountrycode = value; }
        }
        [XmlAttribute("Notification")]
        public virtual String Notification
        {
            get { return notification; }
            set { notification = value; }
        }
        [XmlAttribute("Status")]
        public virtual Int16 Status
        {
            get { return status; }
            set { status = value; }
        }
        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }
        [XmlAttribute("DebitCurr")]
        public virtual String DebitCurr
        {
            get { return debitcurr; }
            set { debitcurr = value; }
        }
        [XmlAttribute("SandiTrxBI")]
        public virtual String SandiTrxBI
        {
            get { return sanditrxbi; }
            set { sanditrxbi = value; }
        }
        [XmlAttribute("BenCategory")]
        public virtual String BenCategory
        {
            get { return bencategory; }
            set { bencategory = value; }
        }
        [XmlAttribute("BenRelation")]
        public virtual String BenRelation
        {
            get { return benrelation; }
            set { benrelation = value; }
        }

        [XmlAttribute("NextProcessor")]
        public virtual String NextProcessor
        {
            get { return nextprocessor; }
            set { nextprocessor = value; }
        }
        [XmlAttribute("FeeOur")]
        public virtual Double FeeOur
        {
            get { return feeour; }
            set { feeour = value; }
        }

        [XmlAttribute("SpesialWorkflow")]
        public virtual Int16 SpesialWorkflow
        {
            get { return spesialworkflow; }
            set { spesialworkflow = value; }
        }

        [XmlAttribute("SWIFTFlag")]
        public virtual Int16 SWIFTFlag
        {
            get { return swiflag; }
            set { swiflag = value; }
        }

        [XmlAttribute("ReportFlag")]
        public virtual int ReportFlag
        {
            get { return reportflag; }
            set { reportflag = value; }
        }

        [XmlAttribute("ReportGenerateTime")]
        public virtual DateTime ReportGenerateTime
        {
            get { return reportgeneratetime; }
            set { reportgeneratetime = value; }
        }

        [XmlAttribute("BookingOffice")]
        public virtual String BookingOffice
        {
            get { return _bookingoffice; }
            set { _bookingoffice = value; }
        }

        private String schedule;
        [XmlAttribute("Schedule")]
        public virtual String Schedule
        {
            get { return schedule; }
            set { schedule = value; }
        }

        private String creditcurr;
        [XmlAttribute("CreditCurr")]
        public virtual String CreditCurr
        {
            get { return creditcurr; }
            set { creditcurr = value; }
        }

        private String debitacctype;
        [XmlAttribute("DebitAccType")]
        public virtual String DebitAccType
        {
            get { return debitacctype; }
            set { debitacctype = value; }
        }

        private String benacctype;
        [XmlAttribute("BenAccType")]
        public virtual String BenAccType
        {
            get { return benacctype; }
            set { benacctype = value; }
        }

        private Double brivaBen;
        [XmlAttribute("BrivaBen")]
        public virtual Double BrivaBen
        {
            get { return brivaBen; }
            set { brivaBen = value; }
        }

        private Double brivaOur;
        [XmlAttribute("BrivaOur")]
        public virtual Double BrivaOur
        {
            get { return brivaOur; }
            set { brivaOur = value; }
        }

        private String reffCode;
        [XmlAttribute("ReffCode")]
        public virtual String ReffCode
        {
            get { return reffCode; }
            set { reffCode = value; }
        }
    }
}
