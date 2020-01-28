using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using BRIChannelSchedulerNew.Payroll.Pocos;

namespace BRIChannelSchedulerNew.Payroll.Helper
{
    public abstract class AuditHelper
    {
        public static void Log(ISession session, String user, String description, Int32 txid, String data)
        {
            Audit audit = new Audit();
            audit.Time = DateTime.Now;
            audit.TransactionId = txid;
            audit.User = user;
            audit.Description = description;
            audit.Data = data;
            audit.IPInfo = getIPInfo();
            session.Save(audit);
            session.Flush();
        }

        public static String getIPInfo()
        {
            try
            {
                String HTTP_X_FORWARDED_FOR = "";
                String HTTP_CLIENT_IP = "";
                String HTTP_X_FORWARDED = "";
                String HTTP_X_CLUSTER_CLIENT_IP = "";
                String HTTP_FORWARDED_FOR = "";
                String HTTP_FORWARDED = "";
                String REMOTE_ADDR = "";

                System.Web.HttpContext current = System.Web.HttpContext.Current;
                REMOTE_ADDR = "REMOTE_ADDR: " + current.Request.ServerVariables["REMOTE_ADDR"];
                HTTP_X_FORWARDED_FOR = "HTTP_X_FORWARDED_FOR: " + current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                HTTP_CLIENT_IP = "HTTP_CLIENT_IP: " + current.Request.ServerVariables["HTTP_CLIENT_IP"];
                HTTP_X_FORWARDED = "HTTP_X_FORWARDED: " + current.Request.ServerVariables["HTTP_X_FORWARDED"];
                HTTP_X_CLUSTER_CLIENT_IP = "HTTP_X_CLUSTER_CLIENT_IP: " + current.Request.ServerVariables["HTTP_X_CLUSTER_CLIENT_IP"];
                HTTP_FORWARDED_FOR = "HTTP_FORWARDED_FOR: " + current.Request.ServerVariables["HTTP_FORWARDED_FOR"];
                HTTP_FORWARDED = "HTTP_FORWARDED: " + current.Request.ServerVariables["HTTP_FORWARDED"];
                return REMOTE_ADDR + System.Environment.NewLine +
                       HTTP_X_FORWARDED_FOR + System.Environment.NewLine +
                       HTTP_CLIENT_IP + System.Environment.NewLine +
                       HTTP_X_FORWARDED + System.Environment.NewLine +
                       HTTP_X_CLUSTER_CLIENT_IP + System.Environment.NewLine +
                       HTTP_FORWARDED_FOR + System.Environment.NewLine +
                       HTTP_FORWARDED;

            }
            catch (Exception ex)
            {
                //EvtLogger.Write("Get IP Info: " + ex.Message + " " + ex.StackTrace, EvtLogger.ERROR);
                return "Unable to get IP Info";
            }
        }
    }
}
