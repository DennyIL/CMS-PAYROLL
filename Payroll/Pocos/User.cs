using System;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class User : AbstractTransaction
    {
        private Int32 id;
        private DateTime lastUpdate;
        private DateTime creationDate;
        private DateTime lastLogon;
        private DateTime lastLogout;
        private DateTime loginExpired;
        private String firstName;
        private String lastName;
        private String initial;
        private String email;
        private String phone;
        private String mphone;
        private String identity;
        private String identityno;
        private DateTime identityexpired;
        private String birthplace;
        private DateTime birthdate;
        private String mothername;
        private String data;
        private String handle;
        private String password;
        private String token;
        private String lastSession;
        private String lastRemoteAddress;
        private String approver;
        private Int32 loginRetry;
        private Boolean logged;
        private int recStatus;
        private String previousPassword;
        private UserMatrix userMatrix;
        private String kotaterbit;
        private String alamatlengkap;
        private int actStat;
        private DateTime passwordexpired;
        private String salt;
        private int loggeduseridcorp;
        private int ipstat;
        private String ippublic;

        public User() { }

        [XmlAttribute("Id")]
        public virtual Int32 Id
        {
            get { return id; }
            set { id = value; }
        }

        public virtual DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        [XmlAttribute("Cdate")]
        public virtual DateTime CreationDate
        {
            get { return creationDate; }
            set { creationDate = value; }
        }

        public virtual DateTime LastLogon
        {
            get { return lastLogon; }
            set { lastLogon = value; }
        }

        public virtual DateTime LastLogout
        {
            get { return lastLogout; }
            set { lastLogout = value; }
        }

        [XmlAttribute("Expired")]
        public virtual DateTime LoginExpired
        {
            get { return loginExpired; }
            set { loginExpired = value; }
        }

        [XmlAttribute("Fname")]
        public virtual String FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        [XmlAttribute("Lname")]
        public virtual String LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public virtual String Initial
        {
            get { return initial; }
            set { initial = value; }
        }

        [XmlAttribute("Email")]
        public virtual String Email
        {
            get { return email; }
            set { email = value; }
        }

        [XmlAttribute("Phone")]
        public virtual String Phone
        {
            get { return phone; }
            set { phone = value; }
        }

        [XmlAttribute("MPhone")]
        public virtual String MPhone
        {
            get { return mphone; }
            set { mphone = value; }
        }

        [XmlAttribute("Identity")]
        public virtual String Identity
        {
            get { return identity; }
            set { identity = value; }
        }

        [XmlAttribute("IdentityNo")]
        public virtual String IdentityNo
        {
            get { return identityno; }
            set { identityno = value; }
        }

        [XmlAttribute("IdentityExpired")]
        public virtual DateTime IdentityExpired
        {
            get { return identityexpired; }
            set { identityexpired = value; }
        }

        [XmlAttribute("BirthPlace")]
        public virtual String BirthPlace
        {
            get { return birthplace; }
            set { birthplace = value; }
        }

        [XmlAttribute("BirthDate")]
        public virtual DateTime BirthDate
        {
            get { return birthdate; }
            set { birthdate = value; }
        }

        [XmlAttribute("MotherName")]
        public virtual String MotherName
        {
            get { return mothername; }
            set { mothername = value; }
        }

        [XmlAttribute("Data")]
        public virtual String Data
        {
            get { return data; }
            set { data = value; }
        }

        [XmlAttribute("Code")]
        public virtual String Handle
        {
            get { return handle; }
            set { handle = value; }
        }

        public virtual String Password
        {
            get { return password; }
            set { password = value; }
        }

        [XmlAttribute("Token")]
        public virtual String Token
        {
            get { return token; }
            set { token = value; }
        }

        public virtual String LastSession
        {
            get { return lastSession; }
            set { lastSession = value; }
        }

        public virtual String LastRemoteAddress
        {
            get { return lastRemoteAddress; }
            set { lastRemoteAddress = value; }
        }

        public virtual String Approver
        {
            get { return approver; }
            set { approver = value; }
        }

        [XmlAttribute("Retry")]
        public virtual Int32 LoginRetry
        {
            get { return loginRetry; }
            set { loginRetry = value; }
        }

        [XmlAttribute("Logged")]
        public virtual Boolean Logged
        {
            get { return logged; }
            set { logged = value; }
        }

        public virtual String PreviousPassword
        {
            get { return previousPassword; }
            set { previousPassword = value; }
        }

        [XmlAttribute("Status")]
        public virtual int RecStatus
        {
            get { return recStatus; }
            set { recStatus = value; }
        }

        [XmlIgnore]
        public virtual UserMatrix UserMatrix
        {
            get { return userMatrix; }
            set { userMatrix = value; }
        }

        [XmlAttribute("KotaTerbit")]
        public virtual String KotaTerbit
        {
            get { return kotaterbit; }
            set { kotaterbit = value; }
        }

        [XmlAttribute("AlamatLengkap")]
        public virtual String AlamatLengkap
        {
            get { return alamatlengkap; }
            set { alamatlengkap = value; }
        }

        [XmlAttribute("ActStat")]
        public virtual int ActStat
        {
            get { return actStat; }
            set { actStat = value; }
        }

        [XmlAttribute("PasswordExpired")]
        public virtual DateTime PasswordExpired
        {
            get { return passwordexpired; }
            set { passwordexpired = value; }
        }

        [XmlAttribute("Salt")]
        public virtual String Salt
        {
            get { return salt; }
            set { salt = value; }
        }

        [XmlAttribute("LoggedUserIdCorp")]
        public virtual int LoggedUserIdCorp
        {
            get { return loggeduseridcorp; }
            set { loggeduseridcorp = value; }
        }

        [XmlAttribute("IPStat")]
        public virtual int IPStat
        {
            get { return ipstat; }
            set { ipstat = value; }
        }

        [XmlAttribute("IPPublic")]
        public virtual string IPPublic
        {
            get { return ippublic; }
            set { ippublic = value; }
        }
    }
}