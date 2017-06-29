using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace VIQRCXML2XLS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Directory.Exists("C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\VIQRCXML2XLS\\Samples\\input"))
                this.inputDirectoryText.Text = "C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\VIQRCXML2XLS\\Samples\\input";

            if (Directory.Exists("C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\VIQRCXML2XLS\\Samples\\output"))
                this.outputDirectoryText.Text = "C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\VIQRCXML2XLS\\Samples\\output";

            this.EnableConvertButton();
        }

        private void EnableConvertButton()
        {
            bool enabled = !string.IsNullOrEmpty(this.inputDirectoryText.Text) && !string.IsNullOrEmpty(this.outputDirectoryText.Text);

            this.convertButton.IsEnabled = enabled;
        }

        private void inputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(this.inputDirectoryText.Text))
                    dialog.SelectedPath = this.inputDirectoryText.Text;

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.inputDirectoryText.Text = dialog.SelectedPath;
                    this.outputDirectoryText.Text = dialog.SelectedPath;
                    this.EnableConvertButton();
                }
            }
        }

        private void outputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(this.outputDirectoryText.Text))
                    dialog.SelectedPath = this.outputDirectoryText.Text;

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.outputDirectoryText.Text = dialog.SelectedPath;
                    this.EnableConvertButton();
                }
            }
        }
        private void EditConfig_Click(object sender, RoutedEventArgs e)
        {
            string fileLocation = this.GetConfigFileLocation();
            FileInfo fileLocationInfo = new FileInfo(fileLocation);
            var process = System.Diagnostics.Process.Start(fileLocation);

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = fileLocationInfo.DirectoryName;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = fileLocationInfo.Name;

            watcher.Changed += Watcher_Changed;

            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(e.FullPath);

                if (fileInfo.Directory.Parent != null && fileInfo.Directory.Parent.Name == "bin" && fileInfo.Directory.Parent.Parent != null)
                {
                    string newDestination = System.IO.Path.Combine(fileInfo.Directory.Parent.Parent.FullName, "XmlConfig.xml");

                    if (File.Exists(newDestination))
                    {
                        using (StreamReader sr = new StreamReader(new FileStream(e.FullPath, FileMode.Open, FileAccess.Read)))
                        {
                            File.WriteAllText(newDestination, sr.ReadToEnd());
                        }
                    }
                }
            }
            catch
            {
                // Ignore
            }
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            var columnConfigs = this.ParseXmlConfig();

            string fileName = DateTime.Now.ToShortDateString().Replace("/", "-") + " " + DateTime.Now.ToShortTimeString().Replace(":", "-") + ".xlsx";
            string filePath = System.IO.Path.Combine(this.outputDirectoryText.Text, fileName);

            string[] xmlFiles = Directory.GetFiles(this.inputDirectoryText.Text, "*.xml");

            try
            {

                using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    // create the workbook
                    spreadSheet.AddWorkbookPart();
                    spreadSheet.WorkbookPart.Workbook = new Workbook();     // create the worksheet
                    spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();
                    spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet();

                    // create sheet data
                    spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.AppendChild(new SheetData());

                    var headerRow = new Row();
                    spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.First().AppendChild(headerRow);

                    foreach (var columnConfig in columnConfigs)
                    {
                        headerRow.AppendChild(
                            new Cell()
                            {
                                DataType = CellValues.String,
                                CellValue = new CellValue(columnConfig.Header)
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

                        foreach (var columnConfig in columnConfigs)
                        {
                            XmlNodeList nodes = null;

                            try
                            {
                                nodes = !string.IsNullOrEmpty(columnConfig.Xpath) ? xmlDoc.SelectNodes(columnConfig.Xpath, nsManager) : null;
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
                                if (!string.IsNullOrEmpty(columnConfig.Xpath))
                                    this.logText.Text += "No data found for XPATH \"" + columnConfig.Xpath + "\r\n";

                                xmlRow.AppendChild(
                                    new Cell()
                                    {
                                        DataType = CellValues.String,
                                        CellValue = new CellValue(string.Empty)
                                    });
                                continue;
                            }

                            string cellValue = string.Empty;
                            for (var i = 0; i < nodes.Count; i++)
                            {
                                if (i > 0)
                                    cellValue += "\r\n\r\n";

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

                            xmlRow.AppendChild(
                                new Cell()
                                {
                                    DataType = CellValues.String,
                                    CellValue = new CellValue(cellValue)
                                });
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

                    foreach (var validationError in validator.Validate(spreadSheet))
                    {
                        Console.WriteLine("Error " + validationError.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetConfigFileLocation()
        {
            FileInfo assemblyFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string fileLocation = System.IO.Path.Combine(assemblyFileInfo.DirectoryName, "XmlConfig.xml");
            return fileLocation;
        }

        private List<ColumnConfig> ParseXmlConfig()
        {
            XmlDocument xmlConfigDoc = new XmlDocument();
            xmlConfigDoc.Load(this.GetConfigFileLocation());

            XmlNodeList columnNodes = xmlConfigDoc.SelectNodes("/config/column");
            List<ColumnConfig> columns = new List<ColumnConfig>();

            foreach (XmlElement columnElement in columnNodes)
            {
                columns.Add(new ColumnConfig()
                {
                    Header = columnElement.Attributes["header"].Value,
                    Xpath = columnElement.InnerText 
                });
            }

            return columns;
        }

        class ColumnConfig
        {
            public string Header;
            public string Xpath;
            public bool ErrorShown;
        }
    }
}
