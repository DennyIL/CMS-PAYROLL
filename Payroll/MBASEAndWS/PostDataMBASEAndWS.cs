using System;
using System.Collections.Generic;
using Quartz;
using Quartz.Impl;
using log4net;
using BRIChannelSchedulerNew.Payroll.Pocos;
using BRIChannelSchedulerNew.Payroll.Helper;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Expression;
using System.Globalization;

namespace BRIChannelSchedulerNew.Payroll.MBASEAndWS
{
    class PostDataMBASEAndWS : IStatefulJob
    {
        private static String SchCode = "PostDataMBASEAndWS - ExpressProcess";
        private static String SchName = "PostDataMBASEAndWS - ExpressProcess";

        private static Configuration cfg;
        private static ISessionFactory factory;
        ISession session;

        void IJob.Execute(JobExecutionContext context)
        {
            ILog log = LogManager.GetLogger(typeof(PostDataMBASEAndWS));

            try
            {
                if (factory == null)
                {
                    cfg = new Configuration();
                    cfg.AddAssembly("BRIChannelSchedulerNew");
                    factory = cfg.BuildSessionFactory();
                }
                session = factory.OpenSession();
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Starting " + SchName + " ===");

                try
                {
                    Commitjob_transaction(log);
                }
                catch (Exception ee)
                {
                    log.Error(SchCode + " === " + SchName + " Process Error Message:: " + ee.Message);
                    log.Error(SchCode + " === " + SchName + " Process Error Inner Exception:: " + ee.InnerException);
                    log.Error(SchCode + " === " + SchName + " Process Error StackTrace:: " + ee.StackTrace);
                }
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === End " + SchName + " ===");
            }
            catch (Exception ex)
            {
                log.Error(SchCode + " === " + SchName + " Failed To Start Message:: " + ex.Message);
                log.Error(SchCode + " === " + SchName + " Failed To Start Inner Exception:: " + ex.InnerException);
                log.Error(SchCode + " === " + SchName + " Failed To Start StackTrace:: " + ex.StackTrace);
            }
            finally
            {
                session.Clear();
                session.Close();
                session.Dispose();
                factory.Close();
                factory.Dispose();
                cfg = null;
                GC.Collect();

            }
        }

