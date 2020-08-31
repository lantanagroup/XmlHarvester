using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace LantanaGroup.XmlHarvester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string REG_SOFTWARE_KEY = @"HKEY_CURRENT_USER\SOFTWARE\XmlHarvester";
        public const string REG_MAPPING_FILE_NAME = "mappingFile";
        public const string REG_INPUT_DIR_NAME = "inputDirectory";
        public const string REG_OUPUT_DIR_NAME = "outputDirectory";
        public const string REG_MOVE_DIR_NAME = "moveDirectory";
        public const string REG_OUTPUT_XLSX_NAME = "outputXlsx";
        public const string REG_OUTPUT_MDB_NAME = "outputMdb";
        public const string REG_OUTPUT_DB2_NAME = "outputDb2";
        public const string REG_SCHEMA_FILE_NAME = "schemaFile";
        public const string REG_SCHEMATRON_FILE_NAME = "schematronFile";

        public MainWindow()
        {
            InitializeComponent();

            MappingFileText.Text = (string)Registry.GetValue(REG_SOFTWARE_KEY, REG_MAPPING_FILE_NAME, "");
            InputDirectoryText.Text = (string)Registry.GetValue(REG_SOFTWARE_KEY, REG_INPUT_DIR_NAME, "");
            SchemaFileText.Text = (string)Registry.GetValue(REG_SOFTWARE_KEY, REG_SCHEMA_FILE_NAME, "");
            SchematronFileText.Text = (string)Registry.GetValue(REG_SOFTWARE_KEY, REG_SCHEMATRON_FILE_NAME, "");

            object outputXlsx = Registry.GetValue(REG_SOFTWARE_KEY, REG_OUTPUT_XLSX_NAME, 0);
            object outputMdb = Registry.GetValue(REG_SOFTWARE_KEY, REG_OUTPUT_MDB_NAME, 0);
            object outputDb2 = Registry.GetValue(REG_SOFTWARE_KEY, REG_OUTPUT_DB2_NAME, 0);

            XlsxButton.IsChecked = outputXlsx != null ? (int)outputXlsx == 1 : false;
            MSAccessButton.IsChecked = outputMdb != null ? (int)outputMdb == 1 : false;
            DB2Button.IsChecked = outputDb2 != null ? (int)outputDb2 == 1 : false;

            EnableConvertButton();
            ChangeSettingsRowHeight();
        }

        private void EnableConvertButton()
        {
            bool hasFormatSelected = XlsxButton.IsChecked == true || MSAccessButton.IsChecked == true || DB2Button.IsChecked == true;

            if (!hasFormatSelected || string.IsNullOrEmpty(MappingFileText.Text) || string.IsNullOrEmpty(InputDirectoryText.Text))
            {
                ConvertButton.IsEnabled = false;
                return;
            }

            if (XlsxButton.IsChecked == true)
                ConvertButton.IsEnabled = XlsxSettings.IsValid;
            else if (MSAccessButton.IsChecked == true)
                ConvertButton.IsEnabled = MdbSettings.IsValid;
            else if (DB2Button.IsChecked == true)
                ConvertButton.IsEnabled = Db2Settings.IsValid;
            else
                ConvertButton.IsEnabled = false;
        }

        #region File/Folder Selection Events

        private void MappingFileButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "XML Files|*.xml";

                dialog.InitialDirectory = Directory.GetCurrentDirectory();

                if (!string.IsNullOrEmpty(MappingFileText.Text))
                    dialog.FileName = MappingFileText.Text;

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    MappingFileText.Text = dialog.FileName;
                    Registry.SetValue(REG_SOFTWARE_KEY, REG_MAPPING_FILE_NAME, MappingFileText.Text);

                    EnableConvertButton();
                }
            }
        }

        private void InputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (!string.IsNullOrEmpty(InputDirectoryText.Text))
                dialog.SelectedPath = InputDirectoryText.Text;
            else
                dialog.SelectedPath = Directory.GetCurrentDirectory();

            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                InputDirectoryText.Text = dialog.SelectedPath;
                Registry.SetValue(REG_SOFTWARE_KEY, REG_INPUT_DIR_NAME, InputDirectoryText.Text);

                EnableConvertButton();
            }
        }

        #endregion

        private void EditExternalConfig_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(MappingFileText.Text);
        }

        private void EditInternalConfig_Click(object sender, RoutedEventArgs e)
        {
            EditConfigWindow editConfigWindow = new EditConfigWindow(MappingFileText.Text);
            editConfigWindow.Show();
        }

        private void convertXlsx()
        {
            try
            {
                LogText.Text = "";
                ConvertButton.IsEnabled = false;
                XlsxConverter converter = new XlsxConverter(MappingFileText.Text, InputDirectoryText.Text, XlsxSettings.OutputDirectory, XlsxSettings.MoveDirectory, SchemaFileText.Text, SchematronFileText.Text);

                converter.LogEvent += delegate (string logText)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LogText.Text += logText + "\n";
                    });
                };

                converter.ConversionComplete += delegate ()
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Done!");
                        EnableConvertButton();
                    });
                };

                Thread thr = new Thread(new ThreadStart(converter.Convert));
                thr.Start();
            }
            catch (Exception ex)
            {
                LogText.Text += "ERROR: " + ex.Message;
            }
        }

        private void convertMSAccess()
        {
            try
            {
                LogText.Text = "";
                ConvertButton.IsEnabled = false;
                MSAccessConverter converter = new MSAccessConverter(MappingFileText.Text, InputDirectoryText.Text, MdbSettings.OutputDirectory, MdbSettings.MoveDirectory, SchemaFileText.Text, SchematronFileText.Text);

                converter.LogEvent += delegate (string logText)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LogText.Text += logText + "\n";
                    });
                };

                converter.ConversionComplete += delegate ()
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Done!");
                        EnableConvertButton();
                    });
                };

                Thread thr = new Thread(new ThreadStart(converter.Convert));
                thr.Start();
            }
            catch (Exception ex)
            {
                LogText.Text += "ERROR: " + ex.Message;
            }
        }

        private void convertDb2()
        {
            try
            {
                LogText.Text = "";
                ConvertButton.IsEnabled = false;
                DB2Converter converter = new DB2Converter(MappingFileText.Text, InputDirectoryText.Text, Db2Settings.Database, Db2Settings.Username, Db2Settings.Password, Db2Settings.MoveDirectory, SchemaFileText.Text, SchematronFileText.Text);

                converter.LogEvent += delegate (string logText)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LogText.Text += logText + "\n";
                    });
                };

                converter.ConversionComplete += delegate ()
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Done!");
                        EnableConvertButton();
                    });
                };

                Thread thr = new Thread(new ThreadStart(converter.Convert));
                thr.Start();
            }
            catch (Exception ex)
            {
                LogText.Text += "ERROR: " + ex.Message;
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (XlsxButton.IsChecked == true)
                convertXlsx();
            else if (MSAccessButton.IsChecked == true)
                convertMSAccess();
            else if (DB2Button.IsChecked == true)
                convertDb2();
            else
                MessageBox.Show("No conversion format selected!");
        }

        private void ToggleOutputFormat_Click(object sender, RoutedEventArgs e)
        {
            if (sender == XlsxButton && XlsxButton.IsChecked == true)
                MSAccessButton.IsChecked = DB2Button.IsChecked = false;
            else if (sender == MSAccessButton && MSAccessButton.IsChecked == true)
                XlsxButton.IsChecked = DB2Button.IsChecked = false;
            else if (sender == DB2Button && DB2Button.IsChecked == true)
                XlsxButton.IsChecked = MSAccessButton.IsChecked = false;

            Registry.SetValue(REG_SOFTWARE_KEY, REG_OUTPUT_XLSX_NAME, XlsxButton.IsChecked == true, RegistryValueKind.DWord);
            Registry.SetValue(REG_SOFTWARE_KEY, REG_OUTPUT_MDB_NAME, MSAccessButton.IsChecked == true, RegistryValueKind.DWord);
            Registry.SetValue(REG_SOFTWARE_KEY, REG_OUTPUT_DB2_NAME, DB2Button.IsChecked == true, RegistryValueKind.DWord);

            EnableConvertButton();
            ChangeSettingsRowHeight();
        }

        private void ChangeSettingsRowHeight()
        {
            if (XlsxButton.IsChecked == true)
                SettingsRow.Height = new GridLength(130);
            else if (MSAccessButton.IsChecked == true)
                SettingsRow.Height = new GridLength(130);
            else if (DB2Button.IsChecked == true)
                SettingsRow.Height = new GridLength(250);
        }

        private void SchemaFileButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "XSD Files|*.xsd";

                dialog.InitialDirectory = Directory.GetCurrentDirectory();

                if (!string.IsNullOrEmpty(MappingFileText.Text))
                    dialog.FileName = MappingFileText.Text;

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    SchemaFileText.Text = dialog.FileName;
                    Registry.SetValue(REG_SOFTWARE_KEY, REG_SCHEMA_FILE_NAME, SchemaFileText.Text);

                    EnableConvertButton();
                }
            }
        }

        private void SchemaClearButton_Click(object sender, RoutedEventArgs e)
        {
            SchemaFileText.Text = string.Empty;
            Registry.SetValue(REG_SOFTWARE_KEY, REG_SCHEMA_FILE_NAME, string.Empty);
        }

        private void SchematronFileButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "SCH Files|*.sch";

                dialog.InitialDirectory = Directory.GetCurrentDirectory();

                if (!string.IsNullOrEmpty(MappingFileText.Text))
                    dialog.FileName = MappingFileText.Text;

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    SchematronFileText.Text = dialog.FileName;
                    Registry.SetValue(REG_SOFTWARE_KEY, REG_SCHEMATRON_FILE_NAME, SchematronFileText.Text);

                    EnableConvertButton();
                }
            }
        }

        private void SchematronClearButton_Click(object sender, RoutedEventArgs e)
        {
            SchematronFileText.Text = string.Empty;
            Registry.SetValue(REG_SOFTWARE_KEY, REG_SCHEMATRON_FILE_NAME, string.Empty);
        }
    }
}
