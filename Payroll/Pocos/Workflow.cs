using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public class Workflow : AbstractTransaction
    {
        private int product;
        private Boolean needApprover;
        private int totalApprover;
        private Boolean isappbyuser;
        private Boolean isprivateapp;
        private Boolean isserialapp;
        private Boolean needVerifier;
        private int totalVerifier;
        private Boolean ischebyuser;
        private Boolean isserialche;
        private Boolean isprivateche;
        private String[] approvers;
        private String[] verifiers;
        private List<UserGroupWorkflow> ugw;
        private String viewgroupuserver;
        private String viewgroupuserapp;

        [XmlAttribute("isSerialApp")]
        public virtual Boolean isSerialApp
        {
            get { return isserialapp; }
            set { isserialapp = value; }
        }

        [XmlAttribute("isSerialChe")]
        public virtual Boolean isSerialChe
        {
            get { return isserialche; }
            set { isserialche = value; }
        }

        [XmlAttribute("isAppByGroup")]
        public virtual Boolean isAppByGroup
        {
            get { return isappbyuser; }
            set { isappbyuser = value; }
        }

        [XmlAttribute("isCheByGroup")]
        public virtual Boolean isCheByGroup
        {
            get { return ischebyuser; }
            set { ischebyuser = value; }
        }

        [XmlAttribute("isPrivateApp")]
        public virtual Boolean isPrivateApp
        {
            get { return isprivateapp; }
            set { isprivateapp = value; }
        }

        [XmlAttribute("isPrivateChe")]
        public virtual Boolean isPrivateChe
        {
            get { return isprivateche; }
            set { isprivateche = value; }
        }

        [XmlAttribute("Product")]
        public virtual int Product
        {
            get { return product; }
            set { product = value; }
        }

        [XmlAttribute("Na")]
        public virtual Boolean NeedApprover
        {
            get { return needApprover; }
            set { needApprover = value; }
        }

        [XmlAttribute("Ta")]
        public virtual int TotalApprover
        {
            get { return totalApprover; }
            set { totalApprover = value; }
        }

        [XmlAttribute("Nv")]
        public virtual Boolean NeedVerifier
        {
            get { return needVerifier; }
            set { needVerifier = value; }
        }

        [XmlAttribute("Tv")]
        public virtual int TotalVerifier
        {
            get { return totalVerifier; }
            set { totalVerifier = value; }
        }

        public virtual String[] Approvers
        {
            get { return approvers; }
            set { approvers = value; }
        }

        public virtual String[] Verifiers
        {
            get { return verifiers; }
            set { verifiers = value; }
        }

        public virtual List<UserGroupWorkflow> UserGroupWorkflow
        {
            get { return ugw; }
            set { ugw = value; }
        }

        [XmlAttribute("ViewGroupUserVer")]
        public virtual String ViewGroupUserVer
        {
            get { return viewgroupuserver; }
            set { viewgroupuserver = value; }
        }

        [XmlAttribute("ViewGroupUserApp")]
        public virtual String ViewGroupUserApp
        {
            get { return viewgroupuserapp; }
            set { viewgroupuserapp = value; }
        }
    }
}
