using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface INodeAttributeParser
    {
        List<IReportItem> Parse(XmlNode schemaNode, XmlNamespaceManager schemaNamespaceManager, XmlNode catalogueNode, XmlNamespaceManager catalogueNamespaceManager);
    }
}