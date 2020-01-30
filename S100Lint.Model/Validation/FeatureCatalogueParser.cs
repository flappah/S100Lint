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
    public class FeatureCatalogueParser : S100LintBase, IFeatureCatalogueParser
    {
        /// <summary>
        /// Checks all simpletype nodes in the XMLSchema(s)
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="fcNsManager"></param>
        /// <param name="schemaDocuments"></param>
        /// <param name="schemaNsManager"></param>
        /// <returns>List<IReportItem></returns>
        private List<IReportItem> SimpleTypeChecker(XmlNodeList nodes, XmlNamespaceManager fcNsManager, XmlDocument[] schemaDocuments, XmlNamespaceManager schemaNsManager)
        {
            if (nodes is null)
            {
                throw new ArgumentNullException(nameof(nodes));
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

            foreach (XmlNode catalogueNode in nodes)
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

            return issues;
        }

        /// <summary>
        /// Checks all complextype nodes in the XMLSchema(s)
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="fcNsManager"></param>
        /// <param name="schemaDocuments"></param>
        /// <param name="schemaNsManager"></param>
        /// <returns>List<IReportItem></returns>
        private List<IReportItem> ComplexTypeChecker(XmlNodeList nodes, XmlNamespaceManager fcNsManager, XmlDocument[] schemaDocuments, XmlNamespaceManager schemaNsManager)
        {
            if (nodes is null)
            {
                throw new ArgumentNullException(nameof(nodes));
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

            foreach (XmlNode catalogueNode in nodes)
            {
                XmlAttribute abstractAttribute = FindAttributeByName(catalogueNode.Attributes, "isAbstract");
                bool isAbstract = false;
                if (abstractAttribute != null)
                {
                    isAbstract = abstractAttribute.InnerText.ToLower(CultureInfo.InvariantCulture) == "true";
                }

                if (!isAbstract)
                {
                    XmlNode fcNameNode = catalogueNode.SelectSingleNode(@"S100FC:code", fcNsManager);
                    XmlNode fcAliasNode = catalogueNode.SelectSingleNode(@"S100FC:alias", fcNsManager);

                    if (fcNameNode != null)
                    {
                        string fcTypeName = catalogueNode.Name.LastPart("_");

                        string fcComplexTypeName = fcNameNode.InnerText;
                        string fcComplexTypeAlias = "";
                        if (fcAliasNode != null)
                        {
                            fcComplexTypeAlias = fcAliasNode.InnerText;
                        }

                        // now check if the simpletype in the feature catalogue exists in the schema. First check the xs:element
                        var schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:element[@name='{fcComplexTypeName}']", schemaNsManager);
                        if (schemaComplexTypeNode == null)
                        {
                            // if there's no xs:element, try xs:complexType
                            schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:complexType[@name='{fcComplexTypeName}']", schemaNsManager);
                            if (schemaComplexTypeNode == null)
                            {
                                // if that does not work, try to see if the node exists but with characters appended to it.
                                if (schemaComplexTypeNode == null)
                                {
                                    schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:complexType[contains(@name, '{fcComplexTypeName}')]", schemaNsManager);

                                    // sometimes the schema uses the alias
                                    if (schemaComplexTypeNode == null && !String.IsNullOrEmpty(fcComplexTypeAlias))
                                    {
                                        schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:element[@name='{fcComplexTypeAlias}']", schemaNsManager);
                                        if (schemaComplexTypeNode == null)
                                        {
                                            schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:complexType[@name='{fcComplexTypeAlias}']", schemaNsManager);

                                            if (schemaComplexTypeNode == null)
                                            {
                                                schemaComplexTypeNode = SelectSingleNode(schemaDocuments, $@"xs:complexType[contains(@name, '{fcComplexTypeAlias}')]", schemaNsManager);
                                            }
                                        }
                                    }
                                }

                                // if this does generate a hit generate a warning that the type exists but with a (slighly) different name
                                if (schemaComplexTypeNode != null)
                                {
                                    var nameAttribute = FindAttributeByName(schemaComplexTypeNode.Attributes, "name");

                                    issues.Add(new ReportItem
                                    {
                                        Level = Enumerations.Level.Warning,
                                        Message = $"The {fcTypeName} defined in the feature catalogue as '{fcComplexTypeName}' is defined in the XML Schema as '{nameAttribute.InnerText}'",
                                        TimeStamp = DateTime.Now,
                                        Type = Enumerations.Type.SimpleType
                                    });
                                }
                            }

                            // if node still isn't defined generate a warning
                            if (schemaComplexTypeNode == null)
                            {
                                issues.Add(new ReportItem
                                {
                                    Level = Enumerations.Level.Warning,
                                    Message = $"The {fcTypeName} with the name '{fcComplexTypeName}' as defined in the feature catalogue does not exist in the schema",
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

            // S100_FC_SimpleAttributes
            var catalogueSimpleTypeNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_SimpleAttributes/S100FC:S100_FC_SimpleAttribute", fcNsManager);
            if (catalogueSimpleTypeNodes != null && catalogueSimpleTypeNodes.Count > 0)
            {
                // Now scan the feature catalogue for simple types
                issues.AddRange(SimpleTypeChecker(catalogueSimpleTypeNodes, fcNsManager, schemaDocuments, schemaNsManager));
            }

            // S100_FC_ComplexAttributes
            var catalogueComplexTypeNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_ComplexAttributes/S100FC:S100_FC_ComplexAttribute", fcNsManager);
            if (catalogueComplexTypeNodes != null && catalogueComplexTypeNodes.Count > 0)
            {
                // Now scan the feature catalogue for complex types
                issues.AddRange(ComplexTypeChecker(catalogueComplexTypeNodes, fcNsManager, schemaDocuments, schemaNsManager));
            }

            // S100_FC_Roles
            var catalogueRoleNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_Roles/S100FC:S100_FC_Role", fcNsManager);
            if (catalogueRoleNodes != null && catalogueRoleNodes.Count > 0)
            {
                // Now scan the feature catalogue for roles
                issues.AddRange(ComplexTypeChecker(catalogueRoleNodes, fcNsManager, schemaDocuments, schemaNsManager));
            }

            // S100_FC_InformationAssociations
            var catalogueInfoAssociationNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_InformationAssociations/S100FC:S100_FC_InformationAssociation", fcNsManager);
            if (catalogueInfoAssociationNodes != null && catalogueInfoAssociationNodes.Count > 0)
            {
                // Now scan the feature catalogue for information assoications
                issues.AddRange(ComplexTypeChecker(catalogueInfoAssociationNodes, fcNsManager, schemaDocuments, schemaNsManager));
            }

            // S100_FC_FeatureAssociations
            var catalogueFtrAssocationNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_FeatureAssociations/S100FC:S100_FC_FeatureAssociation", fcNsManager);
            if (catalogueFtrAssocationNodes != null && catalogueFtrAssocationNodes.Count > 0)
            {
                // Now scan the feature catalogue for feature associations
                issues.AddRange(ComplexTypeChecker(catalogueFtrAssocationNodes, fcNsManager, schemaDocuments, schemaNsManager));
            }

            // S100_FC_InformationTypes
            var catalogueInformationTypeNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_InformationTypes/S100FC:S100_FC_InformationType", fcNsManager);
            if (catalogueInformationTypeNodes != null && catalogueInformationTypeNodes.Count > 0)
            {
                // Now scan the feature catalogue for complex types
                issues.AddRange(ComplexTypeChecker(catalogueInformationTypeNodes, fcNsManager, schemaDocuments, schemaNsManager));
            }

            // S100_FC_FeatureTypes
            var catalogueFeatureTypeNodes = featureCatalogue.LastChild.SelectNodes($@"S100FC:S100_FC_FeatureTypes/S100FC:S100_FC_FeatureType", fcNsManager);
            if (catalogueFeatureTypeNodes != null && catalogueFeatureTypeNodes.Count > 0)
            {
                // Now scan the feature catalogue for feature types
                issues.AddRange(ComplexTypeChecker(catalogueFeatureTypeNodes, fcNsManager, schemaDocuments, schemaNsManager));
            }

            return issues;
        }
    }
}
