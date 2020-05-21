using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LantanaGroup.XmlDocumentConverter
{
    public partial class MappingConfig
    {
        private static void AssociateParents(IEnumerable<MappingGroup> groups, MappingGroup parent = null)
        {
            foreach (MappingGroup next in groups)
            {
                next.Parent = parent;

                AssociateParents(next.Group, next);
            }
        }

        private static void ValidateColumns(String groupName, List<MappingColumn> columns)
        {
            columns.ForEach(c =>
            {
                if (string.IsNullOrEmpty(c.Name))
                    throw new Exception(string.Format("{0} has a column without a name", groupName));

                if (columns.Where(n => n.Name.Trim().ToLower() == c.Name.Trim().ToLower()).Count() > 1)
                    throw new Exception(string.Format("{0}'s column {1} is a duplicate", groupName, c.Name));
            });
        }

        private static void ValidateGroup(MappingGroup group)
        {
            MappingConfig.ValidateColumns(group.TableName, group.Column);

            group.Group.ForEach(g => MappingConfig.ValidateGroup(g));
        }

        public static MappingConfig LoadFromFileWithParents(string fileName)
        {
            try
            {
                MappingConfig obj = LoadFromFile(fileName);
                AssociateParents(obj.Group);

                MappingConfig.ValidateColumns(obj.TableName, obj.Column);

                obj.Group.ForEach(g => MappingConfig.ValidateGroup(g));

                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading config file: " + ex.Message, ex);
            }
        }

        public static string GetOutputFileNameWithoutExtension()
        {
            return DateTime.Now.ToString("yyyy-MM-dd hhmm tt");
        }
    }
}
