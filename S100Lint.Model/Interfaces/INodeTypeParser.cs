using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface INodeTypeParser
    {
        List<IReportItem> Parse(XmlNodeList typeNodes, XmlDocument featureCatalogue);
    }
}