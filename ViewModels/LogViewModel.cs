using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TXABackupTool.Services;

namespace TXABackupTool.ViewModels;

public class LogItem : INotifyPropertyChanged
{
    public string Timestamp { get; set; } = "";
    public string Message { get; set; } = "";
    public string Type { get; set; } = "App";

    private string _copyIcon = "📋"; // Icon bảng tính chuẩn
    public string CopyIcon 
    { 
        get => _copyIcon; 
        set { _copyIcon = value; OnPropertyChanged(); } 
    }

    public ICommand CopyCommand { get; set; } = null!;

    public System.Windows.Media.Brush Color => Type switch
    {
        "Error" => (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("ErrorBrush"),
        "Backup" => (System.Windows.Media.Brushes.DodgerBlue),
        "Localization" => (System.Windows.Media.Brushes.Orange),
        "Startup" => (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("SurfaceBrightBrush"),
        "App" => (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("SurfaceContainerHighestBrush"),
        _ => (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource("OnSurfaceBrush")
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class LogViewModel : INotifyPropertyChanged
{
    private string _filter = "All";
    public string FilterSelected
    {
        get => _filter;
        set { _filter = value; OnPropertyChanged(); RefreshLogs(); }
    }

    public ObservableCollection<string> FilterOptions { get; } = new() { "All", "Startup", "App", "Error", "Backup", "Localization" };
    public ObservableCollection<LogItem> Logs { get; } = new();
    
    public bool HasLogs => Logs.Count > 0;
    public string EmptyLogsText => LanguageService.Get("txa_logs_empty");
    public string NoLogsSubText => LanguageService.Get("txa_logs_no_data");

    public string Title => LanguageService.Get("txa_logs");
    public string ClearLogsText => LanguageService.Get("txa_btn_clear_logs");
    public string ClearCategoryText => LanguageService.Get("txa_btn_clear_category");
    public string CopyText => LanguageService.Get("txa_btn_copy");
    public string CategoryText => LanguageService.Get("txa_category");
    public string AutoLangStatus => string.Format(LanguageService.Get("txa_auto_lang_status"), LanguageService.Get("txa_on"));

    public void RefreshUI()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(ClearLogsText));
        OnPropertyChanged(nameof(ClearCategoryText));
        OnPropertyChanged(nameof(CopyText));
        OnPropertyChanged(nameof(CategoryText));
        OnPropertyChanged(nameof(AutoLangStatus));
        OnPropertyChanged(nameof(EmptyLogsText));
        OnPropertyChanged(nameof(NoLogsSubText));
        RefreshLogs();
    }

    public LogViewModel()
    {
        RefreshLogs();
        ClearLogsCommand = new RelayCommand(_ => {
            TXALogger.ClearLogs();
            RefreshLogs();
        });

        ClearCategoryCommand = new RelayCommand(_ => {
            if (FilterSelected == "All") 
            {
                TXALogger.ClearLogs();
            }
            else 
            {
                if (Enum.TryParse<LogType>(FilterSelected, out var type)) {
                    TXALogger.ClearLogs(type); 
                }
            }
            RefreshLogs();
        });
    }

    public ICommand ClearLogsCommand { get; }
    public ICommand ClearCategoryCommand { get; }

    public void RefreshLogs()
    {
        Logs.Clear();
        if (_filter == "All" || _filter == "App") LoadLogs(LogType.App);
        if (_filter == "All" || _filter == "Error") LoadLogs(LogType.Error);
        if (_filter == "All" || _filter == "Backup") LoadLogs(LogType.Backup);
        if (_filter == "All" || _filter == "Startup") LoadLogs(LogType.Startup);
        if (_filter == "All" || _filter == "Localization") LoadLogs(LogType.Localization);
        
        var sorted = Logs.OrderByDescending(l => l.Timestamp).ToList();
        Logs.Clear();
        foreach (var item in sorted) Logs.Add(item);
        
        OnPropertyChanged(nameof(HasLogs));
    }

    private void LoadLogs(LogType type)
    {
        string content = TXALogger.GetLogs(type);
        if (string.IsNullOrEmpty(content)) return;

        // Pattern matching: [Timestamp] Message...
        // Hỗ trợ nhiều định dạng ngày tháng khác nhau
        var regex = new System.Text.RegularExpressions.Regex(@"^\[(.*?)\] (.*)", System.Text.RegularExpressions.RegexOptions.Multiline);
        var matches = regex.Matches(content);

        for (int i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            string timestamp = match.Groups[1].Value;
            string message = match.Groups[2].Value;

            // Find start of next match to capture everything in between (multi-line)
            int currentPos = match.Index + match.Length;
            int nextPos = (i + 1 < matches.Count) ? matches[i + 1].Index : content.Length;
            
            if (nextPos > currentPos)
            {
                message += content.Substring(currentPos, nextPos - currentPos).TrimEnd();
            }

            var item = new LogItem {
                Timestamp = timestamp,
                Message = message.Trim(),
                Type = type.ToString()
            };
            item.CopyCommand = new RelayCommand(async _ => {
                try 
                { 
                    System.Windows.Clipboard.SetText($"[{item.Timestamp}] [{item.Type}] {item.Message}"); 
                    item.CopyIcon = "✅";
                    await System.Threading.Tasks.Task.Delay(1500);
                    item.CopyIcon = "📋";
                } catch { }
            });
            Logs.Add(item);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
