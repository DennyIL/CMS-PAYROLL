using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxWalkincustomer : AbstractTransaction
    {
        private Int32 id;
        private int clientid;
        private String trxid;
        private DateTime trxdate;
        private String debitaccount;
        private String debitaccounttype;
        private Double debitamount;
        private String debitcurrency;
        private String creditaccount;
        private String creditaccounttype;
        private Double creditamount;
        private String creditcurrency;
        private Double feeamount;
        private String feecurrency;
        private String trxbeneficiary;
        private String status;
        private DateTime creationdate;
        private DateTime lastupdate;
        private String maker;
        private String cheker;
        private String approver;
        private String noid;
        private String accountname;
        private String address;
        private String namauker;
        private String alamatpenerima1;
        private String alamatpenerima2;
        private String alamatpenerima3;
        private String zipcode;
        private String phone;
        private String jenisid;
        private String noremittance;
        private String kotapenerbit;
        private String remark2;
        private String branchcode;
        private String city;
        private int statusrec;
        private String statusrem;





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

        [XmlAttribute("CreditAccountType")]
        public virtual String CreditAccountType
        {
            get { return creditaccounttype; }
            set { creditaccounttype = value; }
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

        [XmlAttribute("TrxBeneficiary")]
        public virtual String TrxBeneficiary
        {
            get { return trxbeneficiary; }
            set { trxbeneficiary = value; }
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

        [XmlAttribute("Cheker")]
        public virtual String Cheker
        {
            get { return cheker; }
            set { cheker = value; }
        }

        [XmlAttribute("Approver")]
        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        [XmlAttribute("NoID")]
        public virtual String NoID
        {
            get { return noid; }
            set { noid = value; }
        }

        [XmlAttribute("AccountName")]
        public virtual String AccountName
        {
            get { return accountname; }
            set { accountname = value; }
        }

        [XmlAttribute("Address")]
        public virtual String Address
        {
            get { return address; }
            set { address = value; }
        }

        [XmlAttribute("NamaUker")]
        public virtual String NamaUker
        {
            get { return namauker; }
            set { namauker = value; }
        }

        [XmlAttribute("AlamatPenerima1")]
        public virtual String AlamatPenerima1
        {
            get { return alamatpenerima1; }
            set { alamatpenerima1 = value; }
        }

        [XmlAttribute("AlamatPenerima2")]
        public virtual String AlamatPenerima2
        {
            get { return alamatpenerima2; }
            set { alamatpenerima2 = value; }
        }

        [XmlAttribute("AlamatPenerima3")]
        public virtual String AlamatPenerima3
        {
            get { return alamatpenerima3; }
            set { alamatpenerima3 = value; }
        }

        [XmlAttribute("ZipCode")]
        public virtual String ZipCode
        {
            get { return zipcode; }
            set { zipcode = value; }
        }

        [XmlAttribute("Phone")]
        public virtual String Phone
        {
            get { return phone; }
            set { phone = value; }
        }

        [XmlAttribute("JenisId")]
        public virtual String JenisId
        {
            get { return jenisid; }
            set { jenisid = value; }
        }

        [XmlAttribute("NoRemittance")]
        public virtual String NoRemittance
        {
            get { return noremittance; }
            set { noremittance = value; }
        }

        [XmlAttribute("KotaPenerbit")]
        public virtual String KotaPenerbit
        {
            get { return kotapenerbit; }
            set { kotapenerbit = value; }
        }

        [XmlAttribute("Remark2")]
        public virtual String Remark2
        {
            get { return remark2; }
            set { remark2 = value; }
        }

        [XmlAttribute("BranchCode")]
        public virtual String BranchCode
        {
            get { return branchcode; }
            set { branchcode = value; }
        }

        [XmlAttribute("City")]
        public virtual String City
        {
            get { return city; }
            set { city = value; }
        }

        [XmlAttribute("StatusRec")]
        public virtual int StatusRec
        {
            get { return statusrec; }
            set { statusrec = value; }
        }

        [XmlAttribute("StatusRem")]
        public virtual String StatusRem
        {
            get { return statusrem; }
            set { statusrem = value; }
        }

    }


}
