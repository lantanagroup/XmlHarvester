using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Validation;
using Saxon.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace LantanaGroup.XmlDocumentConverter
{
    public class XlsxConverter : BaseConverter
    {
        private string outputDirectory;

        private SpreadsheetDocument spreadsheet;
        private ExcelFormat excelFormat;

        public XlsxConverter(string configFileName, string inputDirectory, string outputDirectory, string moveDirectory) : 
            base(configFileName, inputDirectory, moveDirectory)
        {
            this.outputDirectory = outputDirectory;
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

        private void ProcessGroup(MappingGroup groupConfig, ExcelFormat excelFormat, XmlNode node, XmlNamespaceManager nsManager)
        {
            string groupXpath = groupConfig.Context;

            try
            {
                XmlNodeList groupNodes = node.SelectNodes(groupXpath, nsManager);

                if (groupNodes.Count == 0)
                {
                    this.Log(string.Format("No data found for group {0} with XPATH \"{1}\"\r\n", groupConfig.TableName, groupConfig.Context));
                    return;
                }

                foreach (XmlNode groupNode in groupNodes)
                {
                    foreach (var columnConfig in groupConfig.Column)
                    {
                        string xpath = columnConfig.Value;
                        string cellValue = GetValue(xpath, groupNode, nsManager, columnConfig.IsNarrative);
                        bool isBold = !string.IsNullOrEmpty(cellValue) ? cellValue.IndexOf("\r\n") >= 0 : false;
                        excelFormat.AddData(groupConfig, columnConfig, cellValue, isBold);
                    }

                    foreach (MappingGroup childGroupConfig in groupConfig.Group)
                    {
                        this.ProcessGroup(childGroupConfig, excelFormat, groupNode, nsManager);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log("Error executing group xpath for " + groupConfig.TableName + ": " + ex.Message + "\r\n");
            }
        }

        protected override int InsertData(string tableName, Dictionary<MappingColumn, object> columns)
        {
            throw new NotImplementedException();
        }

        protected override void ProcessFile(XmlDocument xmlDoc, XmlNamespaceManager nsManager, FileInfo fileInfo)
        {
            this.excelFormat.AddRow(fileInfo.Name);

            foreach (var columnConfig in this.config.Column)
            {
                string xpath = columnConfig.Value;
                string cellValue = GetValue(xpath, xmlDoc.DocumentElement, nsManager, columnConfig.IsNarrative);
                bool isBold = !string.IsNullOrEmpty(cellValue) ? cellValue.IndexOf("\r\n") >= 0 : false;
                this.excelFormat.AddData(null, columnConfig, cellValue, isBold);
            }

            foreach (var groupConfig in this.config.Group)
            {
                this.ProcessGroup(groupConfig, excelFormat, xmlDoc.DocumentElement, nsManager);
            }
        }

        protected override bool InitializeOutput()
        {
            try
            {
                string fileName = MappingConfig.GetOutputFileNameWithoutExtension() + ".xlsx";
                string filePath = System.IO.Path.Combine(this.outputDirectory, fileName);

                this.excelFormat = new ExcelFormat();
                this.spreadsheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);

                // create the workbook
                spreadsheet.AddWorkbookPart();
                spreadsheet.WorkbookPart.Workbook = new Workbook();     // create the worksheet

                AddStyleSheet(spreadsheet);

                spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                spreadsheet.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet();

                // create sheet data
                spreadsheet.WorkbookPart.WorksheetParts.First().Worksheet.AppendChild(new SheetData());
            }
            catch (Exception ex)
            {
                this.Log(String.Format("Error initializing spreadsheet output: {0}", ex.Message));
                return false;
            }

            return true;
        }

        protected override void FinalizeOutput()
        {
            excelFormat.PopulateSpreadsheet(this.config, spreadsheet);

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
                this.Log("\r\n\r\nExcel validation errors:");

                foreach (var validationError in validationResults)
                {
                    this.Log("\r\n" + validationError.Description);
                }
            }

            this.spreadsheet.Close();
        }
    }
}
