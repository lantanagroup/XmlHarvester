using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIQRCXML2XLS
{
    public class ExcelFormat
    {
        private List<ExcelRow> rows = new List<ExcelRow>();
        private ExcelRow currentRow = null;
        private List<ExcelHeading> headings = new List<ExcelHeading>();

        public void AddRow(string fileName)
        {
            this.currentRow = new ExcelRow()
            {
                FileName = fileName
            };
            this.rows.Add(this.currentRow);
        }

        public void AddData(MappingGroup group, MappingColumn column, string data, bool isBold = false)
        {
            ExcelData excelColumn = new ExcelData()
            {
                Group = group,
                Column = column,
                Data = data == null ? string.Empty : data
            };
            this.currentRow.Data.Add(excelColumn);

            // Add a heading if one doesn't arleady exist
            int dataCount = this.currentRow.Data.Count(y => y.Column == column);
            int headingCount = this.headings.Count(y => y.Column == column);

            if (headingCount < dataCount)
            {
                this.headings.Add(new ExcelHeading()
                {
                    Group = group,
                    Column = column,
                    DataIndex = headingCount
                });
            }
        }

        private void PopulateHeaders(MappingConfig config, SpreadsheetDocument spreadsheet)
        {
            var excelHeadingRow = new Row();
            spreadsheet.WorkbookPart.WorksheetParts.First().Worksheet.First().AppendChild(excelHeadingRow);

            excelHeadingRow.AppendChild(new Cell()
            {
                DataType = CellValues.String,
                CellValue = new CellValue("File Name"),
                StyleIndex = 1
            });

            foreach (var heading in this.headings)
            {
                string headingText = string.Format("{0} {1}", heading.Column.GetHeading(), heading.DataIndex+1);

                if (heading.Group != null)
                {
                    if (string.IsNullOrEmpty(heading.Group.ColumnPrefix))
                        headingText = string.Format("{0} {1} {2}", heading.Group.TableName, heading.DataIndex + 1, heading.Column.GetHeading());
                    else
                        headingText = string.Format("{0} {1} {2}", heading.Group.ColumnPrefix, heading.DataIndex + 1, heading.Column.GetHeading());
                }

                Cell cell = new Cell()
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(headingText),
                    StyleIndex = 1
                };
                excelHeadingRow.AppendChild(cell);
            }
        }

        public void PopulateSpreadsheet(MappingConfig config, SpreadsheetDocument spreadsheet)
        {
            this.PopulateHeaders(config, spreadsheet);

            foreach (var row in this.rows)
            {
                var excelDataRow = new Row();
                spreadsheet.WorkbookPart.WorksheetParts.First().Worksheet.First().AppendChild(excelDataRow);

                Cell fileNameCell = new Cell()
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(row.FileName)
                };

                excelDataRow.AppendChild(fileNameCell);

                foreach (var heading in this.headings)
                {
                    var datas = row.Data.Where(y => y.Column == heading.Column).ToArray();

                    if (datas.Length - 1 < heading.DataIndex)
                    {
                        excelDataRow.AppendChild(new Cell()
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(string.Empty)
                        });
                        continue;
                    }

                    var data = datas[heading.DataIndex];

                    Cell cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(data.Data)
                    };

                    if (data.IsBold)
                        cell.StyleIndex = 1;

                    excelDataRow.AppendChild(cell);
                }
            }
        }

        public class ExcelHeading
        {
            public MappingGroup Group { get; set; }
            public MappingColumn Column { get; set; }
            public int DataIndex { get; set; }
        }

        public class ExcelRow
        {
            public ExcelRow()
            {
                this.Data = new List<ExcelData>();
            }

            public List<ExcelData> Data { get; set; }
            public string FileName { get; set; }
        }

        public class ExcelData
        {
            public MappingGroup Group { get; set; }
            public MappingColumn Column { get; set; }
   
            public string Data { get; set; }
            public bool IsBold { get; set; }
        }
    }
}
