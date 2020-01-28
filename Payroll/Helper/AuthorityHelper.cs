using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BRIChannelSchedulerNew.Payroll.Pocos;
using System.Xml;
using System.Xml.Serialization;
using NHibernate;
using NHibernate.Expression;

namespace BRIChannelSchedulerNew.Payroll.Helper
{
    [Serializable]
    public class AuthorityHelper
    {
        private Authority[] authorities;
        private IDictionary<String, String[]> menuDictionary;
        private IDictionary<int, String[]> accountDictionary;
        private IDictionary<String, float> userLimitDictionary;
        private IDictionary<int, String> linkMenuDictionary;

        public AuthorityHelper()
        {
        }

        public AuthorityHelper(String serializedXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(serializedXml);
            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            XmlSerializer ser = new XmlSerializer(typeof(Authority[]));
            object obj = ser.Deserialize(reader);
            authorities = (Authority[])obj;
        }

        public String SerializeToXml()
        {
            XmlSerializer ser = new XmlSerializer(authorities.GetType());
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            ser.Serialize(writer, authorities);
            return sb.ToString();
        }

        public Authority[] Authorities
        {
            get { return authorities; }
            set { authorities = value; }
        }

        public IDictionary<String, String[]> GetMenuDictionary()
        {
            IDictionary<String, String[]> result = new Dictionary<String, String[]>();
            foreach (Authority authority in authorities)
            {
                result.Add(authority.Product, authority.Menus);
            }
            menuDictionary = result;
            return result;
        }

        public IDictionary<int, String[]> GetAccountDictionary()
        {
            IDictionary<int, String[]> result = new Dictionary<int, String[]>();
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                result.Add(int.Parse(sid[0]), authority.Accounts);
            }
            accountDictionary = result;
            return result;
        }

        public IDictionary<String, float> GetUserLimitDictionary()
        {
            //if (userLimitDictionary != null) return userLimitDictionary;
            IDictionary<String, float> result = new Dictionary<String, float>();
            foreach (Authority authority in authorities)
            {
                result.Add(authority.Product, authority.UserLimit);
            }
            userLimitDictionary = result;
            return result;
        }

        public IDictionary<String, float> GetGroupLimitDictionary()
        {
            if (userLimitDictionary != null) return userLimitDictionary;
            IDictionary<String, float> result = new Dictionary<String, float>();
            foreach (Authority authority in authorities)
            {
                result.Add(authority.Product, authority.GroupLimit);
            }
            userLimitDictionary = result;
            return result;
        }

        public bool IsValidAccount(int product, String account)
        {
            bool result = false;
            IDictionary<int, String[]> workDictionary = GetAccountDictionary();
            if (workDictionary.ContainsKey(product))
            {
                String[] accounts = workDictionary[product];
                if (accounts.Contains(account)) result = true;
            }
            return result;
        }

        public bool IsValidProduct(int product)
        {
            bool result = false;
            IDictionary<int, String[]> workDictionary = GetAccountDictionary();
            if (workDictionary.ContainsKey(product)) result = true;
            return result;
        }

        public bool AddValidAccount(int product, String account)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            IList<String> laccounts = null;
            String[] accounts = authorities[index].Accounts;
            if (accounts == null)
            {
                laccounts = new List<String>();
            }
            else
            {
                laccounts = accounts.ToList<String>();
            }
            laccounts.Add(account);
            accounts = laccounts.ToArray<String>();
            authorities[index].Accounts = accounts;

