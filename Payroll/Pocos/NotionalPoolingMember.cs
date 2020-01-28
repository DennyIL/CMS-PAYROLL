using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class NotionalPoolingMember : AbstractTransaction
    {
        private int id;
        private int groupcode;
        private string accountnumber;
        private string accountname;
        private decimal memberoverdraftlimit;
        private string status;
        private string previousstatus;
        private string usercreate;
        private DateTime datecreate;
        private string usermodified;
        private DateTime datemodified;

        [XmlAttribute("Id")]
        public virtual int Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("GroupCode")]
        public virtual int GroupCode
        {
            get { return groupcode; }
            set { groupcode = value; }
        }

        [XmlAttribute("AccountNumber")]
        public virtual string AccountNumber
        {
            get { return accountnumber; }
            set { accountnumber = value; }
        }

        [XmlAttribute("AccountName")]
        public virtual string AccountName
        {
            get { return accountname; }
            set { accountname = value; }
        }

        [XmlAttribute("MemberOverdraftLimit")]
        public virtual decimal MemberOverdraftLimit
        {
            get { return memberoverdraftlimit; }
            set { memberoverdraftlimit = value; }
        }

        [XmlAttribute("Status")]
        public virtual string Status
        {
            get { return status; }
            set { status = value; }
        }

        [XmlAttribute("PreviousStatus")]
        public virtual string PreviousStatus
        {
            get { return previousstatus; }
            set { previousstatus = value; }
        }

        [XmlAttribute("UserCreate")]
        public virtual string UserCreate
        {
            get { return usercreate; }
            set { usercreate = value; }
        }

        [XmlAttribute("DateCreate")]
        public virtual DateTime DateCreate
        {
            get { return datecreate; }
            set { datecreate = value; }
        }

        [XmlAttribute("UserModified")]
        public virtual string UserModified
        {
            get { return usermodified; }
            set { usermodified = value; }
        }

        [XmlAttribute("DateModified")]
        public virtual DateTime DateModified
        {
            get { return datemodified; }
            set { datemodified = value; }
        }
    }
}
