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
    /// Interaction logic for DB2SettingsPanel.xaml
    /// </summary>
    public partial class DB2SettingsPanel : System.Windows.Controls.UserControl
    {
        private const string REG_DB2_DATABASE_NAME = "db2Database";
        private const string REG_DB2_USERNAME_NAME = "db2Username";

        public delegate void SettingsChangedEventHandler();
        public event SettingsChangedEventHandler SettingsChanged;

        public DB2SettingsPanel()
        {
            InitializeComponent();

            this.DatabaseText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, REG_DB2_DATABASE_NAME, string.Empty);
            this.UsernameText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, REG_DB2_USERNAME_NAME, string.Empty);
            this.OutputDirectoryText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_OUPUT_DIR_NAME, string.Empty);
        }

        public string Database
        {
            get { return this.DatabaseText.Text; }
        }
        
        public string Username
        {
            get { return this.UsernameText.Text; }
        }

        public string Password
        {
            get { return this.PasswordText.Password; }
        }

        public string OutputDirectory
        {
            get { return this.OutputDirectoryText.Text; }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.DatabaseText.Text) && !string.IsNullOrEmpty(this.UsernameText.Text) && !string.IsNullOrEmpty(this.PasswordText.Password);
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
                    this.SettingsChanged?.Invoke();
                }
            }
        }

        private void DatabaseText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, REG_DB2_DATABASE_NAME, this.DatabaseText.Text);
            this.SettingsChanged?.Invoke();
        }

        private void UsernameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, REG_DB2_USERNAME_NAME, this.UsernameText.Text);
            this.SettingsChanged?.Invoke();
        }

        private void PasswordText_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.SettingsChanged?.Invoke();
        }
    }
}
