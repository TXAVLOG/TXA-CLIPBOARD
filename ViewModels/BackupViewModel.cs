using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TXABackupTool.Models;
using TXABackupTool.Services;

namespace TXABackupTool.ViewModels;

public class ShortcutItem : INotifyPropertyChanged
{
    private bool _isSelected = false; // Default false to skip as requested
    public string FullPath { get; set; } = string.Empty;
    public string FileName => Path.GetFileName(FullPath);
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class BackupViewModel : INotifyPropertyChanged
{
    public readonly BackupService BackupService = new();
    private readonly ClipboardService _clipboardService = new();
    private readonly TXAZipper _zipper = new();
    private string _logText = "";
    private double _overallProgress;
    private bool _isRunning;
    private string _destinationPath = "";
    private CancellationTokenSource? _cts;
    private string _etaText = "--:--";
    private bool _isSummaryModalOpen;
    private string _summaryTitle = "";
    private string _summaryContent = "";

    public ObservableCollection<BackupItem> BackupItems { get; } = new();

    public string LogText
    {
        get => _logText;
        set { _logText = value; OnPropertyChanged(); }
    }

    public double OverallProgress
    {
        get => _overallProgress;
        set { _overallProgress = value; OnPropertyChanged(); }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    public string DestinationPath
    {
        get => _destinationPath;
        set { _destinationPath = value; OnPropertyChanged(); }
    }

    private bool _isBackupInProgress;
    public bool IsBackupInProgress
    {
        get => _isBackupInProgress;
        set { _isBackupInProgress = value; OnPropertyChanged(); }
    }

    private bool _isShortcutModalOpen;
    public bool IsShortcutModalOpen
    {
        get => _isShortcutModalOpen;
        set { _isShortcutModalOpen = value; OnPropertyChanged(); }
    }

    private bool _isResumeModalOpen;
    public bool IsResumeModalOpen
    {
        get => _isResumeModalOpen;
        set { _isResumeModalOpen = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ShortcutItem> DetectedShortcuts { get; } = new();
    private TaskCompletionSource<List<string>>? _shortcutTcs;
    private TaskCompletionSource<bool>? _resumeTcs;

    public string ETAText
    {
        get => _etaText;
        set { _etaText = value; OnPropertyChanged(); }
    }

    public bool IsSummaryModalOpen
    {
        get => _isSummaryModalOpen;
        set { _isSummaryModalOpen = value; OnPropertyChanged(); }
    }

    public string SummaryTitle
    {
        get => _summaryTitle;
        set { _summaryTitle = value; OnPropertyChanged(); }
    }

    public string SummaryContent
    {
        get => _summaryContent;
        set { _summaryContent = value; OnPropertyChanged(); }
    }

    public string Title => LanguageService.Get("txa_backup_title");
    public string ChooseFolderText => LanguageService.Get("txa_choose_folder");
    public string DestText => LanguageService.Get("txa_dest");
    public string LogsTitle => LanguageService.Get("txa_logs");
    public string StartText => LanguageService.Get("txa_start");
    public string CancelText => LanguageService.Get("txa_cancel");
    public string OpenTmText => LanguageService.Get("txa_open_tm");
    public string AddFolderText => LanguageService.Get("txa_btn_add_folder");
    public string ChangeText => LanguageService.Get("txa_btn_change");
    public string OverallProgressLabel => LanguageService.Get("txa_overall_progress");
    
    // Localized Modal Props
    public string ShortcutModalTitle => LanguageService.Get("txa_modal_shortcut_title");
    public string ShortcutModalDesc => LanguageService.Get("txa_modal_shortcut_desc");
    public string BtnContinueBackup => LanguageService.Get("txa_btn_continue_backup");
    public string ResumeModalTitle => LanguageService.Get("txa_modal_resume_title");
    public string ResumeModalDesc => LanguageService.Get("txa_modal_resume_desc");
    public string BtnResume => LanguageService.Get("txa_btn_resume");
    public string BtnOverwrite => LanguageService.Get("txa_btn_overwrite");
    public string BtnOk => LanguageService.Get("txa_btn_ok");

    public void RefreshUI()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(ChooseFolderText));
        OnPropertyChanged(nameof(DestText));
        OnPropertyChanged(nameof(StartText));
        OnPropertyChanged(nameof(CancelText));
        OnPropertyChanged(nameof(OpenTmText));
        OnPropertyChanged(nameof(AddFolderText));
        OnPropertyChanged(nameof(ChangeText));
        OnPropertyChanged(nameof(OverallProgressLabel));
        
        OnPropertyChanged(nameof(ShortcutModalTitle));
        OnPropertyChanged(nameof(ShortcutModalDesc));
        OnPropertyChanged(nameof(BtnContinueBackup));
        OnPropertyChanged(nameof(ResumeModalTitle));
        OnPropertyChanged(nameof(ResumeModalDesc));
        OnPropertyChanged(nameof(BtnResume));
        OnPropertyChanged(nameof(BtnOverwrite));
        OnPropertyChanged(nameof(BtnOk));
        OnPropertyChanged(nameof(SummaryTitle)); // In case summary is open during lang change

        foreach (var item in BackupItems) {
            item.NotifyMetadataChanged();
        }
    }

    public ICommand StartCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BrowseDestinationCommand { get; }
    public ICommand AddFolderCommand { get; }
    public ICommand OpenDestCommand { get; }
    public ICommand ConfirmShortcutsCommand { get; }
    public ICommand ResumeDecisionCommand { get; }
    public ICommand CloseSummaryCommand { get; }

    public BackupViewModel()
    {
        var settings = SettingsService.Load();
        DestinationPath = string.IsNullOrEmpty(settings.DestinationPath)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TXA_Backup", "Backup", Environment.UserName)
            : settings.DestinationPath;

        // Đồng bộ hóa khi settings thay đổi ở tab khác
        SettingsService.SettingsChanged += (s) => {
            DestinationPath = s.DestinationPath;
        };

        StartCommand = new RelayCommand(_ => _ = StartBackupAsync(), _ => !IsRunning && BackupItems.Any(i => i.IsSelected));
        CancelCommand = new RelayCommand(_ => _cts?.Cancel(), _ => IsRunning);
        BrowseDestinationCommand = new RelayCommand(_ => BrowseDestination());
        AddFolderCommand = new RelayCommand(_ => AddNewFolder());
        OpenDestCommand = new RelayCommand(_ => {
            try { System.Diagnostics.Process.Start("explorer.exe", DestinationPath); }
            catch { }
        });
        ConfirmShortcutsCommand = new RelayCommand(_ => {
            var selected = DetectedShortcuts.Where(s => s.IsSelected).Select(s => s.FullPath).ToList();
            IsShortcutModalOpen = false;
            _shortcutTcs?.SetResult(selected);
        });
        ResumeDecisionCommand = new RelayCommand(p => {
            bool resume = false;
            if (p is bool b) resume = b;
            else if (p is string s) bool.TryParse(s, out resume);
            
            IsResumeModalOpen = false;
            _resumeTcs?.SetResult(resume);
        });
        CloseSummaryCommand = new RelayCommand(_ => IsSummaryModalOpen = false);

        BackupService.LogReceived += msg => {
            RunOnUI(() => {
                if (msg.StartsWith("txa_")) Log(LanguageService.Get(msg));
                else Log(msg);
            });
        };
        BackupService.OverallProgressChanged += p => {
            RunOnUI(() => {
                OverallProgress = p;
                ETAText = LanguageService.Get("txa_eta", TXAFormat.ETA(BackupService.EstimatedRemainingTime));
            });
        };
        
        BackupService.OnShortcutsDetected = async (shortcuts) => {
            _shortcutTcs = new TaskCompletionSource<List<string>>();
            RunOnUI(() => {
                DetectedShortcuts.Clear();
                foreach (var s in shortcuts) DetectedShortcuts.Add(new ShortcutItem { FullPath = s });
                IsShortcutModalOpen = true;
            });
            return await _shortcutTcs.Task;
        };

        _zipper.LogReceived += msg => Log(msg);
        _zipper.ProgressChanged += p => {
            RunOnUI(() => OverallProgress = p);
        };

        LoadDefaultFolders();
    }

    private void RunOnUI(Action action)
    {
        if (Application.Current?.Dispatcher == null)
        {
            action();
            return;
        }

        if (Application.Current.Dispatcher.CheckAccess())
            action();
        else
            Application.Current.Dispatcher.Invoke(action);
    }

    private void LoadDefaultFolders()
    {
        var folders = new[]
        {
            new { Name = "Game MiniWorld", Key = "txa_folder_game", Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"miniworddata410\data\mods"), Icon = "🎮" },
            new { Name = "Cốc Cốc", Key = "txa_folder_browser", Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"CocCoc\Browser\User Data"), Icon = "🌐" },
            new { Name = "Tải xuống", Key = "txa_folder_downloads", Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), Icon = "📥" },
            new { Name = "Ảnh", Key = "txa_folder_pictures", Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), ""), Icon = "🖼️" },
            new { Name = "Video", Key = "txa_folder_videos", Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), ""), Icon = "🎬" },
            new { Name = "Desktop", Key = "txa_folder_desktop", Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ""), Icon = "🖥️" },
            new { Name = "Clipboard history", Key = "txa_folder_clipboard", Path = "CLIPBOARD", Icon = "📋" }
        };

        foreach (var f in folders)
        {
            if (f.Path == "CLIPBOARD" || Directory.Exists(f.Path))
            {
                BackupItems.Add(new BackupItem { 
                    Name = f.Name, 
                    NameKey = f.Key,
                    SourcePath = f.Path, 
                    Icon = f.Icon, 
                    IsAvailable = true 
                });
            }
        }
    }

    private async Task StartBackupAsync()
    {
        if (string.IsNullOrEmpty(DestinationPath)) return;

        IsRunning = true;
        IsBackupInProgress = true;
        LogText = "";
        _cts = new CancellationTokenSource();

        var sets = SettingsService.Load();
        BackupService.SkipShortcuts = sets.SkipShortcuts;

        try
        {
            // Check for existing partial backup
            bool resumeMode = false;
            var selected = BackupItems.Where(i => i.IsSelected).ToList();
            if (selected.Any(i => Directory.Exists(Path.Combine(DestinationPath, i.Name))))
            {
                _resumeTcs = new TaskCompletionSource<bool>();
                RunOnUI(() => IsResumeModalOpen = true);
                resumeMode = await _resumeTcs.Task;
            }

            Log(LanguageService.Get("txa_log_start_backup"));
            if (resumeMode) Log(LanguageService.Get("txa_log_resume_mode"));

            // --- Bổ sung: Tạo thư mục SESSION riêng bộ để đảm bảo "chỉ file đã chọn" ---
            string sessionName = $"TXA_Backup_{DateTime.Now:yyyyMMdd_HHmmss}";
            string actualDest = Path.Combine(DestinationPath, sessionName);
            if (!Directory.Exists(actualDest)) Directory.CreateDirectory(actualDest);

            await Task.Run(async () =>
            {
                long totalCopied = 0;

                try
                {
                    var clipboardItem = selected.FirstOrDefault(i => i.SourcePath == "CLIPBOARD");
                    if (clipboardItem != null)
                    {
                        RunOnUI(() => clipboardItem.StatusKey = "txa_status_copying");

                        string cbFile = "";
                        await Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            cbFile = await _clipboardService.BackupClipboardHistoryAsync(actualDest);
                        }).Task.Unwrap();

                        if (File.Exists(cbFile))
                        {
                            totalCopied += new FileInfo(cbFile).Length;
                        }

                        RunOnUI(() => {
                            clipboardItem.StatusKey = "txa_status_completed";
                            clipboardItem.Progress = 100;
                        });
                        selected.Remove(clipboardItem);
                    }

                    if (selected.Any())
                    {
                        await BackupService.BackupAsync(selected, actualDest, _cts!.Token, resumeMode);
                        totalCopied += BackupService.TotalBytesCopied;
                    }

                    try
                    {
                        string infoPath = Path.Combine(actualDest, "TXA_BACKUP_INFO.txt");
                        string info = LanguageService.Get("txa_info_header", TXAFormat.Time(DateTime.Now)) + "\n" +
                                      "------------------------------------------------------\n" +
                                      LanguageService.Get("txa_info_user", Environment.UserName) + "\n" +
                                      LanguageService.Get("txa_info_machine", Environment.MachineName) + "\n" +
                                      LanguageService.Get("txa_info_files", BackupItems.Where(i => i.IsSelected).Count()) + "\n" +
                                      LanguageService.Get("txa_info_data", TXAFormat.FileSize(totalCopied)) + "\n" +
                                      "Software: TXA Backup Premium 1.0\n" +
                                      LanguageService.Get("txa_info_copyright", DateTime.Now.Year) + "\n";
                        File.WriteAllText(infoPath, info);
                    }
                    catch { }

                    // --- Bước Nén ZIP Toàn bộ Dữ liệu trong Session ---
                    var settingsSet = SettingsService.Load();
                    if (settingsSet.CompressBackup)
                    {
                        RunOnUI(() => {
                            Log(LanguageService.Get("txa_zip_compressing"));
                            OverallProgress = 0;
                        });
                        
                        // Nén ALL (Chỉ những gì trong actualDest của session này)
                        await _zipper.CompressAsync(actualDest, settingsSet.ZipPassword, _cts.Token);
                    }

                    RunOnUI(() =>
                    {
                        OverallProgress = 100;
                        BackupService.NotifyStatusChanged(true);
                        LogText += $"\n{LanguageService.Get("txa_log_finish", TXAFormat.Time(DateTime.Now))}";
                        LogText += $"\n{LanguageService.Get("txa_log_stats", TXAFormat.FileSize(totalCopied))}";
                        
                        var duration = DateTime.Now - (BackupService.StartTime ?? DateTime.Now);
                        ShowSummary(duration);
                    });
                }
                catch (OperationCanceledException)
                {
                    RunOnUI(() => LogText += $"\n{LanguageService.Get("txa_log_cancel")}");
                }
                catch (Exception ex)
                {
                    RunOnUI(() => LogText += $"\n{LanguageService.Get("txa_log_error", ex.Message)}");
                }
            }, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            LogText += $"\n{LanguageService.Get("txa_log_cancel")}";
        }
        catch (Exception ex)
        {
            LogText += $"\n{LanguageService.Get("txa_log_error", ex.Message)}";
        }
        finally
        {
            IsRunning = false;
            IsBackupInProgress = false;
            ETAText = "--:--";
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void ShowSummary(TimeSpan totalTime)
    {
        SummaryTitle = LanguageService.Get("txa_summary_title");
        string content = LanguageService.Get("txa_summary_total_time", TXAFormat.Duration(totalTime)) + "\n";
        content += LanguageService.Get("txa_summary_files", BackupService.FilesProcessed, BackupService.FilesSkipped) + "\n";
        
        int foldersOk = BackupService.FoldersProcessed;
        int foldersFail = BackupService.FoldersFailed;
        
        // Check clipboard item
        var cbItem = BackupItems.FirstOrDefault(i => i.SourcePath == "CLIPBOARD" && i.IsSelected);
        if (cbItem != null)
        {
            if (cbItem.Progress >= 100) foldersOk++;
            else foldersFail++;
        }

        int totalFolders = foldersOk + foldersFail;
        if (totalFolders > 1)
        {
            content += LanguageService.Get("txa_summary_folders", foldersOk, foldersFail);
        }
        
        SummaryContent = content;
        IsSummaryModalOpen = true;
    }

    private void Log(string msg) => LogText += $"[{DateTime.Now:HH:mm:ss}] {msg}\n";

    private void BrowseDestination()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog();
        dialog.InitialDirectory = DestinationPath;
        if (dialog.ShowDialog() == true)
        {
            DestinationPath = dialog.FolderName;
        }
    }

    private void AddNewFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog() {
            Title = LanguageService.Get("txa_choose_folder")
        };
        if (dialog.ShowDialog() == true)
        {
            string path = dialog.FolderName;
            BackupItems.Add(new BackupItem { 
                Name = Path.GetFileName(path).ToUpper(), 
                SourcePath = path, 
                Icon = "📁", 
                IsAvailable = true, 
                IsSelected = true,
                StatusKey = "txa_status_new"
            });
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
