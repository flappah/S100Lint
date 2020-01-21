using S100Lint.Model.Interfaces;
using S100Lint.Model.Validation;
using S100Lint.Model.XReference;
using S100Lint.Types.Interfaces;
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
        /// <param name="schemaFileNameSource"></param>
        /// <param name="schemaFileNameTarget"></param>
        /// <returns></returns>
        public virtual List<IReportItem> XReference(string schemaFileNameSource, string schemaFileNameTarget)
        {
            var xmlFileReader = new XmlFileReader();
            var issues = new List<IReportItem>();

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
                issues.AddRange(schemaParser.Parse(xmlSourceSchemas.ToArray(), xmlTargetSchemas.ToArray()));
            }

            return issues;
        }


        /// <summary>
        /// Validates the specified schema file with the specified catalogue file
        /// </summary>
        /// <param name="schemaFilename"></param>
        /// <param name="catalogueFileName"></param>
        /// <returns></returns>
        public virtual List<IReportItem> Validate(string schemaFilename, string catalogueFileName)
        {
            var xmlFileReader = new XmlFileReader();
            var issues = new List<IReportItem>();

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

                    // parse all simpletypes
                    var simpleTypeNodes = xmlSchema.LastChild.SelectNodes(@"xs:simpleType", xsdNsmgr);

                    var simpleTypeParser = new SchemaSimpleNodeParser();
                    issues.AddRange(simpleTypeParser.Parse(simpleTypeNodes, xmlSchemas.ToArray(), featureCatalogue));

                    // parse all complexTypes
                    var complexTypeNodes = xmlSchema.LastChild.SelectNodes(@"xs:complexType", xsdNsmgr);

                    var complexTypeParser = new SchemaComplexNodeParser();
                    issues.AddRange(complexTypeParser.Parse(complexTypeNodes, xmlSchemas.ToArray(), featureCatalogue));
                }
            }

            return issues;
        }
    }
}
