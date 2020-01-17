using S100Lint.Model.Interfaces;
using S100Lint.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using S100Lint.Types.Interfaces;

namespace S100Lint.Model
{
    public class SimpleNodeAttributesParser : NodeAttributeParserBase, ISimpleNodeAttributesParser
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

            var attributeTypeNode = catalogueNode.SelectSingleNode(@"S100FC:valueType", catalogueNamespaceManager);
            string attributeType = "";
            if (attributeTypeNode != null && attributeTypeNode.HasChildNodes)
            {
                attributeType = attributeTypeNode.InnerText;
            }

            // tests for attribute values
            switch (attributeType.ToLower(CultureInfo.InvariantCulture))
            {
                case "enumeration":
                    var listedValuesNodes = catalogueNode.SelectNodes("S100FC:listedValues", catalogueNamespaceManager);
                    if (listedValuesNodes != null && listedValuesNodes.Count > 0 && listedValuesNodes[0].HasChildNodes)
                    {
                        foreach (XmlNode listedValueNode in listedValuesNodes[0].ChildNodes)
                        {
                            string label = "";
                            string definition = "";
                            if (listedValueNode != null && listedValueNode.HasChildNodes)
                            {
                                foreach (XmlNode lvChildNode in listedValueNode.ChildNodes)
                                {
                                    if (lvChildNode.Name.Contains("label", StringComparison.InvariantCulture))
                                    {
                                        label = lvChildNode.InnerText;
                                    }
                                    else if (lvChildNode.Name.Contains("definition", StringComparison.InvariantCulture))
                                    {
                                        definition = lvChildNode.InnerText;
                                    }
                                }
                            }

                            if (!String.IsNullOrEmpty(label))
                            {
                                var schemaLabelNode = schemaNode.SelectSingleNode($@"//xs:enumeration[@value='{label}']", schemaNamespaceManager);
                                if (schemaLabelNode == null || !schemaLabelNode.HasChildNodes)
                                {
                                    items.Add(
                                        new ReportItem
                                        {
                                            Level = Enumerations.Level.Error,
                                            Message = $"Attribute with enumeration-value '{label}' is not defined for SimpleType '{schemaNode.Attributes[0].Value}'",
                                            TimeStamp = DateTime.Now,
                                            Type = Enumerations.Type.SimpleAttribute
                                        });
                                }
                            }
                        }
                    }

                    break;


            }


            return items;
        }
    }
}
