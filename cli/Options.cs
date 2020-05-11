using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LantanaGroup.XmlDocumentConverter
{
    internal enum Formats
    {
        MDB,
        XLSX
    }

    internal class Options
    {
        [Option('f', "format", Default = Formats.MDB, HelpText = "The output format to produce (MDB or XLSX)." )]
        public Formats Format { get; set; }

        [Option('c', "config", Required = true, HelpText = "The location of the mapping config XML file.")]
        public String MappingConfig { get; set; }

        [Option('i', "input", Required = true, HelpText = "The directory that contains the input XML files.")]
        public String InputDirectory { get; set; }

        [Option('o', "output", Required = true, HelpText = "The directory where output (XLSX and MDB) files should go.")]
        public String OutputDirectory { get; set; }
    }
}
