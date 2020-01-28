using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [XmlRoot("TPDetail")]
    public class TrxPayrollDetail
    {
        private Double id;
        private String pid;
        private String name;
        private String account;
        private Double amount;
        private Int32 status;
        private String description;
        private String errordescription;
        private DateTime lastupdate;
        private TrxPayroll parent;
        private String email;
        private int emailtransactionid;
        private String customerreff;

        [XmlAttribute("Id")]
        public virtual Double Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("Pid")]
        public virtual String Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        [XmlAttribute("Name")]
        public virtual String Name
        {
            get { return name; }
            set { name = value; }
        }
        
        [XmlAttribute("Acct")]
        public virtual String Account
        {
            get { return account; }
            set { account = value; }
        }

        [XmlAttribute("Amount")]
        public virtual Double Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        [XmlAttribute("Stat")]
        public virtual Int32 Status
        {
            get { return status; }
            set { status = value; }
        }

        [XmlAttribute("Desc")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        [XmlAttribute("ErrorDescription")]
        public virtual String ErrorDescription
        {
            get { return errordescription; }
            set { errordescription = value; }
        }

        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return lastupdate; }
            set { lastupdate = value; }
        }

        [XmlAttribute("Email")]
        public virtual String Email
        {
            get { return email; }
            set { email = value; }
        }

        [XmlAttribute("EmailTransactionId")]
        public virtual int EmailTransactionId
        {
            get { return emailtransactionid; }
            set { emailtransactionid = value; }
        }

        [XmlAttribute("CustomerReff")]
        public virtual String CustomerReff
        {
            get { return customerreff; }
            set { customerreff = value; }
        }

        private String bankcode;
        [XmlAttribute("BankCode")]
        public virtual String BankCode
        {
            get { return bankcode; }
            set { bankcode = value; }
        }

        private String benaddress;
        [XmlAttribute("BenAddress")]
        public virtual String BenAddress
        {
            get { return benaddress; }
            set { benaddress = value; }
        }

        private String instructioncode;
        [XmlAttribute("InstructionCode")]
        public virtual String InstructionCode
        {
            get { return instructioncode; }
            set { instructioncode = value; }
        }

        private int idbooking;
        [XmlAttribute("IdBooking")]
        public virtual int IdBooking
        {
            get { return idbooking; }
            set { idbooking = value; }
        }

        private int idMBASEAndWS;
        [XmlAttribute("IdMBASEAndWS")]
        public virtual int IdMBASEAndWS
        {
            get { return idMBASEAndWS; }
            set { idMBASEAndWS = value; }
        }

        private String trxremark;
        [XmlAttribute("TrxRemark")]
        public virtual String TrxRemark
        {
            get { return trxremark; }
            set { trxremark = value; }
        }

        private String remittancenumber;
        [XmlAttribute("RemittanceNumber")]
        public virtual String RemittanceNumber
        {
            get { return remittancenumber; }
            set { remittancenumber = value; }
        }


        [XmlIgnore]
        public virtual TrxPayroll Parent
        {
            get { return parent; }
            set { parent = value; }
        }

    }
}
