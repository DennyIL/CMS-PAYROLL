using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Web.UI.WebControls;
//using BRIChannelSchedulerNew.GlobalPocos;
using BRIChannelSchedulerNew.Payroll.Pocos;
using System.Xml;
using System.Xml.Serialization;
using NHibernate;
using NHibernate.Expression;


public abstract class TotalOBHelper
{
    public static String Overbooking = "OB";
    public static String Payroll = "PAYROLL";
    public static String Pooling = "LMS";
    public static String RTGS = "RTGS";
    public static String LLG = "LLG";
    public static String SWIFT = "SWIFT";
    public static String PSW = "PSW";
    public static String WIC = "WIC";
    public static String IFT = "IFT";
    public static String LC = "LC";
    public static String SBLC = "SBLC";

    //Penambahan untuk Payroll BRIS - 2020-01-16
    public static String BRIS = "BRIS";

    public static String onTotalOutLimitIDR(ISession session, int Cid, String ValDate, Double Amt)
    {
        String result = "";
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(TrxOBHist));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("ValueDate", ValDate));
            criteria.Add(Expression.Eq("CreditCur", "IDR"));
            criteria.Add(Expression.Not(Expression.Or(Expression.Or(Expression.Eq("TrxCode", TotalOBHelper.Overbooking), Expression.Eq("TrxCode", TotalOBHelper.Pooling)), Expression.Eq("TrxCode", TotalOBHelper.Payroll))));
            criteria.Add(Expression.Not(Expression.Eq("Status", 4)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("BaseAmt"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            double currTrx = trx[0].TotalAmount;
            double anotTrx = getTotalOutIDR(session, Cid, ValDate);

            if (anotTrx != -99)
            {
                currTrx += anotTrx;

                if ((currTrx + Amt) > cli.LimitOutIDR)
                {
                    result = "Melewati Limit Harian Transaksi Keluar BRI (IDR)";
                }
                else
                {
                    result = "[VALID]";
                }
            }
            else
            {
                result = "Penghitungan Akumulasi Transaksi Gagal";
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static String onTotalOutLimitValas(ISession session, int Cid, String ValDate, String Curr, Double Amt)
    {
        String result = "";
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(TrxOBHist));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("ValueDate", ValDate));
            criteria.Add(Expression.Not(Expression.Eq("CreditCur", "IDR")));
            criteria.Add(Expression.Eq("TrxCode", TotalOBHelper.SWIFT));
            criteria.Add(Expression.Not(Expression.Eq("Status", 4)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("BaseAmt"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            Currency curr = session.CreateCriteria(typeof(Currency))
                    .Add(Expression.Like("Code", "%" + Curr + "%"))
                    .UniqueResult<Currency>();

            Amt = Amt * (double)curr.BookingRate;

            Double tot_amt = trx[0].TotalAmount + Amt;

            curr = session.CreateCriteria(typeof(Currency))
                    .Add(Expression.Like("Code", "%USD%"))
                    .UniqueResult<Currency>();

            tot_amt = tot_amt / (double)curr.BookingRate;

            double anotTrx = getTotalOutValas(session, Cid, ValDate);

            if (anotTrx != -99)
            {
                tot_amt += anotTrx;
                if (tot_amt > cli.LimitOutValas)
                {
                    result = "Melewati Limit Harian Transaksi Keluar BRI (Valas)";
                }
                else
                {
                    result = "[VALID]";
                }
            }
            else
            {
                result = "Penghitungan Akumulasi Transaksi Gagal";
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static String onTotalOBLimitOld(ISession session, int Cid, String ValDate, String Curr, Double Amt)
    {
        String result = "";
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(TrxOBHist));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("ValueDate", ValDate));
            criteria.Add(Expression.Not(Expression.Eq("Status", 4)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("BaseAmt"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            if (!Curr.Equals("IDR"))
            {
                Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + Curr + "%"))
                        .UniqueResult<Currency>();

                Amt = Amt * (double)curr.BookingRate;
            }

            double currTrx = trx[0].TotalAmount;
            double anotTrx = getTotalOB(session, Cid, ValDate);

            if (anotTrx != -99)
            {
                currTrx += anotTrx;
                if ((currTrx + Amt) > cli.LimitTotalOB)
                {
                    result = "Melewati Limit Harian Transaksi OB & Transfer";
                }
                else
                {
                    result = "[VALID]";
                }
            }
            else
            {
                result = "Penghitungan Akumulasi Transaksi Gagal";
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static String onTotalOBLimit(ISession session, int Cid, String ValDate, String Curr, Double Amt)
    {
        //SayedZul 20161020 - optimizing, sering timeout pakek createcriteria
        String result = "";
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            //IList<SummaryOBList> trx = null;
            //ICriteria criteria = session.CreateCriteria(typeof(TrxOBHist));
            //criteria.Add(Expression.Eq("ClientId", Cid));
            //criteria.Add(Expression.Eq("ValueDate", ValDate));
            //criteria.Add(Expression.Not(Expression.Eq("Status", 4)));

            //trx = criteria.SetProjection(Projections.ProjectionList()
            //                   .Add(Projections.Sum("BaseAmt"), "TotalAmount"))
            //                .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
            //                .List<SummaryOBList>();

            string q = @"select sum(baseamt) from trxobhist where clientid = " + Cid + " and valuedate = '" + ValDate + "' and status <> 4";
            string dbResult = "";
            string msg = "";
            if (!PayrollHelper.ExecuteQueryValue(session, q, out dbResult, out msg))
            {
                //EvtLogger.Write("Exception on Total OB Helper :: " + msg, System.Diagnostics.EventLogEntryType.Error);
                result = "Penghitungan Akumulasi Transaksi Gagal";
            }
            else
            {
                double amtObHist = 0;
                if (!String.IsNullOrEmpty(dbResult))
                    amtObHist = Double.Parse(dbResult);

                if (!Curr.Equals("IDR"))
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                            .Add(Expression.Like("Code", "%" + Curr + "%"))
                            .UniqueResult<Currency>();

                    Amt = Amt * (double)curr.BookingRate;
                }

                double currTrx = amtObHist;
                double anotTrx = getTotalOB(session, Cid, ValDate);

                if (anotTrx != -99)
                {
                    currTrx += anotTrx;
                    if ((currTrx + Amt) > cli.LimitTotalOB)
                    {
                        result = "Melewati Limit Harian Transaksi OB & Transfer";
                    }
                    else
                    {
                        result = "[VALID]";
                    }
                }
                else
                {
                    result = "Penghitungan Akumulasi Transaksi Gagal";
                }
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }


    /* Harry 1 Des 2014 */

    public static String onTotalOutLimitNCL(ISession session, int Cid, String ValDate, String Curr, Double Amt)
    {
        String result = "";
        int[] xxx = new int[] { 4, 5, 6 };
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(HistoryNCL));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("DebetKredit", "D"));
            criteria.Add(Expression.Eq("CreatedTime", ValDate));
            criteria.Add(Expression.Not(Expression.In("Status", xxx)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("ConvAmount"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            Currency curr = session.CreateCriteria(typeof(Currency))
                    .Add(Expression.Like("Code", "%" + Curr + "%"))
                    .UniqueResult<Currency>();

            Amt = Amt * (double)curr.BookingRate;

            Double tot_amt = -1 * (trx[0].TotalAmount) + Amt;

            curr = session.CreateCriteria(typeof(Currency))
                    .Add(Expression.Like("Code", "%USD%"))
                    .UniqueResult<Currency>();

            tot_amt = tot_amt / (double)curr.BookingRate;

            double anotTrx = getTotalOutValas(session, Cid, ValDate);

            if (anotTrx != -99)
            {
                tot_amt += anotTrx;
                if (tot_amt > cli.LimitOutValas)
                {
                    result = "Melewati Limit Harian Transaksi LC BRI (Valas)";
                }
                else
                {
                    result = "[VALID]";
                }
            }
            else
            {
                result = "Penghitungan Akumulasi Transaksi Gagal";
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static String onTotalNCLLimit(ISession session, int Cid, String ValDate, String Curr, Double Amt)
    {
        String result = "";
        int[] xxx = new int[] { 4, 5, 6 };
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(HistoryNCL));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("DebetKredit", "D"));
            criteria.Add(Expression.Eq("CreatedTime", ValDate));
            criteria.Add(Expression.Not(Expression.In("Status", xxx)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("ConvAmount"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            if (!Curr.Equals("IDR"))
            {
                Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + Curr + "%"))
                        .UniqueResult<Currency>();

                Amt = Amt * (double)curr.BookingRate;
            }

            double currTrx = trx[0].TotalAmount;
            double anotTrx = getTotalOB(session, Cid, ValDate);

            if (anotTrx != -99)
            {
                currTrx += anotTrx;
                if ((currTrx + Amt) > cli.LimitTotalOB)
                {
                    result = "Melewati Limit Harian Transaksi LC";
                }
                else
                {
                    result = "[VALID]";
                }
            }
            else
            {
                result = "Penghitungan Akumulasi Transaksi Gagal";
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    /* End */

    public static String onTotalHarianLimit(ISession session, int Cid, String TrxType, String ValDate, String Curr, Double Amt)
    {
        if (ValDate.Equals("991231"))
        {
            ValDate = DateTime.Now.ToString("yyMMdd");
        }

        String result = "";
        try
        {
            if ((TrxType.Equals(TotalOBHelper.SWIFT)) || (TrxType.Equals(TotalOBHelper.RTGS)) || (TrxType.Equals(TotalOBHelper.LLG)) || (TrxType.Equals(TotalOBHelper.PSW)) || (TrxType.Equals(TotalOBHelper.WIC)))
            {
                if (Curr.Equals("IDR"))
                {
                    result = TotalOBHelper.onTotalOutLimitIDR(session, Cid, ValDate, Amt);
                }
                else
                {
                    result = TotalOBHelper.onTotalOutLimitValas(session, Cid, ValDate, Curr, Amt);
                }
            }
            else if ((TrxType.Equals(TotalOBHelper.LC)) || (TrxType.Equals(TotalOBHelper.SBLC)))
            {
                result = TotalOBHelper.onTotalOutLimitNCL(session, Cid, ValDate, Curr, Amt);
            }
            else
            {
                result = "[VALID]";
            }

            if (result.Contains("[VALID]"))
            {
                if ((TrxType.Equals(TotalOBHelper.LC)) || (TrxType.Equals(TotalOBHelper.SBLC)))
                {
                    result = TotalOBHelper.onTotalNCLLimit(session, Cid, ValDate, Curr, Amt);
                }
                else
                {
                    result = TotalOBHelper.onTotalOBLimit(session, Cid, ValDate, Curr, Amt);
                }
            }
        }
        catch (Exception ex)
        {
            result = ex.Message;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxSWIFTTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        try
        {
            Int16[] stat = new Int16[] { 63, 6314, 64, 6414, 59, 5914 };
            IList<TrxSwift> trx = session.CreateCriteria(typeof(TrxSwift))
                .Add(Expression.Not(Expression.Eq("Type", 3)))
                .Add(Expression.In("Status", stat))
                .Add(Expression.Eq("ClientID", Cid.ToString()))
                .Add(Expression.Eq("ValueDate", ValDate))
                .List<TrxSwift>();

            foreach (TrxSwift o in trx)
            {
                if (o.CreditCurrency.Equals("IDR"))
                {
                    result += o.CreditAmount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.CreditCurrency + "%"))
                        .UniqueResult<Currency>();

                    result += o.CreditAmount * (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxSWIFTIDRTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        try
        {
            IList<TrxSwift> trx = session.CreateCriteria(typeof(TrxSwift))
                .Add(Expression.Not(Expression.Eq("Type", 3)))
                .Add(Expression.Le("Status", 64))
                .Add(Expression.Eq("ClientID", Cid.ToString()))
                .Add(Expression.Eq("ValueDate", ValDate))
                .List<TrxSwift>();

            foreach (TrxSwift o in trx)
            {
                if (o.CreditCurrency.Equals("IDR"))
                {
                    result += o.CreditAmount;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxSWIFTValasTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        try
        {
            Int16[] stat = new Int16[] { 63, 6314, 64, 6414, 59, 5914 };
            IList<TrxSwift> trx = session.CreateCriteria(typeof(TrxSwift))
                .Add(Expression.Not(Expression.Eq("Type", 3)))
                .Add(Expression.In("Status", stat))
                .Add(Expression.Eq("ClientID", Cid.ToString()))
                .Add(Expression.Eq("ValueDate", ValDate))
                .List<TrxSwift>();

            foreach (TrxSwift o in trx)
            {
                double amt = 0;
                if (!o.CreditCurrency.Equals("IDR"))
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.CreditCurrency + "%"))
                        .UniqueResult<Currency>();

                    amt = o.CreditAmount * (double)curr.BookingRate;

                    curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%USD%"))
                        .UniqueResult<Currency>();

                    result += amt / (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxRTGSTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 3, 4 };

        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

        try
        {
            IList<TrxRtgsBRI> trx = session.CreateCriteria(typeof(TrxRtgsBRI))
                .Add(Expression.Like("Status", "WAITING%"))
                .Add(Expression.Eq("ClientID", Cid))
                .Add(Expression.Ge("TrxDate", start))
                .Add(Expression.Le("TrxDate", end))
                .List<TrxRtgsBRI>();

            foreach (TrxRtgsBRI o in trx)
            {
                if (o.InstCur.Equals("IDR"))
                {
                    result += o.InstAmount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.InstCur + "%"))
                        .UniqueResult<Currency>();

                    result += o.InstAmount * (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxLLGTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 3, 4 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxClearingBRI> trx = session.CreateCriteria(typeof(TrxClearingBRI))
                .Add(Expression.Like("Status", "WAITING%"))
                .Add(Expression.Eq("ClientID", Cid))
                .Add(Expression.Ge("TrxDate", start))
                .Add(Expression.Le("TrxDate", end))
                .List<TrxClearingBRI>();

            foreach (TrxClearingBRI o in trx)
            {
                if (o.InstCur.Equals("IDR"))
                {
                    result += o.InstAmount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.InstCur + "%"))
                        .UniqueResult<Currency>();

                    result += o.InstAmount * (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxTransferBRITotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 3, 4 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxTransferBRI> trx = session.CreateCriteria(typeof(TrxTransferBRI))
                .Add(Expression.Like("Status", "WAITING%"))
                .Add(Expression.Eq("ClientID", Cid))
                .Add(Expression.Ge("TrxDate", start))
                .Add(Expression.Le("TrxDate", end))
                .List<TrxTransferBRI>();

            foreach (TrxTransferBRI o in trx)
            {

                if (o.InstCur.Equals("IDR"))
                {
                    result += o.InstAmount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.InstCur + "%"))
                        .UniqueResult<Currency>();

                    result += o.InstAmount * (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxPSWTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 3, 4 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxTransferRTO> trx = session.CreateCriteria(typeof(TrxTransferRTO))
                .Add(Expression.Like("Status", "WAITING"))
                .Add(Expression.Eq("ClientID", Cid))
                .Add(Expression.Ge("TrxDate", start))
                .Add(Expression.Le("TrxDate", end))
                .List<TrxTransferRTO>();

            foreach (TrxTransferRTO o in trx)
            {

                if (o.CreditCurrency.Equals("IDR"))
                {
                    result += o.CreditAmount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.CreditCurrency + "%"))
                        .UniqueResult<Currency>();

                    result += o.CreditAmount * (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }
    /* Update Lia - 30 July 2012 */
    public static double getTrxWICTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 3, 4 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxWalkincustomer> trx = session.CreateCriteria(typeof(TrxWalkincustomer))
                .Add(Expression.Like("Status", "WAITING%"))
                .Add(Expression.Eq("ClientID", Cid))
                .Add(Expression.Ge("TrxDate", start))
                .Add(Expression.Le("TrxDate", end))
                .List<TrxWalkincustomer>();

            foreach (TrxWalkincustomer o in trx)
            {

                if (o.CreditCurrency.Equals("IDR"))
                {
                    result += o.CreditAmount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.CreditCurrency + "%"))
                        .UniqueResult<Currency>();

                    result += o.CreditAmount * (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }
    /* End Update Lia - 30 July 2012 */

    public static double getTrxMFTTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        bool isNow = false;
        if (ValDate.Equals(DateTime.Now.ToString("yyMMdd")))
            isNow = true;
        int[] stat = new int[] { 14 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        IList<Currency> currencies = session.CreateCriteria(typeof(Currency))
            .List<Currency>();
        try
        {
            IList<TrxMassFT> trx = new List<TrxMassFT>();

            trx = session.CreateCriteria(typeof(TrxMassFT))
                .Add(Expression.Eq("Client", Cid))
                .Add(Expression.Ge("RunningTime", start))
                .Add(Expression.Le("RunningTime", end))
                .Add(Expression.Not(Expression.In("Status", new int[] { 3, 4 })))
                .List<TrxMassFT>();

            foreach (TrxMassFT o in trx)
            {
                IList<TrxMassFTDetail> trx2 = session.CreateCriteria(typeof(TrxMassFTDetail))
                    .Add(Expression.In("Status", stat))
                    .Add(Expression.Eq("ParentId", o.Id))
                .List<TrxMassFTDetail>();

                foreach (TrxMassFTDetail o2 in trx2)
                {
                    if (o2.CreditCurrency.Equals("IDR"))
                    {
                        result += o2.Amount;
                    }
                    else
                    {
                        //Currency curr = session.CreateCriteria(typeof(Currency))
                        //    .Add(Expression.Like("Code", "%" + o2.CreditCurrency + "%"))
                        //    .UniqueResult<Currency>();

                        Currency curr = currencies.SingleOrDefault(c => c.Code.IndexOf(o2.CreditCurrency.Trim()) >= 0);


                        result += o2.Amount * (double)curr.BookingRate;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxMRTGSTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 14 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxMassRTGS> trx = session.CreateCriteria(typeof(TrxMassRTGS))
                .Add(Expression.Eq("Client", Cid))
                .Add(Expression.Ge("RunningTime", start))
                .Add(Expression.Le("RunningTime", end))
                .List<TrxMassRTGS>();

            foreach (TrxMassRTGS o in trx)
            {
                IList<TrxMassRTGSDetail> trx2 = session.CreateCriteria(typeof(TrxMassRTGSDetail))
                    .Add(Expression.In("Status", stat))
                    .Add(Expression.Eq("ParentId", o.Id))
                .List<TrxMassRTGSDetail>();

                foreach (TrxMassRTGSDetail o2 in trx2)
                {
                    result += o2.CreditAmount;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxMLLGTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 14 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxMassLLG> trx = session.CreateCriteria(typeof(TrxMassLLG))
                .Add(Expression.Eq("Client", Cid))
                .Add(Expression.Ge("RunningTime", start))
                .Add(Expression.Le("RunningTime", end))
                .List<TrxMassLLG>();

            foreach (TrxMassLLG o in trx)
            {
                IList<TrxMassLLGDetail> trx2 = session.CreateCriteria(typeof(TrxMassLLGDetail))
                    .Add(Expression.In("Status", stat))
                    .Add(Expression.Eq("ParentId", o.Id))
                .List<TrxMassLLGDetail>();

                foreach (TrxMassLLGDetail o2 in trx2)
                {
                    result += o2.CreditAmount;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxPPTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 14 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxPaymentPriorityBRIDetail> trx = session.CreateCriteria(typeof(TrxPaymentPriorityBRIDetail))
                .Add(Expression.Eq("ClientId", Cid))
                .Add(Expression.Ge("ValueDate", start))
                .Add(Expression.Le("ValueDate", end))
                .Add(Expression.In("Status", stat))
                .List<TrxPaymentPriorityBRIDetail>();

            foreach (TrxPaymentPriorityBRIDetail o in trx)
            {
                if (o.Currency.Equals("IDR"))
                {
                    result += o.Amount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.Currency + "%"))
                        .UniqueResult<Currency>();

                    if (o.InstructionCode.Equals("SWI"))
                    {
                        if (String.IsNullOrEmpty(o.CmsRefNo.Trim()))
                        {
                            result += o.Amount * (double)curr.BookingRate;
                        }
                        else
                        {
                            IList<TrxSwift> trxswi = session.CreateCriteria(typeof(TrxSwift))
                                .Add(Expression.Eq("ReferenceNum", o.CmsRefNo))
                                .AddOrder(new Order("Id", false))
                                .List<TrxSwift>();

                            if (trxswi.Count > 0)
                            {
                                Int16[] statue = new Int16[] { 63, 6314, 64, 6414, 59, 5914 };
                                if (statue.Contains<Int16>((Int16)trxswi[0].Status))
                                {
                                    result += o.Amount * (double)curr.BookingRate;
                                }
                            }
                        }
                    }
                    else
                    {
                        result += o.Amount * (double)curr.BookingRate;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxPPOutIDRTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 14 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxPaymentPriorityBRIDetail> trx = session.CreateCriteria(typeof(TrxPaymentPriorityBRIDetail))
                .Add(Expression.Eq("ClientId", Cid))
                .Add(Expression.Ge("ValueDate", start))
                .Add(Expression.Le("ValueDate", end))
                .Add(Expression.In("Status", stat))
                .Add(Expression.Not(Expression.Eq("InstructionCode", "IFT")))
                .Add(Expression.Not(Expression.Eq("InstructionCode", "WIC")))
                .List<TrxPaymentPriorityBRIDetail>();

            foreach (TrxPaymentPriorityBRIDetail o in trx)
            {
                if (o.Currency.Equals("IDR"))
                {
                    result += o.Amount;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxPPOutValasTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 14 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxPaymentPriorityBRIDetail> trx = session.CreateCriteria(typeof(TrxPaymentPriorityBRIDetail))
                .Add(Expression.Eq("ClientId", Cid))
                .Add(Expression.Ge("ValueDate", start))
                .Add(Expression.Le("ValueDate", end))
                .Add(Expression.In("Status", stat))
                .Add(Expression.Not(Expression.Eq("InstructionCode", "IFT")))
                .Add(Expression.Not(Expression.Eq("InstructionCode", "WIC")))
                .List<TrxPaymentPriorityBRIDetail>();

            foreach (TrxPaymentPriorityBRIDetail o in trx)
            {
                double amt = 0;
                if (!o.Currency.Equals("IDR"))
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                             .Add(Expression.Like("Code", "%" + o.Currency + "%"))
                             .UniqueResult<Currency>();

                    amt = o.Amount * (double)curr.BookingRate;

                    curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%USD%"))
                        .UniqueResult<Currency>();

                    if (o.InstructionCode.Equals("SWI"))
                    {
                        if (String.IsNullOrEmpty(o.CmsRefNo.Trim()))
                        {
                            result += amt / (double)curr.BookingRate;
                        }
                        else
                        {
                            IList<TrxSwift> trxswi = session.CreateCriteria(typeof(TrxSwift))
                                .Add(Expression.Eq("ReferenceNum", o.CmsRefNo))
                                .AddOrder(new Order("Id", false))
                                .List<TrxSwift>();

                            if (trxswi.Count > 0)
                            {
                                Int16[] statue = new Int16[] { 63, 6314, 64, 6414, 59, 5914 };
                                if (statue.Contains<Int16>((Int16)trxswi[0].Status))
                                {
                                    result += amt / (double)curr.BookingRate;
                                }
                            }
                        }
                    }
                    else
                    {
                        result += amt / (double)curr.BookingRate;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    /* Harry 1 Des 2014 */
    public static double getTrxNCLTotal(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 3, 4 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<HistoryNCL> trx = session.CreateCriteria(typeof(HistoryNCL))
                .Add(Expression.Eq("Status", 3))
                .Add(Expression.Eq("ClientId", Cid))
                .Add(Expression.Ge("CreatedTime", start))
                .Add(Expression.Le("CreatedTime", end))
                .List<HistoryNCL>();

            foreach (HistoryNCL o in trx)
            {

                if (o.Currency.Equals("IDR"))
                {
                    result += o.Amount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.Currency + "%"))
                        .UniqueResult<Currency>();

                    result += o.Amount * (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    /* End Update */

    public static double getTotalOB(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        double restemp = 0;
        try
        {
            result = getTrxSWIFTTotal(session, Cid, ValDate);
            restemp += result;
            if (result != -99)
            {
                restemp += result;
                result = getTrxRTGSTotal(session, Cid, ValDate);
                if (result != -99)
                {
                    restemp += result;
                    result = getTrxLLGTotal(session, Cid, ValDate);
                    if (result != -99)
                    {
                        restemp += result;
                        result = getTrxPPTotal(session, Cid, ValDate);
                        if (result != -99)
                        {
                            restemp += result;
                            result = getTrxMRTGSTotal(session, Cid, ValDate);
                            if (result != -99)
                            {
                                restemp += result;
                                result = getTrxMLLGTotal(session, Cid, ValDate);
                                if (result != -99)
                                {
                                    restemp += result;
                                    result = getTrxMFTTotal(session, Cid, ValDate);
                                    if (result != -99)
                                    {
                                        restemp += result;
                                        result = getTrxTransferBRITotal(session, Cid, ValDate);
                                        if (result != -99)
                                        {
                                            restemp += result;
                                            result = getTrxWICTotal(session, Cid, ValDate);
                                            if (result != -99)
                                            {
                                                restemp += result;
                                                result = getTrxNCLTotal(session, Cid, ValDate);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (result != -99)
            {
                restemp += result;
                result = restemp;
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTotalOutIDR(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        double restemp = 0;
        try
        {
            result = getTrxSWIFTIDRTotal(session, Cid, ValDate);
            restemp += result;
            if (result != -99)
            {
                restemp += result;
                result = getTrxRTGSTotal(session, Cid, ValDate);
                if (result != -99)
                {
                    restemp += result;
                    result = getTrxLLGTotal(session, Cid, ValDate);
                    if (result != -99)
                    {
                        restemp += result;
                        result = getTrxPPOutIDRTotal(session, Cid, ValDate);
                        if (result != -99)
                        {
                            restemp += result;
                            result = getTrxMRTGSTotal(session, Cid, ValDate);
                            if (result != -99)
                            {
                                restemp += result;
                                result = getTrxMLLGTotal(session, Cid, ValDate);
                                if (result != -99)
                                {
                                    restemp += result;
                                    result = getTrxWICTotal(session, Cid, ValDate);
                                    if (result != -99)
                                    {
                                        restemp += result;
                                        result = getTrxNCLTotal(session, Cid, ValDate);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (result != -99)
            {
                restemp += result;
                result = restemp;
            }

        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTotalOutValas(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        double restemp = 0;
        try
        {
            result = getTrxSWIFTValasTotal(session, Cid, ValDate);
            restemp += result;
            if (result != -99)
            {
                restemp += result;
                result = getTrxPPOutValasTotal(session, Cid, ValDate);
            }

            if (result != -99)
            {
                restemp += result;
                result = restemp;
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxPPTotalFT(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        Int16[] stat = new Int16[] { 14 };
        DateTime start = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 00:00:00", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        DateTime end = DateTime.ParseExact(ValDate.Substring(4, 2) + "/" + ValDate.Substring(2, 2) + "/20" + ValDate.Substring(0, 2) + " 23:59:59", "dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            IList<TrxPaymentPriorityBRIDetail> trx = session.CreateCriteria(typeof(TrxPaymentPriorityBRIDetail))
                .Add(Expression.Eq("ClientId", Cid))
                .Add(Expression.Ge("ValueDate", start))
                .Add(Expression.Le("ValueDate", end))
                .Add(Expression.In("Status", stat))
                .Add(Expression.Le("InstructionCode", IFT))//tambahan untuk OB
                .List<TrxPaymentPriorityBRIDetail>();

            foreach (TrxPaymentPriorityBRIDetail o in trx)
            {
                if (o.Currency.Equals("IDR"))
                {
                    result += o.Amount;
                }
                else
                {
                    Currency curr = session.CreateCriteria(typeof(Currency))
                        .Add(Expression.Like("Code", "%" + o.Currency + "%"))
                        .UniqueResult<Currency>();

                    result += o.Amount * (double)curr.BookingRate;
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTrxOBHistIFT(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        try
        {
            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(TrxOBHist));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("ValueDate", ValDate));
            criteria.Add(Expression.Eq("TrxCode", Overbooking));//untuk OB 
            criteria.Add(Expression.Not(Expression.Eq("Status", 4)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("BaseAmt"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            result = trx[0].TotalAmount;

        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }

    public static double getTotalOBIFT(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        try
        {
            result += getTrxOBHistIFT(session, Cid, ValDate);
            if (result != -99)
            {
                result += getTrxMFTTotal(session, Cid, ValDate);
                if (result != -99)
                {
                    result += getTrxTransferBRITotal(session, Cid, ValDate);
                    if (result != -99)
                    {
                        result += getTrxPPTotalFT(session, Cid, ValDate);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result = -99;
            //EvtLogger.Write("Exception on Total OB Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }


    public static double TotalOutLimitIDRHist(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(TrxOBHist));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("ValueDate", ValDate));
            criteria.Add(Expression.Eq("CreditCur", "IDR"));
            criteria.Add(Expression.Not(Expression.Or(Expression.Or(Expression.Eq("TrxCode", TotalOBHelper.Overbooking), Expression.Eq("TrxCode", TotalOBHelper.Pooling)), Expression.Eq("TrxCode", TotalOBHelper.Payroll))));
            criteria.Add(Expression.Not(Expression.Eq("Status", 4)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("BaseAmt"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            result = trx[0].TotalAmount;

        }
        catch (Exception ex)
        {
            //EvtLogger.Write("Exception on Total OB Helper Hist :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }


    public static double TotalOutLimitValasHist(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(TrxOBHist));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("ValueDate", ValDate));
            criteria.Add(Expression.Not(Expression.Eq("CreditCur", "IDR")));
            criteria.Add(Expression.Eq("TrxCode", TotalOBHelper.SWIFT));
            criteria.Add(Expression.Not(Expression.Eq("Status", 4)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("BaseAmt"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            Double tot_amt = trx[0].TotalAmount;
            Currency curr = session.CreateCriteria(typeof(Currency))
                    .Add(Expression.Like("Code", "%USD%"))
                    .UniqueResult<Currency>();

            tot_amt = tot_amt / (double)curr.BookingRate;
            result = tot_amt;

        }
        catch (Exception ex)
        {
            //EvtLogger.Write("Exception on Total OB Valas Helper Hist :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }


    public static double TotalOBLimitHist(ISession session, int Cid, String ValDate)
    {
        double result = 0;
        try
        {
            Client cli = session.CreateCriteria(typeof(Client))
                   .Add(Expression.Eq("Id", Cid))
                   .UniqueResult<Client>();

            IList<SummaryOBList> trx = null;
            ICriteria criteria = session.CreateCriteria(typeof(TrxOBHist));
            criteria.Add(Expression.Eq("ClientId", Cid));
            criteria.Add(Expression.Eq("ValueDate", ValDate));
            criteria.Add(Expression.Not(Expression.Eq("Status", 4)));

            trx = criteria.SetProjection(Projections.ProjectionList()
                               .Add(Projections.Sum("BaseAmt"), "TotalAmount"))
                            .SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(SummaryOBList)))
                            .List<SummaryOBList>();

            result = trx[0].TotalAmount;

        }
        catch (Exception ex)
        {
            //EvtLogger.Write("Exception on Total OB Hist Helper :: " + ex.Message + " => " + ex.StackTrace, System.Diagnostics.EventLogEntryType.Error);
        }

        return result;
    }
}

public class SummaryOBList
{
    public Double TotalAmount { get; set; }
}
