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
        protected MappingConfig config;
        protected Processor processor;
        protected DocumentBuilder builder;
        protected XPathCompiler compiler;

        protected BaseConverter(string configFileName)
        {
            this.config = MappingConfig.LoadFromFileWithParents(configFileName);
            this.processor = new Processor();
            this.builder = this.processor.NewDocumentBuilder();
            this.compiler = this.processor.NewXPathCompiler();

            foreach (var theNs in this.config.Namespace)
                this.compiler.DeclareNamespace(theNs.Prefix, theNs.Uri);
        }

        protected abstract int InsertData(string tableName, Dictionary<string, object> columns);

        protected void Log(string message)
        {
            this.LogEvent?.Invoke(message);
        }

        protected virtual void ProcessGroup(MappingGroup groupConfig, XmlNode parentNode, XmlNamespaceManager nsManager, int parentId, string parentName)
        {
            var groupNodes = parentNode.SelectNodes(groupConfig.Context, nsManager);

            if (groupNodes.Count == 0)
                return;

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
            Dictionary<string, object> headerColumnData = new Dictionary<string, object>();
            headerColumnData["FILENAME"] = fileInfo.Name;

            // Read the header columns
            foreach (var colConfig in this.config.Column)
            {
                string xpath = colConfig.Value;
                string cellValue = this.GetValue(xpath, xmlDoc.DocumentElement, nsManager, colConfig.IsNarrative);
                headerColumnData.Add(colConfig.Name.ToUpper(), cellValue);
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

                    this.Log("---------------------------------------------\r\nReading XML file: " + fileInfo.Name + "\r\n");

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

                    this.ProcessFile(xmlDoc, nsManager, fileInfo);
                }
            }
            catch (Exception ex)
            {
                this.Log("Failed to process data due to: " + ex.Message);
            }
            finally
            {
                this.FinalizeOutput();
                this.ConversionComplete?.Invoke();
            }
        }
    }
}
