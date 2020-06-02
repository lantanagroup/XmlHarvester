using ADOX;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Xml;
using System.Data.Common;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Math;
using Saxon.Api;

namespace LantanaGroup.XmlDocumentConverter
{
    public class DB2Converter
    {
        public delegate void LogEventHandler(string logText);
        public event LogEventHandler LogEvent;
        public delegate void ConversionCompleteEventHandler();
        public event ConversionCompleteEventHandler ConversionComplete;

        private string inputDirectory;
        private string database;
        private string username;
        private string password;
        private string outputDirectory;
        private MappingConfig config;

        private Processor processor;
        private DocumentBuilder builder;
        private XPathCompiler compiler;

        public DB2Converter(string configFileName, string inputDirectory, string database, string username, string password, string outputDirectory)
        {
            this.inputDirectory = inputDirectory;
            this.database = database;
            this.username = username;
            this.password = password;
            this.outputDirectory = outputDirectory;
            this.config = MappingConfig.LoadFromFileWithParents(configFileName);

            this.processor = new Processor();
            this.builder = this.processor.NewDocumentBuilder();
            this.compiler = this.processor.NewXPathCompiler();

            foreach (var theNs in this.config.Namespace)
                this.compiler.DeclareNamespace(theNs.Prefix, theNs.Uri);
        }

        private int InsertData(DbConnection conn, string tableName, Dictionary<string, object> columns)
        {
            var columnsNames = columns.Keys;
            string insertQuery = "INSERT INTO " + tableName.ToUpper() + " (" + string.Join(", ", columnsNames) + ") VALUES (";

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
                DbCommand insertCommand = conn.CreateCommand();
                insertCommand.CommandText = insertQuery;
                insertCommand.ExecuteNonQuery();

                DbCommand getIdCommand = conn.CreateCommand();
                getIdCommand.CommandText = string.Format("SELECT SYSIBM.IDENTITY_VAL_LOCAL() AS ID FROM {0}", tableName.ToUpper());
                decimal ret = (decimal) getIdCommand.ExecuteScalar();
                int res = Decimal.ToInt32(ret);
                return res;
            }
            catch (Exception ex)
            {
                this.LogEvent?.Invoke("Error inserting data into database: " + ex.Message + "\r\n");
                return -1;
            }
        }

        private void ProcessGroup(DbConnection conn, MappingGroup groupConfig, XmlNode parentNode, XmlNamespaceManager nsManager, int parentId, string parentName)
        {
            var groupNodes = parentNode.SelectNodes(groupConfig.Context, nsManager);

            if (groupNodes.Count == 0)
            {
                this.LogEvent?.Invoke(string.Format("No data found for group {0} with XPATH \"{1}\"\r\n", groupConfig.TableName, groupConfig.Context));
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
                var parentXdmNode = this.builder.Build(parent);
                var compiledXpath = compiler.Compile(xpath);
                var selector = compiledXpath.Load();
                selector.ContextItem = parentXdmNode;
                var results = selector.Evaluate().GetList();

                if (results.Count == 1)
                {
                    return results[0].GetStringValue();
                }
                else if (results.Count > 1)
                {
                    String ret = "";

                    foreach (var next in results)
                    {
                        if (!string.IsNullOrEmpty(ret)) ret += ", ";
                        ret += next.GetStringValue();
                    }

                    return ret;
                }

                return null;
            }
            catch (Exception ex)
            {
                this.LogEvent?.Invoke("XPATH/Configuration error \"" + xpath + "\": " + ex.Message + "\r\n");
            }

            return null;
        }

        #region Schema Creation

