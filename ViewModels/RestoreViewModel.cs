using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using TXABackupTool.Services;

namespace TXABackupTool.ViewModels;

public class RestoreViewModel : INotifyPropertyChanged
{
    private readonly ClipboardService _clipboardService = new();
    private string _backupFolderPath = "";
    private string _logText = "";
    private bool _isRestoring;
    private string _zipPassword = "";
    private readonly TXAZipper _zipper = new();

    public string BackupFolderPath
    {
        get => _backupFolderPath;
        set { 
            _backupFolderPath = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(IsPathValid));
            OnPropertyChanged(nameof(SourceDirText));
        }
    }

    public string ZipPassword
    {
        get => _zipPassword;
        set { _zipPassword = value; OnPropertyChanged(); }
    }

    public bool IsPathValid
    {
        get {
            if (string.IsNullOrWhiteSpace(BackupFolderPath)) return false;
            var s = SettingsService.Load();
            if (s.CompressBackup) return File.Exists(BackupFolderPath) && Path.GetExtension(BackupFolderPath).ToLower() == ".zip";
            return Directory.Exists(BackupFolderPath);
        }
    }

    private bool _isModalOpen;
    public bool IsModalOpen 
    { 
        get => _isModalOpen; 
        set { _isModalOpen = value; OnPropertyChanged(); }
    }

    public string ModalTitle => LanguageService.Get("txa_summary_error");
    private string _modalMessage = "";
    public string ModalMessage 
    { 
        get => _modalMessage; 
        set { _modalMessage = value; OnPropertyChanged(); }
    }

    public string LogText
    {
        get => _logText;
        set { _logText = value; OnPropertyChanged(); }
    }

    public bool IsRestoring
    {
        get => _isRestoring;
        set { _isRestoring = value; OnPropertyChanged(); }
    }

    public string Title => LanguageService.Get("txa_restore_title");
    public string RestoreCbText => LanguageService.Get("txa_restore_clipboard");
    public string RestoreDesc => LanguageService.Get("txa_restore_desc");
    public string IncludeText => LanguageService.Get("txa_restore_include_text");
    public string StartText => LanguageService.Get("txa_start");
    public string LogsTitle => LanguageService.Get("txa_logs");
    public string BrowseText => LanguageService.Get("txa_browse");
    public string SourceDirText 
    {
        get {
            var s = SettingsService.Load();
            return s.CompressBackup ? LanguageService.Get("txa_restore_zip_source") : LanguageService.Get("txa_source_dir");
        }
    }
    public string CategoryText => LanguageService.Get("txa_category");
    public string BtnOk => LanguageService.Get("txa_btn_ok");
    public string ZipPasswordLabel => LanguageService.Get("txa_restore_zip_password_label");
    public bool IsZipEnabled => SettingsService.Load().CompressBackup;

    public void RefreshUI()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(RestoreCbText));
        OnPropertyChanged(nameof(RestoreDesc));
        OnPropertyChanged(nameof(IncludeText));
        OnPropertyChanged(nameof(StartText));
        OnPropertyChanged(nameof(SourceDirText));
        OnPropertyChanged(nameof(BrowseText));
        OnPropertyChanged(nameof(CategoryText));
        OnPropertyChanged(nameof(BtnOk));
        OnPropertyChanged(nameof(IsZipEnabled));
    }

    public ICommand BrowseBackupCommand { get; }
    public ICommand StartRestoreCommand { get; }

    public RestoreViewModel()
    {
        var settings = SettingsService.Load();
        BackupFolderPath = settings.DestinationPath; // Lấy từ cài đặt thực tế
        
        BrowseBackupCommand = new RelayCommand(_ => {
            var s = SettingsService.Load();
            if (s.CompressBackup)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog();
                dialog.Filter = "TXA Backup ZIP (*.zip)|*.zip";
                if (dialog.ShowDialog() == true) BackupFolderPath = dialog.FileName;
            }
            else
            {
                var dialog = new Microsoft.Win32.OpenFolderDialog();
                if (dialog.ShowDialog() == true) BackupFolderPath = dialog.FolderName;
            }
        });

        StartRestoreCommand = new RelayCommand(async _ => await DoRestore());

        // Đồng bộ hóa khi settings thay đổi ở tab khác
        SettingsService.SettingsChanged += (s) => {
            BackupFolderPath = s.DestinationPath;
        };
    }

    private async Task DoRestore()
    {
        if (IsRestoring) return;

        if (string.IsNullOrWhiteSpace(BackupFolderPath))
        {
            ModalMessage = LanguageService.Get("txa_restore_no_folder_err");
            IsModalOpen = true;
            return;
        }

        if (!IsPathValid)
        {
            ModalMessage = LanguageService.Get("txa_restore_invalid_err");
            IsModalOpen = true;
            return;
        }

        IsRestoring = true;
        LogText = "";
        
        string activeRestorePath = BackupFolderPath;
        string? tempDir = null;

        try
        {
            var s = SettingsService.Load();
            if (s.CompressBackup)
            {
                Log(LanguageService.Get("txa_zip_decompressing"));
                tempDir = Path.Combine(Path.GetTempPath(), "TXA_RESTORE_" + Path.GetRandomFileName());
                bool unzipOk = await _zipper.DecompressAsync(BackupFolderPath, tempDir, ZipPassword);
                if (!unzipOk) throw new Exception("DECOMPRESS_FAILED");
                activeRestorePath = tempDir;
                Log(LanguageService.Get("txa_zip_decompress_ok"));
            }

            Log(LanguageService.Get("txa_log_restore_cb"));
            
            var (success, failed) = await _clipboardService.RestoreClipboardHistoryAsync(
                activeRestorePath,
                msg => Log(msg)
            );

            if (failed == 0)
                Log(LanguageService.Get("txa_log_restore_ok"));
            else
                Log($"[WARN] {success} OK, {failed} failed.");
        }
        catch (Exception ex)
        {
            if (ex.Message == "PASSWORD_INVALID")
                Log(LanguageService.Get("txa_zip_password_wrong"));
            else
                Log($"[ERROR] {ex.Message}");
        }
        finally
        {
            if (tempDir != null && Directory.Exists(tempDir))
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
            IsRestoring = false;
        }
    }

    private void Log(string msg) => LogText += $"[{DateTime.Now:HH:mm:ss}] {msg}\n";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
