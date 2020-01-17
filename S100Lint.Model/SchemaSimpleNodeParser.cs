﻿using S100Lint.Model.Interfaces;
using S100Lint.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using S100Lint.Types.Interfaces;

namespace S100Lint.Model
{
    public class SchemaSimpleNodeParser : NodeTypeParserBase, ISchemaSimpleNodeParser
    {
        /// <summary>
        /// Parse nodes and validate them against the catalogue
        /// </summary>
        /// <param name="typeNodes">Nodes to parse</param>
        /// <param name="featureCatalogue">Feature catalogue to use</param>
        /// <returns>List<ReportItem></returns>
        public override List<IReportItem> Parse(XmlNodeList typeNodes, XmlDocument featureCatalogue)
        {
            if (typeNodes is null)
            {
                throw new ArgumentNullException(nameof(typeNodes));
            }

            if (featureCatalogue is null)
            {
                throw new ArgumentNullException(nameof(featureCatalogue));
            }

            var issues = new List<IReportItem>();
            issues.Add(new ReportItem { Chapter = Enumerations.Chapter.SimpleTypes });

            if (typeNodes != null && typeNodes.Count > 0)
            {
                XmlNamespaceManager fcNsmgr = new XmlNamespaceManager(featureCatalogue.NameTable);
                fcNsmgr.AddNamespace("S100FC", "http://www.iho.int/S100FC");

                XmlNamespaceManager xsdNsmgr = new XmlNamespaceManager(featureCatalogue.NameTable);
                xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

                foreach (XmlNode xmlNode in typeNodes)
                {
                    // check on the existence of the simpletype in the featurecatalogue
                    string simpleTypeName = xmlNode.Attributes["name"].Value.Replace("Type", "", StringComparison.InvariantCulture);

                    XmlNodeList fcSimpleTypesStrict =
                        featureCatalogue.LastChild.SelectNodes($@"//S100FC:S100_FC_SimpleAttribute[S100FC:code='{simpleTypeName}']", fcNsmgr);

                    XmlNodeList fcSimpleTypesLoose =
                        featureCatalogue.LastChild.SelectNodes($@"//S100FC:S100_FC_SimpleAttribute[translate(S100FC:code, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{simpleTypeName.ToLower(CultureInfo.InvariantCulture)}']", fcNsmgr);

                    XmlNodeList fcSimpleTypesAlias =
                        featureCatalogue.LastChild.SelectNodes($@"//S100FC:S100_FC_SimpleAttribute[S100FC:alias='{simpleTypeName}']", fcNsmgr);

                    if (fcSimpleTypesStrict == null || fcSimpleTypesStrict.Count == 0)
                    {
                        if (fcSimpleTypesLoose != null && fcSimpleTypesLoose.Count > 0)
                        {
                            string nodeName = "";
                            foreach (XmlNode node in fcSimpleTypesLoose[0].ChildNodes)
                            {
                                if (node.Name.Contains("S100FC:code", StringComparison.InvariantCulture))
                                {
                                    nodeName = node.InnerText;
                                    break;
                                }
                            }

                            issues.Add(
                                new ReportItem
                                {
                                    Level = Enumerations.Level.Error,
                                    Type = Enumerations.Type.SimpleType,
                                    Message = $"{simpleTypeName} is spelled with a different set of upper- and lower case characters ('{simpleTypeName}' where it should be '{nodeName}')",
                                    TimeStamp = DateTime.Now
                                });
                        }
                        else if (fcSimpleTypesAlias != null && fcSimpleTypesAlias.Count > 0)
                        {
                            issues.Add(
                                new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Type = Enumerations.Type.SimpleType,
                                    Message = $"{simpleTypeName} is using the alias-value in the feature catalogue instead of the code-value",
                                    TimeStamp = DateTime.Now
                                });
                        }
                        else
                        {
                            issues.Add(
                                new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Type = Enumerations.Type.SimpleType,
                                    Message = $"{simpleTypeName} is not defined in the feature catalogue",
                                    TimeStamp = DateTime.Now
                                });
                        }
                    }

                    //now do attribute validation
                    var attributeParser = new SimpleNodeAttributesParser();
                    if (fcSimpleTypesStrict != null && fcSimpleTypesStrict.Count > 0)
                    {
                        issues.AddRange(attributeParser.Parse(xmlNode, xsdNsmgr, fcSimpleTypesStrict[0], fcNsmgr));
                    }
                    else if (fcSimpleTypesLoose != null && fcSimpleTypesLoose.Count > 0)
                    {
                        issues.AddRange(attributeParser.Parse(xmlNode, xsdNsmgr, fcSimpleTypesLoose[0], fcNsmgr));
                    }
                    else if (fcSimpleTypesAlias != null && fcSimpleTypesAlias.Count > 0)
                    {
                        issues.AddRange(attributeParser.Parse(xmlNode, xsdNsmgr, fcSimpleTypesAlias[0], fcNsmgr));
                    }
                }
            }

            return issues;
        }
    }
}
