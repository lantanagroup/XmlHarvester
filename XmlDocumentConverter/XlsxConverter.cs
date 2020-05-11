using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Validation;
using System.Windows;

namespace XmlDocumentConverter
{
    public class XlsxConverter
    {
        private MappingConfig xlsxConfig;
        private string outputDirectory;
        private string inputDirectory;
        private TextBox logText;

        public XlsxConverter(string configFileName, string inputDirectory, string outputDirectory, TextBox logText)
        {
            this.xlsxConfig = MappingConfig.LoadFromFileWithParents(configFileName);
            this.inputDirectory = inputDirectory;
            this.outputDirectory = outputDirectory;
            this.logText = logText;
        }

        private WorkbookStylesPart AddStyleSheet(SpreadsheetDocument spreadsheet)
        {
            WorkbookStylesPart stylesheet = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();

            Stylesheet workbookstylesheet = new Stylesheet();

            Font font0 = new Font();         // Default font

            Font font1 = new Font();         // Bold font
            DocumentFormat.OpenXml.Spreadsheet.Bold bold = new DocumentFormat.OpenXml.Spreadsheet.Bold();
            font1.Append(bold);

            DocumentFormat.OpenXml.Spreadsheet.Fonts fonts = new DocumentFormat.OpenXml.Spreadsheet.Fonts();      // <APENDING Fonts>
            fonts.Append(font0);
            fonts.Append(font1);

            // <Fills>
            Fill fill0 = new Fill();        // Default fill

            Fills fills = new Fills();      // <APENDING Fills>
            fills.Append(fill0);

            // <Borders>
            DocumentFormat.OpenXml.Spreadsheet.Border border0 = new DocumentFormat.OpenXml.Spreadsheet.Border();     // Defualt border

            Borders borders = new Borders();    // <APENDING Borders>
            borders.Append(border0);

            // <CellFormats>
            CellFormat cellformat0 = new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 }; // Default style : Mandatory | Style ID =0

            CellFormat cellformat1 = new CellFormat() { FontId = 1 };  // Style with Bold text ; Style ID = 1


            // <APENDING CellFormats>
            CellFormats cellformats = new CellFormats();
            cellformats.Append(cellformat0);
            cellformats.Append(cellformat1);


            // Append FONTS, FILLS , BORDERS & CellFormats to stylesheet <Preserve the ORDER>
            workbookstylesheet.Append(fonts);
            workbookstylesheet.Append(fills);
            workbookstylesheet.Append(borders);
            workbookstylesheet.Append(cellformats);

            // Finalize
            stylesheet.Stylesheet = workbookstylesheet;
            stylesheet.Stylesheet.Save();

            return stylesheet;
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

        private string GetValue(string xpath, XmlNode parent, XmlNamespaceManager nsManager, bool isNarrative)
        {
            try
            {
                XmlNodeList nodes = !string.IsNullOrEmpty(xpath) ? parent.SelectNodes(xpath, nsManager) : null;
                return GetValue(nodes, isNarrative);
            }
            catch
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

            return string.Empty;
        }

        private void ProcessGroup(MappingGroup groupConfig, ExcelFormat excelFormat, XmlNode node, XmlNamespaceManager nsManager)
        {
            string groupXpath = groupConfig.Context;

            try
            {
                XmlNodeList groupNodes = node.SelectNodes(groupXpath, nsManager);

                if (groupNodes.Count == 0)
                {
                    this.logText.Text += string.Format("No data found for group {0} with XPATH \"{1}\"\r\n", groupConfig.TableName, groupConfig.Context);
                    return;
                }

                foreach (XmlNode groupNode in groupNodes)
                {
                    foreach (var columnConfig in groupConfig.Column)
                    {
                        string xpath = columnConfig.Value;
                        string cellValue = GetValue(xpath, groupNode, nsManager, columnConfig.IsNarrative);
                        excelFormat.AddData(groupConfig, columnConfig, cellValue, cellValue.IndexOf("\r\n") >= 0);
                    }

                    foreach (MappingGroup childGroupConfig in groupConfig.Group)
                    {
                        this.ProcessGroup(childGroupConfig, excelFormat, groupNode, nsManager);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logText.Text += "Error executing group xpath for " + groupConfig.TableName + ": " + ex.Message + "\r\n";
            }
        }

        public void Convert()
        {
            string fileName = MappingConfig.GetOutputFileNameWithoutExtension() + ".xlsx";
            string filePath = System.IO.Path.Combine(this.outputDirectory, fileName);

            string[] xmlFiles = Directory.GetFiles(this.inputDirectory, "*.xml");
            ExcelFormat excelFormat = new ExcelFormat();

            try
            {

                using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    // create the workbook
                    spreadsheet.AddWorkbookPart();
                    spreadsheet.WorkbookPart.Workbook = new Workbook();     // create the worksheet

                    AddStyleSheet(spreadsheet);

                    spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                    spreadsheet.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet();

                    // create sheet data
                    spreadsheet.WorkbookPart.WorksheetParts.First().Worksheet.AppendChild(new SheetData());

                    foreach (var xmlFile in xmlFiles)
                    {
                        FileInfo fileInfo = new FileInfo(xmlFile);

                        this.logText.Text += "\r\nReading XML file: " + xmlFile + "\r\n";
                        excelFormat.AddRow(fileInfo.Name);

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(xmlFile);

                        XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                        
                        foreach (var configNamespace in this.xlsxConfig.Namespace)
                        {
                            nsManager.AddNamespace(configNamespace.Prefix, configNamespace.Uri);
                        }

                        foreach (var columnConfig in this.xlsxConfig.Column)
                        {
                            string xpath = columnConfig.Value;
                            string cellValue = GetValue(xpath, xmlDoc.DocumentElement, nsManager, columnConfig.IsNarrative);
                            excelFormat.AddData(null, columnConfig, cellValue, cellValue.IndexOf("\r\n") >= 0);
                        }

                        foreach (var groupConfig in this.xlsxConfig.Group)
                        {
                            this.ProcessGroup(groupConfig, excelFormat, xmlDoc.DocumentElement, nsManager);
                        }
                    }

                    excelFormat.PopulateSpreadsheet(this.xlsxConfig, spreadsheet);

                    // save worksheet
                    spreadsheet.WorkbookPart.WorksheetParts.First().Worksheet.Save();

                    // create the worksheet to workbook relation
                    spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
                    spreadsheet.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
                    {
                        Id = spreadsheet.WorkbookPart.GetIdOfPart(spreadsheet.WorkbookPart.WorksheetParts.First()),
                        SheetId = 1,
                        Name = "Summary"
                    });

                    spreadsheet.WorkbookPart.Workbook.Save();

                    OpenXmlValidator validator = new OpenXmlValidator();
                    var validationResults = validator.Validate(spreadsheet);

                    if (validationResults.Count() > 0)
                    {
                        this.logText.Text += "\r\n\r\nExcel validation errors:";

                        foreach (var validationError in validationResults)
                        {
                            this.logText.Text += "\r\n" + validationError.Description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
