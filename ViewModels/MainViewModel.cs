using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TXABackupTool.Services;

namespace TXABackupTool.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private object? _currentView;
    private readonly BackupViewModel _backupViewModel = new();
    private readonly RestoreViewModel _restoreViewModel = new();
    private readonly LogViewModel _logViewModel = new();
    private readonly StorageViewModel _storageViewModel = new();
    private readonly SettingsViewModel _settingsViewModel = new();
    private bool _isTopmost;
    
    // Global Modal State (TXA PREMIUM MODAL ENGINE)
    private bool _isModalOpen;
    private string _modalTitle = "TXA NOTIFICATION";
    private string _modalContent = "";
    private string _modalOkText = "OK";
    public bool IsModalOpen { get => _isModalOpen; set { _isModalOpen = value; OnPropertyChanged(); } }
    public string ModalTitle { get => _modalTitle; set { _modalTitle = value; OnPropertyChanged(); } }
    public string ModalContent { get => _modalContent; set { _modalContent = value; OnPropertyChanged(); } }
    public string ModalOkText { get => _modalOkText; set { _modalOkText = value; OnPropertyChanged(); } }
    public ICommand CloseModalCommand { get; }

    public object? CurrentView
    {
        get => _currentView;
        set { 
            _currentView = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(IsBackupActive));
            OnPropertyChanged(nameof(IsRestoreActive));
            OnPropertyChanged(nameof(IsLogsActive));
            OnPropertyChanged(nameof(IsStorageActive));
            OnPropertyChanged(nameof(IsSettingsActive));
        }
    }

    public bool IsBackupActive => CurrentView == _backupViewModel;
    public bool IsRestoreActive => CurrentView == _restoreViewModel;
    public bool IsLogsActive => CurrentView == _logViewModel;
    public bool IsStorageActive => CurrentView == _storageViewModel;
    public bool IsSettingsActive => CurrentView == _settingsViewModel;

    public string NavBackup => LanguageService.Get("txa_nav_backup");
    public string NavRestore => LanguageService.Get("txa_nav_restore");
    public string NavLogs => LanguageService.Get("txa_nav_logs");
    public string NavStorage => LanguageService.Get("txa_nav_storage");
    public string NavSettings => LanguageService.Get("txa_nav_settings");
    public string LabelLang => LanguageService.Get("txa_label_lang");
    public string BtnOpenTm => LanguageService.Get("txa_btn_open_tm");
    
    // Status Bar Properties
    private string _statusFiles = "● 0 items";
    public string StatusFiles { get => _statusFiles; set { _statusFiles = value; OnPropertyChanged(); } }
    
    private string _statusSkipped = "▲ 0 skipped";
    public string StatusSkipped { get => _statusSkipped; set { _statusSkipped = value; OnPropertyChanged(); } }
    
    private string _statusSize = "📦 Ready";
    public string StatusSize { get => _statusSize; set { _statusSize = value; OnPropertyChanged(); } }
    
    public bool IsTopmost { get => _isTopmost; set { _isTopmost = value; OnPropertyChanged(); } }

    public ICommand NavBackupCommand { get; }
    public ICommand NavRestoreCommand { get; }
    public ICommand NavLogsCommand { get; }
    public ICommand NavStorageCommand { get; }
    public ICommand NavSettingsCommand { get; }
    public ICommand OpenInstallDirCommand { get; }

    public ObservableCollection<LanguageItem> AvailableLanguages { get; } = new();
    private LanguageItem? _selectedLanguage;
    public LanguageItem? SelectedLanguage
    {
        get => _selectedLanguage;
        set {
            if (_selectedLanguage == value) return;
            _selectedLanguage = value;
            if (value != null) 
            {
                LanguageService.LoadLanguage(value.Path);
                var s = SettingsService.Load();
                s.LastLanguagePath = value.Path;
                SettingsService.Save(s);
                
                // Đồng bộ sang SettingsViewModel nếu đang hiển thị
                if (_settingsViewModel.SelectedLanguage?.Path != value.Path)
                    _settingsViewModel.SyncLanguage(value.Path);
            }
            RefreshAllUI();
        }
    }

    /// <summary>
    /// Xử lý nhập file ngôn ngữ .txaf với 2 tầng bảo mật (TXA IMPORT CORE)
    /// </summary>
    public void ImportLanguageFile(string filePath)
    {
        var result = LanguageService.ImportLanguage(filePath);
        
        System.Windows.Application.Current.Dispatcher.Invoke(() => {
            ModalTitle = result.Status switch {
                LanguageService.ValidationStatus.Success => "✅ SUCCESS",
                LanguageService.ValidationStatus.Layer1Invalid => "❌ ERROR (FORMAT)",
                LanguageService.ValidationStatus.Layer2Invalid => "❌ ERROR (CONTENT)",
                LanguageService.ValidationStatus.AlreadyExists => "⚠️ WARNING",
                _ => "INFO"
            };
            
            ModalOkText = LanguageService.Get("txa_btn_modal_ok");
            ModalContent = result.Message;
            IsModalOpen = true;

            if (result.Status == LanguageService.ValidationStatus.Success)
            {
                // Refresh list
                AvailableLanguages.Clear();
                foreach (var l in LanguageService.AvailableLanguages)
                {
                    AvailableLanguages.Add(new LanguageItem { Name = l.Name, Path = l.Path, Description = l.Description });
                }
                
                // Switch to new language automatically if successful
                var target = AvailableLanguages.FirstOrDefault(l => l.Name == result.LangName);
                if (target != null) SelectedLanguage = target;
            }
        });
    }

    public void RefreshAllUI()
    {
        OnPropertyChanged(nameof(NavBackup));
        OnPropertyChanged(nameof(NavRestore));
        OnPropertyChanged(nameof(NavLogs));
        OnPropertyChanged(nameof(NavStorage));
        OnPropertyChanged(nameof(NavSettings));
        OnPropertyChanged(nameof(LabelLang));
        OnPropertyChanged(nameof(BtnOpenTm));
        OnPropertyChanged(nameof(SelectedLanguage));
        OnPropertyChanged(nameof(ModalOkText));
        
        // Cập nhật tên động cho nhãn Việt Nam
        foreach (var item in AvailableLanguages)
        {
            if (item.Path.ToLower().Contains("vietnam"))
                item.Name = LanguageService.Get("txa_lang_vi_name");
        }
        
        _backupViewModel.RefreshUI();
        _restoreViewModel.RefreshUI();
        _logViewModel.RefreshUI();
        _storageViewModel.RefreshUI();
        _settingsViewModel.RefreshUI();

        UpdateStatusBar();
    }

    public class LanguageItem : INotifyPropertyChanged
    { 
        private string _name = "";
        public string Name { get => _name; set { if (_name == value) return; _name = value; OnPropertyChanged(); } }
        public string Path { get; set; } = ""; 
        public string Description { get; set; } = ""; 
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public MainViewModel()
    {
        NavBackupCommand = new RelayCommand(_ => CurrentView = _backupViewModel);
        NavRestoreCommand = new RelayCommand(_ => CurrentView = _restoreViewModel);
        NavLogsCommand = new RelayCommand(_ => { 
            _logViewModel.RefreshLogs();
            CurrentView = _logViewModel; 
        });
        NavStorageCommand = new RelayCommand(_ => CurrentView = _storageViewModel);
        NavSettingsCommand = new RelayCommand(_ => CurrentView = _settingsViewModel);
        OpenInstallDirCommand = new RelayCommand(_ => {
            try { System.Diagnostics.Process.Start("explorer.exe", AppContext.BaseDirectory); }
            catch { }
        });
        CloseModalCommand = new RelayCommand(_ => IsModalOpen = false);

        // Scan languages only if not already done
        if (!LanguageService.AvailableLanguages.Any())
        {
            LanguageService.GetAvailableLanguages();
        }

        foreach (var l in LanguageService.AvailableLanguages)
        {
            AvailableLanguages.Add(new LanguageItem { Name = l.Name, Path = l.Path, Description = l.Description });
        }

        // Load Settings & Apply initial language
        var settings = SettingsService.Load();
        IsTopmost = settings.Topmost;
        SettingsService.SettingsChanged += s => {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                IsTopmost = s.Topmost;
            });
        };
        if (!string.IsNullOrEmpty(settings.LastLanguagePath)) 
        {
            var target = AvailableLanguages.FirstOrDefault(l => l.Path == settings.LastLanguagePath);
            if (target != null) _selectedLanguage = target;
        }

        if (_selectedLanguage == null) {
            // System Language Detection for first run
            bool isVi = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi";
            var target = isVi 
                ? AvailableLanguages.FirstOrDefault(l => l.Name.Contains("Tiếng Việt")) 
                : AvailableLanguages.FirstOrDefault(l => l.Name.Contains("English"));
            
            _selectedLanguage = target ?? AvailableLanguages.FirstOrDefault();
        }

        // Đảm bảo LanguageService đã load đúng file được chọn
        if (_selectedLanguage != null) {
            LanguageService.LoadLanguage(_selectedLanguage.Path);
        }

        // Initial view
        CurrentView = _backupViewModel;
        
        UpdateStatusBar();
        
        // Refresh all UI elements to ensure they match the loaded language
        RefreshAllUI();

        // Đăng ký sự kiện từ SettingsViewModel để đồng bộ ngược lại sidebar
        _settingsViewModel.LanguageChanged = () => {
             var path = _settingsViewModel.SelectedLanguage?.Path;
             if (path != null && SelectedLanguage?.Path != path)
             {
                 SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Path == path);
             }
        };

        // Subscribe to backup status changes to update status bar
        _backupViewModel.BackupService.StatusChanged += UpdateStatusBar;
        _backupViewModel.BackupService.OverallProgressChanged += _ => UpdateStatusBar();

        // Đồng bộ hóa ngôn ngữ khi cài đặt thay đổi từ tab khác (như SettingsPage)
        SettingsService.SettingsChanged += (s) => {
            if (!string.IsNullOrEmpty(s.LastLanguagePath) && SelectedLanguage?.Path != s.LastLanguagePath)
            {
                var lang = AvailableLanguages.FirstOrDefault(l => l.Path == s.LastLanguagePath);
                if (lang != null) 
                {
                    // Chạy trên UI Thread để cập nhật Dropdown ngay lập tức
                    System.Windows.Application.Current.Dispatcher.Invoke(() => {
                        _selectedLanguage = lang;
                        OnPropertyChanged(nameof(SelectedLanguage));
                        OnPropertyChanged(nameof(NavBackup));
                        OnPropertyChanged(nameof(NavRestore));
                        OnPropertyChanged(nameof(NavLogs));
                        OnPropertyChanged(nameof(NavStorage));
                        OnPropertyChanged(nameof(NavSettings));
                        OnPropertyChanged(nameof(LabelLang));
                        OnPropertyChanged(nameof(BtnOpenTm));
                        RefreshAllUI();
                    });
                }
            }
        };
    }

    private void UpdateStatusBar()
    {
        // Must update on UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() => {
            StatusFiles = LanguageService.Get("txa_status_processed", _backupViewModel.BackupService.FilesProcessed);
            StatusSkipped = LanguageService.Get("txa_status_skipped", _backupViewModel.BackupService.FilesSkipped);
            StatusSize = $"📦 {TXABackupTool.Services.TXAFormat.FileSize(_backupViewModel.BackupService.TotalBytesCopied)}";
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
