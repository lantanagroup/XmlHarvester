using CommandLine;

namespace LantanaGroup.XmlHarvester
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

        [Option('x', "xsd", Required = false, HelpText = "The path to an XML Schema (XSD) that should be used to validate the structure of each XMl document processed.")]
        public string SchemaPath { get; set; }

        [Option('s', "sch", Required = false, HelpText = "The path to an ISO Schematron (SCH) file that should be used to validate the content of each XMl document processed.")]
        public string SchematronPath { get; set; }
    }
}
