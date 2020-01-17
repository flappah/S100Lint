using S100Lint.Types.Interfaces;
using System.Collections.Generic;

namespace S100Lint.Model.Interfaces
{
    public interface ISchemaParser
    {
        List<IReportItem> Parse(string schemaFilename, string catalogueFileName);
    }
}