using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TXABackupTool.Services;

namespace TXABackupTool.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private bool _skipShortcuts;
    private bool _autoLangDetect = true;
    private bool _topmost;

    public SettingsViewModel()
    {
        OpenInstallDirCommand = new RelayCommand(_ => {
            try { System.Diagnostics.Process.Start("explorer.exe", AppContext.BaseDirectory); } catch { }
        });
        OpenContactCommand = new RelayCommand(_ => {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://fb.com/vlog.txa.2311") { UseShellExecute = true }); } catch { }
        });

        LoadSettings();

        // Language list
        if (!LanguageService.AvailableLanguages.Any())
        {
            LanguageService.GetAvailableLanguages();
        }

        foreach (var l in LanguageService.AvailableLanguages)
        {
            AvailableLanguages.Add(new MainViewModel.LanguageItem { Name = l.Name, Path = l.Path });
        }

        // Chọn ngôn ngữ hiện tại
        var s = SettingsService.Load();
        if (!string.IsNullOrEmpty(s.LastLanguagePath))
        {
            _selectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Path.Equals(s.LastLanguagePath, StringComparison.OrdinalIgnoreCase));
        }
        
        // Nếu chưa có (first run), fallback theo LanguageService
        if (_selectedLanguage == null)
        {
            _selectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Path.Equals(LanguageService.CurrentLanguagePath, StringComparison.OrdinalIgnoreCase))
                               ?? AvailableLanguages.FirstOrDefault();
        }
        OnPropertyChanged(nameof(SelectedLanguage));

        // Đồng bộ hóa khi sidebar đổi ngôn ngữ
        SettingsService.SettingsChanged += (newSettings) => {
            // Chạy trên UI Thread để cập nhật UI
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                bool changed = false;
                if (_skipShortcuts != newSettings.SkipShortcuts) { _skipShortcuts = newSettings.SkipShortcuts; OnPropertyChanged(nameof(SkipShortcuts)); changed = true; }
                if (_autoLangDetect != newSettings.AutoLangDetect) { _autoLangDetect = newSettings.AutoLangDetect; OnPropertyChanged(nameof(AutoLangDetect)); changed = true; }
                if (_topmost != newSettings.Topmost) { _topmost = newSettings.Topmost; OnPropertyChanged(nameof(Topmost)); changed = true; }
                
                if (!string.IsNullOrEmpty(newSettings.LastLanguagePath) && SelectedLanguage?.Path != newSettings.LastLanguagePath)
                {
                    SyncLanguage(newSettings.LastLanguagePath);
                    changed = true;
                }
                
                if (changed) RefreshUI();
            });
        };
    }

    // ---- i18n labels ----
    public string Title => LanguageService.Get("txa_nav_settings");
    public string SubTitle => LanguageService.Get("txa_settings_subtitle");
    public string LangSectionTitle => LanguageService.Get("txa_settings_lang_section");
    public string LangTitle => LanguageService.Get("txa_label_lang");
    public string LangDesc => LanguageService.Get("txa_settings_lang_desc");
    public string BehaviorSectionTitle => LanguageService.Get("txa_settings_behavior_section");
    public string TopmostTitle => LanguageService.Get("txa_settings_topmost_title");
    public string TopmostDesc => LanguageService.Get("txa_settings_topmost_desc");
    public string SkipShortcutsTitle => LanguageService.Get("txa_settings_skip_shortcuts_title");
    public string SkipShortcutsDesc => LanguageService.Get("txa_settings_skip_shortcuts_desc");
    public string AutoLangTitle => LanguageService.Get("txa_settings_autolang_title");
    public string AutoLangDesc => LanguageService.Get("txa_settings_autolang_desc");
    public string AboutSectionTitle => LanguageService.Get("txa_settings_about_section");
    public string VersionText => LanguageService.Get("txa_settings_version");
    public string AuthorText => LanguageService.Get("txa_settings_author");
    public string ContactUrl => LanguageService.Get("txa_settings_contact");
    public string CopyrightText => LanguageService.Get("txa_info_copyright", DateTime.Now.Year);
    public string BtnOpenInstallDir => LanguageService.Get("txa_btn_open_tm");

    // ---- Settings props ----
    public bool SkipShortcuts
    {
        get => _skipShortcuts;
        set { if (_skipShortcuts == value) return; _skipShortcuts = value; OnPropertyChanged(); SaveSettings(); }
    }

    public bool AutoLangDetect
    {
        get => _autoLangDetect;
        set { if (_autoLangDetect == value) return; _autoLangDetect = value; OnPropertyChanged(); SaveSettings(); }
    }

    public bool Topmost
    {
        get => _topmost;
        set { if (_topmost == value) return; _topmost = value; OnPropertyChanged(); SaveSettings(); }
    }

    // ---- Language ----
    public ObservableCollection<MainViewModel.LanguageItem> AvailableLanguages { get; } = new();
    private MainViewModel.LanguageItem? _selectedLanguage;
    public MainViewModel.LanguageItem? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage == value) return;
            _selectedLanguage = value;
            if (value != null)
            {
                LanguageService.LoadLanguage(value.Path);
                var s = SettingsService.Load();
                s.LastLanguagePath = value.Path;
                SettingsService.Save(s);
                // Notify language changed globally if needed
                LanguageChanged?.Invoke();
            }
            OnPropertyChanged();
        }
    }

    public Action? LanguageChanged { get; set; }

    public void SyncLanguage(string path)
    {
        var lang = AvailableLanguages.FirstOrDefault(l => l.Path == path);
        if (lang != null)
        {
            _selectedLanguage = lang;
            OnPropertyChanged(nameof(SelectedLanguage));
            RefreshUI();
        }
    }

    public ICommand OpenInstallDirCommand { get; }
    public ICommand OpenContactCommand { get; }

    private void LoadSettings()
    {
        var s = SettingsService.Load();
        _skipShortcuts = s.SkipShortcuts;
        _autoLangDetect = s.AutoLangDetect;
        _topmost = s.Topmost;
    }

    private void SaveSettings()
    {
        var s = SettingsService.Load();
        s.SkipShortcuts = _skipShortcuts;
        s.AutoLangDetect = _autoLangDetect;
        s.Topmost = _topmost;
        SettingsService.Save(s);
    }

    public void RefreshUI()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SubTitle));
        OnPropertyChanged(nameof(LangSectionTitle));
        OnPropertyChanged(nameof(LangTitle));
        OnPropertyChanged(nameof(LangDesc));
        OnPropertyChanged(nameof(BehaviorSectionTitle));
        OnPropertyChanged(nameof(SkipShortcutsTitle));
        OnPropertyChanged(nameof(SkipShortcutsDesc));
        OnPropertyChanged(nameof(AutoLangTitle));
        OnPropertyChanged(nameof(AutoLangDesc));
        OnPropertyChanged(nameof(AboutSectionTitle));
        OnPropertyChanged(nameof(VersionText));
        OnPropertyChanged(nameof(AuthorText));
        OnPropertyChanged(nameof(ContactUrl));
        OnPropertyChanged(nameof(CopyrightText));
        OnPropertyChanged(nameof(BtnOpenInstallDir));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
