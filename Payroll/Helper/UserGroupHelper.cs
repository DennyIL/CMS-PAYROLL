using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Expression;
using BRIChannelSchedulerNew.Payroll.Pocos;
using BRIChannelSchedulerNew.Payroll.Helper;
using System.Threading;

/// <summary>
/// Summary description for TransferHelper
/// </summary>

namespace BRIChannelSchedulerNew.Payroll.Helper
{
    public abstract class UserGroupHelper
    {
        public static int CHECKER = 1;
        public static int APPROVER = 2;

        public UserGroupHelper()
        {
        }

        private static String getNextCheByUser(ISession session, int fid, int cid, String txcode, String curr, double amount, String debitacc)
        {
            String result = "";
            IList<UserMatrix2> txList = session.CreateCriteria(typeof(UserMatrix2))
                        .Add(Expression.Like("Matrix", "%\"" + fid.ToString() + "|%"))
                        .Add(Expression.Eq("Cid", cid))
                        .List<UserMatrix2>();

            if (txList.Count > 0)
            {
                IList<UserMatrix2> txList2 = new List<UserMatrix2>();

                foreach (UserMatrix2 um2 in txList)
                {
                    AuthorityHelper ah = new AuthorityHelper(um2.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um2.Matrix.Contains(">" + (fid + 3).ToString() + "|")); //remark sari 30092014 -- dibuka Harry

                    if (fid == 3610)
                    {
                        ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                            .Add(Expression.Eq("Id", um2.Cid))
                            .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um2.Matrix.Contains(">3615|") && (um2.Matrix.Contains(">3616|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um2.Matrix.Contains(">3616|")) ? true : false;
                        }
                    }
					if (fid == 3640)
                    {
                        ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                            .Add(Expression.Eq("Id", um2.Cid))
                            .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um2.Matrix.Contains(">3642|") && (um2.Matrix.Contains(">3643|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um2.Matrix.Contains(">3643|")) ? true : false;
                        }
                    }
                    if (fid == 3660)
                    {
                        ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                            .Add(Expression.Eq("Id", um2.Cid))
                            .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um2.Matrix.Contains(">3665|") && (um2.Matrix.Contains(">3666|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um2.Matrix.Contains(">3666|")) ? true : false;
                        }
                    }
                    if (fid == 1900)
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                    }
                    else if (fid == 1920 || fid == 3400) //1920:masstax, 3400:taxpln
                    {
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 3).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 7).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">3408|"));
                        }
                    }
                    else if (fid == 100)//rfq payroll
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                    }
                    else if (fid == 4030)//rfq mass credit
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                    }
					else if (fid == 4290) //Alfian masscashcard
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                        if (txcode.Equals("ChangeGroup CashCard Gen2"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 8).ToString() + "|"));
                        }
                        else if (txcode.Equals("Setting Kartu"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                        }
						else if (txcode.Equals("LifeCycle CashCard Gen2"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 4).ToString() + "|"));
                        }
                    }
                    else if (fid == 4040) //Purnomo cashcard Pertamina
                    {
                        if (txcode.Equals("Limit"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (4046).ToString() + "|"));
                        }
                        if (txcode.Equals("Block"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (4049).ToString() + "|"));
                        }
                        if (txcode.Equals("Report"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (4042).ToString() + "|"));
                        }
                    }

                    if (cid == 190) //untuk PLN
                    {
                        fid = 3400;
                    }

                    /*End Sari, 30092014*/

                    if (fid == 1140)
                    {
                        ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                            .Add(Expression.Eq("Id", um2.Cid))
                            .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um2.Matrix.Contains(">1143|") && (um2.Matrix.Contains(">1144|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um2.Matrix.Contains(">1144|")) ? true : false;
                        }
                    }
                    #endregion
                    if ((ah.isVerifier(fid)) && hasMenu)
                    {
                        txList2.Add(um2);
                    }
                }

                if (txList2.Count > 0)
                {

                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txList2.Count < pembagi) pembagi = txList2.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txList2.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txList2[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txList2[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];
                    ThdAuth[] fibArray = new ThdAuth[pembagi];
                    ThdAuth f;
                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);
                            f = new ThdAuth(factory, cid, fid, txcode, debitacc, curr, amount, bla[xy].ToArray<UserMatrix2>(), doneEvents[xy]);
                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuth ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }
            }

            return result;
        }

        private static String getNextCheByUser(ISession session, int fid, int cid, String txcode, String curr
                                                , double amount, String debitacc, IList<UserMatrix2> txList
                                                , IList<ClientAccount> clientAccounts, IList<Currency> currencies
                                                , IList<UserAccount> userAccounts, ClientMatrix cm)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            String result = "";
            //IList<UserMatrix2> txList = session.CreateCriteria(typeof(UserMatrix2))
            //            .Add(Expression.Like("Matrix", "%\"" + fid.ToString() + "|%"))
            //            .Add(Expression.Eq("Cid", cid))
            //            .List<UserMatrix2>();

            if (txList.Count > 0)
            {
                IList<UserMatrix2> txList2 = new List<UserMatrix2>();

                foreach (UserMatrix2 um2 in txList)
                {
                    AuthorityHelper ah = new AuthorityHelper(um2.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um2.Matrix.Contains(">" + (fid + 3).ToString() + "|")); //remark sari 30092014 -- dibuka Harry

                    if (fid == 3610)
                    {
                        //ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        //    .Add(Expression.Eq("Id", um2.Cid))
                        //    .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um2.Matrix.Contains(">3615|") && (um2.Matrix.Contains(">3616|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um2.Matrix.Contains(">3616|")) ? true : false;
                        }
                    }
					if (fid == 3640)
                    {
                        //ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        //    .Add(Expression.Eq("Id", um2.Cid))
                        //    .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um2.Matrix.Contains(">3642|") && (um2.Matrix.Contains(">3643|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um2.Matrix.Contains(">3643|")) ? true : false;
                        }
                    }
                    if (fid == 3660)
                    {
                        //ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        //    .Add(Expression.Eq("Id", um2.Cid))
                        //    .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um2.Matrix.Contains(">3665|") && (um2.Matrix.Contains(">3666|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um2.Matrix.Contains(">3666|")) ? true : false;
                        }
                    }
                    if (fid == 1900)
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                    }
                    else if (fid == 1920 || fid == 3400)
                    {
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 3).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 7).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">3408|"));
                        }
                    }
                    else if (fid == 100)//rfq payroll
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                    }
                    else if (fid == 4030)//rfq mass credit
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                    }

                    if (cid == 190) //untuk PLN
                    {
                        fid = 3400;
                    }

                    /*End Sari, 30092014*/

                    if (fid == 1140)
                    {
                        //ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        //    .Add(Expression.Eq("Id", um2.Cid))
                        //    .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um2.Matrix.Contains(">1143|") && (um2.Matrix.Contains(">1144|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um2.Matrix.Contains(">1144|")) ? true : false;
                        }
                    }
                    #endregion
                    if ((ah.isVerifier(fid)) && hasMenu)
                    {
                        txList2.Add(um2);
                    }
                }

                if (txList2.Count > 0)
                {

                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txList2.Count < pembagi) pembagi = txList2.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txList2.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txList2[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txList2[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];

                    //ThdAuth[] fibArray = new ThdAuth[pembagi];
                    //ThdAuth f;

                    ThdAuthMass[] fibArray = new ThdAuthMass[pembagi];
                    ThdAuthMass f;
                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);
                            //f = new ThdAuth(factory, cid, fid, txcode, debitacc, curr, amount
                            //                , bla[xy].ToArray<UserMatrix2>(), doneEvents[xy]);
                            f = new ThdAuthMass(factory, cid, fid, txcode, debitacc, curr
                                                , amount, bla[xy].ToArray<UserMatrix2>(), clientAccounts
                                                , currencies, userAccounts, doneEvents[xy]);
                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuthMass ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }
            }

            return result;
        }

        private static String getNextAppByUser(ISession session, int fid, int cid, String txcode, String curr, double amount, String debitacc)
        {
            String result = "";
            IList<UserMatrix2> txList = session.CreateCriteria(typeof(UserMatrix2))
                        .Add(Expression.Like("Matrix", "%\"" + fid.ToString() + "|%"))
                        .Add(Expression.Eq("Cid", cid))
                        .List<UserMatrix2>();

            if (txList.Count > 0)
            {
                IList<UserMatrix2> txList2 = new List<UserMatrix2>();

                foreach (UserMatrix2 um2 in txList)
                {
                    AuthorityHelper ah = new AuthorityHelper(um2.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um2.Matrix.Contains(">" + (fid + 3).ToString() + "|")); // remark by sari -- dibuka Harry

                    if (fid == 3610) hasMenu = (um2.Matrix.Contains(">3615|")) ? true : false;
					if (fid == 3640) hasMenu = (um2.Matrix.Contains(">3642|")) ? true : false;
                    if (fid == 3660) hasMenu = (um2.Matrix.Contains(">3665|")) ? true : false;

                    if (fid == 1900)
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                    }
                    else if (fid == 1920 || fid == 3400)
                    {
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 3).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 7).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">3408|"));
                        }
                    }
                    else if (fid == 100)//rfq payroll
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                    }
                    else if (fid == 4030)//rfq mass credit
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                    }
					else if (fid == 4290) //Alfian masscashcard
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                        if (txcode.Equals("ChangeGroup CashCard Gen2"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 8).ToString() + "|"));
                        }
                        else if (txcode.Equals("Setting Kartu"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                        }
						else if (txcode.Equals("LifeCycle CashCard Gen2"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 4).ToString() + "|"));
                        }
                    }
					
                    if (cid == 190) //untuk PLN
                    {
                        fid = 3400;
                    }
                    /*End Sari, 30092014*/
                    if (fid == 1140) hasMenu = (um2.Matrix.Contains(">1145|")) ? true : false;

                    #endregion
                    if ((ah.isApprover(fid)) && hasMenu)
                    {
                        txList2.Add(um2);
                    }
                }

                if (txList2.Count > 0)
                {

                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txList2.Count < pembagi) pembagi = txList2.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txList2.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txList2[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txList2[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];
                    ThdAuth[] fibArray = new ThdAuth[pembagi];
                    ThdAuth f;
                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);
                            f = new ThdAuth(factory, cid, fid, txcode, debitacc, curr, amount, bla[xy].ToArray<UserMatrix2>(), doneEvents[xy]);
                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuth ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }
            }

            return result;
        }

        private static String getNextAppByUser(ISession session, int fid, int cid, String txcode, String curr, double amount, String debitacc
                                            , IList<UserMatrix2> txList, IList<ClientAccount> clientAccounts
                                            , IList<Currency> currencies, IList<UserAccount> userAccounts)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             * Method ini digunakan untuk pemanggilan secara thread pada mass transaction,
             * sehingga tidak dilakukan query yang sama secara berulang-ulang
             * untuk transaksi dalam 1 batch.
             * Query yang berulang-ulang antara lain :
             *      - get client workflow
             *      - get user matrix
             *      
             */

            String result = "";
            /*
             * 2015-07-88 : Hidayat
             * 
             * Bagian ini sudah tidak diperlukan karena sudah disediakan dari parameter
             * 
             */

            //IList<UserMatrix2> txList = session.CreateCriteria(typeof(UserMatrix2))
            //            .Add(Expression.Like("Matrix", "%\"" + fid.ToString() + "|%"))
            //            .Add(Expression.Eq("Cid", cid))
            //            .List<UserMatrix2>();

            if (txList.Count > 0)
            {
                IList<UserMatrix2> txList2 = new List<UserMatrix2>();

                foreach (UserMatrix2 um2 in txList)
                {
                    AuthorityHelper ah = new AuthorityHelper(um2.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um2.Matrix.Contains(">" + (fid + 3).ToString() + "|")); // remark by sari -- dibuka Harry

                    if (fid == 3610) hasMenu = (um2.Matrix.Contains(">3615|")) ? true : false;
					if (fid == 3640) hasMenu = (um2.Matrix.Contains(">3642|")) ? true : false;
                    if (fid == 3660) hasMenu = (um2.Matrix.Contains(">3665|")) ? true : false;

                    if (fid == 1900)
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                    }
                    else if (fid == 1920 || fid == 3400)
                    {
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 3).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (fid + 7).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um2.Matrix.Contains(">3408|"));
                        }
                    }
                    else if (fid == 100)//rfq payroll
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                    }
                    else if (fid == 4030)//rfq mass credit
                    {
                        hasMenu = (um2.Matrix.Contains(">" + (fid + 2).ToString() + "|"));
                    }
                    else if (fid == 4040) //Purnomo cashcard Pertamina
                    {
                        if (txcode.Equals("Limit"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (4046).ToString() + "|"));
                        }
                        if (txcode.Equals("Block"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (4049).ToString() + "|"));
                        }
                        if (txcode.Equals("Report"))
                        {
                            hasMenu = (um2.Matrix.Contains(">" + (4042).ToString() + "|"));
                        }
                    }
                    if (cid == 190) //untuk PLN
                    {
                        fid = 3400;
                    }
                    /*End Sari, 30092014*/
                    if (fid == 1140) hasMenu = (um2.Matrix.Contains(">1145|")) ? true : false;

                    #endregion
                    if ((ah.isApprover(fid)) && hasMenu)
                    {
                        txList2.Add(um2);
                    }
                }

                if (txList2.Count > 0)
                {

                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txList2.Count < pembagi) pembagi = txList2.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txList2.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txList2[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txList2[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];
                    ThdAuthMass[] fibArray = new ThdAuthMass[pembagi];
                    ThdAuthMass f;
                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);
                            f = new ThdAuthMass(factory, cid, fid, txcode, debitacc, curr
                                                , amount, bla[xy].ToArray<UserMatrix2>(), clientAccounts
                                                , currencies, userAccounts, doneEvents[xy]);
                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuthMass ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }
            }

            return result;
        }

        private static int getGroupSeqNum(ISession session, int fid, int cid, int gid, int type)
        {

            IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                        .Add(Expression.Eq("Cid", cid))
                        .Add(Expression.Eq("Fid", fid))
                        .Add(Expression.Eq("Gid", gid))
                        .List<UserGroupWorkflow>();

            if (txList.Count > 0)
            {
                if (type == 1)
                {
                    return txList[0].Sv;
                }
                else
                {
                    return txList[0].Sa;
                }
            }
            else
            {
                return 99;
            }
        }

        private static int getGroupTotalWork(ISession session, int fid, int cid, int gid, int type)
        {

            IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                        .Add(Expression.Eq("Cid", cid))
                        .Add(Expression.Eq("Fid", fid))
                        .Add(Expression.Eq("Gid", gid))
                        .List<UserGroupWorkflow>();

            if (txList.Count > 0)
            {
                if (type == 1)
                {
                    return txList[0].Tv;
                }
                else
                {
                    return txList[0].Ta;
                }
            }
            else
            {
                return 99;
            }
        }

        private static int getGroupTotalWork(ISession session, int fid, int cid, int gid, int type, IList<UserGroupWorkflow> userGroupWorkflows)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            //IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
            //            .Add(Expression.Eq("Cid", cid))
            //            .Add(Expression.Eq("Fid", fid))
            //            .Add(Expression.Eq("Gid", gid))
            //            .List<UserGroupWorkflow>();

            IList<UserGroupWorkflow> txList = userGroupWorkflows.Where(ugw => ugw.Gid == gid).ToList<UserGroupWorkflow>();

            if (txList.Count > 0)
            {
                if (type == 1)
                {
                    return txList[0].Tv;
                }
                else
                {
                    return txList[0].Ta;
                }
            }
            else
            {
                return 99;
            }
        }

        public static String resultCleanser(ISession session, int cid, String AppList, String nextP)
        {
            nextP = "|" + nextP + "|";
            nextP = nextP.Replace("||", "|").Substring(1);
            String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < List.Length; i++)
            {
                String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString().Trim();
                IList<UserMap> user = session.CreateCriteria(typeof(UserMap))
                         .Add(Expression.Eq("ClientEntity", cid))
                         .Add(Expression.Eq("UserHandle", uhandle))
                         .List<UserMap>();

                if (user.Count > 0)
                {

                    nextP = nextP.Replace(user[0].UserEntity + "|", "");
                }
            }

            nextP = "|" + nextP + "|";
            return nextP.Replace("||", "|");
        }

        public static String resultCleanser(ISession session, IList<UserMap> umapList, String AppList, String nextP)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             * Method result cleanser untuk menghindari loop call to UserMap
             * 
             */ 
            nextP = "|" + nextP + "|";
            nextP = nextP.Replace("||", "|").Substring(1);
            String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < List.Length; i++)
            {
                String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString().Trim();
                UserMap umap = umapList.SingleOrDefault(p => p.UserHandle == uhandle);
                if (umap != null)
                    nextP = nextP.Replace(umap.UserEntity.ToString() + "|", "");
            }

            nextP = "|" + nextP + "|";
            return nextP.Replace("||", "|");
        }

        public static int getGroupByHandle(ISession session, String handle, int Cid)
        {
            try
            {
                UserGroupMaps ugm = session.CreateCriteria(typeof(UserGroupMaps))
                    .Add(Expression.Eq("Cid", Cid))
                    .Add(Expression.Eq("Handle", handle.ToUpper()))
                    .UniqueResult<UserGroupMaps>();

                return ugm.Gid;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static int getGroupByHandle(ISession session, String handle, int Cid, IList<UserGroupMaps> userGroupMaps)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            try
            {
                /*UserGroupMaps ugm = session.CreateCriteria(typeof(UserGroupMaps))
                    .Add(Expression.Eq("Cid", Cid))
                    .Add(Expression.Eq("Handle", handle.ToUpper()))
                    .UniqueResult<UserGroupMaps>();*/
                
                UserGroupMaps ugm = userGroupMaps.SingleOrDefault(u => u.Handle.ToUpper() == handle.ToUpper().Trim());

                if (null == ugm)
                {
                    return 0;
                }
                return ugm.Gid;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        //ApproverFinder

        private static int getNextAppGroupID(ISession session, int fid, int cid, String AppList)
        {
            String result = "";
            int lastGid = 0;
            int currGid = 0;

            if (!AppList.Equals(""))
            {
                String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                int[] gid = new int[List.Length];
                List<int> disGid = new List<int>(); ;
                for (int i = 0; i < List.Length; i++)
                {
                    String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                    int guid = getGroupByHandle(session, uhandle, cid);
                    gid[i] = guid;
                    if (!disGid.Contains(guid))
                    {
                        disGid.Add(guid);
                    }
                }

                for (int j = 0; j < disGid.Count; j++)
                {
                    int total = getGroupTotalWork(session, fid, cid, disGid[j], 2);
                    int[] array = Array.FindAll(gid, delegate(int i) { return i == disGid[j]; });
                    if (array.Length < total)
                    {
                        lastGid = disGid[j];
                        break;
                    }
                    else
                    {
                        currGid = disGid[j];
                    }
                }

                if (lastGid == 0)
                {
                    int seqA = getGroupSeqNum(session, fid, cid, currGid, 2);
                    IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Gt("Sa", seqA))
                             .AddOrder(new Order("Sa", true))
                             .List<UserGroupWorkflow>();
                    if (uGo.Count > 0)
                    {
                        lastGid = uGo[0].Gid;
                    }
                }
            }
            else
            {
                IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Gt("Sa", 0))
                             .AddOrder(new Order("Sa", true))
                             .List<UserGroupWorkflow>();
                if (uGo.Count > 0)
                {
                    lastGid = uGo[0].Gid;
                }
            }

            return lastGid;
        }

        private static String getNextAppByGroupID(ISession session, int fid, int cid, int gid, String txcode, String curr, double amount, String debitacc)
        {
            String result = "";
            IList<UserMatrix2> txListNew = new List<UserMatrix2>();
            IList<UserGroupMaps> txList = session.CreateCriteria(typeof(UserGroupMaps))
                         .Add(Expression.Eq("Cid", cid))
                         .Add(Expression.Eq("Gid", gid))
                         .List<UserGroupMaps>();

            if (txList.Count > 0)
            {

                foreach (UserGroupMaps um2 in txList)
                {
                    UserMatrix2 um = session.CreateCriteria(typeof(UserMatrix2))
                        .Add(Expression.Eq("Id", um2.Uid))
                        .UniqueResult<UserMatrix2>();

                    AuthorityHelper ah = new AuthorityHelper(um.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um.Matrix.Contains(">" + (fid + 3).ToString() + "|")); // remark sari -- dibuka Harry

                    if (fid == 3610) hasMenu = (um.Matrix.Contains(">3615|")) ? true : false;
					if (fid == 3640) hasMenu = (um.Matrix.Contains(">3642|")) ? true : false;
                    if (fid == 3660) hasMenu = (um.Matrix.Contains(">3665|")) ? true : false;

                    if (fid == 1900)
                    {
                        hasMenu = (um.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                    }
                    else if (fid == 1920 || fid == 3400)
                    {
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">" + (fid + 3).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">" + (fid + 7).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">3408|"));
                        }
                    }

                    if (cid == 190) //untuk PLN
                    {
                        fid = 3400;
                    }
                    /*End Sari, 30092014*/
                    if (fid == 1140) hasMenu = (um.Matrix.Contains(">1145|")) ? true : false;
                    
                    #endregion
                    if ((ah.isApprover(fid))&&hasMenu)
                    {
                        txListNew.Add(um);
                    }
                }

                if (txListNew.Count > 0)
                {
                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txListNew.Count < pembagi) pembagi = txListNew.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txListNew.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txListNew[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txListNew[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];
                    ThdAuth[] fibArray = new ThdAuth[pembagi];
                    ThdAuth f;
                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);
                            f = new ThdAuth(factory, cid, fid, txcode, debitacc, curr, amount, bla[xy].ToArray<UserMatrix2>(), doneEvents[xy]);
                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuth ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }
                
            }

            return result;

        }

        private static String getNextAppByGroupID(  ISession session, int fid, int cid, int gid, String txcode
                                                    , String curr, double amount, String debitacc
                                                    , IList<UserMatrix2> umatrixs, IList<UserGroupMaps> userGroupMaps
                                                    , IList<ClientAccount> clientAccounts, IList<Currency> currencies
                                                    , IList<UserAccount> userAccounts)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             * Method untuk menghindari looping pada UserGroupMaps dan UserMatrix2
             * 
             */
            String result = "";
            IList<UserMatrix2> txListNew = new List<UserMatrix2>();
            //IList<UserGroupMaps> txList = session.CreateCriteria(typeof(UserGroupMaps))
            //             .Add(Expression.Eq("Cid", cid))
            //             .Add(Expression.Eq("Gid", gid))
            //             .List<UserGroupMaps>();
            IList<UserGroupMaps> txList = userGroupMaps.Where(u => u.Gid == gid).ToList<UserGroupMaps>();

            if (txList.Count > 0)
            {

                foreach (UserGroupMaps um2 in txList)
                {
                    //UserMatrix2 um = session.CreateCriteria(typeof(UserMatrix2))
                    //    .Add(Expression.Eq("Id", um2.Uid))
                    //    .UniqueResult<UserMatrix2>();
                    UserMatrix2 um = umatrixs.SingleOrDefault(p => p.Id == um2.Uid);
                    
                    AuthorityHelper ah = new AuthorityHelper(um.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um.Matrix.Contains(">" + (fid + 3).ToString() + "|")); // remark sari -- dibuka Harry

                    if (fid == 3610) hasMenu = (um.Matrix.Contains(">3615|")) ? true : false;
					if (fid == 3640) hasMenu = (um.Matrix.Contains(">3642|")) ? true : false;
                    if (fid == 3660) hasMenu = (um.Matrix.Contains(">3665|")) ? true : false;

                    if (fid == 1900)
                    {
                        hasMenu = (um.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                    }
                    else if (fid == 1920 || fid == 3400)
                    {
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">" + (fid + 3).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">" + (fid + 7).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">3408|"));
                        }
                    }
                    else if (fid == 4040) //Purnomo cashcard Pertamina
                    {
                        if (txcode.Equals("Limit"))
                        {
                            hasMenu = (um.Matrix.Contains(">" + (4046).ToString() + "|"));
                        }
                        if (txcode.Equals("Block"))
                        {
                            hasMenu = (um.Matrix.Contains(">" + (4049).ToString() + "|"));
                        }
                        if (txcode.Equals("Report"))
                        {
                            hasMenu = (um.Matrix.Contains(">" + (4042).ToString() + "|"));
                        }
                    }

                    if (cid == 190) //untuk PLN
                    {
                        fid = 3400;
                    }
                    /*End Sari, 30092014*/
                    if (fid == 1140) hasMenu = (um.Matrix.Contains(">1145|")) ? true : false;

                    #endregion
                    if ((ah.isApprover(fid)) && hasMenu)
                    {
                        txListNew.Add(um);
                    }
                }

                if (txListNew.Count > 0)
                {
                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txListNew.Count < pembagi) pembagi = txListNew.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txListNew.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txListNew[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txListNew[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];
                    ThdAuthMass[] fibArray = new ThdAuthMass[pembagi];
                    ThdAuthMass f;
                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);
                            f = new ThdAuthMass(factory, cid, fid, txcode, debitacc, curr, amount
                                                , bla[xy].ToArray<UserMatrix2>()
                                                , clientAccounts
                                                , currencies
                                                , userAccounts
                                                , doneEvents[xy]);
                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuthMass ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }

            }

            return result;

        }

        private static String getRemainAppGroupID(ISession session, int fid, int cid, String AppList)
        {
            String result = "";
            String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            int[] gid = new int[List.Length];
            List<int> disGid = new List<int>(); ;
            for (int i = 0; i < List.Length; i++)
            {
                String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                int guid = getGroupByHandle(session, uhandle, cid);
                gid[i] = guid;
                if (!disGid.Contains(guid))
                {
                    disGid.Add(guid);
                }
            }

            for (int j = 0; j < disGid.Count; j++)
            {
                int total = getGroupTotalWork(session, fid, cid, disGid[j], 2);
                int[] array = Array.FindAll(gid, delegate(int i) { return i == disGid[j]; });
                if (array.Length >= total)
                {
                    if (result.Equals(""))
                    {
                        result += disGid[j];
                    }
                    else
                    {
                        result += "|" + disGid[j];
                    }
                }
            }
            
            IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                     .Add(Expression.Eq("Cid", cid))
                     .Add(Expression.Eq("Fid", fid))
                     .Add(Expression.Not(Expression.In("Gid",result.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries))))
                     .Add(Expression.Gt("Ta", 0))
                     .List<UserGroupWorkflow>();

            result = "";

            if (uGo.Count > 0)
            {
                for (int x = 0; x < uGo.Count; x++) {
                    if (result.Equals(""))
                    {
                        result += uGo[x].Gid;
                    }
                    else
                    {
                        result += "|" + uGo[x].Gid;
                    }
                }
            }
            

            return result;
        }

        private static String getRemainAppGroupID(ISession session, int fid, int cid, String AppList
                                                , IList<UserGroupMaps> userGroupMaps
                                                , IList<UserGroupWorkflow> userGroupWorkflows)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            String result = "";
            String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            int[] gid = new int[List.Length];
            List<int> disGid = new List<int>(); ;
            for (int i = 0; i < List.Length; i++)
            {
                String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                int guid = getGroupByHandle(session, uhandle, cid, userGroupMaps);
                gid[i] = guid;
                if (!disGid.Contains(guid))
                {
                    disGid.Add(guid);
                }
            }

            for (int j = 0; j < disGid.Count; j++)
            {
                int total = getGroupTotalWork(session, fid, cid, disGid[j], 2, userGroupWorkflows);
                int[] array = Array.FindAll(gid, delegate(int i) { return i == disGid[j]; });
                if (array.Length >= total)
                {
                    if (result.Equals(""))
                    {
                        result += disGid[j];
                    }
                    else
                    {
                        result += "|" + disGid[j];
                    }
                }
            }

            //IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
            //         .Add(Expression.Eq("Cid", cid))
            //         .Add(Expression.Eq("Fid", fid))
            //         .Add(Expression.Not(Expression.In("Gid", result.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries))))
            //         .Add(Expression.Gt("Ta", 0))
            //         .List<UserGroupWorkflow>();

            string[] existGid_ = result.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            IList<Int32> existGid = new List<Int32>();
            foreach (string item in existGid_)
                existGid.Add(int.Parse(item));

            IList<UserGroupWorkflow> uGo = userGroupWorkflows.Where(ugw => (ugw.Ta > 0 && !existGid.Contains(ugw.Gid))).ToList<UserGroupWorkflow>();

            result = "";

            if (uGo.Count > 0)
            {
                for (int x = 0; x < uGo.Count; x++)
                {
                    if (result.Equals(""))
                    {
                        result += uGo[x].Gid;
                    }
                    else
                    {
                        result += "|" + uGo[x].Gid;
                    }
                }
            }


            return result;
        }

        private static String getNextAppByRemainGroupID(ISession session, int fid, int cid, String arrGroup, String txcode, String curr, double amount, String debitacc)
        {
            String result = "";
            String[] List = arrGroup.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < List.Length; i++)
            {
                String temp = getNextAppByGroupID(session, fid, cid, int.Parse(List[i]), txcode, curr, amount, debitacc);
                if (!temp.Equals(""))
                {
                    if (result.Equals(""))
                    {
                        result += temp;
                    }
                    else
                    {
                        result += "|" + temp;
                    }
                }
            }
            return result;
        }

        private static String getNextAppByRemainGroupID(ISession session, int fid, int cid, String arrGroup
                                                    , String txcode, String curr, double amount, String debitacc
                                                    , IList<UserMatrix2> userMatrixs, IList<UserGroupMaps> userGroupMaps
                                                    , IList<ClientAccount> clientAccounts, IList<Currency> currencies
                                                    , IList<UserAccount> userAccounts)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            String result = "";
            String[] List = arrGroup.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < List.Length; i++)
            {
                IList<UserGroupMaps> ugm = userGroupMaps.Where(u => u.Gid == int.Parse(List[i])).ToList<UserGroupMaps>();

                String temp = getNextAppByGroupID(session, fid, cid, int.Parse(List[i]), txcode, curr, amount, debitacc
                                    , userMatrixs, ugm, clientAccounts, currencies, userAccounts);
                if (!temp.Equals(""))
                {
                    if (result.Equals(""))
                    {
                        result += temp;
                    }
                    else
                    {
                        result += "|" + temp;
                    }
                }
            }
            return result;
        }
        //CheckerFinder

        private static int getNextCheGroupID(ISession session, int fid, int cid, String AppList)
        {
            String result = "";
            int lastGid = 0;
            int currGid = 0;

            if (!AppList.Equals(""))
            {
                String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                int[] gid = new int[List.Length];
                List<int> disGid = new List<int>(); ;
                for (int i = 0; i < List.Length; i++)
                {
                    String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                    int guid = getGroupByHandle(session, uhandle, cid);
                    gid[i] = guid;
                    if (!disGid.Contains(guid))
                    {
                        disGid.Add(guid);
                    }
                }

                for (int j = 0; j < disGid.Count; j++)
                {
                    int total = getGroupTotalWork(session, fid, cid, disGid[j], 1);
                    int[] array = Array.FindAll(gid, delegate(int i) { return i == disGid[j]; });
                    if (array.Length < total)
                    {
                        lastGid = disGid[j];
                        break;
                    }
                    else
                    {
                        currGid = disGid[j];
                    }
                }

                if (lastGid == 0)
                {
                    int seqV = getGroupSeqNum(session, fid, cid, currGid, 1);
                    IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Gt("Sv", seqV))
                             .AddOrder(new Order("Sv", true))
                             .List<UserGroupWorkflow>();
                    if (uGo.Count > 0)
                    {
                        lastGid = uGo[0].Gid;
                    }
                }
            }
            else
            {
                IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                            .Add(Expression.Eq("Cid", cid))
                            .Add(Expression.Eq("Fid", fid))
                            .Add(Expression.Gt("Sv", 0))
                            .AddOrder(new Order("Sv", true))
                            .List<UserGroupWorkflow>();
                if (uGo.Count > 0)
                {
                    lastGid = uGo[0].Gid;
                }
            }

            return lastGid;
        }

        private static IList<int> getNextCheGroupIDTemp(ISession session, int fid, int cid, String AppList)
        {
            IList<int> nextGid = new List<int>();
            int counter = 0;
            int i = 0;

            #region new
            if (!AppList.Equals(""))
            {
                String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                int[] gid = new int[List.Length]; //menyimpan semua Gid dr AppList
                List<int> disGid = new List<int>(); //menyimpan semua Gid distinct
                String uhandle = "";
                int guid = 0;

                //simpan GrupID dari AppList
                for (i = 0; i < List.Length; i++)
                {
                    uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                    guid = getGroupByHandle(session, uhandle, cid);

                    gid[i] = guid;
                    if (!disGid.Contains(guid))
                    {
                        disGid.Add(guid);
                    }
                }
                int[] disGidCount = new int[disGid.Count]; // menyimpan jumlah tiap� Gid
                Array.Sort(gid);
                disGid.Sort();
                int gidTemp = 0;
                int countGid = 0;
                counter = 0;
                for (i = 0; i < gid.Length; i++)
                {
                    if (gid.Length == 1) disGidCount[i] = countGid + 1;
                    else
                    {
                        if (countGid == 0) gidTemp = gid[i];
                        if (gidTemp == gid[i]) countGid++;
                        try
                        {
                            if (gidTemp != gid[i + 1])
                            {
                                disGidCount[counter] = countGid;
                                counter++;
                                countGid = 0;
                            }
                        }
                        catch
                        {
                            disGidCount[counter] = countGid;
                            counter++;
                            countGid = 0;
                        }
                    }
                }

                counter = 0;
                int priority = 0;
                int priorityTemp = 0;
                List<int> completeGid = new List<int>(); //menyimpan semua Gid yang sudah komplit
                foreach (int item in disGid)
                {
                    priorityTemp = getPriorityTemp(session, fid, cid, disGid[counter], 1);

                    if (priorityTemp > priority) priority = priorityTemp;

                    int total = getGroupTotalWork(session, fid, cid, disGid[counter], 1);
                    if (disGidCount[counter] < total)
                    {
                        nextGid.Add(item);
                    }
                    else completeGid.Add(item);
                    counter++;
                }

                #region aman
                if (nextGid.Count != 0)
                {
                    IList<UserGroupWorkflow> uGo = null;
                    uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Eq("Sv", priority))
                             .Add(Expression.Not(Expression.In("Gid", nextGid.Concat(completeGid).ToArray())))
                             .AddOrder(Order.Asc("Sv"))
                             .List<UserGroupWorkflow>();
                    if (uGo.Count != 0)
                    {
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            nextGid.Add(item.Gid);
                        }
                    }
                }
                else
                {
                    bool markNextGrup = false;
                    IList<UserGroupWorkflow> uGo = null;
                    uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Eq("Sv", priority))
                             .AddOrder(Order.Asc("Sv"))
                             .List<UserGroupWorkflow>();

                    if (uGo.Count != 0)
                    {
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            nextGid.Add(item.Gid);
                        }

                        foreach (int a in disGid)
                            nextGid.Remove(a);
                        if (nextGid.Count == 0) markNextGrup = true;
                    }

                    if (markNextGrup)
                    {
                        uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Gt("Sv", priority))
                             .AddOrder(Order.Asc("Sv"))
                             .List<UserGroupWorkflow>();

                        IList<UserGroupWorkflow> uGW2 = new List<UserGroupWorkflow>();
                        IList<UserGroupWorkflow> uGW3 = new List<UserGroupWorkflow>();
                        int count2 = 0;
                        int count3 = 0;
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            if (item.Sv == 2)
                            {
                                count2++;
                                uGW2.Add(item);
                            }
                            else
                            {
                                count3++;
                                uGW3.Add(item);
                            }
                        }
                        if (count2 != 0) foreach (UserGroupWorkflow item in uGW2) nextGid.Add(item.Gid);

                        else foreach (UserGroupWorkflow item in uGW3) nextGid.Add(item.Gid);
                    }
                }
                #endregion
            }
            #endregion
            else
            {
                IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Gt("Sv", 0))
                             .AddOrder(Order.Asc("Sv"))
                             .List<UserGroupWorkflow>();

                IList<UserGroupWorkflow> uGW1 = new List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGW2 = new List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGW3 = new List<UserGroupWorkflow>();
                int count1 = 0;
                int count2 = 0;
                int count3 = 0;
                foreach (UserGroupWorkflow item in uGo)
                {
                    if (item.Sv == 1)
                    {
                        count1++;
                        uGW1.Add(item);
                    }
                    else if (item.Sv == 2)
                    {
                        count2++;
                        uGW2.Add(item);
                    }
                    else
                    {
                        count3++;
                        uGW3.Add(item);
                    }
                }
                if (count1 != 0) foreach (UserGroupWorkflow item in uGW1) nextGid.Add(item.Gid);

                else if (count2 != 0) foreach (UserGroupWorkflow item in uGW2) nextGid.Add(item.Gid);

                else foreach (UserGroupWorkflow item in uGW3) nextGid.Add(item.Gid);
            }

            return nextGid;
        }

        private static IList<int> getNextCheGroupIDTemp(ISession session, int fid, int cid, String AppList
                                                        , IList<UserGroupWorkflow> userGroupWorkflows
                                                        , IList<UserGroupMaps> userGroupMaps)
        {
            /*
             * 2015-07-08 : Hidayat
             */
            IList<int> nextGid = new List<int>();
            int counter = 0;
            int i = 0;

            #region new
            if (!AppList.Equals(""))
            {
                String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                int[] gid = new int[List.Length]; //menyimpan semua Gid dr AppList
                List<int> disGid = new List<int>(); //menyimpan semua Gid distinct
                String uhandle = "";
                int guid = 0;

                //simpan GrupID dari AppList
                for (i = 0; i < List.Length; i++)
                {
                    uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                    guid = getGroupByHandle(session, uhandle, cid, userGroupMaps);

                    gid[i] = guid;
                    if (!disGid.Contains(guid))
                    {
                        disGid.Add(guid);
                    }
                }
                int[] disGidCount = new int[disGid.Count]; // menyimpan jumlah tiap� Gid
                Array.Sort(gid);
                disGid.Sort();
                int gidTemp = 0;
                int countGid = 0;
                counter = 0;
                for (i = 0; i < gid.Length; i++)
                {
                    if (gid.Length == 1) disGidCount[i] = countGid + 1;
                    else
                    {
                        if (countGid == 0) gidTemp = gid[i];
                        if (gidTemp == gid[i]) countGid++;
                        try
                        {
                            if (gidTemp != gid[i + 1])
                            {
                                disGidCount[counter] = countGid;
                                counter++;
                                countGid = 0;
                            }
                        }
                        catch
                        {
                            disGidCount[counter] = countGid;
                            counter++;
                            countGid = 0;
                        }
                    }
                }

                counter = 0;
                int priority = 0;
                int priorityTemp = 0;
                List<int> completeGid = new List<int>(); //menyimpan semua Gid yang sudah komplit
                foreach (int item in disGid)
                {
                    IList<UserGroupWorkflow> ugw = userGroupWorkflows.Where(v => v.Gid == disGid[counter]).ToList<UserGroupWorkflow>();

                    priorityTemp = getPriorityTemp(session, fid, cid, disGid[counter], 1, ugw);

                    if (priorityTemp > priority) priority = priorityTemp;

                    int total = getGroupTotalWork(session, fid, cid, disGid[counter], 1, ugw);
                    if (disGidCount[counter] < total)
                    {
                        nextGid.Add(item);
                    }
                    else completeGid.Add(item);
                    counter++;
                }

                #region aman
                if (nextGid.Count != 0)
                {
                    IList<UserGroupWorkflow> uGo = null;
                    //uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                    //         .Add(Expression.Eq("Cid", cid))
                    //         .Add(Expression.Eq("Fid", fid))
                    //         .Add(Expression.Eq("Sv", priority))
                    //         .Add(Expression.Not(Expression.In("Gid", nextGid.Concat(completeGid).ToArray())))
                    //         .AddOrder(Order.Asc("Sv"))
                    //         .List<UserGroupWorkflow>();

                    uGo = userGroupWorkflows.Where(u => (u.Sv == priority && !nextGid.Concat(completeGid).ToArray().Contains(u.Gid)))
                            .OrderBy(y => y.Sv).ToList<UserGroupWorkflow>();

                    if (uGo.Count != 0)
                    {
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            nextGid.Add(item.Gid);
                        }
                    }
                }
                else
                {
                    bool markNextGrup = false;
                    IList<UserGroupWorkflow> uGo = null;
                    //uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                    //         .Add(Expression.Eq("Cid", cid))
                    //         .Add(Expression.Eq("Fid", fid))
                    //         .Add(Expression.Eq("Sv", priority))
                    //         .AddOrder(Order.Asc("Sv"))
                    //         .List<UserGroupWorkflow>();

                    uGo = userGroupWorkflows.Where(u => u.Sv == priority).OrderBy(y => y.Sv).ToList<UserGroupWorkflow>();

                    if (uGo.Count != 0)
                    {
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            nextGid.Add(item.Gid);
                        }

                        foreach (int a in disGid)
                            nextGid.Remove(a);
                        if (nextGid.Count == 0) markNextGrup = true;
                    }

                    if (markNextGrup)
                    {
                        //uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                        //     .Add(Expression.Eq("Cid", cid))
                        //     .Add(Expression.Eq("Fid", fid))
                        //     .Add(Expression.Gt("Sv", priority))
                        //     .AddOrder(Order.Asc("Sv"))
                        //     .List<UserGroupWorkflow>();

                        uGo = userGroupWorkflows.Where(u => (u.Sv > priority)).OrderBy(y => y.Sv).ToList<UserGroupWorkflow>();

                        IList<UserGroupWorkflow> uGW2 = new List<UserGroupWorkflow>();
                        IList<UserGroupWorkflow> uGW3 = new List<UserGroupWorkflow>();
                        int count2 = 0;
                        int count3 = 0;
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            if (item.Sv == 2)
                            {
                                count2++;
                                uGW2.Add(item);
                            }
                            else
                            {
                                count3++;
                                uGW3.Add(item);
                            }
                        }
                        if (count2 != 0) foreach (UserGroupWorkflow item in uGW2) nextGid.Add(item.Gid);

                        else foreach (UserGroupWorkflow item in uGW3) nextGid.Add(item.Gid);
                    }
                }
                #endregion
            }
            #endregion
            else
            {
                //IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                //             .Add(Expression.Eq("Cid", cid))
                //             .Add(Expression.Eq("Fid", fid))
                //             .Add(Expression.Gt("Sv", 0))
                //             .AddOrder(Order.Asc("Sv"))
                //             .List<UserGroupWorkflow>();

                IList<UserGroupWorkflow> uGo = userGroupWorkflows.Where(u => u.Sv > 0).OrderBy(v => v.Sv).ToList<UserGroupWorkflow>();

                IList<UserGroupWorkflow> uGW1 = new List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGW2 = new List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGW3 = new List<UserGroupWorkflow>();
                int count1 = 0;
                int count2 = 0;
                int count3 = 0;
                foreach (UserGroupWorkflow item in uGo)
                {
                    if (item.Sv == 1)
                    {
                        count1++;
                        uGW1.Add(item);
                    }
                    else if (item.Sv == 2)
                    {
                        count2++;
                        uGW2.Add(item);
                    }
                    else
                    {
                        count3++;
                        uGW3.Add(item);
                    }
                }
                if (count1 != 0) foreach (UserGroupWorkflow item in uGW1) nextGid.Add(item.Gid);

                else if (count2 != 0) foreach (UserGroupWorkflow item in uGW2) nextGid.Add(item.Gid);

                else foreach (UserGroupWorkflow item in uGW3) nextGid.Add(item.Gid);
            }

            return nextGid;
        }

        private static String getNextCheByGroupID(ISession session, int fid, int cid, int gid, String txcode, String curr, double amount, String debitacc)
        {
            String result = "";
            IList<UserMatrix2> txListNew = new List<UserMatrix2>();
            IList<UserGroupMaps> txList = session.CreateCriteria(typeof(UserGroupMaps))
                         .Add(Expression.Eq("Cid", cid))
                         .Add(Expression.Eq("Gid", gid))
                         .List<UserGroupMaps>();

            if (txList.Count > 0)
            {
                foreach (UserGroupMaps um2 in txList)
                {
                    UserMatrix2 um = session.CreateCriteria(typeof(UserMatrix2))
                        .Add(Expression.Eq("Id", um2.Uid))
                        .UniqueResult<UserMatrix2>();

                    AuthorityHelper ah = new AuthorityHelper(um.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um.Matrix.Contains(">" + (fid + 3).ToString() + "|")); // remark by sari -- dibuka Harry

                    if (fid == 3610)
                    {
                        ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        .Add(Expression.Eq("Id", um2.Cid))
                        .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um.Matrix.Contains(">3615|") && (um.Matrix.Contains(">3616|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um.Matrix.Contains(">3616|")) ? true : false;
                        }
                    }
					
                    if (fid == 3640)
                    {
                        ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        .Add(Expression.Eq("Id", um2.Cid))
                        .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um.Matrix.Contains(">3642|") && (um.Matrix.Contains(">3643|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um.Matrix.Contains(">3642|")) ? true : false;
                        }
                    }
                    if (fid == 3660)
                    {
                        ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        .Add(Expression.Eq("Id", um2.Cid))
                        .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um.Matrix.Contains(">3665|") && (um.Matrix.Contains(">3666|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um.Matrix.Contains(">3666|")) ? true : false;
                        }
                    } 
                    if (fid == 1900)
                    {
                        hasMenu = (um.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                    }
                    else if (fid == 1920 || fid == 3400)
                    {
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">" + (fid + 3).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">" + (fid + 7).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">3408|"));
                        }
                    }

                    if (cid == 190) //untuk PLN
                    {
                        fid = 3400;
                    }
                    /*End Sari, 30092014*/
                    if (fid == 1140)
                    {
                        ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        .Add(Expression.Eq("Id", um2.Cid))
                        .UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um.Matrix.Contains(">1143|") && (um.Matrix.Contains(">1144|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um.Matrix.Contains(">1144|")) ? true : false;
                        }
                    }
                    
                    #endregion
                    if ((ah.isVerifier(fid)) && hasMenu)
                    {
                        txListNew.Add(um);
                    }
                }

                if (txListNew.Count > 0)
                {

                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txListNew.Count < pembagi) pembagi = txListNew.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txListNew.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txListNew[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txListNew[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];
                    ThdAuth[] fibArray = new ThdAuth[pembagi];
                    ThdAuth f;
                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);
                            f = new ThdAuth(factory, cid, fid, txcode, debitacc, curr, amount, bla[xy].ToArray<UserMatrix2>(), doneEvents[xy]);
                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuth ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }
            }

            return result;

        }

        private static String getNextCheByGroupID(ISession session, int fid, int cid, int gid, String txcode
                                                    , String curr, double amount, String debitacc
                                                    , IList<UserMatrix2> umatrixs, IList<UserGroupMaps> userGroupMaps
                                                    , IList<ClientAccount> clientAccounts, IList<Currency> currencies
                                                    , IList<UserAccount> userAccounts, ClientMatrix cm)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            String result = "";
            IList<UserMatrix2> txListNew = new List<UserMatrix2>();
            //IList<UserGroupMaps> txList = session.CreateCriteria(typeof(UserGroupMaps))
            //             .Add(Expression.Eq("Cid", cid))
            //             .Add(Expression.Eq("Gid", gid))
            //             .List<UserGroupMaps>();
            IList<UserGroupMaps> txList = userGroupMaps.Where(u => u.Gid == gid).ToList<UserGroupMaps>();

            if (txList.Count > 0)
            {
                foreach (UserGroupMaps um2 in txList)
                {
                    //UserMatrix2 um = session.CreateCriteria(typeof(UserMatrix2))
                    //    .Add(Expression.Eq("Id", um2.Uid))
                    //    .UniqueResult<UserMatrix2>();
                    UserMatrix2 um = umatrixs.SingleOrDefault(z => z.Id == um2.Uid);

                    AuthorityHelper ah = new AuthorityHelper(um.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um.Matrix.Contains(">" + (fid + 3).ToString() + "|")); // remark by sari -- dibuka Harry

                    if (fid == 3610)
                    {
                        //ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        //.Add(Expression.Eq("Id", um2.Cid))
                        //.UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um.Matrix.Contains(">3615|") && (um.Matrix.Contains(">3616|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um.Matrix.Contains(">3616|")) ? true : false;
                        }
                    }
					if (fid == 3640)
                    {
                        //ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        //.Add(Expression.Eq("Id", um2.Cid))
                        //.UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um.Matrix.Contains(">3642|") && (um.Matrix.Contains(">3643|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um.Matrix.Contains(">3643|")) ? true : false;
                        }
                    }
                    if (fid == 3660)
                    {
                        //ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        //.Add(Expression.Eq("Id", um2.Cid))
                        //.UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um.Matrix.Contains(">3665|") && (um.Matrix.Contains(">3666|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um.Matrix.Contains(">3666|")) ? true : false;
                        }
                    }
                    if (fid == 1900)
                    {
                        hasMenu = (um.Matrix.Contains(">" + (fid + 5).ToString() + "|"));
                    }
                    else if (fid == 1920 || fid == 3400)
                    {
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">" + (fid + 3).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">" + (fid + 7).ToString() + "|"));
                        }
                        if (hasMenu == false)
                        {
                            hasMenu = (um.Matrix.Contains(">3408|"));
                        }
                    }
                    else if (fid == 4040) //Purnomo cashcard Pertamina
                    {
                        if (txcode.Equals("Limit"))
                        {
                            hasMenu = (um.Matrix.Contains(">" + (4046).ToString() + "|"));
                        }
                        if (txcode.Equals("Block"))
                        {
                            hasMenu = (um.Matrix.Contains(">" + (4049).ToString() + "|"));
                        }
                        if (txcode.Equals("Report"))
                        {
                            hasMenu = (um.Matrix.Contains(">" + (4042).ToString() + "|"));
                        }
                    }
                    if (cid == 190) //untuk PLN
                    {
                        fid = 3400;
                    }
                    /*End Sari, 30092014*/
                    if (fid == 1140)
                    {
                        //ClientMatrix cm = session.CreateCriteria(typeof(ClientMatrix))
                        //.Add(Expression.Eq("Id", um2.Cid))
                        //.UniqueResult<ClientMatrix>();

                        AuthorityHelper ahm = new AuthorityHelper(cm.Matrix);

                        if ((double)ahm.GetTransactionFee(fid, 6) <= 1)
                        {
                            hasMenu = (um.Matrix.Contains(">1143|") && (um.Matrix.Contains(">1144|"))) ? true : false;
                        }
                        else
                        {
                            hasMenu = (um.Matrix.Contains(">1144|")) ? true : false;
                        }
                    }

                    #endregion
                    if ((ah.isVerifier(fid)) && hasMenu)
                    {
                        txListNew.Add(um);
                    }
                }

                if (txListNew.Count > 0)
                {

                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txListNew.Count < pembagi) pembagi = txListNew.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txListNew.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txListNew[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txListNew[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];

                    //ThdAuth[] fibArray = new ThdAuth[pembagi];
                    //ThdAuth f;

                    ThdAuthMass[] fibArray = new ThdAuthMass[pembagi];
                    ThdAuthMass f;

                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);

                            //f = new ThdAuth(factory, cid, fid, txcode, debitacc, curr, amount, bla[xy].ToArray<UserMatrix2>(), doneEvents[xy]);

                            f = new ThdAuthMass(factory, cid, fid, txcode, debitacc, curr, amount
                                                , bla[xy].ToArray<UserMatrix2>()
                                                , clientAccounts
                                                , currencies
                                                , userAccounts
                                                , doneEvents[xy]);

                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuthMass ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }
            }

            return result;

        }

        private static String getRemainCheGroupID(ISession session, int fid, int cid, String AppList)
        {
            String result = "";
            String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            int[] gid = new int[List.Length];
            List<int> disGid = new List<int>(); ;
            for (int i = 0; i < List.Length; i++)
            {
                String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                int guid = getGroupByHandle(session, uhandle, cid);
                gid[i] = guid;
                if (!disGid.Contains(guid))
                {
                    disGid.Add(guid);
                }
            }

            for (int j = 0; j < disGid.Count; j++)
            {
                int total = getGroupTotalWork(session, fid, cid, disGid[j], 1);
                int[] array = Array.FindAll(gid, delegate(int i) { return i == disGid[j]; });
                if (array.Length >= total)
                {
                    if (result.Equals(""))
                    {
                        result += disGid[j];
                    }
                    else
                    {
                        result += "|" + disGid[j];
                    }
                }
            }

            IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                     .Add(Expression.Eq("Cid", cid))
                     .Add(Expression.Eq("Fid", fid))
                     .Add(Expression.Not(Expression.In("Gid", result.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries))))
                     .Add(Expression.Gt("Tv", 0))
                     .List<UserGroupWorkflow>();

            result = "";
            if (uGo.Count > 0)
            {
                for (int x = 0; x < uGo.Count; x++)
                {
                    if (result.Equals(""))
                    {
                        result += uGo[x].Gid;
                    }
                    else
                    {
                        result += "|" + uGo[x].Gid;
                    }
                }
            }

            return result;
        }

        private static String getRemainCheGroupID(ISession session, int fid, int cid, String AppList
                                                    , IList<UserGroupMaps> userGroupMaps
                                                    , IList<UserGroupWorkflow> userGroupWorkflows)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            String result = "";
            String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            int[] gid = new int[List.Length];
            List<int> disGid = new List<int>(); ;
            for (int i = 0; i < List.Length; i++)
            {
                String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                int guid = getGroupByHandle(session, uhandle, cid, userGroupMaps);
                gid[i] = guid;
                if (!disGid.Contains(guid))
                {
                    disGid.Add(guid);
                }
            }

            for (int j = 0; j < disGid.Count; j++)
            {
                int total = getGroupTotalWork(session, fid, cid, disGid[j], 1, userGroupWorkflows);
                int[] array = Array.FindAll(gid, delegate(int i) { return i == disGid[j]; });
                if (array.Length >= total)
                {
                    if (result.Equals(""))
                    {
                        result += disGid[j];
                    }
                    else
                    {
                        result += "|" + disGid[j];
                    }
                }
            }

            //IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
            //         .Add(Expression.Eq("Cid", cid))
            //         .Add(Expression.Eq("Fid", fid))
            //         .Add(Expression.Not(Expression.In("Gid", result.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries))))
            //         .Add(Expression.Gt("Tv", 0))
            //         .List<UserGroupWorkflow>();

            string[] existGid_ = result.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            IList<int> existGid = new List<int>();
            foreach (string item in existGid_)
                existGid.Add(int.Parse(item));

            IList<UserGroupWorkflow> uGo = userGroupWorkflows.Where(v => v.Tv > 0 && !existGid.Contains(v.Gid))
                                                    .ToList<UserGroupWorkflow>();

            result = "";
            if (uGo.Count > 0)
            {
                for (int x = 0; x < uGo.Count; x++)
                {
                    if (result.Equals(""))
                    {
                        result += uGo[x].Gid;
                    }
                    else
                    {
                        result += "|" + uGo[x].Gid;
                    }
                }
            }

            return result;
        }

        private static String getNextCheByRemainGroupID(ISession session, int fid, int cid, String arrGroup, String txcode, String curr, double amount, String debitacc)
        {
            String result = "";
            String[] List = arrGroup.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < List.Length; i++)
            {
                String temp = getNextCheByGroupID(session, fid, cid, int.Parse(List[i]), txcode, curr, amount, debitacc);
                if (!temp.Equals(""))
                {
                    if (result.Equals(""))
                    {
                        result += temp;
                    }
                    else
                    {
                        result += "|" + temp;
                    }
                }
            }
            return result;
        }

        private static String getNextCheByRemainGroupID(ISession session, int fid, int cid, String arrGroup
                                                    , String txcode, String curr, double amount, String debitacc
                                                    , IList<UserMatrix2> userMatrixs, IList<UserGroupMaps> userGroupMaps
                                                    , IList<ClientAccount> clientAccounts, IList<Currency> currencies
                                                    , IList<UserAccount> userAccounts, ClientMatrix clientMatrix)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            String result = "";
            String[] List = arrGroup.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < List.Length; i++)
            {
                IList<UserGroupMaps> ugm = userGroupMaps.Where(u => u.Gid == int.Parse(List[i])).ToList<UserGroupMaps>();

                String temp = getNextCheByGroupID(session, fid, cid, int.Parse(List[i]), txcode, curr, amount, debitacc
                                                    , userMatrixs, ugm, clientAccounts, currencies, userAccounts, clientMatrix);
                if (!temp.Equals(""))
                {
                    if (result.Equals(""))
                    {
                        result += temp;
                    }
                    else
                    {
                        result += "|" + temp;
                    }
                }
            }
            return result;
        }

        //MakingFinderForPPOnly

        public static String getNextMakerPP(ISession session, int fid, int cid, String txcode, String curr, double amount, String debitacc)
        {
            String result = "";
            IList<UserMatrix2> txList = session.CreateCriteria(typeof(UserMatrix2))
                        .Add(Expression.Like("Matrix", "%\"" + fid.ToString() + "|%"))
                        .Add(Expression.Eq("Cid", cid))
                        .List<UserMatrix2>();

            if (txList.Count > 0)
            {
                IList<UserMatrix2> txList2 = new List<UserMatrix2>();

                foreach (UserMatrix2 um2 in txList)
                {
                    AuthorityHelper ah = new AuthorityHelper(um2.Matrix);
                    #region cekMenu
                    Boolean hasMenu = (um2.Matrix.Contains(">1142|") && (um2.Matrix.Contains(">1143|"))) ? true : false;
                    #endregion

                    if ((!ah.isVerifier(fid)) && (!ah.isApprover(fid)) && hasMenu)
                    {
                        txList2.Add(um2);
                    }
                }

                if (txList2.Count > 0)
                {
                    #region Pembagian Kue
                    int pembagi = 20;
                    if (txList2.Count < pembagi) pembagi = txList2.Count;

                    IList<UserMatrix2>[] bla = new List<UserMatrix2>[pembagi];

                    for (int jx = 0; jx < txList2.Count; jx++)
                    {
                        int mod = jx % pembagi;
                        if (bla[mod] == null)
                        {
                            bla[mod] = new List<UserMatrix2>();
                            bla[mod].Add(txList2[jx]);
                        }
                        else
                        {
                            bla[mod].Add(txList2[jx]);
                        }
                    }
                    #endregion

                    #region process
                    ManualResetEvent[] doneEvents = new ManualResetEvent[pembagi];
                    ThdAuth[] fibArray = new ThdAuth[pembagi];
                    ThdAuth f;
                    ISessionFactory factory = session.SessionFactory;

                    for (int xy = 0; xy < pembagi; xy++)
                    {
                        try
                        {
                            doneEvents[xy] = new ManualResetEvent(false);
                            f = new ThdAuth(factory, cid, fid, txcode, debitacc, curr, amount, bla[xy].ToArray<UserMatrix2>(), doneEvents[xy]);
                            fibArray[xy] = f;
                            ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, xy);

                        }
                        catch (Exception ex)
                        {
                            GC.Collect();
                        }
                    }

                    // Wait for all threads in pool to calculation...
                    WaitHandle.WaitAll(doneEvents);
                    // Display the results...
                    int counter = 0;
                    while (counter < pembagi)
                    {
                        ThdAuth ff = fibArray[counter];

                        if (result.Equals(""))
                        {
                            result += ff.listId;
                        }
                        else
                        {
                            result += "|" + ff.listId;
                        }

                        counter++;
                    }
                    #endregion
                }
            }

            return result;
        }

        public static Boolean isSinarMasCID(ISession session, int cid)
        {
            BRIChannelSchedulerNew.Payroll.Pocos.Parameter param = session.Load<BRIChannelSchedulerNew.Payroll.Pocos.Parameter>("CID_GROUP_SINARMAS");

            String meter = param.Data;

            return meter.Contains("|"+cid+"|");
        }

        public static String getNextChecker(ISession session, int fid, UserMap currP, String checkerInfo, String instCode, String debAcc, String curr, Double amount)
        {

            #region workflow getter
            ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
                    .Add(Expression.Eq("Id", currP.ClientEntity))
                    .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion


            String nextProcessor = "";

            if (!wf.isCheByGroup)
            {
                nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc));
            }
            else
            {
                if (wf.isPrivateChe)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session,currP.UserHandle.Trim(),currP.ClientEntity), instCode, curr, amount, debAcc));
                }
                else
                {
                    if (wf.isSerialChe)
                    {
                        int nextGroupId = UserGroupHelper.getNextCheGroupID(session, fid, currP.ClientEntity, checkerInfo);
                        nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, currP.ClientEntity, nextGroupId, instCode, curr, amount, debAcc));
                    }
                    else
                    {
                        String arrProcessor = UserGroupHelper.getRemainCheGroupID(session, fid, currP.ClientEntity, checkerInfo);
                        nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc));
                    }
                }
            }

            return nextProcessor;
        }

        public static String getNextApprover(ISession session, int fid, UserMap currP, String approveInfo, String instCode, String debAcc, String curr, Double amount)
        {

            #region workflow getter
            ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
                    .Add(Expression.Eq("Id", currP.ClientEntity))
                    .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion


            String nextProcessor = "";

            if (!wf.isAppByGroup)
            {
                nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc));
            }
            else
            {
                if (wf.isPrivateApp)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity), instCode, curr, amount, debAcc));
                }
                else
                {
                    if (wf.isSerialApp)
                    {
                        int nextGroupId = UserGroupHelper.getNextAppGroupID(session, fid, currP.ClientEntity, approveInfo);
                        nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, nextGroupId, instCode, curr, amount, debAcc));
                    }
                    else
                    {
                        String arrProcessor = UserGroupHelper.getRemainAppGroupID(session, fid, currP.ClientEntity, approveInfo);
                        nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc));
                    }
                }
            }

            return nextProcessor;
        }

        public static Boolean isAuthComplete(ISession session, UserMap umap, int fid,String checkerInfo,String approverInfo,String instCode, String debitAcc,String curr, Double amount, out String msg)
        {
            Boolean result = true;
            String nextProcessor = "";
            String[] arrProc;
            String[] arrDone;
            msg = "";
            int cid = umap.ClientEntity;

            #region workflow getter
            ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
                    .Add(Expression.Eq("Id", umap.ClientEntity))
                    .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion

            //Approver
            #region Check Approver
            if (wf.TotalApprover > 0)
            {
                if (!wf.isAppByGroup)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc));
                    if (nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        msg = "On Approver";
                        return false;
                    }
                    else {
                        arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrProc.Length+arrDone.Length < wf.TotalApprover) {
                            msg = "On Approver";
                            return false;
                        }
                    }
                }
                else
                {
                    if (wf.isPrivateApp)
                    {
                        nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, UserGroupHelper.getGroupByHandle(session,umap.UserHandle,umap.ClientEntity), instCode, curr, amount, debitAcc));
                        if (nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            msg = "On Approver";
                            return false;
                        }
                        else
                        {
                            arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrProc.Length + arrDone.Length < wf.TotalApprover)
                            {
                                msg = "On Approver";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                               .Add(Expression.Eq("Cid", cid))
                               .Add(Expression.Eq("Fid", fid))
                               .Add(Expression.Gt("Ta", 0))
                               .List<UserGroupWorkflow>();
                        if (txList.Count > 0)
                        {
                            for (int i = 0; i < txList.Count; i++)
                            {
                                nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc));
                                if (nextProcessor.Trim().Replace("|", "").Equals(""))
                                {
                                    msg = "On Approver";
                                    return false;
                                }
                                else
                                {
                                    arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arrProc.Length + arrDone.Length < txList[i].Ta)
                                    {
                                        msg = "On Approver";
                                        return false;
                                    }
                                }
                            }
                        }
                        else {
                            if (nextProcessor.Trim().Replace("|", "").Equals(""))
                            {
                                msg = "On Approver";
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion

            //Verifier
            #region Check Verify
            if (wf.TotalVerifier > 0)
            {
                if (!wf.isCheByGroup)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc));
                    if (nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        msg = "On Verifier";
                        return false;
                    }
                    else
                    {
                        arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrProc.Length+arrDone.Length < wf.TotalVerifier)
                        {
                            msg = "On Verifier";
                            return false;
                        }
                    }
                }
                else
                {
                    if (wf.isPrivateChe)
                    {
                        nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity), instCode, curr, amount, debitAcc));
                        if (nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            msg = "On Verifier";
                            return false;
                        }
                        else
                        {
                            arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrProc.Length + arrDone.Length < wf.TotalVerifier)
                            {
                                msg = "On Verifier";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                           .Add(Expression.Eq("Cid", cid))
                           .Add(Expression.Eq("Fid", fid))
                           .Add(Expression.Gt("Tv", 0))
                           .List<UserGroupWorkflow>();

                        if (txList.Count > 0)
                        {
                            for (int i = 0; i < txList.Count; i++)
                            {
                                nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc));
                                if (nextProcessor.Trim().Replace("|", "").Equals(""))
                                {
                                    msg = "On Verifier";
                                    return false;
                                }
                                else
                                {
                                    arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arrProc.Length + arrDone.Length < txList[i].Tv)
                                    {
                                        msg = "On Verifier";
                                        return false;
                                    }
                                }
                            }
                        }
                        else {
                            if (nextProcessor.Trim().Replace("|", "").Equals(""))
                            {
                                msg = "On Verifier";
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion

            return result;
        }

        private static int getPriorityTemp(ISession session, int fid, int cid, int gid, int type)
        {

            IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                        .Add(Expression.Eq("Cid", cid))
                        .Add(Expression.Eq("Fid", fid))
                        .Add(Expression.Eq("Gid", gid))
                        .List<UserGroupWorkflow>();

            if (txList.Count > 0)
            {
                if (type == 1)
                {
                    return txList[0].Sv;
                }
                else
                {
                    return txList[0].Sa;
                }
            }
            else
            {
                return 99;
            }
        }

        private static int getPriorityTemp(ISession session, int fid, int cid, int gid, int type, IList<UserGroupWorkflow> userGroupWorkflows)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            //IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
            //            .Add(Expression.Eq("Cid", cid))
            //            .Add(Expression.Eq("Fid", fid))
            //            .Add(Expression.Eq("Gid", gid))
            //            .List<UserGroupWorkflow>();

            IList<UserGroupWorkflow> txList = userGroupWorkflows.Where(x => x.Gid == gid).ToList<UserGroupWorkflow>();

            if (txList.Count > 0)
            {
                if (type == 1)
                {
                    return txList[0].Sv;
                }
                else
                {
                    return txList[0].Sa;
                }
            }
            else
            {
                return 99;
            }
        }

        private static IList<int> getNextAppGroupIDTemp(ISession session, int fid, int cid, String AppList)
        {
            IList<int> nextGid = new List<int>();
            int counter = 0;
            int i = 0;

            if (!AppList.Equals(""))
            {
                String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                int[] gid = new int[List.Length]; //menyimpan semua Gid dr AppList
                List<int> disGid = new List<int>(); //menyimpan semua Gid distinct

                //simpan GrupID dari AppList
                for (i = 0; i < List.Length; i++)
                {
                    String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                    int guid = getGroupByHandle(session, uhandle, cid);

                    gid[i] = guid;
                    if (!disGid.Contains(guid))
                    {
                        disGid.Add(guid);
                    }
                }
                int[] disGidCount = new int[disGid.Count]; // menyimpan jumlah tiap� Gid
                Array.Sort(gid);
                disGid.Sort();
                int gidTemp = 0;
                int countGid = 0;
                counter = 0;
                for (i = 0; i < gid.Length; i++)
                {
                    if (gid.Length == 1) disGidCount[i] = countGid + 1;
                    else
                    {
                        if (countGid == 0) gidTemp = gid[i];
                        if (gidTemp == gid[i]) countGid++;
                        try
                        {
                            if (gidTemp != gid[i + 1])
                            {
                                disGidCount[counter] = countGid;
                                counter++;
                                countGid = 0;
                            }
                        }
                        catch
                        {
                            disGidCount[counter] = countGid;
                            counter++;
                            countGid = 0;
                        }
                    }
                }

                counter = 0;
                int priority = 0;
                int priorityTemp = 0;
                List<int> completeGid = new List<int>(); //menyimpan semua Gid yang sudah komplit
                foreach (int item in disGid)
                {
                    priorityTemp = getPriorityTemp(session, fid, cid, disGid[counter], 2);

                    if (priorityTemp > priority) priority = priorityTemp;

                    int total = getGroupTotalWork(session, fid, cid, disGid[counter], 2);
                    if (disGidCount[counter] < total)
                    {
                        nextGid.Add(item);
                    }
                    else completeGid.Add(item);
                    counter++;

                }

                #region aman
                if (nextGid.Count != 0)
                {
                    IList<UserGroupWorkflow> uGo = null;
                    uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Eq("Sa", priority))
                             .Add(Expression.Not(Expression.In("Gid", nextGid.Concat(completeGid).ToArray())))
                             .AddOrder(Order.Asc("Sa"))
                             .List<UserGroupWorkflow>();
                    if (uGo.Count != 0)
                    {
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            nextGid.Add(item.Gid);
                        }
                    }
                }
                else
                {
                    bool markNextGrup = false;
                    IList<UserGroupWorkflow> uGo = null;
                    uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Eq("Sa", priority))
                             .AddOrder(Order.Asc("Sa"))
                             .List<UserGroupWorkflow>();

                    if (uGo.Count != 0)
                    {
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            nextGid.Add(item.Gid);
                        }

                        foreach (int b in nextGid) Console.WriteLine("nextGid1: " + b);
                        foreach (int a in disGid)
                            nextGid.Remove(a);

                        foreach (int c in nextGid) Console.WriteLine("nextGid2: " + c);
                        if (nextGid.Count == 0) markNextGrup = true;
                    }

                    if (markNextGrup)
                    {
                        uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Gt("Sa", priority))
                             .AddOrder(Order.Asc("Sa"))
                             .List<UserGroupWorkflow>();

                        IList<UserGroupWorkflow> uGW2 = new List<UserGroupWorkflow>();
                        IList<UserGroupWorkflow> uGW3 = new List<UserGroupWorkflow>();
                        int count2 = 0;
                        int count3 = 0;
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            if (item.Sa == 2)
                            {
                                count2++;
                                uGW2.Add(item);
                            }
                            else
                            {
                                count3++;
                                uGW3.Add(item);
                            }
                        }
                        if (count2 != 0) foreach (UserGroupWorkflow item in uGW2) nextGid.Add(item.Gid);

                        else foreach (UserGroupWorkflow item in uGW3) nextGid.Add(item.Gid);
                    }
                }
                #endregion
            }
            else
            {
                IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                             .Add(Expression.Eq("Cid", cid))
                             .Add(Expression.Eq("Fid", fid))
                             .Add(Expression.Gt("Sa", 0))
                             .AddOrder(Order.Asc("Sa"))
                    //.AddOrder(new Order("Sa", true))
                             .List<UserGroupWorkflow>();

                IList<UserGroupWorkflow> uGW1 = new List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGW2 = new List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGW3 = new List<UserGroupWorkflow>();
                int count1 = 0;
                int count2 = 0;
                int count3 = 0;
                foreach (UserGroupWorkflow item in uGo)
                {
                    if (item.Sa == 1)
                    {
                        count1++;
                        uGW1.Add(item);
                    }
                    else if (item.Sa == 2)
                    {
                        count2++;
                        uGW2.Add(item);
                    }
                    else
                    {
                        count3++;
                        uGW3.Add(item);
                    }
                }
                if (count1 != 0) foreach (UserGroupWorkflow item in uGW1) nextGid.Add(item.Gid);

                else if (count2 != 0) foreach (UserGroupWorkflow item in uGW2) nextGid.Add(item.Gid);

                else foreach (UserGroupWorkflow item in uGW3) nextGid.Add(item.Gid);
            }
            return nextGid;
        }

        private static IList<int> getNextAppGroupIDTemp(ISession session, int fid, int cid, String AppList
                                                , IList<UserGroupWorkflow> userGroupWorkflows
                                                , IList<UserGroupMaps> userGroupMaps)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             */
            IList<int> nextGid = new List<int>();
            int counter = 0;
            int i = 0;

            if (!AppList.Equals(""))
            {
                String[] List = AppList.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                int[] gid = new int[List.Length]; //menyimpan semua Gid dr AppList
                List<int> disGid = new List<int>(); //menyimpan semua Gid distinct

                //simpan GrupID dari AppList
                for (i = 0; i < List.Length; i++)
                {
                    String uhandle = List[i].Substring(0, List[i].LastIndexOf("-")).ToString();
                    int guid = getGroupByHandle(session, uhandle, cid, userGroupMaps);

                    gid[i] = guid;
                    if (!disGid.Contains(guid))
                    {
                        disGid.Add(guid);
                    }
                }
                int[] disGidCount = new int[disGid.Count]; // menyimpan jumlah tiap� Gid
                Array.Sort(gid);
                disGid.Sort();
                int gidTemp = 0;
                int countGid = 0;
                counter = 0;
                for (i = 0; i < gid.Length; i++)
                {
                    if (gid.Length == 1) disGidCount[i] = countGid + 1;
                    else
                    {
                        if (countGid == 0) gidTemp = gid[i];
                        if (gidTemp == gid[i]) countGid++;
                        try
                        {
                            if (gidTemp != gid[i + 1])
                            {
                                disGidCount[counter] = countGid;
                                counter++;
                                countGid = 0;
                            }
                        }
                        catch
                        {
                            disGidCount[counter] = countGid;
                            counter++;
                            countGid = 0;
                        }
                    }
                }

                counter = 0;
                int priority = 0;
                int priorityTemp = 0;
                List<int> completeGid = new List<int>(); //menyimpan semua Gid yang sudah komplit
                foreach (int item in disGid)
                {
                    IList<UserGroupWorkflow> ugw = userGroupWorkflows.Where(u => u.Gid == disGid[counter]).ToList<UserGroupWorkflow>();

                    priorityTemp = getPriorityTemp(session, fid, cid, disGid[counter], 2, ugw);

                    if (priorityTemp > priority) priority = priorityTemp;

                    int total = getGroupTotalWork(session, fid, cid, disGid[counter], 2, ugw);
                    if (disGidCount[counter] < total)
                    {
                        nextGid.Add(item);
                    }
                    else completeGid.Add(item);
                    counter++;

                }

                #region aman
                if (nextGid.Count != 0)
                {
                    IList<UserGroupWorkflow> uGo = null;
                    //uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                    //         .Add(Expression.Eq("Cid", cid))
                    //         .Add(Expression.Eq("Fid", fid))
                    //         .Add(Expression.Eq("Sa", priority))
                    //         .Add(Expression.Not(Expression.In("Gid", nextGid.Concat(completeGid).ToArray())))
                    //         .AddOrder(Order.Asc("Sa"))
                    //         .List<UserGroupWorkflow>();

                    uGo = userGroupWorkflows.Where(x => (x.Sa == priority && !nextGid.Concat(completeGid).ToArray().Contains(x.Gid)))
                                            .OrderBy(s => s.Sa).ToList<UserGroupWorkflow>();
                    if (uGo.Count != 0)
                    {
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            nextGid.Add(item.Gid);
                        }
                    }
                }
                else
                {
                    bool markNextGrup = false;
                    IList<UserGroupWorkflow> uGo = null;
                    //uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                    //         .Add(Expression.Eq("Cid", cid))
                    //         .Add(Expression.Eq("Fid", fid))
                    //         .Add(Expression.Eq("Sa", priority))
                    //         .AddOrder(Order.Asc("Sa"))
                    //         .List<UserGroupWorkflow>();

                    uGo = userGroupWorkflows.Where(v => v.Sa == priority).OrderBy(y => y.Sa).ToList<UserGroupWorkflow>();

                    if (uGo.Count != 0)
                    {
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            nextGid.Add(item.Gid);
                        }

                        foreach (int b in nextGid) Console.WriteLine("nextGid1: " + b);
                        foreach (int a in disGid)
                            nextGid.Remove(a);

                        foreach (int c in nextGid) Console.WriteLine("nextGid2: " + c);
                        if (nextGid.Count == 0) markNextGrup = true;
                    }

                    if (markNextGrup)
                    {
                        //uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                        //     .Add(Expression.Eq("Cid", cid))
                        //     .Add(Expression.Eq("Fid", fid))
                        //     .Add(Expression.Gt("Sa", priority))
                        //     .AddOrder(Order.Asc("Sa"))
                        //     .List<UserGroupWorkflow>();

                        uGo = userGroupWorkflows.Where(v => v.Sa > priority).OrderBy(y => y.Sa).ToList<UserGroupWorkflow>();

                        IList<UserGroupWorkflow> uGW2 = new List<UserGroupWorkflow>();
                        IList<UserGroupWorkflow> uGW3 = new List<UserGroupWorkflow>();
                        int count2 = 0;
                        int count3 = 0;
                        foreach (UserGroupWorkflow item in uGo)
                        {
                            if (item.Sa == 2)
                            {
                                count2++;
                                uGW2.Add(item);
                            }
                            else
                            {
                                count3++;
                                uGW3.Add(item);
                            }
                        }
                        if (count2 != 0) foreach (UserGroupWorkflow item in uGW2) nextGid.Add(item.Gid);

                        else foreach (UserGroupWorkflow item in uGW3) nextGid.Add(item.Gid);
                    }
                }
                #endregion
            }
            else
            {
                //IList<UserGroupWorkflow> uGo = session.CreateCriteria(typeof(UserGroupWorkflow))
                //             .Add(Expression.Eq("Cid", cid))
                //             .Add(Expression.Eq("Fid", fid))
                //             .Add(Expression.Gt("Sa", 0))
                //             .AddOrder(Order.Asc("Sa"))
                //    /*.AddOrder(new Order("Sa", true))*/
                //             .List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGo = userGroupWorkflows.Where(v => v.Sa > 0).OrderBy(y => y.Sa).ToList<UserGroupWorkflow>();

                IList<UserGroupWorkflow> uGW1 = new List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGW2 = new List<UserGroupWorkflow>();
                IList<UserGroupWorkflow> uGW3 = new List<UserGroupWorkflow>();
                int count1 = 0;
                int count2 = 0;
                int count3 = 0;
                foreach (UserGroupWorkflow item in uGo)
                {
                    if (item.Sa == 1)
                    {
                        count1++;
                        uGW1.Add(item);
                    }
                    else if (item.Sa == 2)
                    {
                        count2++;
                        uGW2.Add(item);
                    }
                    else
                    {
                        count3++;
                        uGW3.Add(item);
                    }
                }
                if (count1 != 0) foreach (UserGroupWorkflow item in uGW1) nextGid.Add(item.Gid);

                else if (count2 != 0) foreach (UserGroupWorkflow item in uGW2) nextGid.Add(item.Gid);

                else foreach (UserGroupWorkflow item in uGW3) nextGid.Add(item.Gid);
            }
            return nextGid;
        }

        public static String getNextAppTemp(ISession session, int fid, UserMap currP, String approveInfo, String instCode, String debAcc, String curr, Double amount)
        {
            #region workflow getter
            ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
                    .Add(Expression.Eq("Id", currP.ClientEntity))
                    .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion
            String nextProcessor = "";
            String _nextP_ = "";
            int _Gid_ = 0;

            if (!wf.isAppByGroup)
            {
                /*
                 * 2015-07-08 : Hidayat
                 * 
                 * Perubahan source code, pemanggilan berulang 
                 * UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc)
                 * 
                 */
                //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc));
                _nextP_ = UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc);
                nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, _nextP_);
            }
            else
            {
                if (wf.isPrivateApp)
                {
                    /*
                     * 2015-07-08 : Hidayat
                     * 
                     * Perubahan source code, pemanggilan berulang 
                     *      1. UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity)
                     *      2. UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity), instCode, curr, amount, debAcc)
                     * 
                     */
                    _Gid_ = UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity);
                    //_nextP_ = UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity), instCode, curr, amount, debAcc);
                    //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity), instCode, curr, amount, debAcc));
                    _nextP_ = UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, _Gid_, instCode, curr, amount, debAcc);
                    nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, _nextP_);
                }
                else
                {
                    if (wf.isSerialApp)
                    {
                        IList<int> nextGroupId = UserGroupHelper.getNextAppGroupIDTemp(session, fid, currP.ClientEntity, approveInfo);
                        foreach (int item in nextGroupId)
                        {
                            _nextP_ = "|" + _nextP_ + UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, item, instCode, curr, amount, debAcc) + "|";
                        }
                        _nextP_ = _nextP_.Replace("||", "|");
                        nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, _nextP_);

                    }
                    else
                    {
                        /*
                         * 2015-07-08 : Hidayat
                         * 
                         * Perubahan source code, pemanggilan berulang 
                         *      UserGroupHelper.getNextAppByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc)
                         * 
                         */
                        String arrProcessor = UserGroupHelper.getRemainAppGroupID(session, fid, currP.ClientEntity, approveInfo);
                        _nextP_ = UserGroupHelper.getNextAppByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc);
                        //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc));
                        nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, _nextP_);
                    }
                }
            }

            return nextProcessor;
        }

        public static String getNextAppTemp(ISession session, int fid, UserMap currP, String approveInfo, String instCode
                                            , String debAcc, String curr, Double amount, ClientWorkflow cm
                                            , IList<UserMatrix2> txList, IList<UserMap> umaps, int Gid
                                            , IList<UserGroupMaps> userGroupMaps, IList<ClientAccount> ClientAccounts
                                            , IList<Currency> Currencies, IList<UserAccount> UserAccounts
                                            , IList<UserGroupWorkflow> userGroupWorkflows)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             * Method ini digunakan untuk pemanggilan secara thread pada mass transaction,
             * sehingga tidak dilakukan query yang sama secara berulang-ulang
             * untuk transaksi dalam 1 batch.
             * Query yang berulang-ulang antara lain :
             *      - get client workflow
             *      - get client matrix
             *      
             */
            #region workflow getter

            /*
             * 2015-07-08 : Hidayat
             * 
             * Bagian ini tidak digunakan lagi karena sudah disediakan dari parameter
             */

            //ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
            //        .Add(Expression.Eq("Id", currP.ClientEntity))
            //        .UniqueResult<ClientWorkflow>();
            

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion
            String nextProcessor = "";
            String _nextP_ = "";
            int _Gid_ = Gid;

            if (!wf.isAppByGroup)
            {
                #region Tanpa Group
                /*
                 * 2015-07-08 : Hidayat
                 * 
                 * 1. Perubahan source code, pemanggilan berulang 
                 *      UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc)
                 * 2. Perubahan penggunaan method dengan mengirimkan parameter txList
                 *      UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc, txList)
                 * 3. Perubahan penggunaan method resultCleanser
                 * 
                 */
                //_nextP_ = UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc);
                //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc));
                _nextP_ = UserGroupHelper.getNextAppByUser(session, fid, currP.ClientEntity, instCode
                                                        , curr, amount, debAcc, txList, ClientAccounts
                                                        , Currencies, UserAccounts);
                nextProcessor = UserGroupHelper.resultCleanser(session, umaps, approveInfo, _nextP_);

                #endregion Tanpa Group
            }
            else
            {
                if (wf.isPrivateApp)
                {
                    #region Group Straight

                    /*
                     * 2015-07-08 : Hidayat
                     * 
                     * A. Perubahan source code, pemanggilan berulang 
                     *      1. UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity)
                     *      2. UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity), instCode, curr, amount, debAcc)
                     * B. Perubahan penggunaan method resultCleanser
                     * C. _Gid_ sudah diassign dari parameter
                     * D. Perubahan penggunaan method getNextAppByGroupID
                     * 
                     */
                    //_Gid_ = UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity);
                    //_nextP_ = UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity), instCode, curr, amount, debAcc);
                    //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity), instCode, curr, amount, debAcc));
                    IList<UserGroupMaps> _UserGroupMaps = userGroupMaps.Where(ugm => ugm.Gid == _Gid_).ToList<UserGroupMaps>();

                    _nextP_ = UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity, _Gid_, instCode, curr, amount, debAcc, txList, _UserGroupMaps, ClientAccounts, Currencies, UserAccounts);
                    nextProcessor = UserGroupHelper.resultCleanser(session, umaps, approveInfo, _nextP_);

                    #endregion Group Straight
                }
                else
                {
                    if (wf.isSerialApp)
                    {
                        #region Group Serial

                        /*
                         * 2015-07-08 : Hidayat
                         * 
                         * Perubahan penggunaan method resultCleanser
                         * 
                         */
                        //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, _nextP_);
 
                        IList<int> nextGroupId = UserGroupHelper.getNextAppGroupIDTemp(session, fid, currP.ClientEntity, approveInfo, userGroupWorkflows, userGroupMaps);

                        foreach (int item in nextGroupId)
                        {
                            IList<UserGroupMaps> _UserGroupMaps = userGroupMaps.Where(ugm => ugm.Gid == item).ToList<UserGroupMaps>();

                            _nextP_ = "|" + _nextP_ + UserGroupHelper.getNextAppByGroupID(session, fid, currP.ClientEntity
                                                            , item, instCode, curr, amount, debAcc
                                                            , txList, _UserGroupMaps, ClientAccounts, Currencies, UserAccounts) + "|";
                        }
                        _nextP_ = _nextP_.Replace("||", "|");
                        nextProcessor = UserGroupHelper.resultCleanser(session, umaps, approveInfo, _nextP_);

                        #endregion Group Serial
                    }
                    else
                    {
                        #region Group Paralel

                        /*
                         * 2015-07-08 : Hidayat
                         * 
                         * Perubahan source code, pemanggilan berulang 
                         *      UserGroupHelper.getNextAppByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc)
                         * 
                         */
                        //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, approveInfo, UserGroupHelper.getNextAppByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc));

                        String arrProcessor = UserGroupHelper.getRemainAppGroupID(session, fid, currP.ClientEntity, approveInfo, userGroupMaps, userGroupWorkflows);
                        _nextP_ = UserGroupHelper.getNextAppByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor
                                                                            , instCode, curr, amount, debAcc
                                                                            , txList, userGroupMaps, ClientAccounts
                                                                            , Currencies, UserAccounts);
                        nextProcessor = UserGroupHelper.resultCleanser(session, umaps, approveInfo, _nextP_);

                        #endregion Group Paralel
                    }
                }
            }

            return nextProcessor;
        }

        public static String getNextCheTemp(ISession session, int fid, UserMap currP, String checkerInfo, String instCode, String debAcc, String curr, Double amount)
        {

            #region workflow getter
            ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
                    .Add(Expression.Eq("Id", currP.ClientEntity))
                    .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion

            String nextProcessor = "";
            String _nextP_ = "";
            int _Gid_ = 0;

            if (!wf.isCheByGroup)
            {
                nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByUser(session, fid, currP.ClientEntity, instCode, curr, amount, debAcc));
            }
            else
            {
                if (wf.isPrivateChe)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, currP.ClientEntity, UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim(), currP.ClientEntity), instCode, curr, amount, debAcc));
                }
                else
                {
                    if (wf.isSerialChe)
                    {
                        IList<int> nextGroupId = UserGroupHelper.getNextCheGroupIDTemp(session, fid, currP.ClientEntity, checkerInfo);
                        foreach (int item in nextGroupId)
                        {
                            _nextP_ = "|" + _nextP_ + UserGroupHelper.getNextCheByGroupID(session, fid, currP.ClientEntity, item, instCode, curr, amount, debAcc) + "|";
                        }
                        _nextP_ = _nextP_.Replace("||", "|");
                        nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, _nextP_);

                    }
                    else
                    {
                        String arrProcessor = UserGroupHelper.getRemainCheGroupID(session, fid, currP.ClientEntity, checkerInfo);
                        nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc));
                    }
                }
            }

            return nextProcessor;
        }

        public static String getNextCheTemp(ISession session, int fid, UserMap currP, String checkerInfo, String instCode
                                            , String debAcc, String curr, Double amount, ClientWorkflow cm
                                            , IList<UserMatrix2> txList, IList<UserMap> umaps, int Gid
                                            , IList<UserGroupMaps> userGroupMaps, IList<ClientAccount> ClientAccounts
                                            , IList<Currency> Currencies, IList<UserAccount> UserAccounts
                                            , IList<UserGroupWorkflow> userGroupWorkflows, ClientMatrix clientMatrix)
        {
            /*
             * 2015-07-08 : Hidayat
             * 
             * Method ini digunakan untuk pemanggilan secara thread pada mass transaction,
             * sehingga tidak dilakukan query yang sama secara berulang-ulang
             * untuk transaksi dalam 1 batch.
             * Query yang berulang-ulang antara lain :
             *      - get client workflow
             *      - get client matrix
             *      
             */

            #region workflow getter
            /*
             * 2015-07-08 : Hidayat
             * 
             * Bagian ini tidak digunakan lagi karena sudah disediakan dari parameter
             */

            //ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
            //        .Add(Expression.Eq("Id", currP.ClientEntity))
            //        .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion

            String nextProcessor = "";
            String _nextP_ = "";
            int _Gid_ = 0;

            if (!wf.isCheByGroup)
            {
                #region Tanpa Group
                //resultCleanser(ISession session, IList<UserMap> umapList, String AppList, String nextP)
                /*
                // old format
                nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo
                                        , UserGroupHelper.getNextCheByUser(session, fid, currP.ClientEntity
                                        , instCode, curr, amount, debAcc));
                */
               
                nextProcessor = UserGroupHelper.resultCleanser(
                                    session
                                    , umaps
                                    , checkerInfo
                                    , UserGroupHelper.getNextCheByUser(session, fid, currP.ClientEntity
                                                        , instCode, curr, amount, debAcc, txList, ClientAccounts
                                                        , Currencies, UserAccounts, clientMatrix)
                                 );
                #endregion Tanpa Group
            }
            else
            {
                if (wf.isPrivateChe)
                {
                    #region Group Straight

                    //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo
                    //                        , UserGroupHelper.getNextCheByGroupID(session, fid, currP.ClientEntity
                    //                                    , UserGroupHelper.getGroupByHandle(session, currP.UserHandle.Trim()
                    //                                    , currP.ClientEntity)
                    //                        , instCode, curr, amount, debAcc));
                    nextProcessor = UserGroupHelper.resultCleanser(session
                                            , umaps
                                            , checkerInfo
                                            , UserGroupHelper.getNextCheByGroupID(
                                                        session
                                                        , fid
                                                        , currP.ClientEntity
                                                        , UserGroupHelper.getGroupByHandle(
                                                                session
                                                                , currP.UserHandle.Trim()
                                                                , currP.ClientEntity, userGroupMaps)
                                                        , instCode
                                                        , curr
                                                        , amount
                                                        , debAcc
                                                        , txList
                                                        , userGroupMaps
                                                        , ClientAccounts
                                                        , Currencies
                                                        , UserAccounts
                                                        , clientMatrix));
                    #endregion Group Straight
                }
                else
                {
                    if (wf.isSerialChe)
                    {
                        #region Group Serial

                        IList<int> nextGroupId = UserGroupHelper.getNextCheGroupIDTemp(
                                                        session
                                                        , fid
                                                        , currP.ClientEntity
                                                        , checkerInfo
                                                        , userGroupWorkflows
                                                        , userGroupMaps);

                        foreach (int item in nextGroupId)
                        {
                            IList<UserGroupMaps> _UserGroupMaps = userGroupMaps.Where(ugm => ugm.Gid == item).ToList<UserGroupMaps>();

                            _nextP_ = "|" + _nextP_ + UserGroupHelper.getNextCheByGroupID(
                                                            session
                                                            , fid
                                                            , currP.ClientEntity
                                                            , item
                                                            , instCode
                                                            , curr
                                                            , amount
                                                            , debAcc
                                                            , txList
                                                            , _UserGroupMaps
                                                            , ClientAccounts
                                                            , Currencies
                                                            , UserAccounts
                                                            , clientMatrix) + "|";
                        }
                        _nextP_ = _nextP_.Replace("||", "|");
                        nextProcessor = UserGroupHelper.resultCleanser(session, umaps, checkerInfo, _nextP_);

                        #endregion Group Serial
                    }
                    else
                    {
                        #region Group Paralel

                        //String arrProcessor = UserGroupHelper.getRemainCheGroupID(session, fid, currP.ClientEntity, checkerInfo);
                        //nextProcessor = UserGroupHelper.resultCleanser(session, currP.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByRemainGroupID(session, fid, currP.ClientEntity, arrProcessor, instCode, curr, amount, debAcc));

                        String arrProcessor = UserGroupHelper.getRemainCheGroupID(
                                                    session, fid, currP.ClientEntity, checkerInfo
                                                    , userGroupMaps, userGroupWorkflows);
                        nextProcessor = UserGroupHelper.resultCleanser(
                                                session
                                                , umaps
                                                , checkerInfo
                                                , UserGroupHelper.getNextCheByRemainGroupID(
                                                            session
                                                            , fid
                                                            , currP.ClientEntity
                                                            , arrProcessor
                                                            , instCode
                                                            , curr
                                                            , amount
                                                            , debAcc
                                                            , txList
                                                            , userGroupMaps
                                                            , ClientAccounts
                                                            , Currencies
                                                            , UserAccounts
                                                            , clientMatrix));
                        #endregion Group Paralel
                    }
                }
            }

            return nextProcessor;
        }

        public static Boolean isAuthCompleteMass(ISession session, UserMap umap, int fid, String checkerInfo, String approverInfo, String instCode, String debitAcc, String curr, Double amount, out String msg, out String author)
        {
            Boolean result = true;
            String nextProcessor = "";
            String[] arrProc;
            String[] arrDone;
            msg = "";
            author = "";
            int cid = umap.ClientEntity;

            #region workflow getter
            ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
                    .Add(Expression.Eq("Id", umap.ClientEntity))
                    .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion

            //Approver
            #region Check Approver
            if (wf.TotalApprover > 0)
            {
                if (!wf.isAppByGroup)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc));
                    if (nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        msg = "On Approver";
                        return false;
                    }
                    else
                    {
                        arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrProc.Length + arrDone.Length < wf.TotalApprover)
                        {
                            msg = "On Approver";
                            return false;
                        }
                    }
                }
                else
                {
                    if (wf.isPrivateApp)
                    {
                        nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity), instCode, curr, amount, debitAcc));
                        if (nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            msg = "On Approver";
                            return false;
                        }
                        else
                        {
                            arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrProc.Length + arrDone.Length < wf.TotalApprover)
                            {
                                msg = "On Approver";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                               .Add(Expression.Eq("Cid", cid))
                               .Add(Expression.Eq("Fid", fid))
                               .Add(Expression.Gt("Ta", 0))
                               .List<UserGroupWorkflow>();
                        if (txList.Count > 0)
                        {
                            for (int i = 0; i < txList.Count; i++)
                            {
                                nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc));
                                if (nextProcessor.Trim().Replace("|", "").Equals(""))
                                {
                                    msg = "On Approver";
                                    return false;
                                }
                                else
                                {
                                    arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arrProc.Length + arrDone.Length < txList[i].Ta)
                                    {
                                        msg = "On Approver";
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (nextProcessor.Trim().Replace("|", "").Equals(""))
                            {
                                msg = "On Approver";
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion
            author = nextProcessor;
            //Verifier
            #region Check Verify
            if (wf.TotalVerifier > 0)
            {
                if (!wf.isCheByGroup)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc));
                    if (nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        msg = "On Verifier";
                        return false;
                    }
                    else
                    {
                        arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrProc.Length + arrDone.Length < wf.TotalVerifier)
                        {
                            msg = "On Verifier";
                            return false;
                        }
                    }
                }
                else
                {
                    if (wf.isPrivateChe)
                    {
                        nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity), instCode, curr, amount, debitAcc));
                        if (nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            msg = "On Verifier";
                            return false;
                        }
                        else
                        {
                            arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrProc.Length + arrDone.Length < wf.TotalVerifier)
                            {
                                msg = "On Verifier";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                           .Add(Expression.Eq("Cid", cid))
                           .Add(Expression.Eq("Fid", fid))
                           .Add(Expression.Gt("Tv", 0))
                           .List<UserGroupWorkflow>();

                        if (txList.Count > 0)
                        {
                            for (int i = 0; i < txList.Count; i++)
                            {
                                nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc));
                                if (nextProcessor.Trim().Replace("|", "").Equals(""))
                                {
                                    msg = "On Verifier";
                                    return false;
                                }
                                else
                                {
                                    arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arrProc.Length + arrDone.Length < txList[i].Tv)
                                    {
                                        msg = "On Verifier";
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (nextProcessor.Trim().Replace("|", "").Equals(""))
                            {
                                msg = "On Verifier";
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion
            author += nextProcessor;
            author = author.Replace("||", "|");
            return result;
        }

        public static Boolean isAuthCompleteMass(ISession session, UserMap umap, int fid, 
            String checkerInfo, String approverInfo, String instCode, String debitAcc,
            String curr, Double amount, out String msg, out String author, ClientWorkflow cm,
            IList<UserGroupWorkflow> userGroupWorkflows, IList<UserMatrix2> lsUm2, IList<ClientAccount> ClientAccounts
            , IList<Currency> currencies, IList<UserAccount> UserAccounts, IList<UserGroupMaps> ugm, ClientMatrix cMatrix)
        {
            Boolean result = true;
            String nextProcessor = "";
            String _nextP_ = "";
            int _gid_ = 0;
            String[] arrProc;
            String[] arrDone;
            msg = "";
            author = "";
            int cid = umap.ClientEntity;

            #region workflow getter
            //ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
            //        .Add(Expression.Eq("Id", umap.ClientEntity))
            //        .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion

            //Approver
            #region Check Approver
            if (wf.TotalApprover > 0)
            {
                if (!wf.isAppByGroup)
                {
                    //nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc));
                   
                    _nextP_ = UserGroupHelper.getNextAppByUser(session, fid, umap.ClientEntity, instCode
                                                        , curr, amount, debitAcc, lsUm2, ClientAccounts
                                                        , currencies, UserAccounts);
                    nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, _nextP_);

                    if (nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        msg = "On Approver";
                        return false;
                    }
                    else
                    {
                        arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrProc.Length + arrDone.Length < wf.TotalApprover)
                        {
                            msg = "On Approver";
                            return false;
                        }
                    }
                }
                else
                {
                    if (wf.isPrivateApp)
                    {
                        //nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, 
                        //    UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity), instCode, curr, amount, debitAcc));
                        _gid_ = UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity, ugm);
                        //_nextP_ = UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, _gid_, instCode, curr, amount, debitAcc);
                        _nextP_ = UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, _gid_, instCode, curr, amount, debitAcc, 
                                        lsUm2, ugm, ClientAccounts, currencies, UserAccounts);
                        nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, _nextP_);
                        if (nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            msg = "On Approver";
                            return false;
                        }
                        else
                        {
                            arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrProc.Length + arrDone.Length < wf.TotalApprover)
                            {
                                msg = "On Approver";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                        //       .Add(Expression.Eq("Cid", cid))
                        //       .Add(Expression.Eq("Fid", fid))
                        //       .Add(Expression.Gt("Ta", 0))
                        //       .List<UserGroupWorkflow>();
                        IList<UserGroupWorkflow> txList = userGroupWorkflows.Where(x => x.Ta > 0).ToList<UserGroupWorkflow>();

                        if (txList.Count > 0)
                        {
                            for (int i = 0; i < txList.Count; i++)
                            {
                                //nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc));
                                _nextP_ = UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, _gid_, instCode, curr, amount, debitAcc,
                                        lsUm2, ugm, ClientAccounts, currencies, UserAccounts);
                                nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, _nextP_);
                        
                                if (nextProcessor.Trim().Replace("|", "").Equals(""))
                                {
                                    msg = "On Approver";
                                    return false;
                                }
                                else
                                {
                                    arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arrProc.Length + arrDone.Length < txList[i].Ta)
                                    {
                                        msg = "On Approver";
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (nextProcessor.Trim().Replace("|", "").Equals(""))
                            {
                                msg = "On Approver";
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion
            author = nextProcessor;
            //Verifier
            #region Check Verify
            if (wf.TotalVerifier > 0)
            {
                if (!wf.isCheByGroup)
                {
                    //nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc));
                    _nextP_ = UserGroupHelper.getNextCheByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc, 
                        lsUm2, ClientAccounts, currencies, UserAccounts, cMatrix);
                    nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, _nextP_);
                        
                    if (nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        msg = "On Verifier";
                        return false;
                    }
                    else
                    {
                        arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrProc.Length + arrDone.Length < wf.TotalVerifier)
                        {
                            msg = "On Verifier";
                            return false;
                        }
                    }
                }
                else
                {
                    if (wf.isPrivateChe)
                    {
                        //nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity), instCode, curr, amount, debitAcc));
                        _gid_ = UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity, ugm);
                       
                        _nextP_ = UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, _gid_, instCode, curr, amount, debitAcc,
                                        lsUm2, ugm, ClientAccounts, currencies, UserAccounts, cMatrix);
                        
                        nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, _nextP_) ;
                        
                        if (nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            msg = "On Verifier";
                            return false;
                        }
                        else
                        {
                            arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrProc.Length + arrDone.Length < wf.TotalVerifier)
                            {
                                msg = "On Verifier";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                        //   .Add(Expression.Eq("Cid", cid))
                        //   .Add(Expression.Eq("Fid", fid))
                        //   .Add(Expression.Gt("Tv", 0))
                        //   .List<UserGroupWorkflow>();

                        IList<UserGroupWorkflow> txList = userGroupWorkflows.Where(x => x.Tv > 0).ToList<UserGroupWorkflow>();

                        if (txList.Count > 0)
                        {
                            for (int i = 0; i < txList.Count; i++)
                            {
                                //nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc));
                                
                                _nextP_ = UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc,
                                        lsUm2, ugm, ClientAccounts, currencies, UserAccounts, cMatrix);
                                nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, _nextP_);
                                if (nextProcessor.Trim().Replace("|", "").Equals(""))
                                {
                                    msg = "On Verifier";
                                    return false;
                                }
                                else
                                {
                                    arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arrProc.Length + arrDone.Length < txList[i].Tv)
                                    {
                                        msg = "On Verifier";
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (nextProcessor.Trim().Replace("|", "").Equals(""))
                            {
                                msg = "On Verifier";
                                return false;
                            }
                        }
                    }
                }
            }
            #endregion
            author += nextProcessor;
            author = author.Replace("||", "|");
            return result;
        }

        public static Boolean isAuthMultiPayment(ISession session, UserMap umap, int fid, int featid, String checkerInfo, String approverInfo, String instCode, String debitAcc, String curr, Double amount, out String msg)
        {
            Boolean result = true;
            String nextProcessor = "";
            String[] arrProc;
            String[] arrDone;
            msg = "";
            int cid = umap.ClientEntity;

            #region workflow getter
            ClientWorkflow cm = session.CreateCriteria(typeof(ClientWorkflow))
                    .Add(Expression.Eq("Id", umap.ClientEntity))
                    .UniqueResult<ClientWorkflow>();

            WorkflowHelper wh = new WorkflowHelper(cm.Workflow);

            Workflow wf = wh.GetWorkflow(fid);
            #endregion

            //Approver
            #region Check Approver
            if (wf.TotalApprover > 0)
            {
                if (!wf.isAppByGroup)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc));
                    if (nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        msg = "On Approver";
                        return false;
                    }
                    else
                    {
                        arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrProc.Length + arrDone.Length < wf.TotalApprover)
                        {
                            msg = "On Approver";
                            return false;
                        }
                    }
                }
                else
                {
                    if (wf.isPrivateApp)
                    {
                        nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity), instCode, curr, amount, debitAcc));
                        if (nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            msg = "On Approver";
                            return false;
                        }
                        else
                        {
                            arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrProc.Length + arrDone.Length < wf.TotalApprover)
                            {
                                msg = "On Approver";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                               .Add(Expression.Eq("Cid", cid))
                               .Add(Expression.Eq("Fid", fid))
                               .Add(Expression.Gt("Ta", 0))
                               .List<UserGroupWorkflow>();
                        if (txList.Count > 0)
                        {
                            for (int i = 0; i < txList.Count; i++)
                            {
                                nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, approverInfo, UserGroupHelper.getNextAppByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc));
                                if (nextProcessor.Trim().Replace("|", "").Equals(""))
                                {
                                    msg = "On Approver";
                                    return false;
                                }
                                else
                                {
                                    arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    arrDone = approverInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arrProc.Length + arrDone.Length < txList[i].Ta)
                                    {
                                        msg = "On Approver";
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (nextProcessor.Trim().Replace("|", "").Equals(""))
                            {
                                msg = "On Approver";
                                return false;
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(nextProcessor))
                {
                    IList xp = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (String x in xp)
                    {
                        try
                        {
                            TrxMultiPayment_Users u = session.CreateCriteria(typeof(TrxMultiPayment_Users))
                                .Add(Expression.Eq("Client_Id", cid))
                                .Add(Expression.Eq("User_Id", int.Parse(x)))
                                .Add(Expression.Eq("Fitur_Id", featid))
                                .UniqueResult<TrxMultiPayment_Users>();

                            if (u != null)
                            {
                                if (u.Amount < amount)
                                {
                                    return false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                    }
                }
                else return false;
            }
            #endregion

            //Verifier
            #region Check Verify
            if (wf.TotalVerifier > 0)
            {
                if (!wf.isCheByGroup)
                {
                    nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByUser(session, fid, umap.ClientEntity, instCode, curr, amount, debitAcc));
                    if (nextProcessor.Trim().Replace("|", "").Equals(""))
                    {
                        msg = "On Verifier";
                        return false;
                    }
                    else
                    {
                        arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        if (arrProc.Length + arrDone.Length < wf.TotalVerifier)
                        {
                            msg = "On Verifier";
                            return false;
                        }
                    }
                }
                else
                {
                    if (wf.isPrivateChe)
                    {
                        nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, UserGroupHelper.getGroupByHandle(session, umap.UserHandle, umap.ClientEntity), instCode, curr, amount, debitAcc));
                        if (nextProcessor.Trim().Replace("|", "").Equals(""))
                        {
                            msg = "On Verifier";
                            return false;
                        }
                        else
                        {
                            arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                            if (arrProc.Length + arrDone.Length < wf.TotalVerifier)
                            {
                                msg = "On Verifier";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        IList<UserGroupWorkflow> txList = session.CreateCriteria(typeof(UserGroupWorkflow))
                           .Add(Expression.Eq("Cid", cid))
                           .Add(Expression.Eq("Fid", fid))
                           .Add(Expression.Gt("Tv", 0))
                           .List<UserGroupWorkflow>();

                        if (txList.Count > 0)
                        {
                            for (int i = 0; i < txList.Count; i++)
                            {
                                nextProcessor = UserGroupHelper.resultCleanser(session, umap.ClientEntity, checkerInfo, UserGroupHelper.getNextCheByGroupID(session, fid, umap.ClientEntity, txList[i].Gid, instCode, curr, amount, debitAcc));
                                if (nextProcessor.Trim().Replace("|", "").Equals(""))
                                {
                                    msg = "On Verifier";
                                    return false;
                                }
                                else
                                {
                                    arrProc = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    arrDone = checkerInfo.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (arrProc.Length + arrDone.Length < txList[i].Tv)
                                    {
                                        msg = "On Verifier";
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (nextProcessor.Trim().Replace("|", "").Equals(""))
                            {
                                msg = "On Verifier";
                                return false;
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(nextProcessor))
                {
                    IList xp = nextProcessor.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (String x in xp)
                    {
                        try
                        {
                            TrxMultiPayment_Users u = session.CreateCriteria(typeof(TrxMultiPayment_Users))
                                .Add(Expression.Eq("Client_Id", cid))
                                .Add(Expression.Eq("User_Id", int.Parse(x)))
                                .Add(Expression.Eq("Fitur_Id", featid))
                                .UniqueResult<TrxMultiPayment_Users>();

                            if (u != null)
                            {
                                if (u.Amount < amount)
                                {
                                    return false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                    }
                }
                else return false;
            }
            #endregion

            return result;
        }
    
    }
}

public class ThdAuth
{
    public ThdAuth(ISessionFactory factory, int cid, int fid, String txcode,String debacc, String cur, double amt, UserMatrix2[] t, ManualResetEvent doneEvent)
    {
        _tc = t;
        _cid = cid;
        _fid = fid;
        _debitAcc = debacc;
        _debitAmt = amt;
        _debitCur = cur;
        _listId = "";
        _txcode = txcode;

        _doneEvent = doneEvent;
        _session = factory.OpenSession();
    }

    public void ThreadPoolCallback(Object threadContext)
    {
        int threadIndex = (int)threadContext;
        //Console.WriteLine("thread {0} started...", threadIndex);

        int count = 0;

        while (count < _tc.Length)
        {
            if (CheckAuthority(_tc[count].Id, _tc[count].Matrix))
            {
                if (_listId.Equals(""))
                {
                    _listId += _tc[count].Id;
                }
                else
                {
                    _listId += "|" + _tc[count].Id;
                }
            }
            count++;
        }

        //_remark = Test(_session, _umap);
        //Console.WriteLine("thread {0} result calculated...", threadIndex);
        _doneEvent.Set();
    }

    public Boolean CheckAuthority(int uid,String matrix)
    {
        
        AuthorityHelper ah = new AuthorityHelper(matrix);
        
        //Add by Dikki
        if (_fid == 4230)
        {
            return true;
        }
        //End
        
        if (uid == 1224)
        {
            //Cek Kewenangan Account
            #region Kewenangan Account
            if (_fid == 3610)
            {
                // No comment
            }
			else if (_fid == 3640)
			{
            // No comment
			}
            else if (_fid == 3660)
            {
                // No comment
            }
            else if (_fid == 4020) //add wira
            {
                ClientAccount ca = _session.CreateCriteria(typeof(ClientAccount))
                    .Add(Expression.Like("Number", _debitAcc))
                    .Add(Expression.Eq("Pid", _cid))
                    .UniqueResult<ClientAccount>();
                if (null == ca)
                {
                    return false;
                }
            }
            else
            {

                ClientAccount ca = _session.CreateCriteria(typeof(ClientAccount))
                        .Add(Expression.Like("Number", _debitAcc))
                        .Add(Expression.Eq("Pid", _cid))
                        .UniqueResult<ClientAccount>();
                if (null == ca)
                {
                    return false;
                }

                UserAccount ua = _session.CreateCriteria(typeof(UserAccount))
                    .Add(Expression.Eq("Aid", ca.Id))
                    .Add(Expression.Eq("Fid", _fid))
                    .Add(Expression.Eq("Uid", uid))
                    .UniqueResult<UserAccount>();
                if (null == ua)
                {
                    return false;
                }
            }
            #endregion

            //Cek Limit User
            #region Kewenangan Limit
            double amt = 0;
            if (!_debitCur.Equals("IDR"))
            {
                Currency currang = _session.CreateCriteria(typeof(Currency))
                    .Add(Expression.Like("Code", "%" + _debitCur.Trim() + "%"))
                    .UniqueResult<Currency>();

                amt = _debitAmt * (double)currang.BookingRate;
            }
            else
            {
                amt = _debitAmt;
            }

            double limitcorp = 0;

            if (_fid == 1140)
            {
                if (txcode.Equals("IFT"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 1));
                }
                else if (txcode.Equals("RTG"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 2));
                }
                else if (txcode.Equals("LLG"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 3));
                }
                else if (txcode.Equals("SWI"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 4));
                }
                else if (txcode.Equals("WIC"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 5));
                }
            }
			else if (_fid == 4200)
            {
                return true;
            }
            else limitcorp = (double)Convert.ToDouble(ah.GetUserLimit(_fid, 1));

            if (amt > limitcorp)
            {
                return false;
            }

            return true;

            #endregion
        }
        else
        {
            //Cek Kewenangan Account
            #region Kewenangan Account
            if (_fid == 3610)
            {
                // No comment
            }
			 else if (_fid == 3640)
            {
                // No comment
            }
            else if (_fid == 3660)
            {
                // No comment
            }
			else if (_fid == 4290) //Alfian MCashCard
            {
                // No comment

                return true;
            }
            else if (_fid == 4040) //Purnomo Cashcard
            {
                return true;
            }
            else if (_fid == 4020) //add wira
            {
                ClientAccount ca = _session.CreateCriteria(typeof(ClientAccount))
                    .Add(Expression.Like("Number", _debitAcc))
                    .Add(Expression.Eq("Pid", _cid))
                    .UniqueResult<ClientAccount>();
                if (null == ca)
                {
                    return false;
                }
            }
            else
            {

                ClientAccount ca = _session.CreateCriteria(typeof(ClientAccount))
                        .Add(Expression.Like("Number", _debitAcc))
                        .Add(Expression.Eq("Pid", _cid))
                        .UniqueResult<ClientAccount>();
                if (null == ca)
                {
                    return false;
                }

                UserAccount ua = _session.CreateCriteria(typeof(UserAccount))
                    .Add(Expression.Eq("Aid", ca.Id))
                    .Add(Expression.Eq("Fid", _fid))
                    .Add(Expression.Eq("Uid", uid))
                    .UniqueResult<UserAccount>();
                if (null == ua)
                {
                    return false;
                }
            }
            #endregion

            //Cek Limit User
            #region Kewenangan Limit
            double amt = 0;
            if (!_debitCur.Equals("IDR"))
            {
                Currency currang = _session.CreateCriteria(typeof(Currency))
                    .Add(Expression.Like("Code", "%" + _debitCur.Trim() + "%"))
                    .UniqueResult<Currency>();

                amt = _debitAmt * (double)currang.BookingRate;
            }
            else
            {
                amt = _debitAmt;
            }

            double limitcorp = 0;

            if (_fid == 1140)
            {
                if (txcode.Equals("IFT"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 1));
                }
                else if (txcode.Equals("RTG"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 2));
                }
                else if (txcode.Equals("LLG"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 3));
                }
                else if (txcode.Equals("SWI"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 4));
                }
                else if (txcode.Equals("WIC"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 5));
                }
            }
            else if (_fid == 4200)
            {
                return true;
            }
            else limitcorp = (double)Convert.ToDouble(ah.GetUserLimit(_fid, 1));

            if (amt > limitcorp)
            {
                return false;
            }

            return true;

            #endregion
        }
    }

    public UserMatrix2[] Tc { get { return _tc; } }
    private UserMatrix2[] _tc;

    public int Fid { get { return _fid; } }
    private int _fid;

    public int Cid { get { return _cid; } }
    private int _cid;

    public String txcode { get { return _txcode; } }
    private String _txcode;

    public String listId { get { return _listId; } }
    private String _listId;

    public String debitAcc { get { return _debitAcc; } }
    private String _debitAcc;

    public String Currency { get { return _debitCur; } }
    private String _debitCur;

    public double Amount { get { return _debitAmt; } }
    private double _debitAmt;

    private ISession _session;
    private ManualResetEvent _doneEvent;
}

