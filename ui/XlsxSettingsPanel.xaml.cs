using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace LantanaGroup.XmlDocumentConverter.UI
{
    /// <summary>
    /// Interaction logic for XlsxSettingsPanel.xaml
    /// </summary>
    public partial class XlsxSettingsPanel : System.Windows.Controls.UserControl
    {
        public XlsxSettingsPanel()
        {
            InitializeComponent();

            this.OutputDirectoryText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_OUPUT_DIR_NAME, "");
        }

        public string OutputDirectory
        {
            get { return this.OutputDirectoryText.Text; }
            set { this.OutputDirectoryText.Text = value; }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.OutputDirectoryText.Text);
            }
        }

        private void OutputDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (!string.IsNullOrEmpty(this.OutputDirectoryText.Text))
                    dialog.SelectedPath = this.OutputDirectoryText.Text;
                else
                    dialog.SelectedPath = Directory.GetCurrentDirectory();

                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.OutputDirectoryText.Text = dialog.SelectedPath;
                    Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_OUPUT_DIR_NAME, this.OutputDirectoryText.Text);
                }
            }
        }
    }
}
