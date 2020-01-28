using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    public class UserMatrix2
    {
        private Int32 id;
        private Int32 cid;
        private String matrix;

        public virtual Int32 Id 
        {
            get { return id; }
            set { id = value; }
        }

        public virtual Int32 Cid
        {
            get { return cid; }
            set { cid = value; }
        }

        public virtual String Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

    }
}
