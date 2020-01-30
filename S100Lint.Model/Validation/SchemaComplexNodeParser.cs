using S100Lint.Base;
using S100Lint.Model.Interfaces;
using S100Lint.Types;
using S100Lint.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace S100Lint.Model.Validation
{
    public class SchemaComplexNodeParser : NodeTypeParserBase, ISchemaComplexNodeParser
    {
        /// <summary>
        /// Parse nodes and validate them against the catalogue
        /// </summary>
        /// <param name="typeNodes">Nodes to parse</param>
        /// <param name="xmlSchemas"></param>
        /// <param name="featureCatalogue">Feature catalogue to use</param>
        /// <returns>List<ReportItem></returns>
        public override List<IReportItem> Parse(XmlNodeList typeNodes, XmlDocument[] xmlSchemas, XmlDocument featureCatalogue)
        {
            if (typeNodes is null)
            {
                throw new ArgumentNullException(nameof(typeNodes));
            }

            if (xmlSchemas is null)
            {
                throw new ArgumentNullException(nameof(xmlSchemas));
            }

            if (featureCatalogue is null)
            {
                throw new ArgumentNullException(nameof(featureCatalogue));
            }

            var issues = new List<IReportItem>();
            issues.Add(new ReportItem { Chapter = Enumerations.Chapter.ComplexTypes });

            if (typeNodes != null && typeNodes.Count > 0)
            {
                XmlNamespaceManager fcNsmgr = new XmlNamespaceManager(featureCatalogue.NameTable);
                fcNsmgr.AddNamespace("S100FC", "http://www.iho.int/S100FC");

                XmlNamespaceManager xsdNsmgr = new XmlNamespaceManager(featureCatalogue.NameTable);
                xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

                foreach (XmlNode xmlNode in typeNodes) // loop over all complextype nodess
                {
                    bool isAbstract = false;
                    if (xmlNode.Attributes != null && xmlNode.Attributes.Count > 0)
                    {
                        var abstractAttribute = FindAttributeByName(xmlNode.Attributes, "abstract");
                        if (abstractAttribute != null)
                        {
                            isAbstract = abstractAttribute.Value.ToLower(CultureInfo.InvariantCulture) == "true";
                        }
                    }

                    if (!isAbstract) // only validate concrete types
                    {
                        // determine the complextype - type
                        string complexTypeName = "";
                        var nameAttribute = FindAttributeByName(xmlNode.Attributes, "name");
                        if (nameAttribute != null)
                        {
                            complexTypeName = nameAttribute.Value;
                        }
                        
                        // sometimes the schema has the typename ended with Type. The catalogue though has it defined without the Type. Check on this
                        if (complexTypeName.EndsWith("Type", StringComparison.InvariantCulture))
                        {
                            var complexTypeWithTypeCheck =
                                featureCatalogue.LastChild.SelectNodes($@"//S100FC:code[.='{complexTypeName}']", fcNsmgr);

                            var complexTypeWithoutTypeCheck =
                                featureCatalogue.LastChild.SelectNodes($@"//S100FC:code[.='{complexTypeName.Replace("Type", "", StringComparison.InvariantCulture)}']", fcNsmgr);

                            if ((complexTypeWithTypeCheck == null || complexTypeWithTypeCheck.Count == 0) &&
                                (complexTypeWithoutTypeCheck != null && complexTypeWithoutTypeCheck.Count > 0))
                            {
                                complexTypeName = complexTypeName.Replace("Type", "", StringComparison.InvariantCulture);
                            }
                        }

                        ReportItem reportItem = null;
                        string extensionType = "";

                        // now check if the node's basetype is something else than a valid basetype 
                        var extensionNode = xmlNode.SelectSingleNode(@"xs:complexContent/xs:extension", xsdNsmgr);
                        if (extensionNode != null &&
                            extensionNode.Attributes.Count > 0)
                        {
                            extensionType = extensionNode.Attributes[0].Value;

                            if (!extensionType.Contains("InformationType", StringComparison.InvariantCulture) &&
                                !extensionType.Contains("FeatureType", StringComparison.InvariantCulture))
                            {
                                // if the node is derived from any other type than informationtype or featuretype go and see of which type
                                // the basetype is derived from. This only goes one level deep though. 
                                var parentTypeNode = GetToplevelElement(xmlNode, xsdNsmgr, @"xs:complexContent/xs:extension");

                                if (parentTypeNode != null && parentTypeNode.HasChildNodes)
                                {
                                    extensionNode = parentTypeNode.SelectSingleNode(@"xs:complexContent/xs:extension", xsdNsmgr);
                                    if (extensionNode != null && extensionNode.Attributes.Count > 0)
                                    {
                                        extensionType = extensionNode.Attributes[0].Value;
                                    }
                                }
                            }
                        }

                        // check on the existence of the type in the featurecatalogue

                        XmlNodeList overallcheckNodeListStrict =
                                featureCatalogue.LastChild.SelectNodes($@"//S100FC:code[.='{complexTypeName}']", fcNsmgr);

                        if ((overallcheckNodeListStrict == null || overallcheckNodeListStrict.Count == 0) && complexTypeName.EndsWith("Type", StringComparison.InvariantCulture))
                        { 
                            // XML Schema's sometimes use 'Type' added to distinguish between typedefinition and concrete implementation. If there is no
                            // hit if removing the 'Type' results in a hit. If so use this result instread
                            overallcheckNodeListStrict =
                                featureCatalogue.LastChild.SelectNodes($@"//S100FC:code[.='{complexTypeName.Replace("Type", "", StringComparison.InvariantCulture)}']", fcNsmgr);
                        }

                        
                        XmlNodeList overallcheckNodeListLoose =
                                featureCatalogue.LastChild.SelectNodes($@"//S100FC:code[translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{complexTypeName.ToLower(CultureInfo.InvariantCulture)}']", fcNsmgr);

                        if ((overallcheckNodeListLoose == null || overallcheckNodeListLoose.Count == 0) && complexTypeName.EndsWith("Type", StringComparison.InvariantCulture))
                        {
                            // XML Schema's sometimes use 'Type' added to distinguish between typedefinition and concrete implementation. If there is no
                            // hit if removing the 'Type' results in a hit. If so use this result instread
                            overallcheckNodeListLoose =
                                featureCatalogue.LastChild.SelectNodes($@"//S100FC:code[translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{complexTypeName.Replace("Type", "", StringComparison.InvariantCulture).ToLower(CultureInfo.InvariantCulture)}']", fcNsmgr);
                        }

                        var parentNodeTypeName =
                            extensionType.Replace("Abstract", "", StringComparison.InvariantCulture)
                                         .Replace("TypeType", "Type", StringComparison.InvariantCulture)
                                         .Replace("S100:", "", StringComparison.InvariantCulture)
                                         .Replace("gml:", "", StringComparison.InvariantCulture);

                        if (String.IsNullOrEmpty(parentNodeTypeName))
                        {
                            parentNodeTypeName = "ComplexType";
                        }

                        if (overallcheckNodeListStrict != null &&
                            overallcheckNodeListStrict.Count > 0 &&
                            overallcheckNodeListStrict[0].ParentNode != null &&
                            !String.IsNullOrEmpty(overallcheckNodeListStrict[0].ParentNode.Name))
                        {
                            parentNodeTypeName = overallcheckNodeListStrict[0].ParentNode?.Name.LastPart(Char.Parse("_"));
                        }
                        else if (overallcheckNodeListLoose != null &&
                            overallcheckNodeListLoose.Count > 0 &&
                            overallcheckNodeListLoose[0].ParentNode != null &&
                            !String.IsNullOrEmpty(overallcheckNodeListLoose[0].ParentNode.Name))
                        {
                            parentNodeTypeName = overallcheckNodeListLoose[0].ParentNode?.Name.LastPart(Char.Parse("_"));
                        }

                        if ((overallcheckNodeListStrict == null || overallcheckNodeListStrict.Count == 0) &&
                            (overallcheckNodeListLoose == null || overallcheckNodeListLoose.Count == 0))
                        {
                            reportItem =
                                new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Type = Enum.Parse<Enumerations.Type>(parentNodeTypeName, true),
                                    Message = $"{complexTypeName} is not defined in the feature catalogue",
                                    TimeStamp = DateTime.Now
                                };
                        }
                        else if ((overallcheckNodeListStrict == null || overallcheckNodeListStrict.Count == 0) &&
                                 (overallcheckNodeListLoose != null && overallcheckNodeListLoose.Count > 0))
                        {
                            reportItem =
                                    new ReportItem
                                    {
                                        Level = Enumerations.Level.Error,
                                        Type = Enum.Parse<Enumerations.Type>(parentNodeTypeName, true),
                                        Message = $"{complexTypeName} is spelled with a different set of upper- and lower case characters ('{complexTypeName}' where it should be '{overallcheckNodeListLoose[0].InnerText}')",
                                        TimeStamp = DateTime.Now
                                    };
                        }
                        else
                        {
                            if (!parentNodeTypeName.Contains(extensionType, StringComparison.InvariantCulture) &&
                                !extensionType.Contains(parentNodeTypeName, StringComparison.InvariantCulture))
                            {
                                reportItem =
                                        new ReportItem
                                        {
                                            Level = Enumerations.Level.Warning,
                                            Type = Enum.Parse<Enumerations.Type>(parentNodeTypeName, true),
                                            Message = $"{complexTypeName} is found in the feature catalogue as an '{overallcheckNodeListStrict[0].ParentNode.Name.LastPart("_")}' but the schema has assigned it to a different basetype ({extensionType})",
                                            TimeStamp = DateTime.Now
                                        };
                            }
                        }

                        // now do attribute validation
                        var attributeParser = new ComplexNodeAttributesParser();
                        if (overallcheckNodeListStrict != null && overallcheckNodeListStrict.Count > 0)
                        {
                            issues.AddRange(attributeParser.Parse(xmlNode, xsdNsmgr, xmlSchemas, overallcheckNodeListStrict[0], fcNsmgr));
                        }
                        else if (overallcheckNodeListLoose != null && overallcheckNodeListLoose.Count > 0)
                        {
                            issues.AddRange(attributeParser.Parse(xmlNode, xsdNsmgr, xmlSchemas, overallcheckNodeListLoose[0], fcNsmgr));
                        }

                        if (reportItem != null)
                        {
                            issues.Add(reportItem);
                        }
                    }
                }
            }

            return issues;
        }
    }
}
