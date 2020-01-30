using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface INodeAttributeParser : IS100LintBase
    {
        List<IReportItem> Parse(XmlNode schemaNode, XmlNamespaceManager schemaNamespaceManager, XmlDocument[] xmlSchemas, XmlNode catalogueNode, XmlNamespaceManager catalogueNamespaceManager);
    }
}