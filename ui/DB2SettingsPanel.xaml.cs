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
            this.MoveDirectoryText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_MOVE_DIR_NAME, string.Empty);
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

        public string MoveDirectory
        {
            get { return this.MoveDirectoryText.Text; }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.DatabaseText.Text) && !string.IsNullOrEmpty(this.UsernameText.Text) && !string.IsNullOrEmpty(this.PasswordText.Password);
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

        private void MoveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (!string.IsNullOrEmpty(this.MoveDirectoryText.Text))
                dialog.SelectedPath = this.MoveDirectoryText.Text;
            else
                dialog.SelectedPath = Directory.GetCurrentDirectory();

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                this.MoveDirectoryText.Text = dialog.SelectedPath;
                Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_MOVE_DIR_NAME, this.MoveDirectoryText.Text);
            }
        }

        private void ClearMoveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            this.MoveDirectoryText.Text = string.Empty;
            Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_MOVE_DIR_NAME, string.Empty);
        }
    }
}
