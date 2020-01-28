using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class UserMap
    {
        private int id;
        private String clientHandle;
        private String userHandle;
        private int clientEntity;
        private int userEntity;
        private int authorityEntity;
        private User user;
        private UserMatrix userMatrix;
        private Client client;
        private ClientWorkflow clientWorkflow;
        private ClientMatrix clientMatrix;

        public UserMap()
        {
        }

        [XmlAttribute("Id")]
        public virtual int Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlAttribute("ClientHandle")]
        public virtual String ClientHandle
        {
            get { return clientHandle; }
            set { clientHandle = value; }
        }

        [XmlAttribute("UserHandle")]
        public virtual String UserHandle
        {
            get { return userHandle; }
            set { userHandle = value; }
        }

        public virtual int ClientEntity
        {
            get { return clientEntity; }
            set { clientEntity = value; }
        }

        [XmlAttribute("UserEntity")]
        public virtual int UserEntity
        {
            get { return userEntity; }
            set { userEntity = value; }
        }

        public virtual int AuthorityEntity
        {
            get { return authorityEntity; }
            set { authorityEntity = value; }
        }

        [XmlIgnoreAttribute]
        public virtual UserMatrix UserMatrix
        {
            get { return userMatrix; }
            set { userMatrix = value; }
        }

        [XmlIgnoreAttribute]
        public virtual User User
        {
            get { return user; }
            set { user = value; }
        }

        [XmlIgnoreAttribute]
        public virtual Client Client
        {
            get { return client; }
            set { client = value; }
        }

        [XmlIgnoreAttribute]
        public virtual ClientWorkflow ClientWorkflow
        {
            get { return clientWorkflow; }
            set { clientWorkflow = value; }
        }

        [XmlIgnoreAttribute]
        public virtual ClientMatrix ClientMatrix
        {
            get { return clientMatrix; }
            set { clientMatrix = value; }
        }

    }
}