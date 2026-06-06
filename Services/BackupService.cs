using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TXABackupTool.Models;

namespace TXABackupTool.Services;

public class BackupService
{
    public event Action<string>? LogReceived;
    public event Action<BackupItem, long, long>? ProgressChanged; // item, current, total
    public event Action<double>? OverallProgressChanged;
    public event Action? StatusChanged; // New event to notify UI about file counts

    private DateTime _lastProgressUpdate = DateTime.MinValue;
    private DateTime _lastStatusUpdate = DateTime.MinValue;

    private int _filesProcessed;
    private int _filesSkipped;
    private long _totalBytesCopied;

    public long TotalBytesCopied => Interlocked.Read(ref _totalBytesCopied);
    public int FilesProcessed => _filesProcessed;
    public int FilesSkipped => _filesSkipped;
    public int FoldersProcessed { get; private set; }
    public int FoldersFailed { get; private set; }
    public DateTime? StartTime { get; private set; }
    public TimeSpan? EstimatedRemainingTime { get; private set; }

    // This callback is called when shortcuts are found. 
    public Func<List<string>, Task<List<string>>>? OnShortcutsDetected;

    public bool SkipShortcuts { get; set; } = true;

    private readonly string[] _excludedFolders = { "node_modules", "bin", "obj", ".git", ".vs" };
    private readonly string[] _excludedFiles = { "desktop.ini", "thumbs.db", "ntuser.dat", "ntuser.ini" };

    public async Task BackupAsync(
        IEnumerable<BackupItem> items,
        string destinationRoot,
        CancellationToken ct,
        bool resumeMode = false)
    {
        _totalBytesCopied = 0;
        _filesProcessed = 0;
        _filesSkipped = 0;
        FoldersProcessed = 0;
        FoldersFailed = 0;
        StartTime = DateTime.Now;
        EstimatedRemainingTime = null;
        StatusChanged?.Invoke();

        var backupItems = items.Where(i => i.IsSelected && (i.SourcePath == "CLIPBOARD" || Directory.Exists(i.SourcePath))).ToList();
        
        // Phase 1: Scan and resolve shortcuts for ALL items first
        var itemFilesMap = new Dictionary<BackupItem, List<string>>();
        long actualTotalSize = 0;

        foreach (var item in backupItems)
        {
            if (ct.IsCancellationRequested) break;
            if (item.SourcePath == "CLIPBOARD") continue; // Handled separately or in map
            
            item.StatusKey = "txa_status_scanning";
            LogReceived?.Invoke(LanguageService.Get("txa_log_scan", item.Name));

            List<string> allFiles = new();
            await Task.Run(() =>
            {
                try { allFiles = GetFilesFiltered(item.SourcePath).ToList(); }
                catch { }
            }, ct);

            // Filter shortcuts
            var shortcuts = allFiles.Where(f => f.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) || 
                                              f.EndsWith(".url", StringComparison.OrdinalIgnoreCase)).ToList();
            var normalFiles = allFiles.Except(shortcuts).ToList();

            if (shortcuts.Any() && OnShortcutsDetected != null)
            {
                LogReceived?.Invoke($"{item.Name}: Phát hiện {shortcuts.Count} shortcut, đang đợi xác nhận...");
                var kept = await OnShortcutsDetected(shortcuts);
                if (kept != null) normalFiles.AddRange(kept);
            }
            
            itemFilesMap[item] = normalFiles;
            
            // Calculate actual size for this item
            foreach(var f in normalFiles) {
                try { 
                    string relPath = Path.GetRelativePath(item.SourcePath, f);
                    string destFile = Path.Combine(destinationRoot, item.Name, relPath);
                    
                    if (resumeMode && File.Exists(destFile))
                    {
                        var srcInfo = new FileInfo(f);
                        var destInfo = new FileInfo(destFile);
                        if (srcInfo.Length == destInfo.Length && Math.Abs((srcInfo.LastWriteTime - destInfo.LastWriteTime).TotalSeconds) < 2)
                        {
                            continue; // Skip calculating size for identical files in resume mode
                        }
                    }
                    actualTotalSize += new FileInfo(f).Length; 
                } catch {}
            }
        }

