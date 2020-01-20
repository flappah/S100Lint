﻿using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface IS100LintBase
    {
        XmlAttribute FindAttributeByName(XmlAttributeCollection collection, string attributeName);
        XmlAttribute FindAttributeByValue(XmlAttributeCollection collection, string attributeValue);
        XmlNodeList SelectNodes(XmlDocument[] documents, string expression, XmlNamespaceManager nsm);
        XmlNode SelectSingleNode(XmlDocument[] documents, string expression, XmlNamespaceManager nsm);
    }
}