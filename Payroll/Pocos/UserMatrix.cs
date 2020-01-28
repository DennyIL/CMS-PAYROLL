using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class UserMatrix : AbstractTransaction
    {
        private int id;
        private DateTime createdTime;
        private DateTime lastUpdate;
        private String approver;
        private String matrix;

        public virtual int Id { 
            get { return id; }
            set { id = value; } 
        }

        public virtual DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }

        public virtual DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        public virtual String Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }
    }
}
