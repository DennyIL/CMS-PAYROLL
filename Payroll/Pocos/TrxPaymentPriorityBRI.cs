using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class TrxPaymentPriorityBRI : AbstractTransaction
    {
        private int id;
        private int fid;
        private int pid;
        private DateTime createdTime;
        private DateTime lastUpdate;
        private String filePath;
        private String filepathoriginalfile;
        private String originalfilename;
        private String fileDescription;
        private String description;
        private int status;
        private int clientId;
        private String serverid;
        private int sharedfolder;
        private String sharedfolderstatus;
        private String errordescription;

        [XmlAttribute("Id")]
        public virtual int Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("ClientId")]
        public virtual int ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }
        [XmlAttribute("Fid")]
        public virtual int Fid
        {
            get { return fid; }
            set { fid = value; }
        }

        [XmlAttribute("Pid")]
        public virtual int Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        [XmlAttribute("CreatedTime")]
        public virtual DateTime CreatedTime
        {
            get { return createdTime; }
            set { createdTime = value; }
        }

        [XmlAttribute("LastUpdate")]
        public virtual DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        [XmlAttribute("FilePath")]
        public virtual String FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        [XmlAttribute("FilePathOriginalFile")]
        public virtual String FilePathOriginalFile
        {
            get { return filepathoriginalfile; }
            set { filepathoriginalfile = value; }
        }

        [XmlAttribute("OriginalFileName")]
        public virtual String OriginalFileName
        {
            get { return originalfilename; }
            set { originalfilename = value; }
        }

        [XmlAttribute("FileDescription")]
        public virtual String FileDescription
        {
            get { return fileDescription; }
            set { fileDescription = value; }
        }

        [XmlAttribute("Description")]
        public virtual String Description
        {
            get { return description; }
            set { description = value; }
        }

        [XmlAttribute("Status")]
        public virtual int Status
        {
            get { return status; }
            set { status = value; }
        }

        [XmlAttribute("ServerID")]
        public virtual String ServerID
        {
            get { return serverid; }
            set { serverid = value; }
        }

        [XmlAttribute("SharedFolder")]
        public virtual int SharedFolder
        {
            get { return sharedfolder; }
            set { sharedfolder = value; }
        }

        [XmlAttribute("SharedFolderStatus")]
        public virtual String SharedFolderStatus
        {
            get { return sharedfolderstatus; }
            set { sharedfolderstatus = value; }
        }

        [XmlAttribute("ErrorDescription")]
        public virtual String ErrorDescription
        {
            get { return errordescription; }
            set { errordescription = value; }
        }

        private String fileNameInq;
        [XmlAttribute("FileNameInq")]
        public virtual String FileNameInq
        {
            get { return fileNameInq; }
            set { fileNameInq = value; }
        }
    }
}