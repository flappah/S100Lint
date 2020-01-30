using S100Lint.Model.Interfaces;
using S100Lint.Types;
using S100Lint.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model.Validation
{
    public class FeatureCatalogueParser : S100LintBase, IFeatureCatalogueParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureCatalogue"></param>
        /// <param name="fcNsManager"></param>
        /// <param name="schemaDocuments"></param>
        /// <param name="schemaNsManager"></param>
        /// <returns></returns>
        public List<IReportItem> Validate(XmlDocument featureCatalogue, XmlNamespaceManager fcNsManager, XmlDocument[] schemaDocuments, XmlNamespaceManager schemaNsManager)
        {
            if (featureCatalogue is null)
            {
                throw new ArgumentNullException(nameof(featureCatalogue));
            }

            if (fcNsManager is null)
            {
                throw new ArgumentNullException(nameof(fcNsManager));
            }

            if (schemaDocuments is null)
            {
                throw new ArgumentNullException(nameof(schemaDocuments));
            }

            if (schemaNsManager is null)
            {
                throw new ArgumentNullException(nameof(schemaNsManager));
            }

            var issues = new List<IReportItem>();

            issues.Add(new ReportItem { Chapter = Enumerations.Chapter.FeatureCatalogueBaseCheck });

            var catalogueSimpleTypeNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_SimpleAttributes/S100FC:S100_FC_SimpleAttribute", fcNsManager);
            if (catalogueSimpleTypeNodes != null && catalogueSimpleTypeNodes.Count > 0)
            {
                // Now scan the feature catalogue for simple types
                foreach (XmlNode catalogueNode in catalogueSimpleTypeNodes)
                {
                    XmlNode fcNameNode = catalogueNode.SelectSingleNode(@"S100FC:code", fcNsManager);
                    XmlNode fcAliasNode = catalogueNode.SelectSingleNode(@"S100FC:alias", fcNsManager);

                    if (fcNameNode != null)
                    {
                        string fcSingleTypeName = fcNameNode.InnerText;
                        string fcSingleTypeAlias = "";
                        if (fcAliasNode != null)
                        {
                            fcSingleTypeAlias = fcAliasNode.InnerText;
                        }

                        // now check if the simpletype in the feature catalogue exists in the schema
                        var schemaSingleTypeNode = SelectSingleNode(schemaDocuments, $@"xs:simpleType[@name='{fcSingleTypeName}']", schemaNsManager);
                        if (schemaSingleTypeNode == null)
                        {
                            schemaSingleTypeNode = SelectSingleNode(schemaDocuments, $@"xs:simpleType[contains(@name, '{fcSingleTypeName}')]", schemaNsManager);

                            if (schemaSingleTypeNode != null)
                            {
                                var nameAttribute = FindAttributeByName(schemaSingleTypeNode.Attributes, "name");

                                issues.Add(new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Message = $"The SimpleType defined in the feature catalogue as '{fcSingleTypeName}' is defined in the XML Schema as '{nameAttribute.InnerText}'",
                                    TimeStamp = DateTime.Now,
                                    Type = Enumerations.Type.SimpleType
                                });
                            }
                        }

                        if (schemaSingleTypeNode == null)
                        {
                            // sometimes the schema uses the alias
                            if (!String.IsNullOrEmpty(fcSingleTypeAlias))
                            {
                                schemaSingleTypeNode = SelectSingleNode(schemaDocuments, $@"xs:simpleType[@name='{fcSingleTypeAlias}']", schemaNsManager);
                            }

                            // if node still isn't defined generate a warning
                            if (schemaSingleTypeNode == null)
                            {
                                issues.Add(new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Message = $"The SimpleType with the name '{fcSingleTypeName}' as defined in the feature catalogue does not exist in the schema",
                                    TimeStamp = DateTime.Now,
                                    Type = Enumerations.Type.SimpleType
                                });
                            }
                        }
                    }
                }
            }

            var catalogueComplexTypeNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_ComplexAttributes/S100FC:S100_FC_ComplexAttribute", fcNsManager);
            if (catalogueComplexTypeNodes != null && catalogueComplexTypeNodes.Count > 0)
            {
                // Now scan the feature catalogue for complex types
                foreach (XmlNode catalogueNode in catalogueComplexTypeNodes)
                {
                    XmlNode fcNameNode = catalogueNode.SelectSingleNode(@"S100FC:code", fcNsManager);
                    XmlNode fcAliasNode = catalogueNode.SelectSingleNode(@"S100FC:alias", fcNsManager);

                    if (fcNameNode != null)
                    {
                        string fcComplexTypeName = fcNameNode.InnerText;
                        string fcComplexTypeAlias = "";
                        if (fcAliasNode != null)
                        {
                            fcComplexTypeAlias = fcAliasNode.InnerText;
                        }

                        // now check if the simpletype in the feature catalogue exists in the schema
                        var schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:complexType[@name='{fcComplexTypeName}']", schemaNsManager);
                        if (schemaComplexTypeNode == null)
                        {
                            schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:complexType[contains(@name, '{fcComplexTypeName}')]", schemaNsManager);

                            if (schemaComplexTypeNode != null)
                            {
                                var nameAttribute = FindAttributeByName(schemaComplexTypeNode.Attributes, "name");

                                issues.Add(new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Message = $"The SimpleType defined in the feature catalogue as '{fcComplexTypeName}' is defined in the XML Schema as '{nameAttribute.InnerText}'",
                                    TimeStamp = DateTime.Now,
                                    Type = Enumerations.Type.SimpleType
                                });
                            }
                        }

                        if (schemaComplexTypeNode == null)
                        {
                            // sometimes the schema uses the alias
                            if (!String.IsNullOrEmpty(fcComplexTypeAlias))
                            {
                                schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:complexType[@name='{fcComplexTypeAlias}']", schemaNsManager);
                            }

                            // if node still isn't defined generate a warning
                            if (schemaComplexTypeNode == null)
                            {
                                issues.Add(new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Message = $"The ComplexType with the name '{fcComplexTypeName}' as defined in the feature catalogueoes not exist in the schema",
                                    TimeStamp = DateTime.Now,
                                    Type = Enumerations.Type.SimpleType
                                });
                            }
                        }
                    }
                }
            }

            return issues;
        }
    }
}
