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
        public const string AccessConfigFileName = "AccessConfig.xml";

        private TextBox logText;
        private string inputDirectory;
        private string outputDirectory;
        private AccessConfig accessConfig;

        public MSAccessConverter(string inputDirectory, string outputDirectory, TextBox logText)
        {
            this.inputDirectory = inputDirectory;
            this.outputDirectory = outputDirectory;
            this.logText = logText;
            this.accessConfig = AccessConfig.LoadFromFile(GetConfigFileName());
        }

        private string GetConnectionString(bool delete = false)
        {
            string fileLocation = System.IO.Path.Combine(this.outputDirectory, DatabaseFileName);

            if (delete && File.Exists(fileLocation))
                File.Delete(fileLocation);

            string connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OLEDB:Engine Type=5", fileLocation);
            return connectionString;
        }

        private ADOX.Table CreateTable(ADOX.CatalogClass cat, ADOX.Table parentTable, string tableName, List<AccessColumn> columns)
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

            if (parentTable != null)
            {
                newTable.Columns.Append(parentTable.Name + "Id", DataTypeEnum.adInteger);
                newTable.Keys.Append(tableName + "FKey", KeyTypeEnum.adKeyForeign, parentTable.Name + "Id", parentTable.Name, "id");
            }

            foreach (var groupColumnConfig in columns)
            {
                var newCol = new ADOX.Column();
                newCol.Name = groupColumnConfig.Name;
                newCol.Type = DataTypeEnum.adVarWChar;
                newCol.ParentCatalog = cat;
                newCol.Attributes = ColumnAttributesEnum.adColNullable;
                newTable.Columns.Append(newCol);
            }

            cat.Tables.Append(newTable);

            return newTable;
        }

        private ADOX.Table CreateGroupTable(ADOX.CatalogClass cat, ADOX.Table parentTable, AccessGroup group)
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

            var recordTable = this.CreateTable(cat, null, "record", this.accessConfig.Column);

            foreach (var groupConfig in this.accessConfig.Group)
            {
                this.CreateGroupTable(cat, recordTable, groupConfig);
            }
        }

        private int InsertData(OleDbConnection conn, string tableName, Dictionary<string, object> columns)
        {
            var columnsNames = columns.Keys;
            string insertQuery = "INSERT INTO " + tableName + "(" + string.Join(", ", columnsNames) + ") VALUES (";

            List<string> values = new List<string>();

            foreach (var value in columns.Values)
            {
                if (value == null)
                    values.Add("null");
                else if (value.GetType() == typeof(string))
                    values.Add("'" + value.ToString() + "'");
                else
                    values.Add(value.ToString());
            }
            
            insertQuery += string.Join(", ", values) + " )";

            Console.WriteLine(insertQuery);

            OleDbCommand insertCommand = new OleDbCommand();
            insertCommand.Connection = conn;
            insertCommand.CommandText = insertQuery;
            insertCommand.ExecuteNonQuery();
            
            OleDbCommand getIdCommand = new OleDbCommand();
            getIdCommand.Connection = conn;
            getIdCommand.CommandText = "SELECT @@Identity";
            int res = (int) getIdCommand.ExecuteScalar();
            return res;
        }

        private string GetValue(XmlNodeList nodes)
        {
            string cellValue = string.Empty;

            for (var i = 0; i < nodes.Count; i++)
            {
                string nodeValue = nodes[i].Value;

                if (string.IsNullOrEmpty(nodeValue))
                    nodeValue = nodes[i].InnerText;

                if (string.IsNullOrEmpty(nodeValue) && nodes[i].Attributes["value"] != null)
                    nodeValue = nodes[i].Attributes["value"].Value;

                if (string.IsNullOrEmpty(nodeValue) && nodes[i].Attributes["displayName"] != null)
                    nodeValue = nodes[i].Attributes["displayName"].Value;

                if (string.IsNullOrEmpty(nodeValue) && nodes[i].Attributes["code"] != null)
                    nodeValue = nodes[i].Attributes["code"].Value;

                cellValue += nodeValue;
            }

            return cellValue;
        }

        private void ProcessGroup(OleDbConnection conn, AccessGroup groupConfig, XmlNode parentNode, XmlNamespaceManager nsManager, int parentId, string parentName)
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

                foreach (var columnConfig in groupConfig.Column)
                {
                    XmlNodeList nodes = null;
                    string xpath = columnConfig.Value;

                    try
                    {
                        nodes = !string.IsNullOrEmpty(xpath) ? groupNode.SelectNodes(xpath, nsManager) : null;
                    }
                    catch (Exception ex)
                    {
                        this.logText.Text += "XPATH/Configuration error for column \"" + columnConfig.Name + "\": " + ex.Message + "\r\n";
                        continue;
                    }

                    if (nodes == null || nodes.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(xpath))
                            this.logText.Text += "No data found for XPATH \"" + xpath + "\r\n";
                        continue;
                    }

                    string cellValue = this.GetValue(nodes);
                    groupColumnData.Add(columnConfig.Name, cellValue);
                }

                int nextId = this.InsertData(conn, groupConfig.TableName, groupColumnData);

                foreach (var childGroup in groupConfig.Group)
                {
                    this.ProcessGroup(conn, childGroup, groupNode, nsManager, nextId, groupConfig.TableName);
                }
            }
        }

        public void Convert()
        {
            this.CreateDatabase();

            OleDbConnection dbConnection = new OleDbConnection(this.GetConnectionString());
            dbConnection.Open();

            string[] xmlFiles = Directory.GetFiles(this.inputDirectory, "*.xml");

            foreach (var xmlFile in xmlFiles)
            {
                this.logText.Text += "\r\nReading XML file: " + xmlFile + "\r\n";

                int recordId;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);

                XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                nsManager.AddNamespace("cda", "urn:hl7-org:v3");
                nsManager.AddNamespace("sdtc", "urn:hl7-org:sdtc");

                Dictionary<string, object> headerColumnData = new Dictionary<string, object>();

                // Read the header columns
                foreach (var headerColConfig in this.accessConfig.Column)
                {
                    XmlNodeList nodes = null;
                    string xpath = headerColConfig.Value;

                    try
                    {
                        nodes = !string.IsNullOrEmpty(xpath) ? xmlDoc.SelectNodes(xpath, nsManager) : null;
                    }
                    catch (Exception ex)
                    {
                        this.logText.Text += "XPATH/Configuration error for column \"" + headerColConfig.Name + "\": " + ex.Message + "\r\n";
                        continue;
                    }

                    if (nodes == null || nodes.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(headerColConfig.Value))
                            this.logText.Text += "No data found for XPATH \"" + headerColConfig.Value + "\r\n";
                        continue;
                    }

                    string cellValue = this.GetValue(nodes);
                    headerColumnData.Add(headerColConfig.Name, cellValue);
                }

                recordId = this.InsertData(dbConnection, this.accessConfig.TableName, headerColumnData);

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
