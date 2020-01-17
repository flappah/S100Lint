using S100Lint.Model.Interfaces;
using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model
{
    public abstract class NodeAttributeParserBase : INodeAttributeParser
    {
        /// <summary>
        /// Parses the attributes in the specified nodes and validates them against the feature catalogue
        /// </summary>
        /// <param name="schemaNode"></param>
        /// <param name="schemaNamespaceManager"></param>
        /// <param name="catalogueNode"></param>
        /// <param name="catalogueNamespaceManager"></param>
        /// <returns>List<ReportItem></returns>
        public abstract List<IReportItem> Parse(XmlNode schemaNode, XmlNamespaceManager schemaNamespaceManager, XmlNode catalogueNode, XmlNamespaceManager catalogueNamespaceManager);
    }
}
