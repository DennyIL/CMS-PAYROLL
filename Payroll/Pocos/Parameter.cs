using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    public class Parameter
    {
        private String id;
        private String data;
        private String description;

        public virtual String Id {
            get { return id; }
            set { id = value; }
        }

        public virtual String Data
        {
            get { return data; }
            set { data = value; }
        }

        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}
