using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LantanaGroup.XmlHarvester
{
    public class SqlServerConverter : BaseConverter
    {
        private string server;
        private string database;
        private string username;
        private string password;

        private DbConnection conn;

        public SqlServerConverter(string configFileName, string inputDirectory, 
            string server, string database, string username, string password, 
            string moveDirectory, string schemaPath, string schematronPath) : 
                base(configFileName, inputDirectory, moveDirectory, schemaPath, schematronPath)
        {
            this.server = server;
            this.database = database;
            this.username = username;
            this.password = password;
        }

        #region Schema Creation

        private void EnsureTable(DbConnection conn, string tableName, List<MappingColumn> columns, string parentTableName = null)
        {
            Log("Validating table " + tableName.ToUpper());

            DbCommand existsCmd = conn.CreateCommand();
            existsCmd.CommandText = string.Format("IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = '{0}') SELECT 1 ELSE SELECT 0", tableName.ToUpper());

            int existsResults = (int)existsCmd.ExecuteScalar();

            // Check the definition of the table compared to what's defined in config
            if (existsResults == 1)
            {
                DbCommand definitionCmd = conn.CreateCommand();
                definitionCmd.CommandText = string.Format("SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_OCTET_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}'", tableName.ToUpper());

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
                    int colLength = 0;
                    if (colType != "int")
                    {
                        colLength = reader.GetInt32(2);
                    }

                    actualCols.Add(new MappingColumn()
                    {
                        Name = colName,
                        IsNarrative = colLength == -1
                    });
                }
                reader.Close();

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

                //TODO Should we put SQL Server foreign key contraints on this column?
                if (!string.IsNullOrEmpty(parentTableName))
                    foreignKeyCol = string.Format("{0}ID INTEGER NOT NULL, ", parentTableName.ToUpper());

                foreach (var col in columns.OrderBy(y => y.Name))
                {
                    string dataType = col.IsNarrative ? "VARCHAR(MAX)" : "VARCHAR(255)";
                    columnDefinitions += string.Format("[{0}] {1}, ", col.Name.ToUpper(), dataType);
                }

                DbCommand createCmd = conn.CreateCommand();
                createCmd.CommandText = string.Format("CREATE TABLE {0} (ID int IDENTITY (1,1) NOT NULL, {1}{2} CONSTRAINT PK_{0} PRIMARY KEY CLUSTERED (ID))", tableName.ToUpper(), foreignKeyCol, columnDefinitions);

                createCmd.ExecuteNonQuery();
            }
        }

        private void EnsureGroup(DbConnection conn, MappingGroup group, string parentTableName)
        {
            EnsureTable(conn, group.TableName, group.Column, parentTableName);

            group.Group.ForEach(delegate (MappingGroup nextGroup)
            {
                EnsureGroup(conn, nextGroup, group.TableName);
            });
        }

        private void ValidateSchema(DbConnection conn)
        {
            List<MappingColumn> columns = new List<MappingColumn>(config.Column);
            columns.Insert(0, new MappingColumn()
            {
                Name = "FILENAME"
            });

            EnsureTable(conn, config.TableName, columns);

            config.Group.ForEach(delegate (MappingGroup nextGroup)
            {
                EnsureGroup(conn, nextGroup, config.TableName);
            });
        }

        #endregion

        protected override bool InitializeOutput()
        {
            string ConnStr = String.Format("Server={0};Database={1};User Id={2};Password={3}", server, database, username, password);
            try
            {
                conn = new SqlConnection(ConnStr);
                conn.Open();
            }
            catch (Exception ex)
            {
                Log(string.Format("Failed to open connected to the SQL Server database: {0}", ex.Message));
                return false;
            }

            try
            {
                ValidateSchema(conn);
            }
            catch (Exception ex)
            {
                Log(string.Format("Failed to validate database and cannot proceed due to: " + ex.Message));
                return false;
            }

            return true;
        }

        protected override int InsertData(string tableName, Dictionary<MappingColumn, object> columns)
        {
            throw new NotImplementedException();
        }

        protected override void FinalizeOutput()
        {
            conn.Close();
        }

    }
}
