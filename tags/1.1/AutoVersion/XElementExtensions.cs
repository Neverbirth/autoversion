using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AutoVersion
{
    static class XElementExtensions
    {

        public static string GetAttributeValue(this XElement baseElement, XName attributeName, string defaultValue)
        {
            XAttribute attribute = baseElement.Attribute(attributeName);
            return attribute != null ? attribute.Value : defaultValue;
        }

    }
}
