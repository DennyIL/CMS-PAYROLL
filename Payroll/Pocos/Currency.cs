using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    public class Currency
    {
        private int id;
        private String code;
        private String description;
        private Decimal buyrate;
        private Decimal sellrate;
        private Decimal bookingrate;
        private DateTime lastupdate;
        private String currencycode;

        public virtual int Id 
        {
            get { return id; }
            set { id = value; }
        }

        public virtual String Code
        {
            get { return code; }
            set { code = value; }
        }

        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        public virtual Decimal BuyRate
        {
            get { return buyrate; }
            set { buyrate = value; }
        }

        public virtual Decimal SellRate
        {
            get { return sellrate; }
            set { sellrate = value; }
        }

        public virtual Decimal BookingRate
        {
            get { return bookingrate; }
            set { bookingrate = value; }
        }

        public virtual DateTime LastUpdate
        {
            get { return lastupdate; }
            set { lastupdate = value; }
        }

        public virtual String CurrencyCode
        {
            get { return currencycode; }
            set { currencycode = value; }
        }
    }
}
