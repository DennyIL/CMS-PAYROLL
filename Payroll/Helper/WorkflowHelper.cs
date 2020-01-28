using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using BRIChannelSchedulerNew.Payroll.Pocos;
using BRIChannelSchedulerNew.Payroll.Helper;

namespace BRIChannelSchedulerNew.Payroll.Helper
{
    [Serializable]
    public class WorkflowHelper
    {
        IDictionary<int, Workflow> flowDictionary = new Dictionary<int, Workflow>();

        public WorkflowHelper(String serializedXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(serializedXml);
            XmlNodeReader reader = new XmlNodeReader(doc.DocumentElement);
            XmlSerializer ser = new XmlSerializer(typeof(Workflow[]));
            Workflow[] workflows = ser.Deserialize(reader) as Workflow[];
            if (null == workflows) return;

            foreach (Workflow flow in workflows)
            {
                flowDictionary.Add(flow.Product, flow);
            }
        }

        public WorkflowHelper(Workflow[] workflows)
        {
            if (null == workflows) return;
            flowDictionary = workflows.ToDictionary(a => a.Product, a => a);
        }

        public Workflow GetWorkflow(int product)
        {
            Workflow result = null;
            if (flowDictionary.ContainsKey(product))
            {
                result = flowDictionary[product];
            }
            return result;
        }
    }
}
