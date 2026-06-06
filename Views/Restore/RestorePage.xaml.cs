using System.Windows.Controls;
using TXABackupTool.ViewModels;

namespace TXABackupTool.Views.Restore;

public partial class RestorePage : UserControl
{
    public RestorePage()
    {
        InitializeComponent();
    }

    private void CloseModal_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is RestoreViewModel vm) vm.IsModalOpen = false;
    }
}
