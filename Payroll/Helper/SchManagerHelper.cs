using System;
using System.Net;
using System.Net.Mail;
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
using System.IO;
using System.Text.RegularExpressions;
using System.Management;


namespace BRIChannelSchedulerNew.Payroll.Helper
{
    public abstract class SchManagerHelper
    {

        public static bool ReadLog(ISession session, SchManager sch, out string msg)
        {
            bool result = true;
            msg = "";

            try
            {
                string LogPath = sch.Path + "\\logs\\log.txt";
                FileInfo FileLogInfo = new FileInfo(LogPath);
                //FileAttributes FileLogAtribut = FileLogInfo.Attributes;
                sch.SchLogLastUpdate = FileLogInfo.LastWriteTime;
                session.Update(sch);
                session.Flush();
            }
            catch (Exception e)
            {
                msg = e.Message + "||" + e.InnerException + "||" + e.StackTrace;
            }

            return result;
        }


        public static bool AbsensiSch(ISession session, string SchCode, string Message, out string OutMsg)
        {
            bool result = true;
            OutMsg = "";

            try
            {
                IList<SchManager> ListSchMan = session.CreateCriteria(typeof(SchManager))
                    .Add(Expression.Eq("SchCode", SchCode))
                    .List<SchManager>();

                if (ListSchMan.Count == 1)
                {
                    //kosongi log tiap 1 hari
                    if (ListSchMan[0].SchLastRun.Day < DateTime.Now.Day)
                    {
                        ListSchMan[0].Message = "";
                        session.Update(ListSchMan[0]);
                        session.Flush();
                        OutMsg = "Message sukses dikosongi.";
                    }

                    int JedaInsertLagi = int.Parse(ParameterHelper.GetString("SCH_MGR_JEDA_INSERT", session));//dalam second, 0 berarti tidak ada jeda
                    if (ListSchMan[0].SchLastRun.AddSeconds(JedaInsertLagi) <= DateTime.Now)
                    {
                        string sekarang = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");

                        ListSchMan[0].Message = sekarang + " " + Message + "|" + ListSchMan[0].Message;
                        ListSchMan[0].SchLastRun = DateTime.Now;
                        ListSchMan[0].LastUpdate = DateTime.Now;
                        session.Update(ListSchMan[0]);
                        session.Flush();
                        OutMsg = "Absen SUKSES. Terimakasih sudah absen :)";
                    }
                    else
                    {
                        OutMsg = "belom " + JedaInsertLagi.ToString() + " detik, belom boleh insert kakak.";
                    }
                }
                else
                {
                    OutMsg = "SCH Code tidak unik atau tidak ditemukan";
                }
            }
            catch (Exception e)
            {
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }


        public static bool Restart(ISession session, SchManager sch, out string msg)
        {
            bool result = true;
            msg = "";
            string OutMsg = "";

            try
            {
                //do stop
                if (!ExecuteProcess(sch.Path, sch.FileName, "KILL", out OutMsg))
                {
                    msg += "Stop SCH GAGAL. " + OutMsg;
                }

                //do start
                if (!ExecuteProcess(sch.Path, sch.FileName, "START", out OutMsg))
                {
                    msg += "Start SCH GAGAL. " + OutMsg;
                }

                string sekarang = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                sch.RestartStatus = ParameterHelper.SCH_ON;
                sch.Message = sekarang + " Success Restart.|" + sch.Message;
                sch.ErrorDescription = sch.Message;
                sch.LastUpdate = DateTime.Now;
                session.Update(sch);
                session.Flush();
            }
            catch (Exception e)
            {
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }


        public static bool ShutDown(ISession session, SchManager sch, out string msg)
        {
            bool result = true;
            msg = "";
            string OutMsg = "";

            try
            {
                //do stop
                if (!ExecuteProcess(sch.Path, sch.FileName, "KILL", out OutMsg))
                {
                    msg += "Stop SCH GAGAL. " + OutMsg;
                }

                string sekarang = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                sch.RestartStatus = ParameterHelper.SCH_OFF;
                sch.Message = sekarang + " Success Shutdown.|" + sch.Message;
                sch.ErrorDescription = sch.Message;
                sch.LastUpdate = DateTime.Now;
                session.Update(sch);
                session.Flush();
            }
            catch (Exception e)
            {
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
                result = false;
            }

            return result;
        }



        private static bool ExecuteProcess(string Path, string FileName, string Mode, out string OutMsg)
        {
            OutMsg = "";
            bool result = true;
            string FullPath = Path + "\\" + FileName;

            try
            {
                if (Mode.Equals("START"))//start process
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = FullPath;
                    proc.Start();
                }
                else if (Mode.Equals("KILL"))//kill process
                {
                    Process[] collectionOfProcess = Process.GetProcessesByName(FileName.Replace(".exe", ""));

                    foreach (Process RunningSch in collectionOfProcess)
                    {
                        Process acrProcess = RunningSch;
                        string processPath = acrProcess.MainModule.FileName;
                        if (processPath == FullPath)
                        {
                            acrProcess.Kill();
                        }
                    }
                }
                else OutMsg = "Gak nyapo nyapo aku.";
            }
            catch (Exception e)
            {
                result = false;
                OutMsg = "Exception " + e.Message + "||" + e.InnerException + "||" + e.StackTrace;
            }

            return result;
        }
    }
}