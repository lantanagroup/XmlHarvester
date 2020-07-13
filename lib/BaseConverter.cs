using Saxon.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace LantanaGroup.XmlDocumentConverter
{
    public abstract class BaseConverter
    {
        public delegate void ConversionCompleteEventHandler();
        public event ConversionCompleteEventHandler ConversionComplete;

        public delegate void LogEventHandler(string logText);
        public event LogEventHandler LogEvent;

        protected string inputDirectory;
        protected string moveDirectory;
        protected MappingConfig config;
        protected Processor processor;
        protected DocumentBuilder builder;
        protected XPathCompiler compiler;

        private Validator validator;

        protected BaseConverter(string configFileName, string inputDirectory, string moveDirectory, string schemaPath, string schematronPath)
        {
            this.config = MappingConfig.LoadFromFileWithParents(configFileName);
            this.inputDirectory = inputDirectory;
            this.moveDirectory = moveDirectory;
            this.processor = new Processor();
            this.builder = this.processor.NewDocumentBuilder();
            this.compiler = this.processor.NewXPathCompiler();

            foreach (var theNs in this.config.Namespace)
                this.compiler.DeclareNamespace(theNs.Prefix, theNs.Uri);

            this.validator = new Validator(schemaPath, schematronPath);
        }

        protected abstract int InsertData(string tableName, Dictionary<MappingColumn, object> columns);

        protected void Log(string message)
        {
            this.LogEvent?.Invoke(message);
        }

        protected virtual void ProcessGroup(MappingGroup groupConfig, XmlNode parentNode, XmlNamespaceManager nsManager, int parentId, string parentName)
        {
            XmlNodeList groupNodes = null;

            try
            {
                groupNodes = parentNode.SelectNodes(groupConfig.Context, nsManager);
            }
            catch (Exception ex)
            {
                this.Log(string.Format("XPATH error when selecting context for the group {0}: {1}", groupConfig.TableName, ex.Message));
                return;
            }

            if (groupNodes.Count == 0)
                return;

            foreach (XmlElement groupNode in groupNodes)
            {
                Dictionary<MappingColumn, object> groupColumnData = new Dictionary<MappingColumn, object>();

                var parentCol = new MappingColumn()
                {
                    Name = parentName + "Id",
                    Heading = parentName + "Id"
                };
                groupColumnData.Add(parentCol, parentId);

                foreach (var colConfig in groupConfig.Column)
                {
                    string xpath = colConfig.Value;
                    string cellValue = this.GetValue(xpath, groupNode, nsManager, colConfig.IsNarrative);
                    groupColumnData.Add(colConfig, cellValue);
                }

                int nextId = this.InsertData(groupConfig.TableName, groupColumnData);

                foreach (var childGroup in groupConfig.Group)
                {
                    this.ProcessGroup(childGroup, groupNode, nsManager, nextId, groupConfig.TableName);
                }
            }
        }

        protected string GetValue(string xpath, XmlNode parent, XmlNamespaceManager nsManager, bool isNarrative)
        {
            try
            {
                if (!xpath.StartsWith("/") && parent is XmlElement)
                {
                    xpath = "/*/" + xpath;
                }

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

        protected abstract bool InitializeOutput();

        protected abstract void FinalizeOutput();

        protected virtual void ProcessFile(XmlDocument xmlDoc, XmlNamespaceManager nsManager, FileInfo fileInfo)
        {
            Dictionary<MappingColumn, object> headerColumnData = new Dictionary<MappingColumn, object>();

            MappingColumn headerCol = new MappingColumn()
            {
                Name = "FILENAME",
                Heading = "FILENAME"
            };

            headerColumnData.Add(headerCol, fileInfo.Name);

            // Read the header columns
            foreach (var colConfig in this.config.Column)
            {
                string xpath = colConfig.Value;
                string cellValue = this.GetValue(xpath, xmlDoc.DocumentElement, nsManager, colConfig.IsNarrative);
                headerColumnData.Add(colConfig, cellValue);
            }

            int recordId = this.InsertData(this.config.TableName, headerColumnData);

            if (recordId < 0)
                return;

            foreach (var groupConfig in this.config.Group)
            {
                this.ProcessGroup(groupConfig, xmlDoc, nsManager, recordId, this.config.TableName);
            }
        }

        public void Convert()
        {
            if (string.IsNullOrEmpty(this.inputDirectory))
            {
                this.Log("No input directory has been specified!");
                return;
            }

            if (!this.InitializeOutput())
            {
                this.ConversionComplete?.Invoke();
                return;
            }

            try
            {
                string[] xmlFiles = Directory.GetFiles(this.inputDirectory, "*.xml");

                foreach (var xmlFile in xmlFiles)
                {
                    FileInfo fileInfo = new FileInfo(xmlFile);
                    XmlDocument xmlDoc = null;

                    this.Log("---------------------------------------------\r\nReading XML file: " + fileInfo.Name);

                    try
                    {
                        xmlDoc = new XmlDocument();
                        xmlDoc.Load(xmlFile);
                    }
                    catch (Exception ex)
                    {
                        this.Log(String.Format("The file {0} is not properly formatted and can't be parsed: {1}", fileInfo.Name, ex.Message));
                        continue;
                    }

                    XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);

                    foreach (var configNamespace in this.config.Namespace)
                    {
                        nsManager.AddNamespace(configNamespace.Prefix, configNamespace.Uri);
                    }

                    try
                    {
                        this.ProcessFile(xmlDoc, nsManager, fileInfo);
                    }
                    catch (Exception ex)
                    {
                        this.Log(String.Format("Failed to process file {0} data due to: {1}", fileInfo.Name, ex.Message));
                        break;
                    }

                    try
                    {
                        // Always run Validate(). If not configured with validation schema and/or schematron, it will just return "valid"
                        bool isSchemaValid = this.validator.ValidateSchema(xmlFile);
                        bool isSchematronValid = this.validator.ValidateSchematron(xmlFile);

                        if (this.validator.WillValidate)
                        {
                            this.Log("Validation results:");

                            if (this.validator.WillValidateSchema)
                                this.Log(string.Format("Schema (XSD): {0}", isSchemaValid ? "valid" : "not valid"));
                            else
                                this.Log("Schema (XSD): n/a");

                            if (this.validator.WillValidateSchematron)
                                this.Log(string.Format("Schematron (SCH): {0}", isSchematronValid ? "valid" : "not valid"));
                            else
                                this.Log("Schematron (SCH): n/a");
                        }

                        if (!String.IsNullOrEmpty(this.moveDirectory))
                        {
                            string destinationFilePath = Path.Combine(this.moveDirectory, fileInfo.Name);

                            // If configured to validate, move the file to a subdirectory "valid" or "invalid" depending on the validation results
                            if (this.validator.WillValidate)
                                destinationFilePath = Path.Combine(destinationFilePath, isSchemaValid ? "valid" : "invalid");

                            fileInfo.MoveTo(destinationFilePath);
                        }

                        this.Log(string.Format("Done processing file {0}", fileInfo.Name));
                    }
                    catch (Exception ex)
                    {
                        this.Log(String.Format("Failed to validate and/or move file {0} data due to: {1}", fileInfo.Name, ex.Message));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log("Failed to process files data due to: " + ex.Message);
            }
            finally
            {
                this.FinalizeOutput();
                this.ConversionComplete?.Invoke();
            }
        }
    }
}
