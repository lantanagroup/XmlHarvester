using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using MessageBox = System.Windows.MessageBox;
using System.Threading;

namespace LantanaGroup.XmlDocumentConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string REG_SOFTWARE_KEY = @"HKEY_CURRENT_USER\SOFTWARE\XmlDocumentConverter";
        public const string REG_MAPPING_FILE_NAME = "mappingFile";
        public const string REG_INPUT_DIR_NAME = "inputDirectory";
        public const string REG_OUPUT_DIR_NAME = "outputDirectory";
        public const string REG_OUTPUT_XLSX_NAME = "outputXlsx";
        public const string REG_OUTPUT_MDB_NAME = "outputMdb";
        public const string REG_OUTPUT_DB2_NAME = "outputDb2";

        public MainWindow()
        {
            InitializeComponent();

            this.MappingFileText.Text = (string) Registry.GetValue(REG_SOFTWARE_KEY, REG_MAPPING_FILE_NAME, "");
            this.InputDirectoryText.Text = (string) Registry.GetValue(REG_SOFTWARE_KEY, REG_INPUT_DIR_NAME, "");

            object outputXlsx = Registry.GetValue(REG_SOFTWARE_KEY, REG_OUTPUT_XLSX_NAME, 0);
            object outputMdb = Registry.GetValue(REG_SOFTWARE_KEY, REG_OUTPUT_MDB_NAME, 0);
            object outputDb2 = Registry.GetValue(REG_SOFTWARE_KEY, REG_OUTPUT_DB2_NAME, 0);

            this.XlsxButton.IsChecked = outputXlsx != null ? (int) outputXlsx == 1 : false;
            this.MSAccessButton.IsChecked = outputMdb != null ? (int) outputMdb == 1 : false;
            this.DB2Button.IsChecked = outputDb2 != null ? (int) outputDb2 == 1 : false;

            this.EnableConvertButton();
            this.ChangeSettingsRowHeight();
        }

        private void EnableConvertButton()
        {
            bool hasFormatSelected = this.XlsxButton.IsChecked == true || this.MSAccessButton.IsChecked == true || this.DB2Button.IsChecked == true;

            if (!hasFormatSelected || string.IsNullOrEmpty(this.MappingFileText.Text) || string.IsNullOrEmpty(this.InputDirectoryText.Text))
            {
                this.ConvertButton.IsEnabled = false;
                return;
            }

            if (this.XlsxButton.IsChecked == true)
                this.ConvertButton.IsEnabled = this.XlsxSettings.IsValid;
            else if (this.MSAccessButton.IsChecked == true)
                this.ConvertButton.IsEnabled = this.MdbSettings.IsValid;
            else if (this.DB2Button.IsChecked == true)
                this.ConvertButton.IsEnabled = this.Db2Settings.IsValid;
            else
                this.ConvertButton.IsEnabled = false;
        }

        #region File/Folder Selection Events

        private void MappingFileButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "XML Files|*.xml";

                dialog.InitialDirectory = Directory.GetCurrentDirectory();

                if (!string.IsNullOrEmpty(this.MappingFileText.Text))
                    dialog.FileName = this.MappingFileText.Text;

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.MappingFileText.Text = dialog.FileName;
                    Registry.SetValue(REG_SOFTWARE_KEY, REG_MAPPING_FILE_NAME, this.MappingFileText.Text);

                    this.EnableConvertButton();
                }
            }
        }

        private void InputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (!string.IsNullOrEmpty(this.InputDirectoryText.Text))
                dialog.SelectedPath = this.InputDirectoryText.Text;
            else
                dialog.SelectedPath = Directory.GetCurrentDirectory();

            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                this.InputDirectoryText.Text = dialog.SelectedPath;
                Registry.SetValue(REG_SOFTWARE_KEY, REG_INPUT_DIR_NAME, this.InputDirectoryText.Text);

                this.EnableConvertButton();
            }
        }

        #endregion

        private void EditExternalConfig_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.MappingFileText.Text);
        }

        private void EditInternalConfig_Click(object sender, RoutedEventArgs e)
        {
            EditConfigWindow editConfigWindow = new EditConfigWindow(this.MappingFileText.Text);
            editConfigWindow.Show();
        }

        private void convertXlsx()
        {
            try
            {
                this.LogText.Text = "";
                this.ConvertButton.IsEnabled = false;
                XlsxConverter converter = new XlsxConverter(this.MappingFileText.Text, this.InputDirectoryText.Text, this.XlsxSettings.OutputDirectory);

                converter.LogEvent += delegate (string logText)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.LogText.Text += logText + "\n";
                    });
                };

                converter.ConversionComplete += delegate ()
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Done!");
                        this.EnableConvertButton();
                    });
                };

                Thread thr = new Thread(new ThreadStart(converter.Convert));
                thr.Start();
            }
            catch (Exception ex)
            {
                this.LogText.Text += "ERROR: " + ex.Message;
            }
        }

        private void convertMSAccess()
        {
            try
            {
                this.LogText.Text = "";
                this.ConvertButton.IsEnabled = false;
                MSAccessConverter converter = new MSAccessConverter(this.MappingFileText.Text, this.InputDirectoryText.Text, this.MdbSettings.OutputDirectory);

                converter.LogEvent += delegate (string logText)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.LogText.Text += logText + "\n";
                    });
                };

                converter.ConversionComplete += delegate ()
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Done!");
                        this.EnableConvertButton();
                    });
                };

                Thread thr = new Thread(new ThreadStart(converter.Convert));
                thr.Start();
            }
            catch (Exception ex)
            {
                this.LogText.Text += "ERROR: " + ex.Message;
            }
        }

        private void convertDb2()
        {
            try
            {
                this.LogText.Text = "";
                this.ConvertButton.IsEnabled = false;
                DB2Converter converter = new DB2Converter(this.MappingFileText.Text, this.InputDirectoryText.Text, this.Db2Settings.Database, this.Db2Settings.Username, this.Db2Settings.Password, this.Db2Settings.OutputDirectory);

                converter.LogEvent += delegate (string logText)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.LogText.Text += logText + "\n";
                    });
                };

                converter.ConversionComplete += delegate ()
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Done!");
                        this.EnableConvertButton();
                    });
                };

                Thread thr = new Thread(new ThreadStart(converter.Convert));
                thr.Start();
            }
            catch (Exception ex)
            {
                this.LogText.Text += "ERROR: " + ex.Message;
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.XlsxButton.IsChecked == true)
                this.convertXlsx();
            else if (this.MSAccessButton.IsChecked == true)
                this.convertMSAccess();
            else if (this.DB2Button.IsChecked == true)
                this.convertDb2();
            else
                MessageBox.Show("No conversion format selected!");
        }

        private void ToggleOutputFormat_Click(object sender, RoutedEventArgs e)
        {
            if (sender == this.XlsxButton && this.XlsxButton.IsChecked == true)
                this.MSAccessButton.IsChecked = this.DB2Button.IsChecked = false;
            else if (sender == this.MSAccessButton && this.MSAccessButton.IsChecked == true)
                this.XlsxButton.IsChecked = this.DB2Button.IsChecked = false;
            else if (sender == this.DB2Button && this.DB2Button.IsChecked == true)
                this.XlsxButton.IsChecked = this.MSAccessButton.IsChecked = false;

            Registry.SetValue(REG_SOFTWARE_KEY, REG_OUTPUT_XLSX_NAME, this.XlsxButton.IsChecked == true, RegistryValueKind.DWord);
            Registry.SetValue(REG_SOFTWARE_KEY, REG_OUTPUT_MDB_NAME, this.MSAccessButton.IsChecked == true, RegistryValueKind.DWord);
            Registry.SetValue(REG_SOFTWARE_KEY, REG_OUTPUT_DB2_NAME, this.DB2Button.IsChecked == true, RegistryValueKind.DWord);

            this.EnableConvertButton();
            this.ChangeSettingsRowHeight();
        }

        private void ChangeSettingsRowHeight()
        {
            if (this.XlsxButton.IsChecked == true)
                this.SettingsRow.Height = new GridLength(60);
            else if (this.MSAccessButton.IsChecked == true)
                this.SettingsRow.Height = new GridLength(60);
            else if (this.DB2Button.IsChecked == true)
                this.SettingsRow.Height = new GridLength(240);
        }
    }
}
