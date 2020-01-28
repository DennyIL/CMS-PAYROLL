using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class Authority
    {
        private String product;
        private String[] menus;
        private String[] uriMenus;
        private String[] accounts;
        private String[] reports;
        private String[] levels;
        private String[] briva;
        private float userLimit;
        private float userLimit2;
        private float userLimit3;
        private float userLimit4;
        private float userLimit5;
        private float groupLimit;
        private float transactionFee;
        private float transactionFee2;
        private float transactionFee3;
        private float transactionFee4;
        private float transactionFee5;
        private float transactionFee6;
        private float transactionFee7;
        private float transactionFee8;
        private float transactionFee9;
        private float transactionFee10;
        private Boolean isfeenego;
        private Boolean isfeenego2;
        private Boolean isfeenego3;
        private Boolean isfeenego4;
        private Boolean isfeenego5;
        private Boolean isfeenego7;
        private Boolean isfeenego8;
        private Boolean isfeenego9;
        private Boolean isfeenego10;
        private String currencyFee;
        private Boolean otherAccountAllowed;
        private Boolean enkriptAllowed;
        private Boolean iaAccount;
        private String role;
        /*Add Sari, 30 juli 2012*/
        private String temp1;
        private String temp2;
        private String temp3;
        private String temp4;
        /*Add Sari, 011012*/
        private String temp5;
        private String temp6;
        /*End Sari, 011012*/
        /*Add Bimo, 210213*/
        private String kodeDistributor;
        /*End Bimo, 210213*/
        /* Add eQ, 09-07-2013 */
        private String pajakaktif;
        private String cabangpersepsi;
        private Boolean isneedrefnum;
        /* End eQ, 09-07-2013 */
        /* Add bayu, 07-11-2013 */
        private Boolean ispendingemail;
        private Boolean isdoneemail;
        /* End bayu, 07-11-2013 */
        private String _ParentTrade;
        //purnomo 30052014
        private string kmkpkg;
        //end purnomo 30052014
        /* Add liz, 2-10-2014 */
        private String pbbkabupaten;
        private String bphtbkabupaten;
        /* End liz, 2-10-2014 */
        private string girocashcard;
        /*Add Wira*/
        private String kodePE;
        /*End Wira*/
        /*Add Rofiq, 051015*/
        private Boolean isdirecttransaction;
        /*End Rofiq, 051015*/
        /*Add Purnomo, 26082016*/
        private string kmkpihc;
        private string custno;
        private string code;
        /*End Purnomo, 26082016*/

        [XmlAttribute("Product")]
        public virtual String Product
        {
            get { return product; }
            set { product = value; }
        }

        public virtual String[] Menus
        {
            get { return menus; }
            set { menus = value; }
        }

        public virtual String[] UriMenus
        {
            get { return uriMenus; }
            set { uriMenus = value; }
        }

        public virtual String[] Accounts
        {
            get { return accounts; }
            set { accounts = value; }
        }

        public virtual String[] Reports
        {
            get { return reports; }
            set { reports = value; }
        }

        public virtual String[] Briva
        {
            get { return briva; }
            set { briva = value; }
        }

        public virtual String[] Levels
        {
            get { return levels; }
            set { levels = value; }
        }

        [XmlAttribute("Ulimit")]
        public virtual float UserLimit
        {
            get { return userLimit; }
            set { userLimit = value; }
        }

        [XmlAttribute("Ulimit2")]
        public virtual float UserLimit2
        {
            get { return userLimit2; }
            set { userLimit2 = value; }
        }

        [XmlAttribute("Ulimit3")]
        public virtual float UserLimit3
        {
            get { return userLimit3; }
            set { userLimit3 = value; }
        }

        [XmlAttribute("Ulimit4")]
        public virtual float UserLimit4
        {
            get { return userLimit4; }
            set { userLimit4 = value; }
        }

        [XmlAttribute("Ulimit5")]
        public virtual float UserLimit5
        {
            get { return userLimit5; }
            set { userLimit5 = value; }
        }


        [XmlAttribute("Glimit")]
        public float GroupLimit
        {
            get { return groupLimit; }
            set { groupLimit = value; }
        }

        [XmlAttribute("Role")]
        public virtual String Role
        {
            get { return role; }
            set { role = value; }
        }

        [XmlAttribute("OtherAccountAllowed")]
        public virtual Boolean OtherAccountAllowed
        {
            get { return otherAccountAllowed; }
            set { otherAccountAllowed = value; }
        }

        [XmlAttribute("EnkriptAllowed")]
        public virtual Boolean EnkriptAllowed
        {
            get { return enkriptAllowed; }
            set { enkriptAllowed = value; }
        }

        [XmlAttribute("IAAccount")]
        public virtual Boolean IAAccount
        {
            get { return iaAccount; }
            set { iaAccount = value; }
        }


        [XmlAttribute("TransactionFee")]
        public virtual float TransactionFee
        {
            get { return transactionFee; }
            set { transactionFee = value; }
        }

        [XmlAttribute("TransactionFee2")]
        public virtual float TransactionFee2
        {
            get { return transactionFee2; }
            set { transactionFee2 = value; }
        }

        [XmlAttribute("TransactionFee3")]
        public virtual float TransactionFee3
        {
            get { return transactionFee3; }
            set { transactionFee3 = value; }
        }

        [XmlAttribute("TransactionFee4")]
        public virtual float TransactionFee4
        {
            get { return transactionFee4; }
            set { transactionFee4 = value; }
        }

        [XmlAttribute("TransactionFee5")]
        public virtual float TransactionFee5
        {
            get { return transactionFee5; }
            set { transactionFee5 = value; }
        }

        [XmlAttribute("TransactionFee6")]
        public virtual float TransactionFee6
        {
            get { return transactionFee6; }
            set { transactionFee6 = value; }
        }

        [XmlAttribute("TransactionFee7")]
        public virtual float TransactionFee7
        {
            get { return transactionFee7; }
            set { transactionFee7 = value; }
        }

        [XmlAttribute("TransactionFee8")]
        public virtual float TransactionFee8
        {
            get { return transactionFee8; }
            set { transactionFee8 = value; }
        }

        [XmlAttribute("TransactionFee9")]
        public virtual float TransactionFee9
        {
            get { return transactionFee9; }
            set { transactionFee9 = value; }
        }

        [XmlAttribute("TransactionFee10")]
        public virtual float TransactionFee10
        {
            get { return transactionFee10; }
            set { transactionFee10 = value; }
        }

        [XmlAttribute("IsFeeNego")]
        public virtual Boolean IsFeeNego
        {
            get { return isfeenego; }
            set { isfeenego = value; }
        }

        [XmlAttribute("IsFee2Nego")]
        public virtual Boolean IsFee2Nego
        {
            get { return isfeenego2; }
            set { isfeenego2 = value; }
        }

        [XmlAttribute("IsFee3Nego")]
        public virtual Boolean IsFee3Nego
        {
            get { return isfeenego3; }
            set { isfeenego3 = value; }
        }

        [XmlAttribute("IsFee4Nego")]
        public virtual Boolean IsFee4Nego
        {
            get { return isfeenego4; }
            set { isfeenego4 = value; }
        }

        [XmlAttribute("IsFee5Nego")]
        public virtual Boolean IsFee5Nego
        {
            get { return isfeenego5; }
            set { isfeenego5 = value; }
        }

        [XmlAttribute("IsFee7Nego")]
        public virtual Boolean IsFee7Nego
        {
            get { return isfeenego7; }
            set { isfeenego7 = value; }
        }

        [XmlAttribute("IsFee8Nego")]
        public virtual Boolean IsFee8Nego
        {
            get { return isfeenego8; }
            set { isfeenego9 = value; }
        }

        [XmlAttribute("IsFee9Nego")]
        public virtual Boolean IsFee9Nego
        {
            get { return isfeenego9; }
            set { isfeenego9 = value; }
        }

        [XmlAttribute("IsFee10Nego")]
        public virtual Boolean IsFee10Nego
        {
            get { return isfeenego10; }
            set { isfeenego10 = value; }
        }

        [XmlAttribute("CurrencyFee")]
        public virtual String CurrencyFee
        {
            get { return currencyFee; }
            set { currencyFee = value; }
        }

        /*Add Sari, 30 Juli 2012*/
        [XmlAttribute("Temp1")]
        public virtual String Temp1
        {
            get { return temp1; }
            set { temp1 = value; }
        }

        [XmlAttribute("Temp2")]
        public virtual String Temp2
        {
            get { return temp2; }
            set { temp2 = value; }
        }

        [XmlAttribute("Temp3")]
        public virtual String Temp3
        {
            get { return temp3; }
            set { temp3 = value; }
        }

        [XmlAttribute("Temp4")]
        public virtual String Temp4
        {
            get { return temp4; }
            set { temp4 = value; }
        }
        /*End Sari*/
        /*Add Sari, 011012*/
        [XmlAttribute("Temp5")]
        public virtual String Temp5
        {
            get { return temp5; }
            set { temp5 = value; }
        }

        [XmlAttribute("Temp6")]
        public virtual String Temp6
        {
            get { return temp6; }
            set { temp6 = value; }
        }
        /*End Sari, 011012*/

        /*Add Bimo, 21022013*/
        [XmlAttribute("KodeDistributor")]
        public virtual String KodeDistributor
        {
            get { return kodeDistributor; }
            set { kodeDistributor = value; }
        }
        /*End Bimo, 011012*/

        /* Add eQ, 09-07-2013 */
        [XmlAttribute("PajakAktif")]
        public virtual String PajakAktif
        {
            get { return pajakaktif; }
            set { pajakaktif = value; }
        }

        [XmlAttribute("CabangPersepsi")]
        public virtual String CabangPersepsi
        {
            get { return cabangpersepsi; }
            set { cabangpersepsi = value; }
        }

        [XmlAttribute("IsNeedReff")]
        public virtual Boolean IsNeedReff
        {
            get { return isneedrefnum; }
            set { isneedrefnum = value; }
        }
        /* End eQ, 09-07-2013 */

        [XmlAttribute("IsPendingEmail")]
        public virtual Boolean IsPendingEmail
        {
            get { return ispendingemail; }
            set { ispendingemail = value; }
        }

        [XmlAttribute("IsDoneEmail")]
        public virtual Boolean IsDoneEmail
        {
            get { return isdoneemail; }
            set { isdoneemail = value; }
        }

        [XmlAttribute("ParentTrade")]
        public virtual String ParentTrade
        {
            get { return _ParentTrade; }
            set { _ParentTrade = value; }
        }

        public void AddMenu(String menu)
        {
            IList<String> lmenus = null;
            if (null == menus)
            {
                lmenus = new List<String>();
            }
            else
            {
                lmenus = menus.ToList<String>();
            }
            lmenus.Add(menu);
            menus = lmenus.ToArray<String>();
        }

        public void AddAccount(String account)
        {
            IList<String> lacounts = null;
            if (null == accounts)
            {
                lacounts = new List<String>();
            }
            else
            {
                lacounts = accounts.ToList<String>();
            }
            lacounts.Add(account);
            accounts = lacounts.ToArray<String>();
        }

        public void AddLevel(String level)
        {
            IList<String> llevels = null;
            if (null == levels)
            {
                llevels = new List<String>();
            }
            else
            {
                llevels = levels.ToList<String>();
            }
            llevels.Add(level);
            levels = llevels.ToArray<String>();
        }

        /*Add Purnomo, 16052014*/
        [XmlAttribute("KMKPKG")]
        public virtual String Kmkpkg
        {
            get { return kmkpkg; }
            set { kmkpkg = value; }
        }
        /*End Purnomo, 16052014*/

        /* Add liz, 18-03-2014 */
        [XmlAttribute("PBBKabupaten")]
        public virtual String PBBKabupaten
        {
            get { return pbbkabupaten; }
            set { pbbkabupaten = value; }
        }

        [XmlAttribute("BPHTBKabupaten")]
        public virtual String BPHTBKabupaten
        {
            get { return bphtbkabupaten; }
            set { bphtbkabupaten = value; }
        }
        /* End liz, 18-03-2014 */

        //cashcardEP
        [XmlAttribute("GIROCASHCARD")]
        public virtual String Girocashcard
        {
            get { return girocashcard; }
            set { girocashcard = value; }
        }

        /*Add Wira*/
        [XmlAttribute("KodePE")]
        public virtual String KodePE
        {
            get { return kodePE; }
            set { kodePE = value; }
        }
        /*End Wira*/

        /*Start Rofiq, 051015*/
        [XmlAttribute("IsDirectTransaction")]
        public virtual Boolean IsDirectTransaction
        {
            get { return isdirecttransaction; }
            set { isdirecttransaction = value; }
        }
        /*End Rofiq, 051015*/

        /*Start Purnomo, 26082016*/
        [XmlAttribute("KMKPIHC")]
        public virtual String Kmkpihc
        {
            get { return kmkpihc; }
            set { kmkpihc = value; }
        }

        [XmlAttribute("CUSTNO")]
        public virtual String Custno
        {
            get { return custno; }
            set { custno = value; }
        }

        [XmlAttribute("CODE")]
        public virtual String Code
        {
            get { return code; }
            set { code = value; }
        }
        /*End Purnomo, 26082016*/

        private bool isByPassPendingIFT;
        [XmlAttribute("IsByPassPendingIFT")]
        public virtual bool IsByPassPendingIFT
        {
            get { return isByPassPendingIFT; }
            set { isByPassPendingIFT = value; }
        }
    }
}
