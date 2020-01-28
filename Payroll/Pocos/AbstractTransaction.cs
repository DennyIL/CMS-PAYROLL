using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace BRIChannelSchedulerNew.Payroll.Pocos
{
    [Serializable]
    public abstract class AbstractTransaction
    {
        private String remark;
        private String txid;

        [XmlAttribute("Remark")]
        public virtual String Remark
        {
            get { return remark; }
            set { remark = value; }
        }

        [XmlAttribute("Txid")]
        public virtual String Txid
        {
            get { return txid; }
            set { txid = value; }
        }

        public override String ToString()
        {
            String result = "";
            XmlSerializer ser = new XmlSerializer(this.GetType());
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            ser.Serialize(writer, this);
            result = sb.ToString();
            return result;
        }

        public virtual Object Load(String xml)
        {
            Object result = null;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            XmlSerializer ser = new XmlSerializer(this.GetType());
            result = ser.Deserialize(reader);
            return result;
        }
    }
}
