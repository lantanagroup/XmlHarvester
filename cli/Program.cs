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
            var result = Parser.Default.ParseArguments<XLSXOptions, MDBOptions, DB2Options>(args)
                .WithParsed<XLSXOptions>(o =>
                {
                    XlsxConverter xlsxConverter = new XlsxConverter(o.MappingConfig, o.InputDirectory, o.OutputDirectory, o.MoveDirectory, o.SchemaPath, o.SchematronPath);
                    xlsxConverter.LogEvent += delegate (string logText)
                    {
                        Console.WriteLine(logText);
                    };
                    xlsxConverter.Convert();
                })
                .WithParsed<MDBOptions>(o =>
                {
                    MSAccessConverter mdbConverter = new MSAccessConverter(o.MappingConfig, o.InputDirectory, o.OutputDirectory, o.MoveDirectory, o.SchemaPath, o.SchematronPath);
                    mdbConverter.LogEvent += delegate (string logText)
                    {
                        Console.WriteLine(logText);
                    };
                    mdbConverter.Convert();
                })
                .WithParsed<DB2Options>(o =>
                {
                    DB2Converter db2Converter = new DB2Converter(o.MappingConfig, o.InputDirectory, o.Database, o.Username, o.Password, o.MoveDirectory, o.SchemaPath, o.SchematronPath);
                    db2Converter.LogEvent += delegate (string logText)
                    {
                        Console.WriteLine(logText);
                    };
                    db2Converter.Convert();
                });
        }
    }
}
