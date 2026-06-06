using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TXABackupTool.Models;
using TXABackupTool.Services;

namespace TXABackupTool.ViewModels;

public class StorageViewModel : INotifyPropertyChanged
{
    private readonly System.Windows.Threading.DispatcherTimer _timer;
    // ---- Drive info ----
    private ObservableCollection<DriveItemViewModel> _drives = new();

    private string _currentDestPath = "";
    private bool _autoCleanEnabled = true;
    private bool _compressEnabled;
    private string _zipPassword = "";

    public StorageViewModel()
    {
        RefreshDrivesCommand = new RelayCommand(_ => RefreshDrives());
        ChangeFolderCommand = new RelayCommand(_ => ChangeFolder());
        OpenDestCommand = new RelayCommand(_ => OpenDestFolder());
        RefreshDrives();
        LoadCurrentDest();
        LoadSettings();

        // Khởi tạo Timer cập nhật thời gian thực mỗi 5 giây
        _timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _timer.Tick += (s, e) => {
             RefreshDrives();
             OnPropertyChanged(nameof(LastUpdated));
        };
        _timer.Start();

        // Đồng bộ hóa khi settings thay đổi ở tab khác
        SettingsService.SettingsChanged += (s) => {
            // Cập nhật backing fields trực tiếp để tránh kích hoạt SaveSettings() gây đệ quy
            bool changed = false;
            if (_currentDestPath != s.DestinationPath) { _currentDestPath = s.DestinationPath; OnPropertyChanged(nameof(CurrentDestPath)); OnPropertyChanged(nameof(LocalPathValue)); changed = true; }
            if (_autoCleanEnabled != s.AutoCleanLogs) { _autoCleanEnabled = s.AutoCleanLogs; OnPropertyChanged(nameof(AutoCleanEnabled)); changed = true; }
            if (_compressEnabled != s.CompressBackup) { _compressEnabled = s.CompressBackup; OnPropertyChanged(nameof(CompressEnabled)); changed = true; }
            if (_zipPassword != s.ZipPassword) { _zipPassword = s.ZipPassword; OnPropertyChanged(nameof(ZipPassword)); changed = true; }
            
            if (changed) OnPropertyChanged(nameof(LastUpdated));
        };
    }

