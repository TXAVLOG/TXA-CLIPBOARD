using System;
using System.IO;

namespace TXABackupTool.Services;

#region Enum
public enum LogType
{
    Startup,
    App,
    Error,
    Backup,
    Localization
}
#endregion

public static class TXALogger
{
    #region Infrastructure
    private static readonly object _lock = new();
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TXABackupTool",
        "Logs"
    );

    static TXALogger()
    {
        try { if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir); }
        catch { }
    }
    #endregion

    #region Logging Operations
    public static void Log(LogType type, string message, string? stackTrace = null)
    {
        lock (_lock)
        {
            try
            {
                string fileName = type switch
                {
                    LogType.Startup => "startup_debug.log",
                    LogType.Error => "error.log",
                    LogType.Backup => "backup.log",
                    LogType.Localization => "localization.log",
                    _ => "app.log"
                };

                string path = Path.Combine(LogDir, fileName);
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logLine = $"[{timestamp}] {message}";
                if (!string.IsNullOrEmpty(stackTrace)) logLine += Environment.NewLine + stackTrace;
                
                File.AppendAllText(path, logLine + Environment.NewLine);
            }
            catch { }
        }
    }

    public static string GetLogs(LogType type)
    {
        lock (_lock)
        {
            string fileName = type switch
            {
                LogType.Startup => "startup_debug.log",
                LogType.Error => "error.log",
                LogType.Backup => "backup.log",
                LogType.Localization => "localization.log",
                _ => "app.log"
            };
            string path = Path.Combine(LogDir, fileName);
            return File.Exists(path) ? File.ReadAllText(path) : "";
        }
    }

    public static void ClearLogs(LogType? type = null)
    {
        lock (_lock)
        {
            try
            {
                if (type == null)
                {
                    foreach (var file in Directory.GetFiles(LogDir)) File.Delete(file);
                }
                else
                {
                    string fileName = type switch
                    {
                        LogType.Startup => "startup_debug.log",
                        LogType.Error => "error.log",
                        LogType.Backup => "backup.log",
                        LogType.Localization => "localization.log",
                        _ => "app.log"
                    };
                    string path = Path.Combine(LogDir, fileName);
                    if (File.Exists(path)) File.Delete(path);
                }
            }
            catch { }
        }
    }
    #endregion
}
