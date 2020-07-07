using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LantanaGroup.XmlDocumentConverter
{
    [Verb("db2", HelpText = "Store data from XML files in a DB2 database")]
    internal class DB2Options
    {
        [Option('c', "config", Required = true, HelpText = "The location of the mapping config XML file.")]
        public string MappingConfig { get; set; }

        [Option('i', "input", Required = true, HelpText = "The directory that contains the input XML files.")]
        public string InputDirectory { get; set; }

        [Option('m', "move", Required = false, HelpText = "The directory to move input files to once they are done being processed.")]
        public string MoveDirectory { get; set; }

        [Option('d', "database", Required = false, HelpText = "The name of the database to convert/output to.", Default = "xdc")]
        public string Database { get; set; }

        [Option('u', "username", Required = true, HelpText = "The authenticated username to access the DB.")]
        public string Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "The authenticated password to access the DB.")]
        public string Password { get; set; }
    }
}
