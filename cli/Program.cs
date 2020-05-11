using CommandLine;
using LantanaGroup.XmlDocumentConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    switch (o.Format)
                    {
                        case Formats.MDB:
                            MSAccessConverter mdbConverter = new MSAccessConverter(o.MappingConfig, o.InputDirectory, o.OutputDirectory);
                            mdbConverter.LogEvent += delegate (string logText)
                            {
                                Console.WriteLine(logText);
                            };
                            mdbConverter.Convert();
                            break;
                        case Formats.XLSX:
                            XlsxConverter xlsxConverter = new XlsxConverter(o.MappingConfig, o.InputDirectory, o.OutputDirectory);
                            xlsxConverter.LogEvent += delegate (string logText)
                            {
                                Console.WriteLine(logText);
                            };
                            xlsxConverter.Convert();
                            break;
                    }

                    Console.WriteLine("Done. Press enter to continue...");
                    Console.ReadLine();
                });
        }
    }
}
