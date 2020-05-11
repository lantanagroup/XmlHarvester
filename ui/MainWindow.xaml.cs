using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace LantanaGroup.XmlDocumentConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string REG_SOFTWARE_KEY = @"HKEY_CURRENT_USER\SOFTWARE\XmlDocumentConverter";
        private const string REG_MAPPING_FILE_NAME = "mappingFile";
        private const string REG_INPUT_DIR_NAME = "inputDirectory";
        private const string REG_OUPUT_DIR_NAME = "outputDirectory";

        public MainWindow()
        {
            InitializeComponent();

            this.mappingFileText.Text = (string) Registry.GetValue(REG_SOFTWARE_KEY, REG_MAPPING_FILE_NAME, "");
            this.inputDirectoryText.Text = (string) Registry.GetValue(REG_SOFTWARE_KEY, REG_INPUT_DIR_NAME, "");
            this.outputDirectoryText.Text = (string) Registry.GetValue(REG_SOFTWARE_KEY, REG_OUPUT_DIR_NAME, "");

            this.EnableConvertButton();
        }

        private void EnableConvertButton()
        {
            bool enabled = !string.IsNullOrEmpty(this.mappingFileText.Text) && !string.IsNullOrEmpty(this.inputDirectoryText.Text) && !string.IsNullOrEmpty(this.outputDirectoryText.Text);

            this.convertXlsxButton.IsEnabled =
                this.convertAccessButton.IsEnabled = enabled;
        }

        #region File/Folder Selection Events

        private void mappingFileButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "XML Files|*.xml";

                dialog.InitialDirectory = Directory.GetCurrentDirectory();

                if (!string.IsNullOrEmpty(this.mappingFileText.Text))
                    dialog.FileName = this.mappingFileText.Text;

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.mappingFileText.Text = dialog.FileName;
                    Registry.SetValue(REG_SOFTWARE_KEY, REG_MAPPING_FILE_NAME, this.mappingFileText.Text);

                    this.EnableConvertButton();
                }
            }
        }

        private void inputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(this.inputDirectoryText.Text))
                    dialog.SelectedPath = this.inputDirectoryText.Text;
                else
                    dialog.SelectedPath = Directory.GetCurrentDirectory();

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.inputDirectoryText.Text = dialog.SelectedPath;
                    Registry.SetValue(REG_SOFTWARE_KEY, REG_INPUT_DIR_NAME, this.inputDirectoryText.Text);

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
                else
                    dialog.SelectedPath = Directory.GetCurrentDirectory();

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.outputDirectoryText.Text = dialog.SelectedPath;
                    Registry.SetValue(REG_SOFTWARE_KEY, REG_OUPUT_DIR_NAME, this.outputDirectoryText.Text);
                    this.EnableConvertButton();
                }
            }
        }

        #endregion

        private void EditExternalConfig_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.mappingFileText.Text);
        }

        private void EditInternalConfig_Click(object sender, RoutedEventArgs e)
        {
            EditConfigWindow editConfigWindow = new EditConfigWindow(this.mappingFileText.Text);
            editConfigWindow.Show();
        }

        private void ConvertXlsx_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.logText.Text = "";
                XlsxConverter xlsxConverter = new XlsxConverter(this.mappingFileText.Text, this.inputDirectoryText.Text, this.outputDirectoryText.Text);

                xlsxConverter.LogEvent += delegate (string logText)
                {
                    this.logText.Text += logText;
                };

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
                MSAccessConverter accessConverter = new MSAccessConverter(this.mappingFileText.Text, this.inputDirectoryText.Text, this.outputDirectoryText.Text);

                accessConverter.LogEvent += delegate (string logText)
                {
                    this.logText.Text += logText;
                };

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
