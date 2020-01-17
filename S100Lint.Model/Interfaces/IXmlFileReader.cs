using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface IXmlFileReader
    {
        XmlDocument Read(string fileName);
    }
}