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

namespace XmlDocumentConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Directory.Exists("C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\XmlDocumentConverter\\Samples\\input"))
                this.inputDirectoryText.Text = "C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\XmlDocumentConverter\\Samples\\input";

            if (Directory.Exists("C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\XmlDocumentConverter\\Samples\\output"))
                this.outputDirectoryText.Text = "C:\\Users\\sean.mcilvenna\\Code\\VIQRCXML2XLS\\XmlDocumentConverter\\Samples\\output";

            this.EnableConvertButton();
        }

        private void EnableConvertButton()
        {
            bool enabled = !string.IsNullOrEmpty(this.inputDirectoryText.Text) && !string.IsNullOrEmpty(this.outputDirectoryText.Text);

            this.convertXlsxButton.IsEnabled =
                this.convertAccessButton.IsEnabled = enabled;
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

        private void EditExternalConfig_Click(object sender, RoutedEventArgs e)
        {
            string fileLocation = MappingConfig.GetConfigFileName();
            FileInfo fileLocationInfo = new FileInfo(fileLocation);
            var process = System.Diagnostics.Process.Start(fileLocation);

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = fileLocationInfo.DirectoryName;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = fileLocationInfo.Name;

            watcher.Changed += ConfigWatcher_Changed;

            watcher.EnableRaisingEvents = true;
        }

        private void EditInternalConfig_Click(object sender, RoutedEventArgs e)
        {
            EditConfigWindow editConfigWindow = new EditConfigWindow();
            editConfigWindow.Show();
        }

        private void ConfigWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(e.FullPath);

                if (fileInfo.Directory.Parent != null && fileInfo.Directory.Parent.Name == "bin" && fileInfo.Directory.Parent.Parent != null)
                {
                    string newDestination = System.IO.Path.Combine(fileInfo.Directory.Parent.Parent.FullName, MappingConfig.ConfigFileName);

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

        private void ConvertXlsx_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.logText.Text = "";
                XlsxConverter xlsxConverter = new XlsxConverter(this.inputDirectoryText.Text, this.outputDirectoryText.Text, this.logText);
                xlsxConverter.Convert();
            }
            catch (Exception ex)
            {
                this.logText.Text += "Error converting: " + ex.Message;
            }

            System.Windows.Forms.MessageBox.Show("Done");
        }

        private void ConvertAccess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.logText.Text = "";
                MSAccessConverter accessConverter = new MSAccessConverter(this.inputDirectoryText.Text, this.outputDirectoryText.Text, this.logText);
                accessConverter.Convert();
            }
            catch (Exception ex)
            {
                this.logText.Text += "Error converting: " + ex.Message;
            }

            System.Windows.Forms.MessageBox.Show("Done");
        }
    }
}
