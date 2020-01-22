using S100Lint.Model.Interfaces;
using S100Lint.Types;
using S100Lint.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace S100Lint.Model.XReference
{
    public class NodeAnalyser : S100LintBase, INodeAnalyser
    {
        /// <summary>
        /// Analyses the source- and target-nodes for inconsistencies
        /// </summary>
        /// <param name="evalNode">name by which to identify the current node under analysis</param>
        /// <param name="sourceNode">source node to compare</param>
        /// <param name="targetNode">target node to compare</param>
        /// <param name="namespaceManager">namespace manager</param>
        /// <returns>List<IReportItem></returns>
        public virtual List<IReportItem> Analyse(string evalNode, XmlNode sourceNode, XmlNode targetNode, XmlNamespaceManager namespaceManager)
        {
            if (sourceNode is null)
            {
                throw new ArgumentNullException(nameof(sourceNode));
            }

            if (targetNode is null)
            {
                throw new ArgumentNullException(nameof(targetNode));
            }

            if (namespaceManager is null)
            {
                throw new ArgumentNullException(nameof(namespaceManager));
            }

            var items = new List<IReportItem>();

            if (sourceNode.HasChildNodes && targetNode.HasChildNodes)
            {
                foreach (XmlNode sourceChildNode in sourceNode.ChildNodes)
                {
                    XmlNode comparableTargetChildNode = null;

                    // Both element and enumeration xmlnodes can occur multiple times as childnode from a given xmlnode. 
                    // Use the name or value attribute to distinguish between multiple values 
                    if (sourceChildNode.Name.ToLower(CultureInfo.InvariantCulture).Contains("element", StringComparison.InvariantCulture))
                    {
                        var elementNameAttribute =
                            FindAttributeByName(sourceChildNode.Attributes, "name");

                        if (elementNameAttribute != null)
                        {
                            comparableTargetChildNode =
                                targetNode.SelectSingleNode($"{sourceChildNode.Name}[@name='{elementNameAttribute.InnerText}']", namespaceManager);
                        }
                    }
                    else if (sourceChildNode.Name.ToLower(CultureInfo.InvariantCulture).Contains("enumeration", StringComparison.InvariantCulture))
                    {
                        var elementValueAttribute =
                            FindAttributeByName(sourceChildNode.Attributes, "value");

                        if (elementValueAttribute != null)
                        {
                            comparableTargetChildNode =
                                targetNode.SelectSingleNode($"{sourceChildNode.Name}[@value='{elementValueAttribute.InnerText}']", namespaceManager);
                        }
                    }
                    else
                    {
                        // lowest level (usually the text) is represented as a node which name contains a hash symbol. No targetnode 
                        // needs to be selected
                        if (sourceChildNode.Name.Contains("#", StringComparison.InvariantCulture))
                        {
                            comparableTargetChildNode = targetNode.FirstChild;
                        }
                        else
                        {
                            comparableTargetChildNode = targetNode.SelectSingleNode(sourceChildNode.Name, namespaceManager);
                        }
                    }

                    if (comparableTargetChildNode != null)
                    {
                        if (comparableTargetChildNode.InnerXml != sourceChildNode.InnerXml ||
                            comparableTargetChildNode.InnerText != sourceChildNode.InnerText)
                        {
                            // if there's a difference between source and target node
                            int originalItemCount = items.Count;

                            if (comparableTargetChildNode.ChildNodes.Count > 0 && sourceChildNode.ChildNodes.Count > 0)
                            {
                                // check if difference originates from childnodes, check childnode(s)
                                items.AddRange(Analyse(evalNode, sourceChildNode, comparableTargetChildNode, namespaceManager));
                            }
                            
                            if (originalItemCount == items.Count)
                            {
                                // render the breadcrumb trail for reporting purposes
                                string breadCrumbTrail =
                                    GenerateXmlNodeBreadCrumbTrail(sourceChildNode);

                                // if difference is not from childnodes, the difference come from the current element. 
                                items.Add(new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Message = $"The XmlNode '{breadCrumbTrail}' in type '{evalNode}' in the first schema is different from the same XmlNode in the second schema",
                                    TimeStamp = DateTime.Now,
                                    Type = Enumerations.Type.SimpleType
                                });
                            }
                        }
                    }
                }
            }

            return items;
        }
    }
}
