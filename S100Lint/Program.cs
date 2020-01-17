using System;
using S100Lint.Model;
using S100Lint.Base;

namespace S100Lint
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            args = new[] { "S123.XSD", "S123_FC.XML"};
#endif 

            if (args.Length != 2)
            {
                Console.WriteLine("Invalid syntax for S100Lint. Valid syntax is S100Lint {SchemaFileName} {FeatureCatalogueFileName}");
            }
            else 
            {
                var schemaFile = args[0];
                var featureCatalogueFile = args[1];

                Console.WriteLine($"Validating schemafile '{schemaFile}' with feature catalogue '{featureCatalogueFile}'.");

                var schemaParser = new SchemaParser();
                var reportItems = schemaParser.Parse(schemaFile, featureCatalogueFile);

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
                        Console.WriteLine($"{reportItem.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss.ffff")}: {reportItem.Level} - {reportItem.Type} - {reportItem.Message}");
                    }
                }
            }
        }
    }
}
