using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TXABackupTool.Helpers;

public class TXAModal : ContentControl
{
    public static readonly DependencyProperty IsOpenProperty = 
        DependencyProperty.Register("IsOpen", typeof(bool), typeof(TXAModal), new PropertyMetadata(false));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = 
        DependencyProperty.Register("Title", typeof(string), typeof(TXAModal), new PropertyMetadata("MODAL"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    static TXAModal()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TXAModal), new FrameworkPropertyMetadata(typeof(TXAModal)));
    }
}
