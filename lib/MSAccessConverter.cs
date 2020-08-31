using ADOX;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;

namespace LantanaGroup.XmlHarvester
{
    public class MSAccessConverter : BaseConverter
    {
        private const string DatabaseFileName = "output.mdb";

        private string outputDirectory;
        private OleDbConnection conn;

        public MSAccessConverter(string configFileName, string inputDirectory, string outputDirectory, string moveDirectory, string schemaPath, string schematronPath) :
            base(configFileName, inputDirectory, moveDirectory, schemaPath, schematronPath)
        {
            this.outputDirectory = outputDirectory;
        }

        private string GetConnectionString(bool delete = false)
        {
            string fileName = MappingConfig.GetOutputFileNameWithoutExtension() + ".mdb";
            string filePath = System.IO.Path.Combine(outputDirectory, fileName);

            if (delete && File.Exists(filePath))
                File.Delete(filePath);

            string connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Engine Type=5", filePath);
            return connectionString;
        }

        private ADOX.Table CreateTable(ADOX.CatalogClass cat, ADOX.Table parentTable, string tableName, List<MappingColumn> columns, bool createIndex)
        {
            var newTable = new ADOX.Table();
            newTable.Name = tableName;

            var idCol = new ADOX.Column();
            idCol.Name = "id";
            idCol.Type = DataTypeEnum.adInteger;
            idCol.ParentCatalog = cat;
            idCol.Properties["AutoIncrement"].Value = true;
            newTable.Columns.Append(idCol);

            newTable.Keys.Append(tableName + "PK", KeyTypeEnum.adKeyPrimary, "id");

            if (tableName == config.TableName)
            {
                var fileNameCol = new ADOX.Column();
                fileNameCol.Name = "fileName";
                fileNameCol.Type = DataTypeEnum.adVarWChar;
                fileNameCol.ParentCatalog = cat;
                newTable.Columns.Append(fileNameCol);
            }

            if (parentTable != null)
            {
                newTable.Columns.Append(parentTable.Name + "Id", DataTypeEnum.adInteger);

                if (createIndex)
                    newTable.Keys.Append(tableName + "FKey", KeyTypeEnum.adKeyForeign, parentTable.Name + "Id", parentTable.Name, "id");
            }

            List<string> columnNames = new List<string>();

            foreach (var groupColumnConfig in columns)
            {
                if (columnNames.Contains(groupColumnConfig.Name))
                    Log(string.Format("Column {0} is a duplicated (occurs more than once)\r\n", groupColumnConfig.Name));
                else
                    columnNames.Add(groupColumnConfig.Name);

                if (groupColumnConfig.Name.ToLower() == "id")
                    Log("Column name \"id\" in table " + tableName + " is reserved for used. Please rename the column in the config.\r\n");

                if (parentTable != null && groupColumnConfig.Name.ToLower() == parentTable.Name.ToLower() + "id")
                    Log("Column name \"" + parentTable.Name + "Id\" is reserved for use. Please rename the column in the config.\r\n");

                var newCol = new ADOX.Column();
                newCol.Name = groupColumnConfig.Name;
                newCol.Type = DataTypeEnum.adVarWChar;

                if (groupColumnConfig.IsNarrative)
                    newCol.Type = DataTypeEnum.adLongVarWChar;

                newCol.ParentCatalog = cat;
                newCol.Attributes = ColumnAttributesEnum.adColNullable;
                newTable.Columns.Append(newCol);
            }

            cat.Tables.Append(newTable);

            return newTable;
        }

        private ADOX.Table CreateGroupTable(ADOX.CatalogClass cat, ADOX.Table parentTable, MappingGroup group, bool createIndex)
        {
            var newTable = CreateTable(cat, parentTable, group.TableName, group.Column, createIndex);

            foreach (var childGroup in group.Group)
            {
                int index = group.Group.IndexOf(childGroup);            // MDB has maximum of 32 indexes per table
                CreateGroupTable(cat, newTable, childGroup, index < 31);
            }

            return newTable;
        }

        private void CreateDatabase()
        {
            ADOX.CatalogClass cat = new ADOX.CatalogClass();

            cat.Create(GetConnectionString(true));

            var recordTable = CreateTable(cat, null, config.TableName, config.Column, false);

            foreach (var groupConfig in config.Group)
            {
                int index = config.Group.IndexOf(groupConfig);     // MDB has maximum of 32 indexes per table

                try
                {
                    CreateGroupTable(cat, recordTable, groupConfig, index < 31);
                }
                catch (Exception ex)
                {
                    Log(ex.Message + "\r\n");
                }
            }
        }

        protected override int InsertData(string tableName, Dictionary<MappingColumn, object> columns)
        {
            var columnsNames = columns.Keys.Select(y => y.Name);
            string insertQuery = "INSERT INTO [" + tableName + "] ([" + string.Join("], [", columnsNames) + "]) VALUES (";

            List<string> values = new List<string>();

            foreach (var key in columns.Keys)
            {
                var value = columns[key];

                if (value == null)
                {
                    values.Add("null");
                }
                else if (value.GetType() == typeof(string))
                {
                    string stringValue = ((string)value).Replace("'", "''");

                    if (!key.IsNarrative && stringValue.Length > 255)
                    {
                        stringValue = stringValue.Substring(0, 254);
                        Log(String.Format("Value for column {0} is more than 255 characters and will be truncated. Consider using isNarrative=true on the column definition.", key.Name, key.Heading));
                    }

                    values.Add("'" + stringValue + "'");
                }
                else
                {
                    values.Add(value.ToString());
                }
            }

            insertQuery += string.Join(", ", values) + ")";

            try
            {
                OleDbCommand insertCommand = new OleDbCommand();
                insertCommand.Connection = conn;
                insertCommand.CommandText = insertQuery;
                insertCommand.ExecuteNonQuery();

                OleDbCommand getIdCommand = new OleDbCommand();
                getIdCommand.Connection = conn;
                getIdCommand.CommandText = "SELECT @@Identity";
                int res = (int)getIdCommand.ExecuteScalar();
                return res;
            }
            catch (Exception ex)
            {
                Log(String.Format("Error inserting data into {0}: {1}", tableName, ex.Message));
                return -1;
            }
        }

        protected override bool InitializeOutput()
        {
            try
            {
                CreateDatabase();
            }
            catch (Exception ex)
            {
                Log(string.Format("Failed to create database and cannot proceed due to: " + ex.Message));
                return false;
            }

            try
            {
                conn = new OleDbConnection(GetConnectionString());
                conn.Open();
            }
            catch (Exception ex)
            {
                Log(string.Format("Failed to open created database and cannot proceed due to: " + ex.Message));
                return false;
            }

            return true;
        }

        protected override void FinalizeOutput()
        {
            conn.Close();
        }
    }
}
