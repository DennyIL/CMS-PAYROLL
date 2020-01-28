using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
	[Serializable]
	[XmlRoot("TrxSwift")]
	public class TrxSwift : AbstractTransaction
	{

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

		private Int32 _id;
		[XmlAttribute("Id")]
		public virtual Int32 Id
		{
			get { return _id; }
			set { _id = value; }
		}

		private int _type;
		[XmlAttribute("Type")]
		public virtual int Type
		{
			get { return _type; }
			set { _type = value; }
		}

		private String _trxid;
		[XmlAttribute("TrxID")]
		public virtual String TrxID
		{
			get { return _trxid; }
			set { _trxid = value; }
		}

		private String _clientid;
		[XmlAttribute("ClientID")]
		public virtual String ClientID
		{
			get { return _clientid; }
			set { _clientid = value; }
		}

		private String _branchid;
		[XmlAttribute("BranchID")]
		public virtual String BranchID
		{
			get { return _branchid; }
			set { _branchid = value; }
		}

		private int _reffid;
		[XmlAttribute("ParentID")]
		public virtual int ParentID
		{
			get { return _reffid; }
			set { _reffid = value; }
		}

		private String _referencenum;
		[XmlAttribute("ReferenceNum")]
		public virtual String ReferenceNum
		{
			get { return _referencenum; }
			set { _referencenum = value; }
		}

		private String _valuedate;
		[XmlAttribute("ValueDate")]
		public virtual String ValueDate
		{
			get { return _valuedate; }
			set { _valuedate = value; }
		}

		private String _debitaccount;
		[XmlAttribute("DebitAccount")]
		public virtual String DebitAccount
		{
			get { return _debitaccount; }
			set { _debitaccount = value; }
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

		private String _creditaccountaddress2;
		[XmlAttribute("CreditAccountAddress2")]
		public virtual String CreditAccountAddress2
		{
			get { return _creditaccountaddress2; }
			set { _creditaccountaddress2 = value; }
		}

		private String _creditaccountaddress3;
		[XmlAttribute("CreditAccountAddress3")]
		public virtual String CreditAccountAddress3
		{
			get { return _creditaccountaddress3; }
			set { _creditaccountaddress3 = value; }
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

		private String _beneficiarybankbic;
		[XmlAttribute("BeneficiaryBankBIC")]
		public virtual String BeneficiaryBankBIC
		{
			get { return _beneficiarybankbic; }
			set { _beneficiarybankbic = value; }
		}

		private String _beneficiarybankname;
		[XmlAttribute("BeneficiaryBankName")]
		public virtual String BeneficiaryBankName
		{
			get { return _beneficiarybankname; }
			set { _beneficiarybankname = value; }
		}

		private String _beneficiarybankaddress;
		[XmlAttribute("BeneficiaryBankAddress")]
		public virtual String BeneficiaryBankAddress
		{
			get { return _beneficiarybankaddress; }
			set { _beneficiarybankaddress = value; }
		}

		private String _beneficiarybankaddress2;
		[XmlAttribute("BeneficiaryBankAddress2")]
		public virtual String BeneficiaryBankAddress2
		{
			get { return _beneficiarybankaddress2; }
			set { _beneficiarybankaddress2 = value; }
		}

		private String _beneficiarybankaddress3;
		[XmlAttribute("BeneficiaryBankAddress3")]
		public virtual String BeneficiaryBankAddress3
		{
			get { return _beneficiarybankaddress3; }
			set { _beneficiarybankaddress3 = value; }
		}

        private String _beneficiarybankcity;
        [XmlAttribute("BeneficiaryBankCity")]
        public virtual String BeneficiaryBankCity
        {
            get { return _beneficiarybankcity; }
            set { _beneficiarybankcity = value; }
        }

        private String _intermediarybankbic;
        [XmlAttribute("IntermediaryBankBIC")]
        public virtual String IntermediaryBankBIC
        {
            get { return _intermediarybankbic; }
            set { _intermediarybankbic = value; }
        }

        private String _intermediarybankname;
        [XmlAttribute("IntermediaryBankName")]
        public virtual String IntermediaryBankName
        {
            get { return _intermediarybankname; }
            set { _intermediarybankname = value; }
        }

        private String _intermediarybankaddress;
        [XmlAttribute("IntermediaryBankAddress")]
        public virtual String IntermediaryBankAddress
        {
            get { return _intermediarybankaddress; }
            set { _intermediarybankaddress = value; }
        }

        private String _intermediarybankaddress2;
        [XmlAttribute("IntermediaryBankAddress2")]
        public virtual String IntermediaryBankAddress2
        {
            get { return _intermediarybankaddress2; }
            set { _intermediarybankaddress2 = value; }
        }

        private String _intermediarybankaddress3;
        [XmlAttribute("IntermediaryBankAddress3")]
        public virtual String IntermediaryBankAddress3
        {
            get { return _intermediarybankaddress3; }
            set { _intermediarybankaddress3 = value; }
        }

        private String _intermediarybankcity;
        [XmlAttribute("IntermediaryBankCity")]
        public virtual String IntermediaryBankCity
        {
            get { return _intermediarybankcity; }
            set { _intermediarybankcity = value; }
        }

		private String _detailofcharge;
		[XmlAttribute("DetailOfCharge")]
		public virtual String DetailOfCharge
		{
			get { return _detailofcharge; }
			set { _detailofcharge = value; }
		}

		private Double _charge;
		[XmlAttribute("Charge")]
		public virtual Double Charge
		{
			get { return _charge; }
			set { _charge = value; }
		}

		private String _chargecurrency;
		[XmlAttribute("ChargeCurrency")]
		public virtual String ChargeCurrency
		{
			get { return _chargecurrency; }
			set { _chargecurrency = value; }
		}

		private Double _chargeia;
		[XmlAttribute("ChargeIA")]
		public virtual Double ChargeIA
		{
			get { return _chargeia; }
			set { _chargeia = value; }
		}

		private String _chargeiacurrency;
		[XmlAttribute("ChargeIACurrency")]
		public virtual String ChargeIACurrency
		{
			get { return _chargeiacurrency; }
			set { _chargeiacurrency = value; }
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

		private DateTime _creationtime;
		[XmlAttribute("CreationTime")]
		public virtual DateTime CreationTime
		{
			get { return _creationtime; }
			set { _creationtime = value; }
		}

		private DateTime _verifiedtime;
		[XmlAttribute("VerifiedTime")]
		public virtual DateTime VerifiedTime
		{
			get { return _verifiedtime; }
			set { _verifiedtime = value; }
		}

		private DateTime _approvedtime;
		[XmlAttribute("ApprovedTime")]
		public virtual DateTime ApprovedTime
		{
			get { return _approvedtime; }
			set { _approvedtime = value; }
		}

		private String _maker;
		[XmlAttribute("Maker")]
		public virtual String Maker
		{
			get { return _maker; }
			set { _maker = value; }
		}

		private String _checker;
		[XmlAttribute("Checker")]
		public virtual String Checker
		{
			get { return _checker; }
			set { _checker = value; }
		}

		private String _checkerworkflow;
		[XmlAttribute("CheckerWorkflow")]
		public virtual String CheckerWorkflow
		{
			get { return _checkerworkflow; }
			set { _checkerworkflow = value; }
		}

		private String _approver;
		[XmlAttribute("Approver")]
		public virtual String Approver
		{
			get { return _approver; }
			set { _approver = value; }
		}

		private String _approverworkflow;
		[XmlAttribute("ApproverWorkflow")]
		public virtual String ApproverWorkflow
		{
			get { return _approverworkflow; }
			set { _approverworkflow = value; }
		}

		private String _rejecter;
		[XmlAttribute("Rejecter")]
		public virtual String Rejecter
		{
			get { return _rejecter; }
			set { _rejecter = value; }
		}

		private int _supoveride;
		[XmlAttribute("Supoveride")]
		public virtual int Supoveride
		{
			get { return _supoveride; }
			set { _supoveride = value; }
		}

		private int _status;
		[XmlAttribute("Status")]
		public virtual int Status
		{
			get { return _status; }
			set { _status = value; }
		}

		private int _laststatus;
		[XmlAttribute("LastStatus")]
		public virtual int LastStatus
		{
			get { return _laststatus; }
			set { _laststatus = value; }
		}

		private String _description;
		[XmlAttribute("Description")]
		public virtual String Description
		{
			get { return _description; }
			set { _description = value; }
		}


        private String _errordescription;
        [XmlAttribute("ErrorDescription")]
        public virtual String ErrorDescription
        {
            get { return _errordescription; }
            set { _errordescription = value; }
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

		private String _jurnalseq3;
		[XmlAttribute("JurnalSeq3")]
		public virtual String JurnalSeq3
		{
			get { return _jurnalseq3; }
			set { _jurnalseq3 = value; }
		}

		private String _jurnalseq4;
		[XmlAttribute("JurnalSeq4")]
		public virtual String JurnalSeq4
		{
			get { return _jurnalseq4; }
			set { _jurnalseq4 = value; }
		}

		private String _jurnalseq5;
		[XmlAttribute("JurnalSeq5")]
		public virtual String JurnalSeq5
		{
			get { return _jurnalseq5; }
			set { _jurnalseq5 = value; }
		}

		private String _jurnalseq6;
		[XmlAttribute("JurnalSeq6")]
		public virtual String JurnalSeq6
		{
			get { return _jurnalseq6; }
			set { _jurnalseq6 = value; }
		}

		private String _jurnalseq7;
		[XmlAttribute("JurnalSeq7")]
		public virtual String JurnalSeq7
		{
			get { return _jurnalseq7; }
			set { _jurnalseq7 = value; }
		}

		private String _jurnalseq8;
		[XmlAttribute("JurnalSeq8")]
		public virtual String JurnalSeq8
		{
			get { return _jurnalseq8; }
			set { _jurnalseq8 = value; }
		}

		private String _makerho;
		[XmlAttribute("MakerHO")]
		public virtual String MakerHO
		{
			get { return _makerho; }
			set { _makerho = value; }
		}

		private DateTime _makehotime;
		[XmlAttribute("MakeHOTime")]
		public virtual DateTime MakeHOTime
		{
			get { return _makehotime; }
			set { _makehotime = value; }
		}

		private String _checkerho;
		[XmlAttribute("CheckerHO")]
		public virtual String CheckerHO
		{
			get { return _checkerho; }
			set { _checkerho = value; }
		}

		private int _checkworkho;
		[XmlAttribute("CheckWorkHO")]
		public virtual int CheckWorkHO
		{
			get { return _checkworkho; }
			set { _checkworkho = value; }
		}

		private int _checktotalho;
		[XmlAttribute("CheckTotalHO")]
		public virtual int CheckTotalHO
		{
			get { return _checktotalho; }
			set { _checktotalho = value; }
		}

		private DateTime _verifiedhotime;
		[XmlAttribute("VerifiedHOTime")]
		public virtual DateTime VerifiedHOTime
		{
			get { return _verifiedhotime; }
			set { _verifiedhotime = value; }
		}

		private String _approverho;
		[XmlAttribute("ApproverHO")]
		public virtual String ApproverHO
		{
			get { return _approverho; }
			set { _approverho = value; }
		}

		private int _approveworkho;
		[XmlAttribute("ApproveWorkHO")]
		public virtual int ApproveWorkHO
		{
			get { return _approveworkho; }
			set { _approveworkho = value; }
		}

		private int _approvetotalho;
		[XmlAttribute("ApproveTotalHO")]
		public virtual int ApproveTotalHO
		{
			get { return _approvetotalho; }
			set { _approvetotalho = value; }
		}

		private DateTime _approvedhotime;
		[XmlAttribute("ApprovedHOTime")]
		public virtual DateTime ApprovedHOTime
		{
			get { return _approvedhotime; }
			set { _approvedhotime = value; }
		}

		private String _bucket5_1207;
		[XmlAttribute("Bucket5_1207")]
		public virtual String Bucket5_1207
		{
			get { return _bucket5_1207; }
			set { _bucket5_1207 = value; }
		}

		private String _bucket6_1207;
		[XmlAttribute("Bucket6_1207")]
		public virtual String Bucket6_1207
		{
			get { return _bucket6_1207; }
			set { _bucket6_1207 = value; }
		}

		private String _bucket7_1207;
		[XmlAttribute("Bucket7_1207")]
		public virtual String Bucket7_1207
		{
			get { return _bucket7_1207; }
			set { _bucket7_1207 = value; }
		}

		private String _bucket8_1207;
		[XmlAttribute("Bucket8_1207")]
		public virtual String Bucket8_1207
		{
			get { return _bucket8_1207; }
			set { _bucket8_1207 = value; }
		}

		private String _bucket5_8722;
		[XmlAttribute("Bucket5_8722")]
		public virtual String Bucket5_8722
		{
			get { return _bucket5_8722; }
			set { _bucket5_8722 = value; }
		}

		private String _bucket6_8722;
		[XmlAttribute("Bucket6_8722")]
		public virtual String Bucket6_8722
		{
			get { return _bucket6_8722; }
			set { _bucket6_8722 = value; }
		}

		private String _bucket7_8722;
		[XmlAttribute("Bucket7_8722")]
		public virtual String Bucket7_8722
		{
			get { return _bucket7_8722; }
			set { _bucket7_8722 = value; }
		}

		private String _bucket8_8722;
		[XmlAttribute("Bucket8_8722")]
		public virtual String Bucket8_8722
		{
			get { return _bucket8_8722; }
			set { _bucket8_8722 = value; }
		}

		private String _bucket5_0007;
		[XmlAttribute("Bucket5_0007")]
		public virtual String Bucket5_0007
		{
			get { return _bucket5_0007; }
			set { _bucket5_0007 = value; }
		}

		private String _bucket6_0007;
		[XmlAttribute("Bucket6_0007")]
		public virtual String Bucket6_0007
		{
			get { return _bucket6_0007; }
			set { _bucket6_0007 = value; }
		}

		private String _bucket7_0007;
		[XmlAttribute("Bucket7_0007")]
		public virtual String Bucket7_0007
		{
			get { return _bucket7_0007; }
			set { _bucket7_0007 = value; }
		}

		private String _bucket8_0007;
		[XmlAttribute("Bucket8_0007")]
		public virtual String Bucket8_0007
		{
			get { return _bucket8_0007; }
			set { _bucket8_0007 = value; }
		}

		private int _rate;
		[XmlAttribute("Rate")]
		public virtual int Rate
		{
			get { return _rate; }
			set { _rate = value; }
		}

		private String _mt103;
		[XmlAttribute("MT103")]
		public virtual String MT103
		{
			get { return _mt103; }
			set { _mt103 = value; }
		}

		private String _mt202;
		[XmlAttribute("MT202")]
		public virtual String MT202
		{
			get { return _mt202; }
			set { _mt202 = value; }
		}

		private String _dest103;
		[XmlAttribute("Dest103")]
		public virtual String Dest103
		{
			get { return _dest103; }
			set { _dest103 = value; }
		}

		private String _dest202;
		[XmlAttribute("Dest202")]
		public virtual String Dest202
		{
			get { return _dest202; }
			set { _dest202 = value; }
		}

		private String _jurnalseqour;
		[XmlAttribute("JurnalSeqOur")]
		public virtual String JurnalSeqOur
		{
			get { return _jurnalseqour; }
			set { _jurnalseqour = value; }
		}

		private String _sanditrxbi;
		[XmlAttribute("SandiTrxBI")]
		public virtual String SandiTrxBI
		{
			get { return _sanditrxbi; }
			set { _sanditrxbi = value; }
		}

		private String _bencategory;
		[XmlAttribute("BenCategory")]
		public virtual String BenCategory
		{
			get { return _bencategory; }
			set { _bencategory = value; }
		}

		private String _benrelation;
		[XmlAttribute("BenRelation")]
		public virtual String BenRelation
		{
			get { return _benrelation; }
			set { _benrelation = value; }
		}

		private String _notification;
		[XmlAttribute("Notification")]
		public virtual String Notification
		{
			get { return _notification; }
			set { _notification = value; }
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

		private String _debitaccountaddress;
		[XmlAttribute("DebitAccountAddress")]
		public virtual String DebitAccountAddress
		{
			get { return _debitaccountaddress; }
			set { _debitaccountaddress = value; }
		}
		
		private Double _baseamount;
		[XmlAttribute("BaseAmount")]
		public virtual Double BaseAmount
		{
			get { return _baseamount; }
			set { _baseamount = value; }
		}

        private String _trxrate;
        [XmlAttribute("TrxRate")]
        public virtual String TrxRate
        {
            get { return _trxrate; }
            set { _trxrate = value; }
        }

        private String _custreff;
        [XmlAttribute("CustReff")]
        public virtual String CustReff
        {
            get { return _custreff; }
            set { _custreff = value; }
        }
	}
}
