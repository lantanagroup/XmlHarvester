using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace LantanaGroup.XmlHarvester.UI
{
    /// <summary>
    /// Interaction logic for XlsxSettingsPanel.xaml
    /// </summary>
    public partial class XlsxSettingsPanel : System.Windows.Controls.UserControl
    {
        public XlsxSettingsPanel()
        {
            InitializeComponent();

            OutputDirectoryText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_OUPUT_DIR_NAME, "");
            MoveDirectoryText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_MOVE_DIR_NAME, "");
        }

        public string OutputDirectory
        {
            get { return OutputDirectoryText.Text; }
        }

        public string MoveDirectory
        {
            get { return MoveDirectoryText.Text; }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(OutputDirectoryText.Text);
            }
        }

        private void OutputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (!string.IsNullOrEmpty(OutputDirectoryText.Text))
                dialog.SelectedPath = OutputDirectoryText.Text;
            else
                dialog.SelectedPath = Directory.GetCurrentDirectory();

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                OutputDirectoryText.Text = dialog.SelectedPath;
                Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_OUPUT_DIR_NAME, OutputDirectoryText.Text);
            }
        }

        private void MoveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (!string.IsNullOrEmpty(OutputDirectoryText.Text))
                dialog.SelectedPath = MoveDirectoryText.Text;
            else
                dialog.SelectedPath = Directory.GetCurrentDirectory();

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                MoveDirectoryText.Text = dialog.SelectedPath;
                Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_MOVE_DIR_NAME, MoveDirectoryText.Text);
            }
        }

        private void ClearMoveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            MoveDirectoryText.Text = string.Empty;
            Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_MOVE_DIR_NAME, string.Empty);
        }
    }
}