        // Phase 2: Perform the copy
        foreach (var item in backupItems)
        {
            if (ct.IsCancellationRequested) break;
            if (item.SourcePath == "CLIPBOARD") continue;
            if (!itemFilesMap.ContainsKey(item)) continue;

            var filesToCopy = itemFilesMap[item];
            item.StatusKey = "txa_status_copying";

            string itemDest = Path.Combine(destinationRoot, item.Name);
            Directory.CreateDirectory(itemDest);

            var semaphore = new SemaphoreSlim(8); // Giới hạn 8 file nén cùng lúc để max speed
            var tasks = filesToCopy.Select(async file =>
            {
                await semaphore.WaitAsync(ct);
                try
                {
                    if (ct.IsCancellationRequested) return;

                    string relPath = Path.GetRelativePath(item.SourcePath, file);
                    string destFile = Path.Combine(itemDest, relPath);

                    bool shouldSkip = false;
                    if (File.Exists(destFile))
                    {
                        var srcInfo = new FileInfo(file);
                        var destInfo = new FileInfo(destFile);
                        if (srcInfo.Length == destInfo.Length && Math.Abs((srcInfo.LastWriteTime - destInfo.LastWriteTime).TotalSeconds) < 2)
                        {
                            shouldSkip = true;
                        }
                    }

                    if (shouldSkip)
                    {
                        Interlocked.Increment(ref _filesProcessed);
                        NotifyStatusChanged();
                        return;
                    }

                    string? destDir = Path.GetDirectoryName(destFile);
                    if (destDir != null) Directory.CreateDirectory(destDir);

                    await CopyFileWithProgressAsync(file, destFile, item, actualTotalSize, ct);
                    Interlocked.Increment(ref _filesProcessed);
                    NotifyStatusChanged();
                    LogReceived?.Invoke(LanguageService.Get("txa_log_copy_ok", relPath));
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Interlocked.Increment(ref _filesSkipped);
                    NotifyStatusChanged();
                    LogReceived?.Invoke(LanguageService.Get("txa_log_copy_err", Path.GetFileName(file), ex.Message));
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            if (ct.IsCancellationRequested) 
            {
                FoldersFailed++;
                item.StatusKey = "txa_status_cancelled";
            }
            else
            {
                FoldersProcessed++;
                item.StatusKey = "txa_status_completed";
            }
            StatusChanged?.Invoke();
            item.Progress = ct.IsCancellationRequested ? item.Progress : 100;
        }
    }

    private IEnumerable<string> GetFilesFiltered(string path)
    {
        var files = new List<string>();
        try
        {
            foreach (var file in Directory.GetFiles(path))
            {
                string fileName = Path.GetFileName(file);
                if (_excludedFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase)) continue;
                
                // Nếu bật SkipShortcuts, bỏ qua .lnk và .url ngay lập tức
                if (SkipShortcuts && (file.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".url", StringComparison.OrdinalIgnoreCase)))
                    continue;

                files.Add(file);
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                string dirName = Path.GetFileName(dir);
                if (_excludedFolders.Contains(dirName, StringComparer.OrdinalIgnoreCase))
                {
                    continue; // Skip excluded folders
                }
                files.AddRange(GetFilesFiltered(dir));
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (DirectoryNotFoundException) { }
        return files;
    }

    private async Task CopyFileWithProgressAsync(string source, string dest, BackupItem item, long totalSizeToCopy, CancellationToken ct)
    {
        const int bufferSize = 1024 * 1024; // 1MB buffer cho tốc độ bàn thờ
        using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
        using var destStream = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

        byte[] buffer = new byte[bufferSize];
        int read;
        while ((read = await sourceStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
        {
            await destStream.WriteAsync(buffer, 0, read, ct);
            
            lock (item) {
                item.CopiedBytes += read;
                if (item.TotalBytes > 0)
                    item.Progress = (double)item.CopiedBytes / item.TotalBytes * 100;
            }
            
            Interlocked.Add(ref _totalBytesCopied, read);
            
            // Throttle progress updates to UI
            if (DateTime.Now - _lastProgressUpdate > TimeSpan.FromMilliseconds(100))
            {
                _lastProgressUpdate = DateTime.Now;
                ProgressChanged?.Invoke(item, item.CopiedBytes, item.TotalBytes);
                if (totalSizeToCopy > 0) {
                    OverallProgressChanged?.Invoke((double)TotalBytesCopied / totalSizeToCopy * 100);
                    UpdateETA(totalSizeToCopy);
                }
            }
        }
        
        // Preserve timestamps
        try
        {
            File.SetAttributes(dest, File.GetAttributes(source));
            File.SetCreationTime(dest, File.GetCreationTime(source));
            File.SetLastWriteTime(dest, File.GetLastWriteTime(source));
        }
        catch { /* skip if can't preserve */ }
    }

    private void UpdateETA(long totalToCopy)
    {
        if (StartTime == null || TotalBytesCopied == 0 || totalToCopy <= TotalBytesCopied)
        {
            EstimatedRemainingTime = null;
            return;
        }

        var elapsed = DateTime.Now - StartTime.Value;
        if (elapsed.TotalSeconds < 1) return;

        double bytesPerSecond = TotalBytesCopied / elapsed.TotalSeconds;
        if (bytesPerSecond < 1) return;

        long remainingBytes = totalToCopy - TotalBytesCopied;
        EstimatedRemainingTime = TimeSpan.FromSeconds(remainingBytes / bytesPerSecond);
    }

    public void NotifyStatusChanged(bool force = false)
    {
        if (force || DateTime.Now - _lastStatusUpdate > TimeSpan.FromMilliseconds(200))
        {
            _lastStatusUpdate = DateTime.Now;
            StatusChanged?.Invoke();
        }
    }
}
