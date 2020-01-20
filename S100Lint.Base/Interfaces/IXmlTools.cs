using System.Xml;

namespace S100Lint.Base.Interfaces
{
    public interface IXmlTools
    {
        XmlNode GetToplevelElement(XmlNode fromNode, XmlNamespaceManager xmlNsManager, string expression);
    }
}