using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace LantanaGroup.XmlHarvester.UI
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

            DatabaseText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, REG_DB2_DATABASE_NAME, string.Empty);
            UsernameText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, REG_DB2_USERNAME_NAME, string.Empty);
            MoveDirectoryText.Text = (string)Registry.GetValue(MainWindow.REG_SOFTWARE_KEY, MainWindow.REG_MOVE_DIR_NAME, string.Empty);
        }

        public string Database
        {
            get { return DatabaseText.Text; }
        }

        public string Username
        {
            get { return UsernameText.Text; }
        }

        public string Password
        {
            get { return PasswordText.Password; }
        }

        public string MoveDirectory
        {
            get { return MoveDirectoryText.Text; }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(DatabaseText.Text) && !string.IsNullOrEmpty(UsernameText.Text) && !string.IsNullOrEmpty(PasswordText.Password);
            }
        }

        private void DatabaseText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, REG_DB2_DATABASE_NAME, DatabaseText.Text);
            SettingsChanged?.Invoke();
        }

        private void UsernameText_TextChanged(object sender, TextChangedEventArgs e)
        {
            Registry.SetValue(MainWindow.REG_SOFTWARE_KEY, REG_DB2_USERNAME_NAME, UsernameText.Text);
            SettingsChanged?.Invoke();
        }

        private void PasswordText_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SettingsChanged?.Invoke();
        }

        private void MoveDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (!string.IsNullOrEmpty(MoveDirectoryText.Text))
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
