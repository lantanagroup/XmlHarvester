using ADOX;
using Saxon.Api;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Xml;

namespace LantanaGroup.XmlDocumentConverter
{
    public class MSAccessConverter : BaseConverter
    {
        private const string DatabaseFileName = "output.mdb";

        private string outputDirectory;
        private OleDbConnection conn;

        public MSAccessConverter(string configFileName, string inputDirectory, string outputDirectory) : base(configFileName)
        {
            this.inputDirectory = inputDirectory;
            this.outputDirectory = outputDirectory;
        }

        private string GetConnectionString(bool delete = false)
        {
            string fileName = MappingConfig.GetOutputFileNameWithoutExtension() + ".mdb";
            string filePath = System.IO.Path.Combine(this.outputDirectory, fileName);

            if (delete && File.Exists(filePath))
                File.Delete(filePath);

            string connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Engine Type=5", filePath);
            return connectionString;
        }

        private ADOX.Table CreateTable(ADOX.CatalogClass cat, ADOX.Table parentTable, string tableName, List<MappingColumn> columns)
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

            if (tableName == this.config.TableName)
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
                newTable.Keys.Append(tableName + "FKey", KeyTypeEnum.adKeyForeign, parentTable.Name + "Id", parentTable.Name, "id");
            }

            List<string> columnNames = new List<string>();

            foreach (var groupColumnConfig in columns)
            {
                if (columnNames.Contains(groupColumnConfig.Name))
                    this.Log(string.Format("Column {0} is a duplicated (occurs more than once)\r\n", groupColumnConfig.Name));
                else
                    columnNames.Add(groupColumnConfig.Name);

                if (groupColumnConfig.Name.ToLower() == "id")
                    this.Log("Column name \"id\" in table " + tableName + " is reserved for used. Please rename the column in the config.\r\n");

                if (parentTable != null && groupColumnConfig.Name.ToLower() == parentTable.Name.ToLower() + "id")
                    this.Log("Column name \"" + parentTable.Name + "Id\" is reserved for use. Please rename the column in the config.\r\n");

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

        private ADOX.Table CreateGroupTable(ADOX.CatalogClass cat, ADOX.Table parentTable, MappingGroup group)
        {
            var newTable = this.CreateTable(cat, parentTable, group.TableName, group.Column);

            foreach (var childGroup in group.Group)
            {
                this.CreateGroupTable(cat, newTable, childGroup);
            }

            return newTable;
        }

        private void CreateDatabase()
        {
            ADOX.CatalogClass cat = new ADOX.CatalogClass();

            cat.Create(this.GetConnectionString(true));

            var recordTable = this.CreateTable(cat, null, this.config.TableName, this.config.Column);

            foreach (var groupConfig in this.config.Group)
            {
                try
                {
                    this.CreateGroupTable(cat, recordTable, groupConfig);
                }
                catch (Exception ex)
                {
                    this.Log(ex.Message + "\r\n");
                }
            }
        }

        protected override int InsertData(string tableName, Dictionary<string, object> columns)
        {
            var columnsNames = columns.Keys;
            string insertQuery = "INSERT INTO [" + tableName + "] ([" + string.Join("], [", columnsNames) + "]) VALUES (";

            List<string> values = new List<string>();

            foreach (var value in columns.Values)
            {
                if (value == null)
                    values.Add("null");
                else if (value.GetType() == typeof(string))
                    values.Add("'" + value.ToString().Replace("'", "''") + "'");
                else
                    values.Add(value.ToString());
            }
            
            insertQuery += string.Join(", ", values) + ")";

            try
            {
                OleDbCommand insertCommand = new OleDbCommand();
                insertCommand.Connection = this.conn;
                insertCommand.CommandText = insertQuery;
                insertCommand.ExecuteNonQuery();

                OleDbCommand getIdCommand = new OleDbCommand();
                getIdCommand.Connection = this.conn;
                getIdCommand.CommandText = "SELECT @@Identity";
                int res = (int)getIdCommand.ExecuteScalar();
                return res;
            }
            catch (Exception ex)
            {
                this.Log(String.Format("Error inserting data into {0}: {1}", tableName, ex.Message));
                return -1;
            }
        }

        protected override bool InitializeOutput()
        {
            try
            {
                this.CreateDatabase();
            }
            catch (Exception ex)
            {
                this.Log(string.Format("Failed to create database and cannot proceed due to: " + ex.Message));
                return false;
            }

            try
            {
                this.conn = new OleDbConnection(this.GetConnectionString());
                this.conn.Open();
            } 
            catch (Exception ex)
            {
                this.Log(string.Format("Failed to open created database and cannot proceed due to: " + ex.Message));
                return false;
            }

            return true;
        }

        protected override void FinalizeOutput()
        {
            this.conn.Close();
        }
    }
}
