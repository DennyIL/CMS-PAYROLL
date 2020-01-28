using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class ExpressProcess : AbstractTransaction
    {
        private Int32 _id;
        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private String _trxtype;
        [XmlAttribute("TrxType")]
        public virtual String TrxType
        {
            get { return _trxtype; }
            set { _trxtype = value; }
        }

        private String _trxid;
        [XmlAttribute("TrxID")]
        public virtual String TrxID
        {
            get { return _trxid; }
            set { _trxid = value; }
        }

        private DateTime _createdtime;
        [XmlAttribute("CreatedTime")]
        public virtual DateTime CreatedTime
        {
            get { return _createdtime; }
            set { _createdtime = value; }
        }

        private DateTime _lastupdate;
        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return _lastupdate; }
            set { _lastupdate = value; }
        }

        private int _status;
        [XmlAttribute("Status")]
        public virtual int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private String _description;
        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private DateTime _tanggal_transaksi;
        [XmlAttribute("Tanggal_Transaksi")]
        public virtual DateTime Tanggal_Transaksi
        {
            get { return _tanggal_transaksi; }
            set { _tanggal_transaksi = value; }
        }

        private String _sarana_transaksi;
        [XmlAttribute("Sarana_Transaksi")]
        public virtual String Sarana_Transaksi
        {
            get { return _sarana_transaksi; }
            set { _sarana_transaksi = value; }
        }

        private String _kode_transaksi;
        [XmlAttribute("Kode_Transaksi")]
        public virtual String Kode_Transaksi
        {
            get { return _kode_transaksi; }
            set { _kode_transaksi = value; }
        }

        private String _peserta_pengirim_asal;
        [XmlAttribute("Peserta_Pengirim_Asal")]
        public virtual String Peserta_Pengirim_Asal
        {
            get { return _peserta_pengirim_asal; }
            set { _peserta_pengirim_asal = value; }
        }

        private String _sandi_kota_asal;
        [XmlAttribute("Sandi_Kota_Asal")]
        public virtual String Sandi_Kota_Asal
        {
            get { return _sandi_kota_asal; }
            set { _sandi_kota_asal = value; }
        }

        private String _peserta_pengirim_penerus;
        [XmlAttribute("Peserta_Pengirim_Penerus")]
        public virtual String Peserta_Pengirim_Penerus
        {
            get { return _peserta_pengirim_penerus; }
            set { _peserta_pengirim_penerus = value; }
        }

        private String _peserta_penerima_akhir;
        [XmlAttribute("Peserta_Penerima_Akhir")]
        public virtual String Peserta_Penerima_Akhir
        {
            get { return _peserta_penerima_akhir; }
            set { _peserta_penerima_akhir = value; }
        }

        private String _sandi_kota_tujuan;
        [XmlAttribute("Sandi_Kota_Tujuan")]
        public virtual String Sandi_Kota_Tujuan
        {
            get { return _sandi_kota_tujuan; }
            set { _sandi_kota_tujuan = value; }
        }

        private String _peserta_penerima_penerus;
        [XmlAttribute("Peserta_Penerima_Penerus")]
        public virtual String Peserta_Penerima_Penerus
        {
            get { return _peserta_penerima_penerus; }
            set { _peserta_penerima_penerus = value; }
        }

        private String _jenis_nasabah;
        [XmlAttribute("Jenis_Nasabah")]
        public virtual String Jenis_Nasabah
        {
            get { return _jenis_nasabah; }
            set { _jenis_nasabah = value; }
        }

        private String _no_rekening_pengirim;
        [XmlAttribute("No_Rekening_Pengirim")]
        public virtual String No_Rekening_Pengirim
        {
            get { return _no_rekening_pengirim; }
            set { _no_rekening_pengirim = value; }
        }

        private String _nama_pengirim;
        [XmlAttribute("Nama_Pengirim")]
        public virtual String Nama_Pengirim
        {
            get { return _nama_pengirim; }
            set { _nama_pengirim = value; }
        }

        private String _no_identitas_pengirim;
        [XmlAttribute("No_Identitas_Pengirim")]
        public virtual String No_Identitas_Pengirim
        {
            get { return _no_identitas_pengirim; }
            set { _no_identitas_pengirim = value; }
        }

        private String _jenis_nasabah_pengirim;
        [XmlAttribute("Jenis_Nasabah_Pengirim")]
        public virtual String Jenis_Nasabah_Pengirim
        {
            get { return _jenis_nasabah_pengirim; }
            set { _jenis_nasabah_pengirim = value; }
        }

        private String _status_kependudukan_pengirim;
        [XmlAttribute("Status_Kependudukan_Pengirim")]
        public virtual String Status_Kependudukan_Pengirim
        {
            get { return _status_kependudukan_pengirim; }
            set { _status_kependudukan_pengirim = value; }
        }

        private String _alamat_pengirim;
        [XmlAttribute("Alamat_Pengirim")]
        public virtual String Alamat_Pengirim
        {
            get { return _alamat_pengirim; }
            set { _alamat_pengirim = value; }
        }

        private String _telepon_pengirim;
        [XmlAttribute("Telepon_Pengirim")]
        public virtual String Telepon_Pengirim
        {
            get { return _telepon_pengirim; }
            set { _telepon_pengirim = value; }
        }

        private String _no_rekening_tujuan;
        [XmlAttribute("No_Rekening_Tujuan")]
        public virtual String No_Rekening_Tujuan
        {
            get { return _no_rekening_tujuan; }
            set { _no_rekening_tujuan = value; }
        }

        private String _nama_penerima;
        [XmlAttribute("Nama_Penerima")]
        public virtual String Nama_Penerima
        {
            get { return _nama_penerima; }
            set { _nama_penerima = value; }
        }

        private String _no_identitas_penerima;
        [XmlAttribute("No_Identitas_Penerima")]
        public virtual String No_Identitas_Penerima
        {
            get { return _no_identitas_penerima; }
            set { _no_identitas_penerima = value; }
        }

        private String _jenis_nasabah_penerima;
        [XmlAttribute("Jenis_Nasabah_Penerima")]
        public virtual String Jenis_Nasabah_Penerima
        {
            get { return _jenis_nasabah_penerima; }
            set { _jenis_nasabah_penerima = value; }
        }

        private String _status_kependudukan_penerima;
        [XmlAttribute("Status_Kependudukan_Penerima")]
        public virtual String Status_Kependudukan_Penerima
        {
            get { return _status_kependudukan_penerima; }
            set { _status_kependudukan_penerima = value; }
        }

        private String _alamat_penerima;
        [XmlAttribute("Alamat_Penerima")]
        public virtual String Alamat_Penerima
        {
            get { return _alamat_penerima; }
            set { _alamat_penerima = value; }
        }

        private String _telepon_penerima;
        [XmlAttribute("Telepon_Penerima")]
        public virtual String Telepon_Penerima
        {
            get { return _telepon_penerima; }
            set { _telepon_penerima = value; }
        }

        private String _cara_penyetoran;
        [XmlAttribute("Cara_Penyetoran")]
        public virtual String Cara_Penyetoran
        {
            get { return _cara_penyetoran; }
            set { _cara_penyetoran = value; }
        }

        private String _no_rekening_asal;
        [XmlAttribute("No_Rekening_Asal")]
        public virtual String No_Rekening_Asal
        {
            get { return _no_rekening_asal; }
            set { _no_rekening_asal = value; }
        }

        private String _nocekbg;
        [XmlAttribute("NoCekBG")]
        public virtual String NoCekBG
        {
            get { return _nocekbg; }
            set { _nocekbg = value; }
        }

        private String _nama;
        [XmlAttribute("Nama")]
        public virtual String Nama
        {
            get { return _nama; }
            set { _nama = value; }
        }

        private String _jumlah_dikirim;
        [XmlAttribute("Jumlah_Dikirim")]
        public virtual String Jumlah_Dikirim
        {
            get { return _jumlah_dikirim; }
            set { _jumlah_dikirim = value; }
        }

        private String _currency_dikirim;
        [XmlAttribute("Currency_Dikirim")]
        public virtual String Currency_Dikirim
        {
            get { return _currency_dikirim; }
            set { _currency_dikirim = value; }
        }

        private String _biaya;
        [XmlAttribute("Biaya")]
        public virtual String Biaya
        {
            get { return _biaya; }
            set { _biaya = value; }
        }

        private String _total;
        [XmlAttribute("Total")]
        public virtual String Total
        {
            get { return _total; }
            set { _total = value; }
        }

        private String _currency_total;
        [XmlAttribute("Currency_Total")]
        public virtual String Currency_Total
        {
            get { return _currency_total; }
            set { _currency_total = value; }
        }

        private String _berita;
        [XmlAttribute("Berita")]
        public virtual String Berita
        {
            get { return _berita; }
            set { _berita = value; }
        }

        private String _kanca_asal;
        [XmlAttribute("Kanca_Asal")]
        public virtual String Kanca_Asal
        {
            get { return _kanca_asal; }
            set { _kanca_asal = value; }
        }

        private String _sumber_dana;
        [XmlAttribute("Sumber_Dana")]
        public virtual String Sumber_Dana
        {
            get { return _sumber_dana; }
            set { _sumber_dana = value; }
        }

        private String _keperluan;
        [XmlAttribute("Keperluan")]
        public virtual String Keperluan
        {
            get { return _keperluan; }
            set { _keperluan = value; }
        }

        private String _pekerjaan;
        [XmlAttribute("Pekerjaan")]
        public virtual String Pekerjaan
        {
            get { return _pekerjaan; }
            set { _pekerjaan = value; }
        }

        private String _jabatan;
        [XmlAttribute("Jabatan")]
        public virtual String Jabatan
        {
            get { return _jabatan; }
            set { _jabatan = value; }
        }

        private String _ttl;
        [XmlAttribute("Ttl")]
        public virtual String Ttl
        {
            get { return _ttl; }
            set { _ttl = value; }
        }

        private String _user_id;
        [XmlAttribute("User_Id")]
        public virtual String User_Id
        {
            get { return _user_id; }
            set { _user_id = value; }
        }

        private String _user_approve;
        [XmlAttribute("User_Approve")]
        public virtual String User_Approve
        {
            get { return _user_approve; }
            set { _user_approve = value; }
        }

        private String _cabang;
        [XmlAttribute("Cabang")]
        public virtual String Cabang
        {
            get { return _cabang; }
            set { _cabang = value; }
        }

        private String _no_remitance;
        [XmlAttribute("No_Remitance")]
        public virtual String No_Remitance
        {
            get { return _no_remitance; }
            set { _no_remitance = value; }
        }

        private String _jurnal_seq;
        [XmlAttribute("Jurnal_Seq")]
        public virtual String Jurnal_Seq
        {
            get { return _jurnal_seq; }
            set { _jurnal_seq = value; }
        }

        private String _cif;
        [XmlAttribute("Cif")]
        public virtual String Cif
        {
            get { return _cif; }
            set { _cif = value; }
        }

        private String _nama_bank_penerima;
        [XmlAttribute("Nama_Bank_Penerima")]
        public virtual String Nama_Bank_Penerima
        {
            get { return _nama_bank_penerima; }
            set { _nama_bank_penerima = value; }
        }

        private String _kode_pos_pengirim;
        [XmlAttribute("Kode_Pos_Pengirim")]
        public virtual String Kode_Pos_Pengirim
        {
            get { return _kode_pos_pengirim; }
            set { _kode_pos_pengirim = value; }
        }

        private String _kode_pos_penerima;
        [XmlAttribute("Kode_Pos_Penerima")]
        public virtual String Kode_Pos_Penerima
        {
            get { return _kode_pos_penerima; }
            set { _kode_pos_penerima = value; }
        }

        private String _provinsi_penerima;
        [XmlAttribute("Provinsi_Penerima")]
        public virtual String Provinsi_Penerima
        {
            get { return _provinsi_penerima; }
            set { _provinsi_penerima = value; }
        }

        private String _jenis_badan_usaha;
        [XmlAttribute("Jenis_Badan_Usaha")]
        public virtual String Jenis_Badan_Usaha
        {
            get { return _jenis_badan_usaha; }
            set { _jenis_badan_usaha = value; }
        }

        private String _kode_bank_brinets_penerima;
        [XmlAttribute("Kode_Bank_Brinets_Penerima")]
        public virtual String Kode_Bank_Brinets_Penerima
        {
            get { return _kode_bank_brinets_penerima; }
            set { _kode_bank_brinets_penerima = value; }
        }

        private String _channel;
        [XmlAttribute("Channel")]
        public virtual String Channel
        {
            get { return _channel; }
            set { _channel = value; }
        }

        private String _request;
        [XmlAttribute("Request")]
        public virtual String Request
        {
            get { return _request; }
            set { _request = value; }
        }

        private String _response;
        [XmlAttribute("Response")]
        public virtual String Response
        {
            get { return _response; }
            set { _response = value; }
        }

        private String _booking_id;
        [XmlAttribute("Booking_Id")]
        public virtual String Booking_Id
        {
            get { return _booking_id; }
            set { _booking_id = value; }
        }

        private int _counter;
        [XmlAttribute("Counter")]
        public virtual int Counter
        {
            get { return _counter; }
            set { _counter = value; }
        }

        

    }
}
