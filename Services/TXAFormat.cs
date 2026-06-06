using System;

namespace TXABackupTool.Services;

public static class TXAFormat
{
    public static string FileSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int unitIndex = 0;
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }
        return $"{size:F2} {units[unitIndex]}";
    }

    public static string Time(DateTime time)
    {
        return time.ToString("dd/MM/yyyy HH:mm:ss");
    }

    public static string Double(double value)
    {
        return value.ToString("F2");
    }

    public static string Duration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours:D2}:{(int)duration.Minutes:D2}:{(int)duration.Seconds:D2}";
        return $"{(int)duration.TotalMinutes:D2}:{(int)duration.Seconds:D2}";
    }

    public static string ETA(TimeSpan? eta)
    {
        if (!eta.HasValue || eta.Value.TotalSeconds <= 0) return "--:--";
        return Duration(eta.Value);
    }
}
