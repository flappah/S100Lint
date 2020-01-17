using S100Lint.Model.Interfaces;
using S100Lint.Types;
using S100Lint.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model
{
    public class ComplexNodeAttributesParser : NodeAttributeParserBase, IComplexNodeAttributesParser
    {
        /// <summary>
        /// Parses the attributes in the specified nodes and validates them against the feature catalogue
        /// </summary>
        /// <param name="schemaNode"></param>
        /// <param name="schemaNamespaceManager"></param>
        /// <param name="catalogueNode"></param>
        /// <param name="catalogueNamespaceManager"></param>
        /// <returns>List<ReportItem></returns>
        public override List<IReportItem> Parse(XmlNode schemaNode, XmlNamespaceManager schemaNamespaceManager, XmlNode catalogueNode, XmlNamespaceManager catalogueNamespaceManager)
        {
            if (schemaNode is null)
            {
                throw new ArgumentNullException(nameof(schemaNode));
            }

            if (schemaNamespaceManager is null)
            {
                throw new ArgumentNullException(nameof(schemaNamespaceManager));
            }

            if (catalogueNode is null)
            {
                throw new ArgumentNullException(nameof(catalogueNode));
            }

            if (catalogueNamespaceManager is null)
            {
                throw new ArgumentNullException(nameof(catalogueNamespaceManager));
            }

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
                        string attributeNameToCheck = "";
                        foreach (XmlNode childNode in subAttributeNode.ChildNodes)
                        {
                            if (childNode.Name.Equals("S100FC:attribute", StringComparison.InvariantCulture) && childNode.Attributes != null && childNode.Attributes.Count > 0)
                            {
                                attributeNameToCheck = childNode.Attributes[0].InnerText;
                                break;
                            }
                        }

                        if (!String.IsNullOrEmpty(attributeNameToCheck))
                        {
                            var schemaNodeStrictNode =
                                schemaNode.SelectSingleNode($@"xs:sequence/xs:element[@name='{attributeNameToCheck}']", schemaNamespaceManager);

                            // validates the existence of all elements defined in the catalogue for the specified complextype
                            if (schemaNodeStrictNode == null || schemaNode.Attributes == null || schemaNode.Attributes.Count == 0)
                            {
                                items.Add(new ReportItem
                                {
                                    Level = Enumerations.Level.Error,
                                    Message = $"Element '{attributeNameToCheck}' is not defined for ComplexType '{complexTypeName}'",
                                    TimeStamp = DateTime.Now,
                                    Type = Enumerations.Type.ComplexAttribute
                                });
                            }
                            else
                            {
                                // if type is a SimpleType or ComplexType which has to be defined in the schema, check if the definition exists in the schema
                                var attributeTypeToCheck = "";
                                foreach (XmlAttribute attribute in schemaNodeStrictNode.Attributes)
                                {
                                    if (attribute.Name == "type")
                                    {
                                        attributeTypeToCheck = attribute.InnerText;
                                        break;
                                    }
                                }

                                if (!attributeTypeToCheck.Contains(":", StringComparison.InvariantCulture))
                                {
                                    XmlNode simleTypeCheckNode =
                                        schemaNode.OwnerDocument.LastChild.SelectSingleNode($@"//xs:simpleType[@name='{attributeTypeToCheck}']", schemaNamespaceManager);

                                    XmlNode complexTypeCheckNode =
                                        schemaNode.OwnerDocument.LastChild.SelectSingleNode($@"xs:complexType[@name='{attributeTypeToCheck}']", schemaNamespaceManager);

                                    if (simleTypeCheckNode == null && complexTypeCheckNode == null)
                                    {
                                        items.Add(new ReportItem
                                        {
                                            Level = Enumerations.Level.Error,
                                            Message = $"Referenced {(simleTypeCheckNode != null ? "SimpleType" : "ComplexType")} '{attributeTypeToCheck}' for '{attributeNameToCheck}' in ComplexType '{complexTypeName}' is not defined in the schema",
                                            TimeStamp = DateTime.Now,
                                            Type = Enumerations.Type.ComplexAttribute
                                        });
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
                                                    Message = $"Element '{attributeNameToCheck}' in ComplexType '{complexTypeName}' its minOccurs value is not equal to the catalogue ({attribute.InnerText} vs {lowerValue})",
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
                                                    Message = $"Element '{attributeNameToCheck}' in ComplexType '{complexTypeName}' its maxOccurs value is not equal to the catalogue ({attribute.InnerText} vs {upperValue})",
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
