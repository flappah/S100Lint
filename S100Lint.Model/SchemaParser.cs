using S100Lint.Model.Interfaces;
using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model
{
    public class SchemaParser : ISchemaParser
    {
        private XmlDocument _xmlSchema;
        private XmlDocument _featureCatalogue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaFilename"></param>
        /// <param name="catalogueFileName"></param>
        /// <returns></returns>
        public virtual List<IReportItem> Parse(string schemaFilename, string catalogueFileName)
        {
            var xmlFileReader = new XmlFileReader();
            var issues = new List<IReportItem>();

            _xmlSchema = xmlFileReader.Read(schemaFilename);
            if (_xmlSchema.HasChildNodes)
            {
                _featureCatalogue = xmlFileReader.Read(catalogueFileName);
                if (_featureCatalogue.HasChildNodes)
                {
                    XmlNamespaceManager xsdNsmgr = new XmlNamespaceManager(_featureCatalogue.NameTable);
                    xsdNsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

                    // parse all simpletypes
                    var simpleTypeNodes = _xmlSchema.LastChild.SelectNodes(@"xs:simpleType", xsdNsmgr);

                    var simpleTypeParser = new SchemaSimpleNodeParser();
                    issues.AddRange(simpleTypeParser.Parse(simpleTypeNodes, _featureCatalogue));

                    // parse all complexTypes
                    var complexTypeNodes = _xmlSchema.LastChild.SelectNodes(@"xs:complexType", xsdNsmgr);

                    var complexTypeParser = new SchemaComplexNodeParser();
                    issues.AddRange(complexTypeParser.Parse(complexTypeNodes, _featureCatalogue));
                }
            }

            return issues;
        }
    }
}
