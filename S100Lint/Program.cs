using S100Lint.Model;
using System;

namespace S100Lint
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            if (args.Length == 0)
            {
                args = new[] { "S127.XSD", "S127_FC.XML" };
            }
#endif 

            if (args.Length != 2)
            {
                Console.WriteLine("Invalid syntax for S100Lint. Valid syntax is S100Lint [SchemaFileName] [FeatureCatalogueFileName]");
            }
            else 
            {
                var schemaFile = args[0];
                var featureCatalogueFile = args[1];

                Console.WriteLine($"Validating schemafile '{schemaFile}' with feature catalogue '{featureCatalogueFile}'.");

                var schemaParser = new SchemaParser();
                var reportItems = schemaParser.Validate(schemaFile, featureCatalogueFile);

                foreach (var reportItem in reportItems)
                {
                    if (reportItem.Chapter != 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Chapter {reportItem.Chapter}");
                        Console.WriteLine("-----------------------------");
                    }
                    else
                    {
                        Console.WriteLine($"{reportItem.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.ffff")}: {reportItem.Level} - {reportItem.Message}");
                    }
                }
            }
        }
    }
}
