using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace BRIChannelSchedulerNew.Payroll.Helper
{
    public abstract class QueryHelper
    {
        public static String ArrayToXMLDataSource(Type type, Object obj)
        {
            String result = "";
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlSerializer ser = new XmlSerializer(type);
                StringBuilder sb = new StringBuilder();
                StringWriter writer = new StringWriter(sb);
                ser.Serialize(writer, obj);
                result = sb.ToString();
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public static Byte[] SerializeToByte(Object obj)
        {
            Byte[] result = null;
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(); // Stream
                bf.Serialize(ms, obj); // "Save" object state
                result = ms.GetBuffer();
            }
            catch (Exception e)
            {
            }
            return result;
        }

        public static Object DeserializeXMLToObject(String xml, Type type)
        {
            Object result = null;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            XmlSerializer ser = new XmlSerializer(type);
            result = ser.Deserialize(reader);
            return result;
        }
    }
}