        private void EnsureTable(DbConnection conn, string tableName, List<MappingColumn> columns, string parentTableName = null)
        {
            DbCommand existsCmd = conn.CreateCommand();
            existsCmd.CommandText = string.Format("SELECT COUNT(0) AS TOTAL FROM SYSIBM.SYSTABLES WHERE NAME = '{0}'", tableName.ToUpper());

            int existsResults = (int) existsCmd.ExecuteScalar();

            this.LogEvent?.Invoke("Validating table " + tableName.ToUpper());

            // Check the definition of the table compared to what's defined in config
            if (existsResults == 1)
            {
                DbCommand definitionCmd = conn.CreateCommand();
                definitionCmd.CommandText = string.Format("SELECT NAME, COLTYPE, LENGTH FROM SYSIBM.SYSCOLUMNS WHERE TBNAME = '{0}'", tableName.ToUpper());

                DbDataReader reader = definitionCmd.ExecuteReader();
                List<MappingColumn> actualCols = new List<MappingColumn>();
                List<MappingColumn> expectedCols = new List<MappingColumn>(columns);
                expectedCols.Add(new MappingColumn() { Name = "ID" });

                if (!string.IsNullOrEmpty(parentTableName))
                    expectedCols.Add(new MappingColumn() { Name = parentTableName.ToUpper() + "ID" });

                while (reader.Read())
                {
                    string colName = reader.GetString(0);
                    string colType = reader.GetString(1);
                    short colLength = reader.GetInt16(2);

                    actualCols.Add(new MappingColumn() {
                        Name = colName,
                        IsNarrative = colType.Trim() == "LONGVAR"
                    });
                }

                expectedCols.ForEach(delegate (MappingColumn expected)
                {
                    if (actualCols.Find(actual => actual.Name == expected.Name.ToUpper() && actual.IsNarrative == expected.IsNarrative) == null)
                        throw new Exception(string.Format("Could not find correct definition of column {0} in table {1}", expected.Name.ToUpper(), tableName));
                });
            }
            else if (existsResults == 0)
            {
                string columnDefinitions = string.Empty;
                string foreignKeyCol = string.Empty;

                if (!string.IsNullOrEmpty(parentTableName))
                    foreignKeyCol = string.Format("{0}ID INTEGER NOT NULL, ", parentTableName.ToUpper());

                foreach (var col in columns.OrderBy(y => y.Name))
                {
                    string dataType = col.IsNarrative ? "LONG VARCHAR" : "VARCHAR(255)";
                    columnDefinitions += string.Format("{0} {1}, ", col.Name.ToUpper(), dataType);
                }

                DbCommand createCmd = conn.CreateCommand();
                createCmd.CommandText = string.Format("CREATE TABLE {0} (ID INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL, {1}{2}PRIMARY KEY (ID))", tableName.ToUpper(), foreignKeyCol, columnDefinitions);

                createCmd.ExecuteNonQuery();
            }
        }

        private void EnsureGroup(DbConnection conn, MappingGroup group, string parentTableName)
        {
            this.EnsureTable(conn, group.TableName, group.Column, parentTableName);

            group.Group.ForEach(delegate (MappingGroup nextGroup)
            {
                this.EnsureGroup(conn, nextGroup, group.TableName);
            });
        }

        private void ValidateSchema(DbConnection conn)
        {
            List<MappingColumn> columns = new List<MappingColumn>(this.config.Column);
            columns.Insert(0, new MappingColumn()
            {
                Name = "FILENAME"
            });

            this.EnsureTable(conn, this.config.TableName, columns);

            this.config.Group.ForEach(delegate (MappingGroup nextGroup)
            {
                this.EnsureGroup(conn, nextGroup, this.config.TableName);
            });
        }

        #endregion

        public void Convert()
        {
            DbProviderFactory factory = DbProviderFactories.GetFactory("IBM.Data.DB2");
            DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = string.Format("Database={0};UID={1};PWD={2}", this.database, this.username, this.password);
            conn.Open();

            try
            {
                this.ValidateSchema(conn);
            }
            catch (Exception ex)
            {
                this.LogEvent?.Invoke(string.Format("Failed to validate database and cannot proceed due to: " + ex.Message));
                this.ConversionComplete?.Invoke();
                return;
            }

            try
            {
                string[] xmlFiles = Directory.GetFiles(this.inputDirectory, "*.xml");

                foreach (var xmlFile in xmlFiles)
                {
                    FileInfo fileInfo = new FileInfo(xmlFile);

                    this.LogEvent?.Invoke("\r\nReading XML file: " + fileInfo.Name + "\r\n");

                    int recordId;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlFile);

                    XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);

                    foreach (var configNamespace in this.config.Namespace)
                    {
                        nsManager.AddNamespace(configNamespace.Prefix, configNamespace.Uri);
                    }

                    Dictionary<string, object> headerColumnData = new Dictionary<string, object>();
                    headerColumnData["FILENAME"] = fileInfo.Name;

                    // Read the header columns
                    foreach (var colConfig in this.config.Column)
                    {
                        string xpath = colConfig.Value;
                        string cellValue = this.GetValue(xpath, xmlDoc.DocumentElement, nsManager, colConfig.IsNarrative);
                        headerColumnData.Add(colConfig.Name.ToUpper(), cellValue);
                    }

                    recordId = this.InsertData(conn, this.config.TableName, headerColumnData);

                    if (recordId < 0)
                        continue;

                    foreach (var groupConfig in this.config.Group)
                    {
                        this.ProcessGroup(conn, groupConfig, xmlDoc, nsManager, recordId, this.config.TableName);
                    }
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                this.LogEvent?.Invoke("Failed to process data due to: " + ex.Message);
            }
            finally
            {
                this.ConversionComplete?.Invoke();
            }
        }
    }
}
