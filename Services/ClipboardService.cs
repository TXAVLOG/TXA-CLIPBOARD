using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;

namespace TXABackupTool.Services;

public class ClipboardService
{
    public async Task<string> BackupClipboardHistoryAsync(string destinationPath)
    {
        var result = await Clipboard.GetHistoryItemsAsync();
        if (result.Status != ClipboardHistoryItemsResultStatus.Success)
            throw new Exception($"Không thể truy cập Clipboard History: {result.Status}");

        string clipDir = Path.Combine(destinationPath, "Clipboard");
        string imgDir = Path.Combine(clipDir, "images");
        Directory.CreateDirectory(imgDir);

        var history = new List<object>();

        foreach (var item in result.Items)
        {
            var entry = new { Timestamp = item.Timestamp, Type = "Unknown", Content = "" };
            
            if (item.Content.Contains(StandardDataFormats.Text))
            {
                string text = await item.Content.GetTextAsync();
                history.Add(new { Timestamp = item.Timestamp, Type = "Text", Content = text });
            }
        }

        string json = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
        string jsonPath = Path.Combine(clipDir, "clipboard_history.json");
        await File.WriteAllTextAsync(jsonPath, json);
        return jsonPath;
    }
    /// <summary>
    /// Restore clipboard history from a backup folder. Returns (success count, fail count).
    /// </summary>
    public async Task<(int success, int failed)> RestoreClipboardHistoryAsync(string backupRoot, Action<string>? log = null)
    {
        string clipDir = Path.Combine(backupRoot, "Clipboard");
        string jsonPath = Path.Combine(clipDir, "clipboard_history.json");

        if (!File.Exists(jsonPath))
        {
            log?.Invoke("[WARN] clipboard_history.json not found.");
            return (0, 0);
        }

        string json = await File.ReadAllTextAsync(jsonPath);
        var history = JsonSerializer.Deserialize<List<JsonElement>>(json);
        if (history == null || history.Count == 0)
        {
            log?.Invoke("[WARN] Clipboard history is empty.");
            return (0, 0);
        }

        // Reverse to restore in correct order (oldest first → newest last = on top)
        history.Reverse();

        int successCount = 0;
        int failCount = 0;
        var dispatcher = System.Windows.Application.Current.Dispatcher;

        log?.Invoke($"[INFO] Found {history.Count} clipboard item(s) to restore.");

        foreach (var item in history)
        {
            string type = "";
            try { type = item.GetProperty("Type").GetString() ?? ""; } catch { continue; }

            if (type != "Text") continue;

            string text = "";
            try { text = item.GetProperty("Content").GetString() ?? ""; } catch { continue; }
            if (string.IsNullOrEmpty(text)) continue;

            bool success = false;
            const int maxRetries = 3;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Must run on UI/STA thread for clipboard access
                    await dispatcher.InvokeAsync(() =>
                    {
                        var dp = new DataPackage();
                        dp.SetText(text);

                        var options = new ClipboardContentOptions { IsAllowedInHistory = true };
                        bool setResult = Clipboard.SetContentWithOptions(dp, options);

                        if (!setResult)
                            throw new Exception("SetContentWithOptions returned false");

                        Clipboard.Flush();
                    });

                    success = true;
                    break; // Exit retry loop on success
                }
                catch (Exception ex)
                {
                    if (attempt < maxRetries)
                    {
                        log?.Invoke($"[RETRY] Attempt {attempt}/{maxRetries} failed: {ex.Message}. Retrying...");
                        await Task.Delay(300); // Wait before retry
                    }
                    else
                    {
                        log?.Invoke($"[FAIL] Could not restore item after {maxRetries} attempts: \"{text.Substring(0, Math.Min(text.Length, 50))}...\"");
                    }
                }
            }

            if (success)
                successCount++;
            else
                failCount++;

            // Delay 500ms between items to let Windows clipboard service register each entry to history
            await Task.Delay(500);
        }

        log?.Invoke($"[DONE] Restored {successCount}/{successCount + failCount} items. ({failCount} failed)");
        return (successCount, failCount);
    }
}
