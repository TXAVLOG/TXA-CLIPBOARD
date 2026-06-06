using System;
using System.IO;
using System.Text.Json;

namespace TXABackupTool.Services;

public class AppSettings
{
    public string LastLanguagePath { get; set; } = "";
    public string DestinationPath { get; set; } = "";
    public bool AutoCleanLogs { get; set; } = true;
    public bool CompressBackup { get; set; } = false;
    public string ZipPassword { get; set; } = "";
    public bool SkipShortcuts { get; set; } = true;
    public bool AutoLangDetect { get; set; } = true;
    public bool Topmost { get; set; } = false;
}

public static class SettingsService
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TXABackupTool",
        "settings.json"
    );

    public static event Action<AppSettings>? SettingsChanged;

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            string dir = Path.GetDirectoryName(SettingsPath)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);

            // Thông báo cập nhật tức thì cho toàn bộ app
            SettingsChanged?.Invoke(settings);
        }
        catch { }
    }
}
