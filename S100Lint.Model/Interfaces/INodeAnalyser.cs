using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface INodeAnalyser
    {
        List<IReportItem> Analyse(string evalNode, XmlNode sourceNode, XmlNode targetNode, XmlNamespaceManager namespaceManager);
    }
}