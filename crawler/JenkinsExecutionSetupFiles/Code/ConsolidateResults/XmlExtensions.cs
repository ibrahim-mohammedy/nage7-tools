using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConsolidateResults
{
    public static class XmlExtensions
    {
        public static string GetAttributeValue(this XmlNode nd, string attributeName)
        {
            if (nd.Attributes[attributeName] == null) return "";

            return nd.Attributes[attributeName].Value;
        }

        public static void SetAttributeValue(this XmlNode nd, string attributeName, string value)
        {
            XmlAttribute attribute = nd.OwnerDocument.CreateAttribute(attributeName);
            attribute.Value = value;

            nd.Attributes.SetNamedItem(attribute);
        }
    }
}
