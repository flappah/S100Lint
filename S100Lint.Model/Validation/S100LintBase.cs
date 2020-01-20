using S100Lint.Model.Interfaces;
using System;
using System.Xml;

namespace S100Lint.Model.Validation
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
