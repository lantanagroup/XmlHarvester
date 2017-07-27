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

namespace VIQRCXML2XLS
{
    public class XlsxConverter
    {
        private XlsxConfig xlsxConfig;
        private string outputDirectory;
        private string inputDirectory;
        private TextBox logText;

        public const string XlsxConfigFileName = "XlsxConfig.xml";

        public XlsxConverter(string inputDirectory, string outputDirectory, TextBox logText)
        {
            this.xlsxConfig = XlsxConfig.LoadFromFile(GetConfigFileName());
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

        public void Convert()
        {
            string fileName = DateTime.Now.ToShortDateString().Replace("/", "-") + " " + DateTime.Now.ToShortTimeString().Replace(":", "-") + ".xlsx";
            string filePath = System.IO.Path.Combine(this.outputDirectory, fileName);

            string[] xmlFiles = Directory.GetFiles(this.inputDirectory, "*.xml");

            try
            {

                using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    // create the workbook
                    spreadSheet.AddWorkbookPart();
                    spreadSheet.WorkbookPart.Workbook = new Workbook();     // create the worksheet

                    AddStyleSheet(spreadSheet);

                    spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
                    spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet();

                    // create sheet data
                    spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.AppendChild(new SheetData());

                    var headerRow = new Row();
                    spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.First().AppendChild(headerRow);

                    foreach (var columnConfig in this.xlsxConfig.Column)
                    {
                        headerRow.AppendChild(
                            new Cell()
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(columnConfig.Header),
                                StyleIndex = 1
                            });
                    }

                    foreach (var xmlFile in xmlFiles)
                    {
                        this.logText.Text += "\r\nReading XML file: " + xmlFile + "\r\n";

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(xmlFile);

                        XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                        nsManager.AddNamespace("cda", "urn:hl7-org:v3");
                        nsManager.AddNamespace("sdtc", "urn:hl7-org:sdtc");

                        var xmlRow = new Row();
                        spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.First().AppendChild(xmlRow);

                        foreach (var columnConfig in this.xlsxConfig.Column)
                        {
                            XmlNodeList nodes = null;
                            string xpath = columnConfig.Value;

                            try
                            {
                                nodes = !string.IsNullOrEmpty(xpath) ? xmlDoc.SelectNodes(xpath, nsManager) : null;
                            }
                            catch (Exception ex)
                            {
                                if (!columnConfig.ErrorShown)
                                {
                                    this.logText.Text += "XPATH/Configuration error for column \"" + columnConfig.Header + "\": " + ex.Message + "\r\n";
                                    columnConfig.ErrorShown = true;
                                }
                            }

                            if (nodes == null || nodes.Count == 0)
                            {
                                if (!string.IsNullOrEmpty(xpath))
                                    this.logText.Text += "No data found for XPATH \"" + xpath + "\r\n";

                                xmlRow.AppendChild(
                                    new Cell()
                                    {
                                        DataType = CellValues.String,
                                        CellValue = new CellValue(string.Empty)
                                    });
                                continue;
                            }

                            string cellValue = string.Empty;
                            bool boldCell = false;

                            for (var i = 0; i < nodes.Count; i++)
                            {
                                if (i > 0)
                                {
                                    cellValue += "\r\n\r\n";
                                    boldCell = true;
                                }

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

                            Cell cell = new Cell()
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(cellValue)
                            };
                            xmlRow.AppendChild(cell);

                            if (boldCell)
                            {
                                this.logText.Text += "Multiple values found for XPath \"" + xpath + "\"\r\n";
                                cell.StyleIndex = 1;
                            }
                        }
                    }

                    // save worksheet
                    spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.Save();

                    // create the worksheet to workbook relation
                    spreadSheet.WorkbookPart.Workbook.AppendChild(new Sheets());
                    spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
                    {
                        Id = spreadSheet.WorkbookPart.GetIdOfPart(spreadSheet.WorkbookPart.WorksheetParts.First()),
                        SheetId = 1,
                        Name = "Summary"
                    });

                    spreadSheet.WorkbookPart.Workbook.Save();

                    OpenXmlValidator validator = new OpenXmlValidator();
                    var validationResults = validator.Validate(spreadSheet);

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

        public static string GetConfigFileName()
        {
            FileInfo assemblyFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string fileLocation = System.IO.Path.Combine(assemblyFileInfo.DirectoryName, XlsxConfigFileName);
            return fileLocation;
        }
    }
}
