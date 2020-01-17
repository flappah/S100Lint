using S100Lint.Base.Interfaces;
using System;
using System.Xml;

namespace S100Lint.Base
{
    public class XmlTools : IXmlTools
    {
        /// <summary>
        /// Retrieves the top element based on the defined expression in which the reference to the parent element is defined
        /// </summary>
        /// <param name="fromNode">node to use</param>
        /// <param name="xmlNsManager">XmlNameSpaceManager for the schema</param>
        /// <param name="expression">expression of the name of the parent element</param>
        /// <returns></returns>
        public XmlNode GetToplevelElement(XmlNode fromNode, XmlNamespaceManager xmlNsManager, string expression)
        {
            if (fromNode is null)
            {
                throw new ArgumentNullException(nameof(fromNode));
            }

            if (xmlNsManager is null)
            {
                throw new ArgumentNullException(nameof(xmlNsManager));
            }

            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("message", nameof(expression));
            }

            if (fromNode != null && fromNode.HasChildNodes)
            {
                var expressionNode = fromNode.SelectSingleNode(expression, xmlNsManager);
                if (expressionNode != null && expressionNode.Attributes.Count > 0)
                {
                    var extensionType = expressionNode.Attributes[0].Value;

                    var parentNodeList = fromNode.OwnerDocument.LastChild.SelectNodes($@"//xs:complexType[@name='{extensionType}']", xmlNsManager);
                    if (parentNodeList != null && parentNodeList.Count > 0)
                    {
                        var expressionParentNode = parentNodeList[0].SelectSingleNode(expression, xmlNsManager);
                        if (expressionParentNode != null && expressionParentNode.Attributes != null && expressionParentNode.Attributes.Count > 0)
                        {
                            foreach (XmlAttribute attribute in expressionParentNode.Attributes)
                            {
                                if (attribute.Name == "base")
                                {
                                    if (attribute.Value.Contains("Type") &&
                                        !attribute.Value.Contains("S100:Abstract"))
                                    {
                                        return GetToplevelElement(parentNodeList[0], xmlNsManager, expression);
                                    }
                                }
                            }

                            return parentNodeList[0];
                        }
                    }

                    return fromNode;
                }
            }

            return null;
        }
    }
}
