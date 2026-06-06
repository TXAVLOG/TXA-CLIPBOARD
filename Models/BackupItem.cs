using System.ComponentModel;
using System.Runtime.CompilerServices;
using TXABackupTool.Services;

namespace TXABackupTool.Models;

public class BackupItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private double _progress;
    private bool _isAvailable;

    public string Name { get; set; } = string.Empty;
    public string NameKey { get; set; } = string.Empty;
    public string DisplayName => !string.IsNullOrEmpty(NameKey) ? LanguageService.Get(NameKey) : Name;
    public string SourcePath { get; set; } = string.Empty;
    public string Icon { get; set; } = "📂";
    
    public bool IsAvailable
    {
        get => _isAvailable;
        set { _isAvailable = value; OnPropertyChanged(); }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public double Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    private string _status = "";
    public string StatusKey { get; set; } = "txa_status_new";
    public string Status
    {
        get => !string.IsNullOrEmpty(StatusKey) ? LanguageService.Get(StatusKey) : _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public long TotalBytes { get; set; }
    public long CopiedBytes { get; set; }
    public bool IsCustom { get; set; }

    public void NotifyMetadataChanged()
    {
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(Status));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
