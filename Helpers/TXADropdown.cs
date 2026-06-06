using System.Windows;
using System.Windows.Controls;

namespace TXABackupTool.Helpers;

public class TXADropdown : ComboBox
{
    static TXADropdown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TXADropdown), new FrameworkPropertyMetadata(typeof(TXADropdown)));
    }
}
