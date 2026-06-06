using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TXABackupTool.ViewModels;
using TXABackupTool.Views;
using TXABackupTool.Services;

namespace TXABackupTool;

public partial class MainWindow : Window
{
    #region Constructor
    public MainWindow()
    {
        try 
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
        catch (Exception ex)
        {
            string errorMsg = $"FATAL WINDOW ERROR:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            MessageBox.Show(errorMsg, "UI Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            try { TXALogger.Log(LogType.Error, "Startup Crash", ex.ToString()); }
            catch { }
            
            Application.Current.Shutdown();
        }
    }
    #endregion

    #region Window Management
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }

    private void MinButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    #endregion

    #region Liquid Glass Mouse Glow
    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(GlowCanvas);
        Canvas.SetLeft(GlowOrb, pos.X - GlowOrb.Width/2);
        Canvas.SetTop(GlowOrb, pos.Y - GlowOrb.Height/2);
    }

    private void Window_MouseEnter(object sender, MouseEventArgs e)
    {
        var fadeIn = new DoubleAnimation(0, 0.4, TimeSpan.FromMilliseconds(500));
        GlowOrb.BeginAnimation(OpacityProperty, fadeIn);
    }

    private void Window_MouseLeave(object sender, MouseEventArgs e)
    {
        var fadeOut = new DoubleAnimation(GlowOrb.Opacity, 0, TimeSpan.FromMilliseconds(600));
        GlowOrb.BeginAnimation(OpacityProperty, fadeOut);
    }
    #endregion
}