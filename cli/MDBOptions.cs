using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LantanaGroup.XmlDocumentConverter
{
    [Verb("mdb", HelpText = "Convert XML files into an MS Access DB")]
    internal class MDBOptions
    {
        [Option('c', "config", Required = true, HelpText = "The location of the mapping config XML file.")]
        public string MappingConfig { get; set; }

        [Option('i', "input", Required = true, HelpText = "The directory that contains the input XML files.")]
        public string InputDirectory { get; set; }

        [Option('o', "output", Required = true, HelpText = "The directory where output (XLSX and MDB) files should go.")]
        public string OutputDirectory { get; set; }

        [Option('m', "move", Required = false, HelpText = "The directory to move input files to once they are done being processed.")]
        public string MoveDirectory { get; set; }
    }
}
