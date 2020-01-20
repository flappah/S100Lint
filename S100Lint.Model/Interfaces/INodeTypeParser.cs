using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface INodeTypeParser : IS100LintBase
    {
        List<IReportItem> Parse(XmlNodeList typeNodes, XmlDocument[] xmlSchemas, XmlDocument featureCatalogue);
    }
}