    public string LastUpdated => LanguageService.Get("txa_storage_last_updated", DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));

    // ---- UI Labels (i18n) ----
    public string Title => LanguageService.Get("txa_nav_storage");
    public string SubTitle => LanguageService.Get("txa_storage_subtitle");
    public string DriveStatsTitle => LanguageService.Get("txa_storage_drive_stats");
    public string BtnRefresh => LanguageService.Get("txa_storage_refresh");
    public string QuickActionsTitle => LanguageService.Get("txa_storage_quick_actions");
    public string BtnChangeFolder => LanguageService.Get("txa_storage_change_folder");
    public string BtnOpenFolder => LanguageService.Get("txa_storage_open_folder");
    public string BackupLocationsTitle => LanguageService.Get("txa_storage_backup_locations");
    public string LocalPathLabel => LanguageService.Get("txa_storage_local_path");
    public string CloudSyncLabel => LanguageService.Get("txa_storage_cloud_sync");
    public string CloudSyncStatus => LanguageService.Get("txa_storage_cloud_status");
    public string CloudSyncConnected => LanguageService.Get("txa_storage_not_connected");
    public string StorageSettingsTitle => LanguageService.Get("txa_storage_settings_title");
    public string AutoCleanTitle => LanguageService.Get("txa_storage_autoclean_title");
    public string AutoCleanDesc => LanguageService.Get("txa_storage_autoclean_desc");
    public string CompressTitle => LanguageService.Get("txa_storage_compress_title");
    public string CompressDesc => LanguageService.Get("txa_storage_compress_desc");
    public string LiveStatus => LanguageService.Get("txa_live_status");
    public string PasswordInputLabel => LanguageService.Get("txa_zip_password_input");
    public string BtnOk => LanguageService.Get("txa_btn_ok");

    public ObservableCollection<DriveItemViewModel> Drives { get => _drives; set { _drives = value; OnPropertyChanged(); } }

    // ---- Paths ----
    public string CurrentDestPath { get => _currentDestPath; set { if (_currentDestPath == value) return; _currentDestPath = value; OnPropertyChanged(); OnPropertyChanged(nameof(LocalPathValue)); } }
    public string LocalPathValue => _currentDestPath;

    // ---- Settings ----
    public bool AutoCleanEnabled
    {
        get => _autoCleanEnabled;
        set { if (_autoCleanEnabled == value) return; _autoCleanEnabled = value; OnPropertyChanged(); SaveSettings(); }
    }
    public bool CompressEnabled
    {
        get => _compressEnabled;
        set { if (_compressEnabled == value) return; _compressEnabled = value; OnPropertyChanged(); SaveSettings(); }
    }
    public string ZipPassword
    {
        get => _zipPassword;
        set { if (_zipPassword == value) return; _zipPassword = value; OnPropertyChanged(); SaveSettings(); }
    }

    // ---- Commands ----
    public ICommand RefreshDrivesCommand { get; }
    public ICommand ChangeFolderCommand { get; }
    public ICommand OpenDestCommand { get; }

    // ---- Methods ----
    private void RefreshDrives()
    {
        TXALogger.Log(LogType.App, "Refreshing drives...");
        try
        {
            var systemDrive = Path.GetPathRoot(Environment.SystemDirectory)?.TrimEnd('\\');
            if (string.IsNullOrEmpty(systemDrive))
            {
                 TXALogger.Log(LogType.Error, "Could not determine system drive.");
                 return;
            }

            // Get the physical disk index of the system drive
            string? diskIndex = null;
            using (var searcher = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{systemDrive}'}} WHERE AssocClass = Win32_LogicalDiskToPartition"))
            {
                foreach (var partition in searcher.Get())
                {
                    using (var diskSearcher = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition"))
                    {
                        foreach (var disk in diskSearcher.Get())
                        {
                            diskIndex = disk["Index"]?.ToString();
                            break;
                        }
                    }
                    if (diskIndex != null) break;
                }
            }

            if (diskIndex != null)
            {
                 TXALogger.Log(LogType.App, $"System disk found at PHYSICALDRIVE{diskIndex}");
            }
            else
            {
                 TXALogger.Log(LogType.Error, $"Could not find physical disk for system drive {systemDrive}");
                 return;
            }

            // Get all partitions on this physical disk and their logical drives
            var systemDiskDrives = new List<string>();
            using (var partitionSearcher = new ManagementObjectSearcher($"SELECT DeviceID FROM Win32_DiskPartition WHERE DiskIndex = {diskIndex}"))
            {
                foreach (var partition in partitionSearcher.Get())
                {
                    using (var logicalSearcher = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE AssocClass = Win32_LogicalDiskToPartition"))
                    {
                        foreach (var logical in logicalSearcher.Get())
                        {
                            var deviceId = logical["DeviceID"]?.ToString();
                            if (!string.IsNullOrEmpty(deviceId)) systemDiskDrives.Add(deviceId);
                        }
                    }
                }
            }

            var updatedDrives = new List<DriveItemViewModel>();
            foreach (var driveName in systemDiskDrives)
            {
                var drive = new DriveInfo(driveName);
                if (!drive.IsReady) continue;

                double pct = Math.Round((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize * 100);
                string usedStr = TXAFormat.FileSize(drive.TotalSize - drive.AvailableFreeSpace);
                string totalStr = $"/ {TXAFormat.FileSize(drive.TotalSize)}";
                string freeStr = TXAFormat.FileSize(drive.AvailableFreeSpace);
                string pctStr = pct >= 100
                    ? LanguageService.Get("txa_storage_drive_full")
                    : LanguageService.Get("txa_storage_drive_usage", freeStr, pct);

                var brush = (System.Windows.Media.Brush)System.Windows.Application.Current.FindResource(
                    pct > 90 ? "ErrorBrush" : (pct > 70 ? "WarningBrush" : "PrimaryBrush"));

                string label = drive.Name.StartsWith(systemDrive, StringComparison.OrdinalIgnoreCase) 
                    ? LanguageService.Get("txa_storage_system_drive", drive.Name.TrimEnd('\\'))
                    : LanguageService.Get("txa_storage_data_partition", drive.Name.TrimEnd('\\'));

                updatedDrives.Add(new DriveItemViewModel
                {
                    Label = label,
                    Used = usedStr,
                    Total = totalStr,
                    Pct = pctStr,
                    PctValue = pct,
                    ColorBrush = brush
                });
            }

            // Update collection on UI thread
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Drives.Clear();
                foreach (var d in updatedDrives) Drives.Add(d);
            });
            TXALogger.Log(LogType.App, $"Found and updated {updatedDrives.Count} partitions.");
        }
        catch (Exception ex)
        {
             TXALogger.Log(LogType.Error, $"RefreshDrives failed: {ex.Message}");
        }
    }

    private void LoadCurrentDest()
    {
        var settings = SettingsService.Load();
        CurrentDestPath = string.IsNullOrEmpty(settings.DestinationPath)
            ? Path.Combine(AppContext.BaseDirectory, "TXA", "Backup")
            : settings.DestinationPath;
    }

    private void ChangeFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog() { Title = LanguageService.Get("txa_dialog_choose_dest") };
        if (dialog.ShowDialog() == true)
        {
            CurrentDestPath = dialog.FolderName;
            var s = SettingsService.Load();
            s.DestinationPath = dialog.FolderName;
            SettingsService.Save(s);
        }
    }

    private void OpenDestFolder()
    {
        try
        {
            if (!Directory.Exists(CurrentDestPath)) Directory.CreateDirectory(CurrentDestPath);
            System.Diagnostics.Process.Start("explorer.exe", CurrentDestPath);
        }
        catch { }
    }

    private void LoadSettings()
    {
        var s = SettingsService.Load();
        _autoCleanEnabled = s.AutoCleanLogs;
        _compressEnabled = s.CompressBackup;
        _zipPassword = s.ZipPassword;
    }

    private void SaveSettings()
    {
        var s = SettingsService.Load();
        s.AutoCleanLogs = _autoCleanEnabled;
        s.CompressBackup = _compressEnabled;
        s.ZipPassword = _zipPassword;
        SettingsService.Save(s);
    }

    public void RefreshUI()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SubTitle));
        OnPropertyChanged(nameof(DriveStatsTitle));
        OnPropertyChanged(nameof(BtnRefresh));
        OnPropertyChanged(nameof(QuickActionsTitle));
        OnPropertyChanged(nameof(BtnChangeFolder));
        OnPropertyChanged(nameof(BtnOpenFolder));
        OnPropertyChanged(nameof(BackupLocationsTitle));
        OnPropertyChanged(nameof(LocalPathLabel));
        OnPropertyChanged(nameof(CloudSyncLabel));
        OnPropertyChanged(nameof(CloudSyncStatus));
        OnPropertyChanged(nameof(CloudSyncConnected));
        OnPropertyChanged(nameof(StorageSettingsTitle));
        OnPropertyChanged(nameof(AutoCleanTitle));
        OnPropertyChanged(nameof(AutoCleanDesc));
        OnPropertyChanged(nameof(CompressTitle));
        OnPropertyChanged(nameof(CompressDesc));
        OnPropertyChanged(nameof(LiveStatus));
        OnPropertyChanged(nameof(PasswordInputLabel));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