public class ThdAuthMass
{
    public ThdAuthMass(ISessionFactory factory, int cid, int fid, String txcode, String debacc, String cur
                        , double amt, UserMatrix2[] t, IList<ClientAccount> clientAccounts
                        , IList<Currency> currencies, IList<UserAccount> userAccounts
                        , ManualResetEvent doneEvent)
    {
        _tc = t;
        _cid = cid;
        _fid = fid;
        _debitAcc = debacc;
        _debitAmt = amt;
        _debitCur = cur;
        _listId = "";
        _txcode = txcode;
        _ClientAccounts = clientAccounts;
        _Currencies = currencies;
        _UserAccounts = userAccounts;

        _doneEvent = doneEvent;
        _session = factory.OpenSession();
    }

    public void ThreadPoolCallback(Object threadContext)
    {
        int threadIndex = (int)threadContext;
        //Console.WriteLine("thread {0} started...", threadIndex);

        int count = 0;

        while (count < _tc.Length)
        {
            if (CheckAuthority(_tc[count].Id, _tc[count].Matrix))
            {
                if (_listId.Equals(""))
                {
                    _listId += _tc[count].Id;
                }
                else
                {
                    _listId += "|" + _tc[count].Id;
                }
            }
            count++;
        }

        //_remark = Test(_session, _umap);
        //Console.WriteLine("thread {0} result calculated...", threadIndex);
        _doneEvent.Set();
    }

