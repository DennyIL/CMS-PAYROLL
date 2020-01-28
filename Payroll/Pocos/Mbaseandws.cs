using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class Mbaseandws : AbstractTransaction
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

        private String _instructioncode;
        [XmlAttribute("InstructionCode")]
        public virtual String InstructionCode
        {
            get { return _instructioncode; }
            set { _instructioncode = value; }
        }

        private int _status;
        [XmlAttribute("Status")]
        public virtual int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private string _description;
        [XmlAttribute("Description")]
        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private int _clientid;
        [XmlAttribute("ClientId")]
        public virtual int ClientId
        {
            get { return _clientid; }
            set { _clientid = value; }
        }

        private DateTime _createdtime;
        [XmlAttribute("CreatedTime")]
        public virtual DateTime CreatedTime
        {
            get { return _createdtime; }
            set { _createdtime = value; }
        }

        private DateTime _trxdate;
        [XmlAttribute("TrxDate")]
        public virtual DateTime TrxDate
        {
            get { return _trxdate; }
            set { _trxdate = value; }
        }

        private DateTime _lastupdate;
        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return _lastupdate; }
            set { _lastupdate = value; }
        }

        private String _ticketnumber;
        [XmlAttribute("TicketNumber")]
        public virtual String TicketNumber
        {
            get { return _ticketnumber; }
            set { _ticketnumber = value; }
        }

        private String _cnnumber;
        [XmlAttribute("CnNumber")]
        public virtual String CnNumber
        {
            get { return _cnnumber; }
            set { _cnnumber = value; }
        }

        private String _debetaccount;
        [XmlAttribute("DebetAccount")]
        public virtual String DebetAccount
        {
            get { return _debetaccount; }
            set { _debetaccount = value; }
        }

        private String _debetaccountname;
        [XmlAttribute("DebetAccountName")]
        public virtual String DebetAccountName
        {
            get { return _debetaccountname; }
            set { _debetaccountname = value; }
        }

        private String _benacc;
        [XmlAttribute("BenAcc")]
        public virtual String BenAcc
        {
            get { return _benacc; }
            set { _benacc = value; }
        }

        private String _benaccname;
        [XmlAttribute("BenAccName")]
        public virtual String BenAccName
        {
            get { return _benaccname; }
            set { _benaccname = value; }
        }

        private String _benaccaddress;
        [XmlAttribute("BenAccAddress")]
        public virtual String BenAccAddress
        {
            get { return _benaccaddress; }
            set { _benaccaddress = value; }
        }

        private String _bucketamount;
        [XmlAttribute("BucketAmount")]
        public virtual String BucketAmount
        {
            get { return _bucketamount; }
            set { _bucketamount = value; }
        }

        private int _idbooking;
        [XmlAttribute("IdBooking")]
        public virtual int IdBooking
        {
            get { return _idbooking; }
            set { _idbooking = value; }
        }

        private String _remark;
        [XmlAttribute("Remark")]
        public virtual String Remark
        {
            get { return _remark; }
            set { _remark = value; }
        }

        private String _log;
        [XmlAttribute("Log")]
        public virtual String Log
        {
            get { return _log; }
            set { _log = value; }
        }

        private int _statusdata;
        [XmlAttribute("StatusData")]
        public virtual int StatusData
        {
            get { return _statusdata; }
            set { _statusdata = value; }
        }

        private String _remittancenumber;
        [XmlAttribute("RemittanceNumner")]
        public virtual String RemittanceNumner
        {
            get { return _remittancenumber; }
            set { _remittancenumber = value; }
        }

        private String _bankcode;
        [XmlAttribute("BankCode")]
        public virtual String BankCode
        {
            get { return _bankcode; }
            set { _bankcode = value; }
        }

        private String _jurnalseq;
        [XmlAttribute("JurnalSeq")]
        public virtual String JurnalSeq
        {
            get { return _jurnalseq; }
            set { _jurnalseq = value; }
        }

        private Double _chargeamount;
        [XmlAttribute("ChargeAmount")]
        public virtual Double ChargeAmount
        {
            get { return _chargeamount; }
            set { _chargeamount = value; }
        }

        private String _chargetype;
        [XmlAttribute("ChargeType")]
        public virtual String ChargeType
        {
            get { return _chargetype; }
            set { _chargetype = value; }
        }

        private String rtgsTrxReff;
        [XmlAttribute("RtgsTrxReff")]
        public virtual String RtgsTrxReff
        {
            get { return rtgsTrxReff; }
            set { rtgsTrxReff = value; }
        }

        private String _debitaccaddress;
        [XmlAttribute("DebitAccAddress")]
        public virtual String DebitAccAddress
        {
            get { return _debitaccaddress; }
            set { _debitaccaddress = value; }
        }

    }
}
