using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADOX;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml;
using System.Data.OleDb;

namespace VIQRCXML2XLS
{
    public class MSAccessConverter
    {
        private const string DatabaseFileName = "output.mdb";
        public const string AccessConfigFileName = "MappingConfig.xml";

        private TextBox logText;
        private string inputDirectory;
        private string outputDirectory;
        private MappingConfig accessConfig;

        public MSAccessConverter(string inputDirectory, string outputDirectory, TextBox logText)
        {
            this.inputDirectory = inputDirectory;
            this.outputDirectory = outputDirectory;
            this.logText = logText;
            this.accessConfig = MappingConfig.LoadFromFileWithParents(GetConfigFileName());
        }

        private string GetConnectionString(bool delete = false)
        {
            string fileLocation = System.IO.Path.Combine(this.outputDirectory, DatabaseFileName);

            if (delete && File.Exists(fileLocation))
                File.Delete(fileLocation);

            string connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Engine Type=5", fileLocation);
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

            if (tableName == this.accessConfig.TableName)
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
                    this.logText.Text += string.Format("Column {0} is a duplicated (occurs more than once)\r\n", groupColumnConfig.Name);
                else
                    columnNames.Add(groupColumnConfig.Name);

                if (groupColumnConfig.Name.ToLower() == "id")
                    this.logText.Text += "Column name \"id\" in table " + tableName + " is reserved for used. Please rename the column in the config.\r\n";

                if (parentTable != null && groupColumnConfig.Name.ToLower() == parentTable.Name.ToLower() + "id")
                    this.logText.Text += "Column name \"" + parentTable.Name + "Id\" is reserved for use. Please rename the column in the config.\r\n";

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

            var recordTable = this.CreateTable(cat, null, this.accessConfig.TableName, this.accessConfig.Column);

            foreach (var groupConfig in this.accessConfig.Group)
            {
                try
                {
                    this.CreateGroupTable(cat, recordTable, groupConfig);
                }
                catch (Exception ex)
                {
                    this.logText.Text += ex.Message + "\r\n";
                }
            }
        }

        private int InsertData(OleDbConnection conn, string tableName, Dictionary<string, object> columns)
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
                this.logText.Text += "Error inserting data into database: " + ex.Message + "\r\n";
                return -1;
            }
        }

        private string GetValue(XmlNodeList nodes, bool isNarrative)
        {
            if (nodes == null)
                return string.Empty;

            string cellValue = string.Empty;

            for (var i = 0; i < nodes.Count; i++)
            {
                string nodeValue = string.Empty;

                if (isNarrative)
                {
                    var allNodes = nodes[i].SelectNodes(".//*/text()");

                    foreach (XmlNode nextNode in allNodes)
                    {
                        if (!string.IsNullOrEmpty(nodeValue))
                            nodeValue += " ";
                        nodeValue += nextNode.Value;
                    }
                }
                else
                {
                    nodeValue = nodes[i].Value;

                    if (string.IsNullOrEmpty(nodeValue) && nodes[i].Attributes["value"] != null)
                        nodeValue = nodes[i].Attributes["value"].Value;

                    if (string.IsNullOrEmpty(nodeValue) && nodes[i].Attributes["displayName"] != null)
                        nodeValue = nodes[i].Attributes["displayName"].Value;

                    if (string.IsNullOrEmpty(nodeValue) && nodes[i].Attributes["code"] != null)
                        nodeValue = nodes[i].Attributes["code"].Value;

                    if (string.IsNullOrEmpty(nodeValue))
                        nodeValue = nodes[i].InnerText;
                }

                if (!string.IsNullOrEmpty(nodeValue))
                {
                    if (i > 0)
                        cellValue += "\r\n";

                    cellValue += nodeValue;
                }
            }

            return cellValue;
        }

        private void ProcessGroup(OleDbConnection conn, MappingGroup groupConfig, XmlNode parentNode, XmlNamespaceManager nsManager, int parentId, string parentName)
        {
            var groupNodes = parentNode.SelectNodes(groupConfig.Context, nsManager);

            if (groupNodes.Count == 0)
            {
                this.logText.Text += "No data found for group XPATH \"" + groupConfig.Context + "\r\n";
                return;
            }

            foreach (XmlElement groupNode in groupNodes)
            {
                Dictionary<string, object> groupColumnData = new Dictionary<string, object>();

                groupColumnData.Add(parentName + "Id", parentId);

                foreach (var colConfig in groupConfig.Column)
                {
                    string xpath = colConfig.Value;
                    string cellValue = this.GetValue(xpath, groupNode, nsManager, colConfig.IsNarrative);
                    groupColumnData.Add(colConfig.Name, cellValue);
                }

                int nextId = this.InsertData(conn, groupConfig.TableName, groupColumnData);

                foreach (var childGroup in groupConfig.Group)
                {
                    this.ProcessGroup(conn, childGroup, groupNode, nsManager, nextId, groupConfig.TableName);
                }
            }
        }

        private string GetValue(string xpath, XmlNode parent, XmlNamespaceManager nsManager, bool isNarrative)
        {
            try
            {
                XmlNodeList nodes = !string.IsNullOrEmpty(xpath) ? parent.SelectNodes(xpath, nsManager) : null;
                return GetValue(nodes, isNarrative);
            }
            catch (Exception ex)
            {
                try
                {
                    var eval = parent.CreateNavigator().Evaluate(xpath, nsManager);

                    if (eval != null)
                        return eval.ToString();
                }
                catch (Exception exx)
                {
                    this.logText.Text += "XPATH/Configuration error \"" + xpath + "\": " + exx.Message + "\r\n";
                }
            }

            return null;
        }

        public void Convert()
        {
            this.CreateDatabase();

            OleDbConnection dbConnection = new OleDbConnection(this.GetConnectionString());
            dbConnection.Open();

            string[] xmlFiles = Directory.GetFiles(this.inputDirectory, "*.xml");

            foreach (var xmlFile in xmlFiles)
            {
                FileInfo fileInfo = new FileInfo(xmlFile);

                this.logText.Text += "\r\nReading XML file: " + fileInfo.Name + "\r\n";

                int recordId;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                nsManager.AddNamespace("cda", "urn:hl7-org:v3");
                nsManager.AddNamespace("sdtc", "urn:hl7-org:sdtc");

                Dictionary<string, object> headerColumnData = new Dictionary<string, object>();
                headerColumnData["fileName"] = fileInfo.Name;

                // Read the header columns
                foreach (var colConfig in this.accessConfig.Column)
                {
                    string xpath = colConfig.Value;
                    string cellValue = this.GetValue(xpath, xmlDoc.DocumentElement, nsManager, colConfig.IsNarrative);
                    headerColumnData.Add(colConfig.Name, cellValue);
                }

                recordId = this.InsertData(dbConnection, this.accessConfig.TableName, headerColumnData);

                if (recordId < 0)
                    continue;

                foreach (var groupConfig in this.accessConfig.Group)
                {
                    this.ProcessGroup(dbConnection, groupConfig, xmlDoc, nsManager, recordId, this.accessConfig.TableName);
                }
            }

            dbConnection.Close();
        }

        public static string GetConfigFileName()
        {
            FileInfo assemblyFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string fileLocation = System.IO.Path.Combine(assemblyFileInfo.DirectoryName, AccessConfigFileName);
            return fileLocation;
        }
    }
}