    public Boolean CheckAuthority(int uid, String matrix)
    {

        try
        {
            AuthorityHelper ah = new AuthorityHelper(matrix);

            //Cek Kewenangan Accountt
            #region Kewenangan Account
            if (_fid == 3610)
            {
                // No comment
            }
			else if (_fid == 3640)
			{
            // No comment
			}
            else if (_fid == 3660)
            {
                // No comment
            }
            else if (_fid == 4020) //add wira
            {
                //ClientAccount ca = _session.CreateCriteria(typeof(ClientAccount))
                //    .Add(Expression.Like("Number", _debitAcc))
                //    .Add(Expression.Eq("Pid", _cid))
                //    .UniqueResult<ClientAccount>();
                ClientAccount ca = ClientAccounts.SingleOrDefault(p => p.Number == _debitAcc);
                if (null == ca)
                {
                    return false;
                }
            }
            else
            {

                //ClientAccount ca = _session.CreateCriteria(typeof(ClientAccount))
                //        .Add(Expression.Like("Number", _debitAcc))
                //        .Add(Expression.Eq("Pid", _cid))
                //        .UniqueResult<ClientAccount>();
                ClientAccount ca = ClientAccounts.SingleOrDefault(p => p.Number == _debitAcc);
                if (null == ca)
                {
                    return false;
                }

                //UserAccount ua = _session.CreateCriteria(typeof(UserAccount))
                //    .Add(Expression.Eq("Aid", ca.Id))
                //    .Add(Expression.Eq("Fid", _fid))
                //    .Add(Expression.Eq("Uid", uid))
                //    .UniqueResult<UserAccount>();
                //UserAccount ua = UserAccounts.SingleOrDefault(u => (u.Uid == uid && u.Aid == ca.Id));
                /*update by sayed, mencegah error gegara balikan nga unik*/
                UserAccount ua = UserAccounts.LastOrDefault(u => (u.Uid == uid && u.Aid == ca.Id));

                if (null == ua)
                {
                    return false;
                }
            }
            #endregion

            //Cek Limit User
            #region Kewenangan Limit
            double amt = 0;
            if (!_debitCur.Equals("IDR"))
            {
                //Currency currang = _session.CreateCriteria(typeof(Currency))
                //    .Add(Expression.Like("Code", "%" + _debitCur.Trim() + "%"))
                //    .UniqueResult<Currency>();

                Currency currang = Currencies.SingleOrDefault(c => c.Code.IndexOf(_debitCur.Trim()) >= 0);

                if (null == currang)
                {
                    amt = 0;
                }
                else
                {
                    amt = _debitAmt * (double)currang.BookingRate;
                }
            }
            else
            {
                amt = _debitAmt;
            }

            double limitcorp = 0;

            if (_fid == 1140)
            {
                if (txcode.Equals("IFT"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 1));
                }
                else if (txcode.Equals("RTG"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 2));
                }
                else if (txcode.Equals("LLG"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 3));
                }
                else if (txcode.Equals("SWI"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 4));
                }
                else if (txcode.Equals("WIC"))
                {
                    limitcorp = Convert.ToDouble(ah.GetUserLimit(_fid, 5));
                }
            }
            else limitcorp = (double)Convert.ToDouble(ah.GetUserLimit(_fid, 1));
            bool abc = false; 
            if (uid == 1230)
            {
                abc = true;
            }

            if (amt > limitcorp)
            {
                return false;
            }

            return true;

            #endregion
        }
        catch
        {
            return false;
        }
    }

    public UserMatrix2[] Tc { get { return _tc; } }
    private UserMatrix2[] _tc;

    public int Fid { get { return _fid; } }
    private int _fid;

    public int Cid { get { return _cid; } }
    private int _cid;

    public String txcode { get { return _txcode; } }
    private String _txcode;

    public String listId { get { return _listId; } }
    private String _listId;

    public String debitAcc { get { return _debitAcc; } }
    private String _debitAcc;

    public String Currency { get { return _debitCur; } }
    private String _debitCur;

    public double Amount { get { return _debitAmt; } }
    private double _debitAmt;

    private ISession _session;
    private ManualResetEvent _doneEvent;

    private IList<ClientAccount> _ClientAccounts;
    public IList<ClientAccount> ClientAccounts { get { return _ClientAccounts; } }

    private IList<Currency> _Currencies;
    public IList<Currency> Currencies { get { return _Currencies; } }

    private IList<UserAccount> _UserAccounts;
    public IList<UserAccount> UserAccounts { get { return _UserAccounts; } }

}
