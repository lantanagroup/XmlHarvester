using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XmlDocumentConverter
{
    public partial class MappingConfig
    {
        public const string ConfigFileName = "MappingConfig.xml";

        public static string GetConfigFileName()
        {
            FileInfo assemblyFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string fileLocation = System.IO.Path.Combine(assemblyFileInfo.DirectoryName, ConfigFileName);
            return fileLocation;
        }

        private static void AssociateParents(IEnumerable<MappingGroup> groups, MappingGroup parent = null)
        {
            foreach (MappingGroup next in groups)
            {
                next.Parent = parent;

                AssociateParents(next.Group, next);
            }
        }
        public static MappingConfig LoadFromFileWithParents(string fileName)
        {
            MappingConfig obj = LoadFromFile(fileName);
            AssociateParents(obj.Group);
            return obj;
        }
    }
}