            return result;
        }

        public bool SetValidAccount(int product, IList<ClientAccount> lacc)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            IList<String> laccounts = new List<String>();
            foreach (ClientAccount item in lacc)
            {
                laccounts.Add(item.Number);
            }
            authorities[index].Accounts = laccounts.ToArray<String>();

            return result;
        }

        public bool AddUserLimit(int product, float ulimit)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].UserLimit = ulimit;

            return result;
        }

        public bool AddUserLimit(int product, float ulimit, int type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            switch (type)
            {
                case 1:
                    authorities[index].UserLimit = ulimit;
                    break;
                case 2:
                    authorities[index].UserLimit2 = ulimit;
                    break;
                case 3:
                    authorities[index].UserLimit3 = ulimit;
                    break;
                case 4:
                    authorities[index].UserLimit4 = ulimit;
                    break;
                case 5:
                    authorities[index].UserLimit5 = ulimit;
                    break;
                default:
                    authorities[index].UserLimit = ulimit;
                    break;
            }

            return result;
        }

        public void SetTransferType(int product, Boolean type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].OtherAccountAllowed = type;
        }

        public void SetTransactionFee(int product, float fee, String currency)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].TransactionFee = fee;
            authorities[index].CurrencyFee = currency;
        }

        public void SetIsNegoFee(int product, bool nego, int type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            switch (type)
            {
                case 1:
                    authorities[index].IsFeeNego = nego;
                    break;
                case 2:
                    authorities[index].IsFee2Nego = nego;
                    break;
                case 3:
                    authorities[index].IsFee3Nego = nego;
                    break;
                case 4:
                    authorities[index].IsFee4Nego = nego;
                    break;
                case 5:
                    authorities[index].IsFee5Nego = nego;
                    break;
                case 7:
                    authorities[index].IsFee7Nego = nego;
                    break;
                case 8:
                    authorities[index].IsFee8Nego = nego;
                    break;
                case 9:
                    authorities[index].IsFee9Nego = nego;
                    break;
                case 10:
                    authorities[index].IsFee10Nego = nego;
                    break;
                default:
                    authorities[index].IsFeeNego = nego;
                    break;
            }
        }

        public void SetTransactionFee(int product, float fee, String currency, int type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            switch (type)
            {
                case 1:
                    authorities[index].TransactionFee = fee;
                    break;
                case 2:
                    authorities[index].TransactionFee2 = fee;
                    break;
                case 3:
                    authorities[index].TransactionFee3 = fee;
                    break;
                case 4:
                    authorities[index].TransactionFee4 = fee;
                    break;
                case 5:
                    authorities[index].TransactionFee5 = fee;
                    break;
                case 6:
                    authorities[index].TransactionFee6 = fee;
                    break;
                case 7:
                    authorities[index].TransactionFee7 = fee;
                    break;
                case 8:
                    authorities[index].TransactionFee8 = fee;
                    break;
                case 9:
                    authorities[index].TransactionFee9 = fee;
                    break;
                case 10:
                    authorities[index].TransactionFee10 = fee;
                    break;
                default:
                    authorities[index].TransactionFee = fee;
                    break;
            }
            authorities[index].CurrencyFee = currency;
        }

        public bool GetTransferType(int product)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            result = authorities[index].OtherAccountAllowed;

            return result;
        }

        public bool isFeeNego(int product, int type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            switch (type)
            {
                case 1:
                    result = authorities[index].IsFeeNego;
                    break;
                case 2:
                    result = authorities[index].IsFee2Nego;
                    break;
                case 3:
                    result = authorities[index].IsFee3Nego;
                    break;
                case 4:
                    result = authorities[index].IsFee4Nego;
                    break;
                case 5:
                    result = authorities[index].IsFee5Nego;
                    break;
                case 7:
                    result = authorities[index].IsFee7Nego;
                    break;
                case 8:
                    result = authorities[index].IsFee8Nego;
                    break;
                case 9:
                    result = authorities[index].IsFee9Nego;
                    break;
                case 10:
                    result = authorities[index].IsFee10Nego;
                    break;
                default:
                    result = authorities[index].IsFeeNego;
                    break;
            }

            return result;
        }

        //purnomo pihc
        public void SetPIHC(int product, String kmkpihc)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].Kmkpihc = kmkpihc;
        }

        public String GetPIHC(int product)
        {
            try
            {
                String result = "";
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }

                result = authorities[index].Kmkpihc;

                if (result == null) result = "";

                return result;
            }
            catch { return String.Empty; }
        }

        public void SetCusPIHC(int product, String custno)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].Custno = custno;
        }

        public String GetCusPIHC(int product)
        {
            try
            {
                String result = "";
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }

                result = authorities[index].Custno;

                if (result == null) result = "";

                return result;
            }
            catch { return String.Empty; }
        }

        public void SetCodePIHC(int product, String code)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].Code = code;
        }

        public String GetCodePIHC(int product)
        {
            try
            {
                String result = "";
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }

                result = authorities[index].Code;

                if (result == null) result = "";

                return result;
            }
            catch { return String.Empty; }
        }
        //end purnomo pihc

        public float GetTransactionFee(int product)
        {
            float result = 0;
            int index = 0;

            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            result = authorities[index].TransactionFee;

            return result;
        }

        public float GetTransactionFee(ISession session, int product)
        {
            float result = 0;
            float defaultFee = 0;
            int index = 0;

            try
            {
                DefaultFee defFee = session.Load<DefaultFee>(product);
                defaultFee = defFee.Fee;
            }
            catch (Exception ex)
            {

            }

            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            result = authorities[index].IsFeeNego == true ? authorities[index].TransactionFee : defaultFee;

            return result;
        }

        public float GetTransactionFee(ISession session, int product, int type)
        {
            float result = 0;
            float defaultFee = 0;
            int index = 0;
            DefaultFee defFee = null;
            try
            {
                defFee = session.Load<DefaultFee>(product);
            }
            catch (Exception ex)
            {

            }

            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            switch (type)
            {
                case 1:
                    defaultFee = defFee != null ? defFee.Fee : 0;
                    result = authorities[index].IsFeeNego == true ? authorities[index].TransactionFee : defaultFee;
                    break;
                case 2:
                    defaultFee = defFee != null ? defFee.Fee2 : 0;
                    result = authorities[index].IsFee2Nego == true ? authorities[index].TransactionFee2 : defaultFee;
                    break;
                case 3:
                    defaultFee = defFee != null ? defFee.Fee3 : 0;
                    result = authorities[index].IsFee3Nego == true ? authorities[index].TransactionFee3 : defaultFee;
                    break;
                case 4:
                    defaultFee = defFee != null ? defFee.Fee4 : 0;
                    result = authorities[index].IsFee4Nego == true ? authorities[index].TransactionFee4 : defaultFee;
                    break;
                case 5:
                    defaultFee = defFee != null ? defFee.Fee5 : 0;
                    result = authorities[index].IsFee5Nego == true ? authorities[index].TransactionFee5 : defaultFee;
                    break;
                case 6:
                    result = authorities[index].TransactionFee6;
                    break;
                case 7:
                    defaultFee = defFee != null ? defFee.Fee7 : 0;
                    result = authorities[index].IsFee7Nego == true ? authorities[index].TransactionFee7 : defaultFee;
                    break;
                case 8:
                    result = authorities[index].TransactionFee8;
                    break;
                case 9:
                    result = authorities[index].TransactionFee9;
                    break;
                case 10:
                    result = authorities[index].TransactionFee10;
                    break;
                default:
                    defaultFee = defFee != null ? defFee.Fee : 0;
                    result = authorities[index].IsFeeNego == true ? authorities[index].TransactionFee : defaultFee;
                    break;
            }

            return result;
        }


        public float GetTransactionFee(int product, int type)
        {
            float result = 0;
            int index = 0;

            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            switch (type)
            {
                case 1:
                    result = authorities[index].TransactionFee;
                    break;
                case 2:
                    result = authorities[index].TransactionFee2;
                    break;
                case 3:
                    result = authorities[index].TransactionFee3;
                    break;
                case 4:
                    result = authorities[index].TransactionFee4;
                    break;
                case 5:
                    result = authorities[index].TransactionFee5;
                    break;
                case 6:
                    result = authorities[index].TransactionFee6;
                    break;
                case 7:
                    result = authorities[index].TransactionFee7;
                    break;
                case 8:
                    result = authorities[index].TransactionFee8;
                    break;
                case 9:
                    result = authorities[index].TransactionFee9;
                    break;
                case 10:
                    result = authorities[index].TransactionFee10;
                    break;
                default:
                    result = authorities[index].TransactionFee;
                    break;
            }

            return result;
        }

        public String GetTransactionFeeCurrency(int product)
        {
            String result = "IDR";
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            result = authorities[index].CurrencyFee;

            return result;
        }

        public float GetUserLimit(int product)
        {
            float result = 0;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            result = authorities[index].UserLimit;

            return result;
        }

        public float GetUserLimit(int product, int type)
        {
            float result = 0;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            switch (type)
            {
                case 1:
                    result = authorities[index].UserLimit;
                    break;
                case 2:
                    result = authorities[index].UserLimit2;
                    break;
                case 3:
                    result = authorities[index].UserLimit3;
                    break;
                case 4:
                    result = authorities[index].UserLimit4;
                    break;
                case 5:
                    result = authorities[index].UserLimit5;
                    break;
                default:
                    result = authorities[index].UserLimit;
                    break;
            }

            return result;
        }

        public String[] GetUserLevels(int product)
        {
            String[] result = null;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            result = authorities[index].Levels;

            return result;
        }

        public bool OnUserLimit(String product, float amount)
        {
            bool result = false;
            IDictionary<String, float> workDictionary = GetUserLimitDictionary();
            foreach (String pk in workDictionary.Keys)
            {
                if (pk.StartsWith(product + "|"))
                {
                    if (workDictionary[pk] >= amount) result = true;
                    break;
                }
            }
            return result;
        }

        public bool isVerifier(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            if (index < authorities.Length)
            {
                if (null == authorities[index].Levels) authorities[index].Levels = new String[] { "" };
                return authorities[index].Levels.Contains<String>("VERIFY");
            }
            else
            {
                return false;
            }
        }

        public bool isApprover(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            if (index < authorities.Length)
            {
                if (null == authorities[index].Levels) authorities[index].Levels = new String[] { "" };
                return authorities[index].Levels.Contains<String>("APPROVE");
            }
            else
            {
                return false;
            }
        }

        public bool OnGroupLimit(String product, float amount)
        {
            bool result = false;
            IDictionary<String, float> workDictionary = GetGroupLimitDictionary();
            if (workDictionary.ContainsKey(product))
            {
                float limit = workDictionary[product];
                if (amount <= limit) result = true;
            }
            return result;
        }

        public String GetMenuLink(int id)
        {
            String result = "";
            String menuid = id + "|";
            if (linkMenuDictionary == null)
            {
                linkMenuDictionary = new Dictionary<int, String>();
                foreach (KeyValuePair<String, String[]> products in GetMenuDictionary())
                {
                    String[] subMenus = products.Value;
                    for (int i = 0; i < subMenus.Length; i++)
                    {
                        String[] subMenuDetail = subMenus[i].Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        linkMenuDictionary.Add(Int32.Parse(subMenuDetail[0]), subMenuDetail[2]);
                    }
                }
            }
            linkMenuDictionary.TryGetValue(id, out result);
            return result;
        }

        public static String GenerateMatrix(int[] fid)
        {
            String result = null;

            try
            {
                ISession session = NHibernateHelper.GetCurrentSession();
                IDictionary<int, Authority> fids = new Dictionary<int, Authority>();
                foreach (int id in fid)
                {
                    Authority auth = null;
                    Function function = session.Load(typeof(Function), id, LockMode.Read) as Function;
                    if (!fids.ContainsKey(function.Parent))
                    {
                        auth = new Authority();
                        Function parent = session.Load(typeof(Function), function.Parent, LockMode.Read) as Function;
                        auth.Product = parent.Id.ToString() + "|" + parent.Name;
                        fids.Add(parent.Id, auth);
                    }
                    auth = fids[function.Parent];
                    auth.AddMenu(function.Id.ToString() + "|" + function.Name + "|" + function.Link);
                }
                Authority[] authorities = new List<Authority>(fids.Values).ToArray<Authority>();
                result = QueryHelper.ArrayToXMLDataSource(typeof(Authority[]), authorities);
            }
            catch (HibernateException ex)
            {
                result = "ERROR" + ex.Message;
            }

            return result;
        }

        public void SetReports(int product, String[] reports)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].Reports = reports;
        }

        public String[] GetReports(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            return authorities[index].Reports;
        }

        //
        public void SetBriva(int product, String[] briva)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].Briva = briva;
        }

        //
        public String[] GetBriva(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            return authorities[index].Briva;
        }

        public bool IsExternalAccount(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            return authorities[index].OtherAccountAllowed;
        }

        /* update enkripsi*/
        public void SetEnkriptType(int product, Boolean type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].EnkriptAllowed = type;
        }

        public bool GetEnkriptType(int product)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            result = authorities[index].EnkriptAllowed;

            return result;
        }

        public bool isProductUser(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            return index < authorities.Length;

        }
        /* update iaaccount*/
        public void SetIaAccount(int product, Boolean type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].IAAccount = type;
        }

        public bool GetIaAccount(int product)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            result = authorities[index].IAAccount;

            return result;
        }

        public bool OnUserLimit(String product, double amount)
        {
            bool result = false;
            IDictionary<String, float> workDictionary = GetUserLimitDictionary();
            foreach (String pk in workDictionary.Keys)
            {
                if (pk.StartsWith(product + "|"))
                {
                    if (Convert.ToDouble(workDictionary[pk]) >= amount) result = true;
                    break;
                }
            }
            return result;
        }

        public bool OnUserLimitBPJSLimit(String product, double amount, out double limit)
        {
            bool result = false;
            limit = 0;
            IDictionary<String, float> workDictionary = GetUserLimitDictionary();
            foreach (String pk in workDictionary.Keys)
            {
                if (pk.StartsWith(product + "|"))
                {
                    limit = Convert.ToDouble(workDictionary[pk]);
                    if (limit >= amount) result = true;
                    break;
                }
            }
            return result;
        }

        public bool OnGroupLimit(String product, double amount)
        {
            bool result = false;
            IDictionary<String, float> workDictionary = GetGroupLimitDictionary();
            if (workDictionary.ContainsKey(product))
            {
                double limit = Convert.ToDouble(workDictionary[product]);
                if (amount <= limit) result = true;
            }
            return result;
        }

        public void SetTempString(int product, String temp, int type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            switch (type)
            {
                case 1:
                    authorities[index].Temp1 = temp;
                    break;
                case 2:
                    authorities[index].Temp2 = temp;
                    break;
                case 3:
                    authorities[index].Temp3 = temp;
                    break;
                case 4:
                    authorities[index].Temp4 = temp;
                    break;
                /*Add Sari, 011012*/
                case 5:
                    authorities[index].Temp5 = temp;
                    break;
                case 6:
                    authorities[index].Temp6 = temp;
                    break;
                /*End Sari, 011012*/
            }
        }

        public String GetTempString(int product, int type)
        {
            String result = "";
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            switch (type)
            {
                case 1:
                    result = authorities[index].Temp1;
                    break;
                case 2:
                    result = authorities[index].Temp2;
                    break;
                case 3:
                    result = authorities[index].Temp3;
                    break;
                case 4:
                    result = authorities[index].Temp4;
                    break;
                /*Add Sari, 011012*/
                case 5:
                    result = authorities[index].Temp5;
                    break;
                case 6:
                    result = authorities[index].Temp6;
                    break;
                /*End Sari, 011012*/
            }

            return result;
        }

        /*Add Sari, 26 Juli 2012*/
        public String GetTransactionURL(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            String resulturl = authorities[index].Temp1;

            return resulturl;
        }

        public String GetTransactionUID(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            String resultuid = authorities[index].Temp2;

            return resultuid;
        }

        public String GetTransactionPASS(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            String resultpass = authorities[index].Temp3;

            return resultpass;
        }

        public String GetTransactionMAKER(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            String resultmaker = authorities[index].Temp4;

            return resultmaker;
        }
        /*End Sari*/

        /*Add Sari, 011012*/
        public String GetTransactionEmail(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            String resultemail = authorities[index].Temp5;

            return resultemail;
        }
        /*End Sari, 011012*/

        //public String GetTransactionCabPersepsi(int product)
        //{
        //    int index = 0;
        //    foreach( Authority authority in authorities )
        //    {
        //        String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        //        if( int.Parse(sid[0]) == product )
        //            break;
        //        index++;
        //    }
        //    String resulturl = authorities[index].CabangPersepsi;

        //    return resulturl;
        //}

        /*Add rfq, 040614*/
        public String GetTransactionKodeFormatFile(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            String resultkodeformatfile = authorities[index].Temp6;

            return resultkodeformatfile;
        }
        /*End rfq, 040614*/



        /*Add rfq, 051015
        * Jika true maka Transasksi Langsung, tanpa verify/approve
        * Jika false mengikuti authority mgt
        */
        public bool isDirectTransaction(int product)
        {
            int index = 0;
            bool isDirectTransaction = false;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            isDirectTransaction = authorities[index].IsDirectTransaction;

            return isDirectTransaction;
        }


        public void SetDirectTransaction(int product, Boolean temp)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].IsDirectTransaction = temp;
        }
        /*End rfq, 040614*/



        public void SetKodeDistributor(int product, String kodeDistributor)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].KodeDistributor = kodeDistributor;
        }

        public String GetKodeDistributor(int product)
        {
            String result = "";
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            result = authorities[index].KodeDistributor;

            if (result == null) result = "";

            return result;
        }

        /* add wira 12112015 */
        public void SetKodePE(int product, String kodePE)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].KodePE = kodePE;
        }

        public String GetKodePE(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }
                String result = authorities[index].KodePE;

                if (result == null) result = "";

                return result;
            }
            catch { return String.Empty; }
        }
        /* end wira 12112015 */

        public void SetCabang(int product, String type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].CabangPersepsi = type;
        }

        public String GetCabang(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }
                String resultcabang = authorities[index].CabangPersepsi;

                if (resultcabang == null) resultcabang = "";

                return resultcabang;
            }
            catch { return String.Empty; }
        }

        public void SetStatusReff(int product, Boolean type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].IsNeedReff = type;
        }

        public Boolean GetStatusRef(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }
                Boolean resultstatusref = false;
                resultstatusref = authorities[index].IsNeedReff;

                return resultstatusref;
            }
            catch { return false; }
        }

        public void SetPajakAktif(int product, String type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].PajakAktif = type;
        }

        public String GetPajakAktif(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }
                String resultpajakaktif = authorities[index].PajakAktif;

                if (resultpajakaktif == null) resultpajakaktif = "00000";

                return resultpajakaktif;
            }
            catch { return "00000"; }
        }

        public String GetTransactionCabPersepsi(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product)
                        break;
                    index++;
                }
                String resulturl = authorities[index].CabangPersepsi;

                if (resulturl == null) resulturl = "";

                return resulturl;
            }
            catch { return String.Empty; }
        }

        public String GetAccount(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product)
                    break;
                index++;
            }
            String resulturl = authorities[index].Accounts[0];

            return resulturl;
        }

        public String[] GetAccountAll(int product)
        {
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product)
                    break;
                index++;
            }
            String[] resulturl = authorities[index].Accounts;

            return resulturl;
        }

        public Boolean isNeedPendingEmail(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }

                return !authorities[index].IsPendingEmail;
            }
            catch { return false; }
        }

        public Boolean isNeedDoneEmail(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }

                return !authorities[index].IsDoneEmail;
            }
            catch { return false; }
        }

        public void setNeedDoneNotif(int product, bool status)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].IsDoneEmail = !status;
        }

        public void setNeedPendingNotif(int product, bool status)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].IsPendingEmail = !status;
        }

        public void SetParentTrade(int product, String _parentTrade)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].ParentTrade = _parentTrade;
        }

        public String GetParentTrade(int product)
        {
            String result = "";
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            result = authorities[index].ParentTrade;

            if (result == null) result = "";

            return result;
        }

        //purnomo
        public void SetPKG(int product, String kmkpkg)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].Kmkpkg = kmkpkg;
        }

        public String GetPKG(int product)
        {
            String result = "";
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            result = authorities[index].Kmkpkg;

            if (result == null) result = "";

            return result;
        }
        //end purnomo

        public void SetBPHTBKabupaten(int product, String type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].BPHTBKabupaten = type;
        }

        public String GetBPHTBKabupaten(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }
                String resultBPHTBKabupaten = authorities[index].BPHTBKabupaten;

                if (resultBPHTBKabupaten == null) resultBPHTBKabupaten = "";

                return resultBPHTBKabupaten;
            }
            catch { return String.Empty; }
        }

        public void SetPBBKabupaten(int product, String type)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].PBBKabupaten = type;
        }

        public String GetPBBKabupaten(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }
                String resultPBBKabupaten = authorities[index].PBBKabupaten;

                if (resultPBBKabupaten == null) resultPBBKabupaten = "";

                return resultPBBKabupaten;
            }
            catch { return String.Empty; }
        }


        //purnomo 11112014
        public void SetBG(int product, String bg)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].Temp1 = bg;
        }

        public String GetBG(int product)
        {
            String result = "";
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }

            result = authorities[index].Temp1;

            if (result == null) result = "";

            return result;
        }
        //end purnomo

        //cashcardEP
        public void SetCcPTMN(int product, String giroCorp)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].Girocashcard = giroCorp;
        }

        public String GetCcPTMN(int product)
        {
            try
            {
                String result = "";
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }

                result = authorities[index].Girocashcard;

                if (result == null) result = "";

                return result;
            }
            catch { return String.Empty; }
        }

        //2017-03-09 sayedzul
        public bool GetByPassPendingIFT(int product)
        {
            try
            {
                int index = 0;
                foreach (Authority authority in authorities)
                {
                    String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (int.Parse(sid[0]) == product) break;
                    index++;
                }

                return authorities[index].IsByPassPendingIFT;
            }
            catch { return false; }
        }

        public void SetByPassPendingIFT(int product, bool status)
        {
            bool result = false;
            int index = 0;
            foreach (Authority authority in authorities)
            {
                String[] sid = authority.Product.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (int.Parse(sid[0]) == product) break;
                index++;
            }
            authorities[index].IsByPassPendingIFT = status;
        }
        //2017-03-09 sayedzul end

    }
}
