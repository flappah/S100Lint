using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface ISchemaParser : IS100LintBase
    {
        List<IReportItem> Analyse(List<XmlNode> sourceSimpleNodes, List<XmlNode> sourceComplexNodes, XmlDocument[] targetSchemas, XmlNamespaceManager namespaceManager);
        List<IReportItem> Parse(XmlDocument[] sourceSchemas, XmlDocument[] targetSchemas);
    }
}