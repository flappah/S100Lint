using S100Lint.Model.Interfaces;
using S100Lint.Model.Validation;
using S100Lint.Model.XReference;
using S100Lint.Types;
using S100Lint.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model
{
    public class SchemaAnalyser : S100LintBase, ISchemaAnalyser
    {
        /// <summary>
        /// Method cross references two schema files to search for commonalities and verifies if those 
        /// commonolities contains no differences
        /// </summary>
        /// <param name="schemaFileNameSource">filename of source</param>
        /// <param name="schemaFileNameTarget">filename of target</param>
        /// <param name="options">command line options</param>
        /// <returns>List<IReportItem></returns>
        public virtual List<IReportItem> XReference(string schemaFileNameSource, string schemaFileNameTarget, List<string> options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var xmlFileReader = new XmlFileReader();
            var items = new List<IReportItem>();

            var xmlSourceSchema = xmlFileReader.Read(schemaFileNameSource);
            var xmlTargetSchema = xmlFileReader.Read(schemaFileNameTarget);

            if (xmlSourceSchema.HasChildNodes && xmlTargetSchema.HasChildNodes)
            {
                XmlNamespaceManager xsdNsmgr = new XmlNamespaceManager(xmlSourceSchema.NameTable);
                xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

                var xmlSourceSchemas = new List<XmlDocument>() { xmlSourceSchema };
                var includedSchemaNodeList =
                        xmlSourceSchema.LastChild.SelectNodes("xs:include", xsdNsmgr);

                if (includedSchemaNodeList != null && includedSchemaNodeList.Count > 0)
                {
                    foreach (XmlNode includedSchemaNode in includedSchemaNodeList)
                    {
                        if (includedSchemaNode.Attributes != null &&
                            includedSchemaNode.Attributes.Count > 0)
                        {
                            XmlAttribute attribute = FindAttributeByName(includedSchemaNode.Attributes, "schemaLocation");
                            if (attribute != null)
                            {
                                string includedSchemaFileName = attribute.InnerText;
                                var includedXmlSchema = xmlFileReader.Read(includedSchemaFileName);
                                xmlSourceSchemas.Add(includedXmlSchema);
                            }
                        }
                    }
                }

                var xmlTargetSchemas = new List<XmlDocument>() { xmlTargetSchema };
                includedSchemaNodeList =
                        xmlTargetSchema.LastChild.SelectNodes("xs:include", xsdNsmgr);

                if (includedSchemaNodeList != null && includedSchemaNodeList.Count > 0)
                {
                    foreach (XmlNode includedSchemaNode in includedSchemaNodeList)
                    {
                        if (includedSchemaNode.Attributes != null &&
                            includedSchemaNode.Attributes.Count > 0)
                        {
                            XmlAttribute attribute = FindAttributeByName(includedSchemaNode.Attributes, "schemaLocation");
                            if (attribute != null)
                            {
                                string includedSchemaFileName = attribute.InnerText;
                                var includedXmlSchema = xmlFileReader.Read(includedSchemaFileName);
                                xmlTargetSchemas.Add(includedXmlSchema);
                            }
                        }
                    }
                }

                var schemaParser = new SchemaParser();
                items.AddRange(schemaParser.Parse(xmlSourceSchemas.ToArray(), xmlTargetSchemas.ToArray()));
            }

            int issues =
                items.FindAll(itm => itm.Level != Enumerations.Level.Info && itm.Chapter == 0).Count;

            if (issues != 0)
            {
                items.Add(new ReportItem
                {
                    Level = Enumerations.Level.Info,
                    Message = $"Cross referencing source- and target XMLSchema results in {issues} issue{(issues == 1 ? "" : "s")}",
                    TimeStamp = DateTime.Now,
                    Type = Enumerations.Type.Info
                });
            }

            return items;
        }

        /// <summary>
        /// Validates the specified schema file with the specified catalogue file
        /// </summary>
        /// <param name="schemaFilename">filename of the schema</param>
        /// <param name="catalogueFileName">filename of the catalogue</param>
        /// <param name="options">command line options</param>
        /// <returns>List<IReportItem><IReportItem></returns>
        public virtual List<IReportItem> Validate(string schemaFilename, string catalogueFileName, List<string> options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var xmlFileReader = new XmlFileReader();
            var items = new List<IReportItem>();

            var xmlSchema = xmlFileReader.Read(schemaFilename);
            List<XmlDocument> xmlSchemas = new List<XmlDocument>() { xmlSchema };

            if (xmlSchema.HasChildNodes)
            {
                XmlDocument featureCatalogue = xmlFileReader.Read(catalogueFileName);
                if (featureCatalogue.HasChildNodes)
                {
                    XmlNamespaceManager xsdNsmgr = new XmlNamespaceManager(xmlSchema.NameTable);
                    xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

                    var includedSchemaNodeList =
                        xmlSchema.LastChild.SelectNodes("xs:include", xsdNsmgr);

                    if (includedSchemaNodeList != null && includedSchemaNodeList.Count > 0)
                    {
                        foreach(XmlNode includedSchemaNode in includedSchemaNodeList)
                        {
                            if (includedSchemaNode.Attributes != null && 
                                includedSchemaNode.Attributes.Count > 0)
                            {
                                XmlAttribute attribute = FindAttributeByName(includedSchemaNode.Attributes, "schemaLocation");
                                if (attribute != null)
                                {
                                    string includedSchemaFileName = attribute.InnerText;
                                    var includedXmlSchema = xmlFileReader.Read(includedSchemaFileName);
                                    xmlSchemas.Add(includedXmlSchema);
                                }
                            }
                        }
                    }

                    XmlNamespaceManager fcNsmgr = new XmlNamespaceManager(featureCatalogue.NameTable);
                    fcNsmgr.AddNamespace("S100FC", "http://www.iho.int/S100FC");

                    if (options.Contains("--fc"))
                    {
                        // validate schema types vs defined types in featurecatalogue
                        var featureCatalogueParser = new FeatureCatalogueParser();
                        items.AddRange(featureCatalogueParser.Validate(featureCatalogue, fcNsmgr, xmlSchemas.ToArray(), xsdNsmgr));
                    }
                    var schemaSimpleTypeNodes = xmlSchema.LastChild.SelectNodes(@"xs:simpleType", xsdNsmgr);
                    if (schemaSimpleTypeNodes != null && schemaSimpleTypeNodes.Count > 0)
                    {
                        // parse all simpletypes
                        var simpleTypeParser = new SchemaSimpleNodeParser();
                        items.AddRange(simpleTypeParser.Parse(schemaSimpleTypeNodes, xmlSchemas.ToArray(), featureCatalogue));
                    }

                    var schemaComplexTypeNodes = xmlSchema.LastChild.SelectNodes(@"xs:complexType", xsdNsmgr);
                    if (schemaComplexTypeNodes != null && schemaComplexTypeNodes.Count > 0)
                    {
                        // parse all complexTypes
                        var complexTypeParser = new SchemaComplexNodeParser();
                        items.AddRange(complexTypeParser.Parse(schemaComplexTypeNodes, xmlSchemas.ToArray(), featureCatalogue));
                    }

                    // add statistics
                    if (schemaSimpleTypeNodes.Count > 0 || schemaComplexTypeNodes.Count > 0)
                    {
                        items.Add(new ReportItem
                        {
                            Level = Enumerations.Level.Info,
                            Message = $"Source XMLSchema contains {schemaSimpleTypeNodes.Count} SimpleNode{(schemaSimpleTypeNodes.Count == 1 ? "" : "s")} {(schemaSimpleTypeNodes.Count > 0 ? $"and {schemaComplexTypeNodes.Count} ComplexNode{(schemaComplexTypeNodes.Count == 1 ? "" : "s")}" : "")}",
                            TimeStamp = DateTime.Now,
                            Type = Enumerations.Type.Info
                        });
                    }

                    int issues =
                        items.FindAll(itm => itm.Level != Enumerations.Level.Info && itm.Chapter == 0).Count;

                    if (issues != 0)
                    {
                        items.Add(new ReportItem
                        {

                            Level = Enumerations.Level.Info,
                            Message = $"Validating the XMLSchema with the Feature Catalogue results in {issues} issue{(issues == 1 ? "" : "s")}",
                            TimeStamp = DateTime.Now,
                            Type = Enumerations.Type.Info
                        });
                    }
                }
            }

            return items;
        }
    }
}
