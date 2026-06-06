using System.Windows.Controls;

namespace TXABackupTool.Views;

public partial class BackupPage : UserControl
{
    public BackupPage()
    {
        InitializeComponent();
    }

    private void LogBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LogBox.ScrollToEnd();
    }
}
