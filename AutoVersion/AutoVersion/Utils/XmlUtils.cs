using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AutoVersion.Utils
{
    static class XmlUtils
    {

        public static string GetAttributeValue(XmlTextReader source, string attributeName, string defaultValue)
        {
            string retVal = source.GetAttribute(attributeName);

            if (retVal == null) return defaultValue;

            return retVal;
        }

    }
}
