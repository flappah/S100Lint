using S100Lint.Types.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace S100Lint.Model.Interfaces
{
    public interface IFeatureCatalogueParser : IS100LintBase
    {
        List<IReportItem> Validate(XmlDocument featureCatalogue, XmlNamespaceManager fcNsManager, XmlDocument[] schemaDocuments, XmlNamespaceManager schemaNsManager);
    }
}