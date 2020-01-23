using S100Lint.Model.Interfaces;
using S100Lint.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using S100Lint.Types;

namespace S100Lint.Model.XReference
{
    public class SchemaParser : S100LintBase, ISchemaParser
    {
        /// <summary>
        /// Parses the supplied XMLDocument's. It parses the source schema's in two lists. One for simpletypes
        /// and one for complextypes. Both lists get sent to the Analyser method along with the targetschemas 
        /// to compare with. 
        /// </summary>
        /// <param name="sourceSchemas"></param>
        /// <param name="targetSchemas"></param>
        /// <returns></returns>
        public virtual List<IReportItem> Parse(XmlDocument[] sourceSchemas, XmlDocument[] targetSchemas)
        {
            if (sourceSchemas is null)
            {
                throw new ArgumentNullException(nameof(sourceSchemas));
            }

            if (targetSchemas is null)
            {
                throw new ArgumentNullException(nameof(targetSchemas));
            }

            var items = new List<IReportItem>();
            if (sourceSchemas.Length > 0 && targetSchemas.Length > 0)
            {
                XmlNamespaceManager xsdNsmgr = new XmlNamespaceManager(sourceSchemas[0].NameTable);
                xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

                List<XmlNode> sourceSimpleNodes = new List<XmlNode>();
                List<XmlNode> sourceComplexNodes = new List<XmlNode>();
                foreach (XmlDocument schema in sourceSchemas)
                {
                    XmlNodeList simpleNodes = schema.LastChild.SelectNodes($@"xs:simpleType", xsdNsmgr);
                    sourceSimpleNodes.AddRange(from XmlNode simpleNode in simpleNodes
                                               select simpleNode);

                    XmlNodeList complexNodes = schema.LastChild.SelectNodes($@"xs:complexType", xsdNsmgr);
                    sourceComplexNodes.AddRange(from XmlNode complexNode in complexNodes
                                                select complexNode);
                }

                // add statistics
                if (sourceSimpleNodes.Count > 0 || sourceComplexNodes.Count > 0)
                {
                    items.Add(new ReportItem
                    {
                        Level = Enumerations.Level.Info,
                        Message = $"Source XMLSchema contains {sourceSimpleNodes.Count} SimpleNode{(sourceSimpleNodes.Count == 1 ? "" : "s")} {(sourceComplexNodes.Count > 0 ? $"and {sourceComplexNodes.Count} ComplexNode{(sourceComplexNodes.Count == 1 ? "" : "s")}" : "")}",
                        TimeStamp = DateTime.Now,
                        Type = Enumerations.Type.Info
                    });
                }

                items.AddRange(Analyse(sourceSimpleNodes, sourceComplexNodes, targetSchemas, xsdNsmgr));
            }

            return items;
        }

        /// <summary>
        /// The analyser method compares the simple-and complex types with the supplied targetschemas. Any simpletype and/or
        /// complextypes that shows differences against the targetschema(s) gets sent to the NodeAnalyser for in-depth
        /// comparison. 
        /// </summary>
        /// <param name="sourceSimpleNodes"></param>
        /// <param name="sourceComplexNodes"></param>
        /// <param name="targetSchemas"></param>
        /// <param name="namespaceManager"></param>
        /// <returns></returns>
        public virtual List<IReportItem> Analyse(List<XmlNode> sourceSimpleNodes, List<XmlNode> sourceComplexNodes, XmlDocument[] targetSchemas, XmlNamespaceManager namespaceManager)
        {
            if (sourceSimpleNodes is null)
            {
                throw new ArgumentNullException(nameof(sourceSimpleNodes));
            }

            if (sourceComplexNodes is null)
            {
                throw new ArgumentNullException(nameof(sourceComplexNodes));
            }

            if (targetSchemas is null)
            {
                throw new ArgumentNullException(nameof(targetSchemas));
            }

            if (namespaceManager is null)
            {
                throw new ArgumentNullException(nameof(namespaceManager));
            }

            var items = new List<IReportItem>();

            int matchingSimpleNodes = 0;
            int matchingComplexNodes = 0;

            if (sourceSimpleNodes.Count > 0 && targetSchemas.Length > 0)
            {
                foreach (XmlNode sourceSimpleNode in sourceSimpleNodes)
                {
                    XmlAttribute nameAttribute = FindAttributeByName(sourceSimpleNode.Attributes, "name");
                    if (nameAttribute != null && !String.IsNullOrEmpty(nameAttribute.InnerText))
                    {
                        var targetSimpleNode = SelectSingleNode(targetSchemas, $@"xs:simpleType[@name='{nameAttribute.InnerText}']", namespaceManager);
                        if (targetSimpleNode != null && targetSimpleNode.ChildNodes.Count > 0)
                        {
                            matchingSimpleNodes++;
                            if (targetSimpleNode.InnerXml != sourceSimpleNode.InnerXml)
                            {
                                var nodeAnalyser = new NodeAnalyser();
                                items.AddRange(nodeAnalyser.Analyse(nameAttribute.InnerText, sourceSimpleNode, targetSimpleNode, namespaceManager));
                            }
                        }
                    }
                }
            }

            if (sourceComplexNodes.Count > 0 && targetSchemas.Length > 0)
            {
                foreach (XmlNode sourceComplexNode in sourceComplexNodes)
                {
                    XmlAttribute nameAttribute = FindAttributeByName(sourceComplexNode.Attributes, "name");
                    if (nameAttribute != null && !String.IsNullOrEmpty(nameAttribute.InnerText))
                    {
                        var targetComplexNode = SelectSingleNode(targetSchemas, $@"xs:complexType[@name='{nameAttribute.InnerText}']", namespaceManager);
                        if (targetComplexNode != null && targetComplexNode.HasChildNodes)
                        {
                            matchingComplexNodes++;
                            if (targetComplexNode.InnerXml != sourceComplexNode.InnerXml)
                            {
                                var nodeAnalyser = new NodeAnalyser();
                                items.AddRange(nodeAnalyser.Analyse(nameAttribute.InnerText, sourceComplexNode, targetComplexNode, namespaceManager));
                            }
                        }
                    }
                }
            }

            // add statistics
            items.Add(new ReportItem
            {
                Level = Enumerations.Level.Info,
                Message = $"Source- and target XMLSchema contain {matchingSimpleNodes} matching SimpleNode{(matchingSimpleNodes == 1 ? "" : "s")}",
                TimeStamp = DateTime.Now,
                Type = Enumerations.Type.Info
            });

            items.Add(new ReportItem
            {
                Level = Enumerations.Level.Info,
                Message = $"Source- and target XMLSchema contain {matchingComplexNodes} matching ComplexNode{(matchingComplexNodes == 1 ? "" : "s")}",
                TimeStamp = DateTime.Now,
                Type = Enumerations.Type.Info
            });

            return items;
        }
    }
}
