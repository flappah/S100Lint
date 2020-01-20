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
    public class ComplexNodeAttributesParser : NodeAttributeParserBase, IComplexNodeAttributesParser
    {
        /// <summary>
        /// Parses the attributes in the specified nodes and validates them against the feature catalogue
        /// </summary>
        /// <param name="schemaNode"></param>
        /// <param name="schemaNamespaceManager"></param>
        /// <param name="xmlSchemas"></param>
        /// <param name="catalogueNode"></param>
        /// <param name="catalogueNamespaceManager"></param>
        /// <returns>List<ReportItem></returns>
        public override List<IReportItem> Parse(XmlNode schemaNode, XmlNamespaceManager schemaNamespaceManager, XmlDocument[] xmlSchemas, XmlNode catalogueNode, XmlNamespaceManager catalogueNamespaceManager)
        {
            if (schemaNode is null)
            {
                throw new ArgumentNullException(nameof(schemaNode));
            }

            if (schemaNamespaceManager is null)
            {
                throw new ArgumentNullException(nameof(schemaNamespaceManager));
            }

            if (xmlSchemas is null)
            {
                throw new ArgumentNullException(nameof(xmlSchemas));
            }

            if (catalogueNode is null)
            {
                throw new ArgumentNullException(nameof(catalogueNode));
            }

            if (catalogueNamespaceManager is null)
            {
                throw new ArgumentNullException(nameof(catalogueNamespaceManager));
            }

            var validTypes = new Dictionary<string, string>()
            {
                { "string", "string, text" },
                { "double", "float, real" },
                { "anyuri", "url" },
                { "date", "text, date" },
                { "integer", "integer" },
                { "boolean", "boolean" },
                { "positiveinteger", "integer" },
                { "nonnegativeinteger", "integer" },
                { "time", "time" },
                { "decimal", "integer, float, real, decimal" }
            };

            var items = new List<IReportItem>();

            string complexTypeName = "";
            if (schemaNode != null && schemaNode.Attributes != null && schemaNode.Attributes.Count > 0)
            {
                complexTypeName = schemaNode.Attributes[0].InnerText;
            }

            var subAttributeNodes = catalogueNode.ParentNode.SelectNodes("S100FC:subAttributeBinding", catalogueNamespaceManager);
            if (subAttributeNodes != null && subAttributeNodes.Count > 0)
            {
                var schemaNodeList =
                    schemaNode.SelectNodes(@"xs:sequence/xs:element", schemaNamespaceManager);

                // grand total check on the number of attributes in the schema vs the catalogue
                if (schemaNodeList != null && schemaNodeList.Count != subAttributeNodes.Count)
                {
                    items.Add(new ReportItem
                    {
                        Level = Enumerations.Level.Warning,
                        Message = $"ComplexType '{complexTypeName}' has {(schemaNodeList.Count > subAttributeNodes.Count ? "more" : "less")} attributes then whats defined in the catalogue",
                        TimeStamp = DateTime.Now,
                        Type = Enumerations.Type.ComplexAttribute
                    });
                }

                // do individual attribute checks
                foreach (XmlNode subAttributeNode in subAttributeNodes)
                {
                    if (subAttributeNode != null && subAttributeNode.ChildNodes.Count > 0)
                    {
                        string schemaAttributeNameToCheck = "";
                        foreach (XmlNode childNode in subAttributeNode.ChildNodes)
                        {
                            if (childNode.Name.Equals("S100FC:attribute", StringComparison.InvariantCulture) && childNode.Attributes != null && childNode.Attributes.Count > 0)
                            {
                                schemaAttributeNameToCheck = childNode.Attributes[0].InnerText;
                                break;
                            }
                        }

                        if (!String.IsNullOrEmpty(schemaAttributeNameToCheck))
                        {
                            var schemaNodeStrictNode =
                                schemaNode.SelectSingleNode($@"xs:sequence/xs:element[@name='{schemaAttributeNameToCheck}']", schemaNamespaceManager);

                            // validates the existence of all elements defined in the catalogue for the specified complextype
                            if (schemaNodeStrictNode == null || schemaNode.Attributes == null || schemaNode.Attributes.Count == 0)
                            {
                                items.Add(new ReportItem
                                {
                                    Level = Enumerations.Level.Error,
                                    Message = $"Element '{schemaAttributeNameToCheck}' is not defined for ComplexType '{complexTypeName}'",
                                    TimeStamp = DateTime.Now,
                                    Type = Enumerations.Type.ComplexAttribute
                                });
                            }
                            else
                            {
                                // if type is a SimpleType or ComplexType which has to be defined in the schema, check if the definition exists in the schema
                                var schemaAttributeType = "";
                                foreach (XmlAttribute attribute in schemaNodeStrictNode.Attributes)
                                {
                                    if (attribute.Name == "type")
                                    {
                                        schemaAttributeType = attribute.InnerText;
                                        break;
                                    }
                                }

                                if (!schemaAttributeType.Contains(":", StringComparison.InvariantCulture))
                                {
                                    XmlNode simpleTypeCheckNode =
                                        SelectSingleNode(xmlSchemas, $@"//xs:simpleType[@name='{schemaAttributeType}']", schemaNamespaceManager);

                                    XmlNode complexTypeCheckNode =
                                        SelectSingleNode(xmlSchemas, $@"//xs:complexType[@name='{schemaAttributeType}']", schemaNamespaceManager);

                                    if (simpleTypeCheckNode == null && complexTypeCheckNode == null)
                                    {
                                        items.Add(new ReportItem
                                        {
                                            Level = Enumerations.Level.Error,
                                            Message = $"Referenced {(simpleTypeCheckNode != null ? "SimpleType" : "ComplexType")} '{schemaAttributeType}' for '{schemaAttributeNameToCheck}' in ComplexType '{complexTypeName}' is not defined in the schema",
                                            TimeStamp = DateTime.Now,
                                            Type = Enumerations.Type.ComplexAttribute
                                        });
                                    }
                                }
                                else
                                {
                                    // if type is defined check if the types in both schema and catalogue are compatible
                                    var referencedCatalogueTypeNodeList =
                                        catalogueNode.OwnerDocument.LastChild.SelectNodes($@"//S100FC:S100_FC_SimpleAttribute/S100FC:code[.='{schemaAttributeNameToCheck}']", catalogueNamespaceManager);

                                    if (referencedCatalogueTypeNodeList != null && referencedCatalogueTypeNodeList.Count > 0)
                                    {
                                        var referencedCatalogueTypeNode =
                                            referencedCatalogueTypeNodeList[0].ParentNode.SelectSingleNode($@"S100FC:valueType", catalogueNamespaceManager);

                                        if (referencedCatalogueTypeNode != null)
                                        {
                                            string catalogueAttributeType = referencedCatalogueTypeNode.InnerText;

                                            if (!validTypes[schemaAttributeType.ToLower(CultureInfo.InvariantCulture).LastPart(":")].Contains(catalogueAttributeType.ToLower(CultureInfo.InvariantCulture), StringComparison.InvariantCulture))
                                            {
                                                items.Add(new ReportItem
                                                {
                                                    Level = Enumerations.Level.Error,
                                                    Message = $"Attribute {schemaAttributeNameToCheck} of type '{complexTypeName}' has an invalid type with respect to the catalogue ({schemaAttributeType} vs {catalogueAttributeType})",
                                                    TimeStamp = DateTime.Now,
                                                    Type = Enumerations.Type.ComplexAttribute
                                                });
                                            }
                                        }
                                    }
                                }

                                // check if min- and maxOccurs values are specified according to the lower- and upper vales in the catalogue
                                var multiplicityNode = subAttributeNode.SelectSingleNode(@"S100FC:multiplicity", catalogueNamespaceManager);
                                if (multiplicityNode != null && multiplicityNode.HasChildNodes)
                                {
                                    string lowerValue = "";
                                    string upperValue = "";

                                    foreach (XmlNode childNode in multiplicityNode.ChildNodes)
                                    {
                                        if (childNode.Name == "S100Base:lower")
                                        {
                                            lowerValue = childNode.InnerText;
                                        }
                                        else if (childNode.Name == "S100Base:upper")
                                        {
                                            upperValue = childNode.InnerText;
                                        }
                                    }

                                    foreach (XmlAttribute attribute in schemaNodeStrictNode.Attributes)
                                    {
                                        if (attribute.Name == "minOccurs")
                                        {
                                            if (!attribute.InnerText.Contains(lowerValue, StringComparison.InvariantCulture))
                                            {
                                                items.Add(new ReportItem
                                                {
                                                    Level = Enumerations.Level.Error,
                                                    Message = $"Attribute '{schemaAttributeNameToCheck}' in ComplexType '{complexTypeName}' its minOccurs value is not equal to the catalogue ({attribute.InnerText} vs {lowerValue})",
                                                    TimeStamp = DateTime.Now,
                                                    Type = Enumerations.Type.ComplexAttribute
                                                });
                                            }
                                        }
                                        else if (attribute.Name == "maxOccurs")
                                        {
                                            if (!attribute.InnerText.Contains(upperValue, StringComparison.InvariantCulture))
                                            {
                                                items.Add(new ReportItem
                                                {
                                                    Level = Enumerations.Level.Error,
                                                    Message = $"Attribute '{schemaAttributeNameToCheck}' in ComplexType '{complexTypeName}' its maxOccurs value is not equal to the catalogue ({attribute.InnerText} vs {upperValue})",
                                                    TimeStamp = DateTime.Now,
                                                    Type = Enumerations.Type.ComplexAttribute
                                                });
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return items;
        }
    }
}
