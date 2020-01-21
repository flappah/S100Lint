using S100Lint.Types.Interfaces;
using System.Collections.Generic;

namespace S100Lint.Model.Interfaces
{
    public interface ISchemaAnalyser : IS100LintBase
    {
        List<IReportItem> Validate(string schemaFilename, string catalogueFileName);
    }
}