using S100Lint.Types.Interfaces;
using System.Collections.Generic;

namespace S100Lint.Model.Interfaces
{
    public interface ISchemaAnalyser : IS100LintBase
    {
        /// <summary>
        /// Method cross references two schema files to search for commonalities and verifies if those 
        /// commonolities contains no differences
        /// </summary>
        /// <param name="schemaFileNameSource">filename of source</param>
        /// <param name="schemaFileNameTarget">filename of target</param>
        /// <param name="options">command line options</param>
        /// <returns>List<IReportItem></returns>
        List<IReportItem> XReference(string schemaFileNameSource, string schemaFileNameTarget, List<string> options);

        /// <summary>
        /// Validates the specified schema file with the specified catalogue file
        /// </summary>
        /// <param name="schemaFilename">filename of the schema</param>
        /// <param name="catalogueFileName">filename of the catalogue</param>
        /// <param name="options">command line options</param>
        /// <returns>List<IReportItem><IReportItem></returns>
        List<IReportItem> Validate(string schemaFilename, string catalogueFileName, List<string> options);
    }
}