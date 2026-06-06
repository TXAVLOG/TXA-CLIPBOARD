using System.ComponentModel;
using System.Runtime.CompilerServices;
using TXABackupTool.Services;

namespace TXABackupTool.ViewModels;

public class ComingSoonViewModel : INotifyPropertyChanged
{
    public string Title => LanguageService.Get("txa_coming_soon");
    public string Description => LanguageService.Get("txa_coming_soon_desc");

    public void Refresh()
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
