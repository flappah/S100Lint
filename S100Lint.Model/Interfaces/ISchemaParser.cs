using S100Lint.Types.Interfaces;
using System.Collections.Generic;

namespace S100Lint.Model.Interfaces
{
    public interface ISchemaParser : IS100LintBase
    {
        List<IReportItem> Validate(string schemaFilename, string catalogueFileName);
    }
}