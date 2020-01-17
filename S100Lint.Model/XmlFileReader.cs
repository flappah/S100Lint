using S100Lint.Model.Interfaces;
using System.Xml;

namespace S100Lint.Model
{
    public class XmlFileReader : IXmlFileReader
    {
        /// <summary>
        /// Reads an XML fie and returns an XmlDocument
        /// </summary>
        /// <param name="fileName">File with full pathname</param>
        /// <returns>XmlDocument</returns>
        public virtual XmlDocument Read(string fileName)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);

            return xmlDocument;
        }
    }
}
