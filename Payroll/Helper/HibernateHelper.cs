using System;
using System.Web;
using NHibernate;
using NHibernate.Cfg;

public sealed class NHibernateHelper
{
    private const String CurrentSessionKey = "nhibernate.current_session";
    private static ISessionFactory sessionFactory;

    static NHibernateHelper()
    {
        sessionFactory = new Configuration().Configure().BuildSessionFactory();
    }

    public static ISession GetCurrentSession()
    {
        HttpContext context = HttpContext.Current;
        ISession currentSession;
        try
        {
            currentSession = context.Items[CurrentSessionKey] as ISession;

            if (currentSession == null)
            {
                currentSession = sessionFactory.OpenSession();
                context.Items[CurrentSessionKey] = currentSession;
            }

            if (!currentSession.IsOpen)
            {
                currentSession = sessionFactory.OpenSession();
            }
        }
        catch (Exception ex)
        {
            currentSession = sessionFactory.OpenSession();
            //context.Items[CurrentSessionKey] = currentSession;
            if (!currentSession.IsOpen)
            {
                currentSession = sessionFactory.OpenSession();
            }              
        }

        return currentSession;
    }

    public static void CloseSession()
    {
        HttpContext context = HttpContext.Current;
        ISession currentSession = context.Items[CurrentSessionKey] as ISession;

        if (currentSession == null)
        {
            return;
        }

        currentSession.Close();
        context.Items.Remove(CurrentSessionKey);
    }

    public static void CloseSessionFactory()
    {
        if (sessionFactory != null)
        {
            sessionFactory.Close();
        }
    }
}