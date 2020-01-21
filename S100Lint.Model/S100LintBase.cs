using S100Lint.Model.Interfaces;
using System;
using System.Xml;

namespace S100Lint.Model
{
    public abstract class S100LintBase : IS100LintBase
    {
        /// <summary>
        /// Looks for an attribute specified by name in the specified attributecollection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public virtual XmlAttribute FindAttributeByName(XmlAttributeCollection collection, string attributeName)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (XmlAttribute attribute in collection)
            {
                if (attribute.Name == attributeName)
                {
                    return attribute;
                }
            }

            return null;
        }

        /// <summary>
        /// Looks for an attribute specified by value in the specified attributecollection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public virtual XmlAttribute FindAttributeByValue(XmlAttributeCollection collection, string attributeValue)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (XmlAttribute attribute in collection)
            {
                if (attribute.Value.Contains(attributeValue, StringComparison.InvariantCulture))
                {
                    return attribute;
                }
            }

            return null;
        }

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
                                    if (attribute.Value.Contains("Type", StringComparison.InvariantCulture) &&
                                        !attribute.Value.Contains("S100:Abstract", StringComparison.InvariantCulture))
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


        /// <summary>
        /// Executes the SelectNodes method on multiple XmlDocuments
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="expression"></param>
        /// <param name="nsm"></param>
        /// <returns></returns>
        public virtual XmlNodeList SelectNodes(XmlDocument[] documents, string expression, XmlNamespaceManager nsm)
        {
            if (documents is null)
            {
                throw new ArgumentNullException(nameof(documents));
            }

            foreach (XmlDocument document in documents)
            {
                var nodeList = document.LastChild.SelectNodes(expression, nsm);
                if (nodeList != null && nodeList.Count > 0)
                {
                    return nodeList;
                }
            }

            return null;
        }

        /// <summary>
        /// Executes the SelectSingleNode method on multiple XmlDocuments
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="expression"></param>
        /// <param name="nsm"></param>
        /// <returns></returns>
        public virtual XmlNode SelectSingleNode(XmlDocument[] documents, string expression, XmlNamespaceManager nsm)
        {
            if (documents is null)
            {
                throw new ArgumentNullException(nameof(documents));
            }

            foreach (XmlDocument document in documents)
            {
                var node = document.LastChild.SelectSingleNode(expression, nsm);
                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }
    }
}
