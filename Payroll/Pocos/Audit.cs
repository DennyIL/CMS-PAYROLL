using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    public class Audit
    {
        private Int32 id;
        private String user;
        private Int32 transactionId;
        private String description;
        private String data;
        private DateTime time;

        public virtual Int32 Id 
        {
            get { return id; }
            set { id = value; }
        }

        public virtual String User
        {
            get { return user; }
            set { user = value; }
        }

        public virtual Int32 TransactionId
        {
            get { return transactionId; }
            set { transactionId = value; }
        }

        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual String Data
        {
            get { return data; }
            set { data = value; }
        }

        public virtual DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        public virtual String IPInfo
        {
            get;
            set;
        }
    }
}