        private void Commitjob_transaction(ILog log)
        {


            IList<TrxPayrollDetail> txList = session.CreateSQLQuery("select * from trxpayrolldetails where status = ? and instructioncode in ('" + TotalOBHelper.RTGS + "','" + TotalOBHelper.LLG + "') and lastupdate < now()")
                        .AddEntity(typeof(TrxPayrollDetail))
                        .SetInt32(0, ParameterHelper.TRXSTATUS_PAYROLLNEW_POST_MBASEANDWS_RTGSANDLLG)
                        .List<TrxPayrollDetail>();

            if (txList.Count > 0)
            {
                foreach (TrxPayrollDetail o in txList)
                {
                    try
                    {
                        #region PARAMETERS
                        int outid = 0;
                        string outmsg = "";
                        Double _amt = 0;

                        TrxPayroll p = o.Parent;
                        ClientAccount clientAcc = new ClientAccount();

                        Booking b = session.Load<Booking>(o.IdBooking);

                        ClientMatrix cm = session.Load<ClientMatrix>(p.ClientID);
                        Client client = session.Load<Client>(p.ClientID);
                        AuthorityHelper ah = new AuthorityHelper(cm.Matrix);

                        if (o.InstructionCode.Equals(TotalOBHelper.RTGS))
                            _amt = Double.Parse(ah.GetTransactionFee(session, 100, 2).ToString());
                        else if (o.InstructionCode.Equals(TotalOBHelper.LLG))
                            _amt = Double.Parse(ah.GetTransactionFee(session, 100, 3).ToString());

                        int fid = 100;
                        string fitur = TotalOBHelper.Payroll;
                        string debitAcc = "";
                        string debitAccName = "";
                        string creditAcc = o.Account;
                        string creditAccName = o.Name;
                        string benAddress = o.BenAddress;
                        string remark = o.TrxRemark.Trim();
                        string bucketAmt = b.Bucket;
                        string chargeType = "OUR";
                        if (b.FeeBen > 0)
                            chargeType = "BEN";
                        string debitAccAddress = client.Address1;

                        try
                        {
                            IList<ClientAccount> clAccs = session.CreateCriteria(typeof(ClientAccount))
                                .Add(Expression.Eq("Number", p.DebitAccount))
                                .Add(Expression.Eq("Pid", p.ClientID))
                                .Add(Expression.Eq("StatusDel", 0))
                                .SetMaxResults(1)
                                .List<ClientAccount>();

                            clientAcc = clAccs[0];

                            debitAcc = p.DebitAccount;
                            debitAccName = clientAcc.Name;
                        }
                        catch { }
                        #endregion

                        #region PROCESS INSERT
                        /*Modified by Mettaw : 1 Oct 2019 
                         * 
                         * Terjadi perubahan Flow MBASE pada Payroll LLG (Kliring) namun tidak pada Payroll RTGS
                         * Code Sedikit dirombak , bila RTGS pakai flow Lama, yaitu Insert ke tbl MBASEandWS
                         * dan dimakan sama Sch MBASEAndWS
                         * 
                         * Sedangkan LLG di insert ke tbl baru ExpressProcess.
                         * dan dimakan sama ExpressProcessSch
                         * 
                         */

                        if (o.InstructionCode.Equals(TotalOBHelper.RTGS))
                        {
                            #region Insert to MBASEandWS
                            log.Info(SchCode + " === (TrxID " + o.Id + ") === Begin Posting to MBASEANDWS");

                            if (MBASEAndWSHelper.insertMbaseandws(session, int.Parse(o.Id.ToString()), fid, fitur, p.ClientID, o.InstructionCode, debitAcc,
                                debitAccName, creditAcc, creditAccName, benAddress, remark, bucketAmt, o.IdBooking, o.RemittanceNumber,
                                o.BankCode.Trim(), b.JournalSeq.Trim(), chargeType, _amt, debitAccAddress, out outid, out outmsg))
                            {
                                o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG;
                                o.IdMBASEAndWS = outid;
                                o.Description = "proses mbaseandws".ToUpper();
                                log.Info(SchCode + " === (TrxID " + o.Id + ") === Posting to MBASEANDWS SUCCESS");
                            }
                            else
                            {
                                if (outid > 0)
                                {
                                    o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG;
                                    o.IdMBASEAndWS = outid;
                                    o.Description = "proses mbaseandws".ToUpper();

                                    log.Info(SchCode + " === (TrxID " + o.Id + ") === Already Exist, Posting to MBASEANDWS SUCCESS");
                                }
                                else
                                {
                                    //gagal insert Mbaseandws
                                    o.LastUpdate = DateTime.Now.AddMinutes(2);

                                    log.Error(SchCode + " === (TrxID " + o.Id + ") === Posting to MBASEANDWS FAILED: " + outmsg);
                                }
                            }
                            session.Update(o);
                            session.Flush();

                            log.Info(SchCode + " === (TrxID " + o.Id + ") === End Posting to MBASEANDWS");
                            #endregion
                        }
                        else if (o.InstructionCode.Equals(TotalOBHelper.LLG))
                        {
                            #region Insert to ExpressProcess
                            log.Info(SchCode + " === (TrxID " + o.Id + ") === Begin Posting to ExpressProcess");

                            #region PARAMETERS FOR ExpressProcess
                            ExpressProcess exprocess = new ExpressProcess();

                            String _fitur = "PAYROLL LLG";

                            BankKliring bank = session.CreateCriteria(typeof(BankKliring))
                                       .Add(Expression.Eq("BankCode", o.Account))
                                       .UniqueResult<BankKliring>();

                            double _instamount = o.Amount;
                            string _instcur = SchHelper._Currency(o.RemittanceNumber);
                            string _benbankname = bank.Nama;
                            #endregion

                            #region ADD ExpressProcess
                            /*New added by Mettaw :  
                             * tampung data disini... untuk parameter WS Express 
                             * dan insert ke tbl expressprocess 
                             * 9 september 2019*/

                            /*DATA MANDATORY*/
                            exprocess.Tanggal_Transaksi = DateTime.Now;
                            exprocess.Sarana_Transaksi = "2";
                            exprocess.Kode_Transaksi = "50";
                            exprocess.Peserta_Pengirim_Asal = "BRINIDJA";
                            exprocess.Sandi_Kota_Asal = "0391";
                            exprocess.Peserta_Pengirim_Penerus = "BRINIDJA";
                            exprocess.Peserta_Penerima_Akhir = bank.BankCode;
                            exprocess.Sandi_Kota_Tujuan = bank.SandiKota;
                            exprocess.Peserta_Penerima_Penerus = bank.BankCode;
                            exprocess.Jenis_Nasabah = "1";
                            exprocess.No_Rekening_Pengirim = debitAcc;
                            exprocess.Nama_Pengirim = debitAccName;
                            exprocess.No_Identitas_Pengirim = "NIKPENGRMCMS";
                            exprocess.Jenis_Nasabah_Pengirim = "1";
                            exprocess.Status_Kependudukan_Pengirim = "1";
                            exprocess.Alamat_Pengirim = client.Address1;
                            exprocess.No_Rekening_Tujuan = creditAcc;
                            exprocess.Nama_Penerima = creditAccName;
                            exprocess.Jenis_Nasabah_Penerima = "1";
                            exprocess.Status_Kependudukan_Penerima = "1";
                            exprocess.Alamat_Penerima = benAddress;
                            exprocess.Cara_Penyetoran = "DEBET_REK";
                            exprocess.No_Rekening_Asal = debitAcc;
                            exprocess.Nama = debitAccName;
                            exprocess.Jumlah_Dikirim = _instamount.ToString("0.00");
                            exprocess.Currency_Dikirim = _instcur;
                            exprocess.Biaya = _amt.ToString("0.00");
                            exprocess.Total = (double.Parse(_instamount.ToString()) + _amt).ToString("0.00");
                            exprocess.Currency_Total = _instcur;
                            exprocess.Kanca_Asal = "CMS";
                            exprocess.User_Approve = "0374891";
                            exprocess.Cabang = "0374";
                            exprocess.No_Remitance = o.RemittanceNumber;
                            exprocess.Jurnal_Seq = o.IdBooking.ToString();
                            exprocess.Nama_Bank_Penerima = _benbankname;
                            exprocess.Provinsi_Penerima = bank.SandiPropinsi;
                            exprocess.Jenis_Badan_Usaha = bank.JenisUsaha.Trim();
                            exprocess.Kode_Bank_Brinets_Penerima = bank.Code;
                            exprocess.Channel = "CMS";
                            exprocess.Booking_Id = o.IdBooking.ToString();


                            /*DATA TIDAK MANDATORY*/
                            exprocess.Telepon_Pengirim = "TELPPNGRMCMS";
                            exprocess.No_Identitas_Penerima = "NIKPENRMMCMS";
                            exprocess.Telepon_Penerima = "TELPPNRMCMS";
                            exprocess.NoCekBG = "";
                            exprocess.Berita = o.TrxRemark;
                            exprocess.Sumber_Dana = "";
                            exprocess.Keperluan = "";
                            exprocess.Pekerjaan = "";
                            exprocess.Jabatan = "";
                            exprocess.Ttl = "";
                            exprocess.User_Id = "";
                            exprocess.Cif = "";
                            exprocess.Kode_Pos_Pengirim = "";
                            exprocess.Kode_Pos_Penerima = "";
                            exprocess.Remark = remark;
                            #endregion

                            #region PROCESS
                            log.Info(SchCode + " === (TrxID " + o.Id + ") === DO INSERT TO ExpressProcess");

                            if (MBASEHelper.InsertMBASEExpress(session, o.Id, _fitur, exprocess, out outmsg , out outid))
                            {
                                #region COMPLETE

                                /*Express Process*/
                                exprocess.Status = ParameterHelper.SCH_MBASE_WAITING; /*1: Waiting*/
                                exprocess.Description = ParameterHelper.MBASEDESC_WAITINGMBASE;
                                exprocess.LastUpdate = DateTime.Now;
                                session.Update(exprocess);

                                /*Trx Payroll*/
                                o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG;
                                o.Description = "proses express".ToUpper();

                                log.Info(SchCode + " === (TrxID " + o.Id + ") === Posting to ExpressProcess SUCCESS");
                                #endregion
                            }
                            else
                            {
                                if (outid > 0)
                                {
                                    #region COMPLETE
                                    /*bila gagal validasi insert, tapi id sudah terbuat, update aja coy */

                                    /*Express Process*/
                                    ExpressProcess ex = session.Load<ExpressProcess>(outid);
                                    ex.Status = ParameterHelper.SCH_MBASE_WAITING; /*1: Waiting MBASE*/
                                    ex.Description = ParameterHelper.MBASEDESC_WAITINGMBASE;
                                    ex.LastUpdate = DateTime.Now;
                                    session.Update(ex);

                                    /*Trx Payroll*/
                                    o.Status = ParameterHelper.TRXSTATUS_PAYROLLNEW_CHECK_MBASEANDWS_RTGSANDLLG;
                                    o.Description = "proses express".ToUpper();

                                    log.Info(SchCode + " === (TrxID " + o.Id + ") === Already Exist, Posting to ExpressProcess SUCCESS");
                                    #endregion
                                }
                                else
                                {
                                    #region REVERSAL PROCESS
                                    /*Gagal Insert to ExpressProcess, do Reversal.*/
                                    
                                    /*Booking*/
                                    Booking book = session.Load<Booking>(o.IdBooking);
                                    book.StatusEC = ParameterHelper.SCH_ECWAITING;

                                    /*Trx Payroll*/
                                    o.Status = ParameterHelper.TRXSTATUS_REJECT;
                                    o.Description = "REJECTED";
                                    o.LastUpdate = DateTime.Now;

                                    session.Update(book);
                                    session.Flush();

                                    log.Info(SchCode + " ===  (TrxID " + o.Id + " ) Insert to ExpressProcess FAILED: " + outmsg + ". DO Reject & REVERSAL");

                                    #endregion
                                }
                            }

                            session.Update(o);
                            session.Flush();

                            log.Info(SchCode + " === (TrxID " + o.Id + ") === End Posting to ExpressProcess");
                            #endregion

                            #endregion
                        }
                        else
                        {
                            log.Error(SchCode + " === " + SchName + " IntructionCode tidak terdefenisi ");
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        log.Error(SchCode + " === (TrxID " + o.Id + ") === Exception: " + ex.Message + " >>> " + ex.InnerException + " >>> " + ex.StackTrace);
                    }
                }
            }
            else
                Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + SchCode + " === Tidak ada Trx yang diproses");
        }

    }
}
