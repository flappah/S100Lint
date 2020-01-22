using S100Lint.Model;
using S100Lint.Types.Interfaces;
using System;
using System.Collections.Generic;

namespace S100Lint
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid syntax for S100Lint. Valid syntax is S100Lint [SchemaFileName] [FeatureCatalogueFileName]");
            }
            else 
            {
                var file1 = args[0];
                var file2 = args[1];
                var reportItems = new List<IReportItem>();
                var schemaParser = new SchemaAnalyser();

                if (file2.ToLower().Contains(".xsd"))
                {
                    Console.WriteLine($"Cross referencing schemafile '{file1}' with schemafile '{file2}'.");
                    reportItems = schemaParser.XReference(file1, file2);
                }
                else
                {
                    Console.WriteLine($"Validating schemafile '{file1}' with feature catalogue '{file2}'.");
                    reportItems = schemaParser.Validate(file1, file2);
                }

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
