using S100Lint.Model;
using S100Lint.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using S100Lint.Base;

namespace S100Lint
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("S100Lint v0.21 (c) 2020, Royal Netherlands Hydrographic Service");

            if (args.Length < 2)
            {
                Console.WriteLine(String.Format("Invalid syntax for S100Lint. Valid syntax is S100Lint [SchemaFileName] [SchemaFileName | FeatureCatalogueFileName] [options]\n\n" +
                    "Options:\n" +
                    "--fc: Checks if all the types included in the feature catalogue are defined in the XML schema.\n" +
                    "--s:  Do a strict validation."));
            }
            else 
            {
                string file1 = args[0]; // source file name
                string file2 = args[1]; // target file name

                // retrieve specified command line options if any
                SystemConfig.Options = 
                    (from string arg in args
                    where arg.Contains("--")
                    select arg).ToList();

                // start analysing
                List<IReportItem> reportItems;
                var schemaParser = new SchemaAnalyser();

                try
                {
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

                    Console.WriteLine("General Information:");
                    foreach (var reportItem in reportItems)
                    {
                        if (reportItem.Type == Types.Enumerations.Type.Info)
                        {
                            Console.WriteLine(reportItem.Message);
                        }
                    }

                    Console.WriteLine("\nIssues:");
                    int issueNumber = 1;
                    foreach (var reportItem in reportItems)
                    {
                        if (reportItem.Type != Types.Enumerations.Type.Info)
                        {
                            if (reportItem.Chapter != 0)
                            {
                                Console.WriteLine();
                                Console.WriteLine(reportItem.Chapter);
                                Console.WriteLine(new String('-', reportItem.Chapter.ToString().Length));
                            }
                            else
                            {
                                Console.WriteLine($"({issueNumber++}) {reportItem.Level} - {reportItem.Message}");
                            }
                        }
                    }
                }
                catch(FileNotFoundException ex)
                {
                    Console.WriteLine($"File not found! ({ex.Message}");
                }
            }
        }
    }
}
