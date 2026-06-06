using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip; // Sử dụng DotNetZip cho hỗ trợ Password cực mạnh
using TXABackupTool.Services;

namespace TXABackupTool.Services;

/// <summary>
/// TXA VLOG Zipper — Nén backup chuyên nghiệp với hỗ trợ mật khẩu
/// Tự động dọn dẹp thư mục tạm sau khi nén thành công.
/// </summary>
public class TXAZipper
{
    #region Fields & Events
    public event Action<string>? LogReceived;
    public event Action<double>? ProgressChanged;
    #endregion

    #region Main Logic
    /// <summary>
    /// Nén thư mục backup thành file ZIP bảo mật
    /// </summary>
    /// <param name="sourceDir">Thư mục đã backup xong</param>
    /// <param name="password">Mật khẩu nén (nếu có)</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Đường dẫn file zip thành phẩm</returns>
    public async Task<string> CompressAsync(string sourceDir, string? password = null, CancellationToken ct = default)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"{sourceDir} not found.");

        // Xác định đường dẫn file zip (đặt cạnh thư mục backup)
        string parentDir = Path.GetDirectoryName(sourceDir) ?? sourceDir;
        string dirName = Path.GetFileName(sourceDir);
        string outputZipPath = Path.Combine(parentDir, $"{dirName}.zip");

        LogReceived?.Invoke(LanguageService.Get("txa_log_zip_start", dirName));

        await Task.Run(() =>
        {
            if (File.Exists(outputZipPath)) File.Delete(outputZipPath);

            using (ZipFile zip = new ZipFile())
            {
                // Cấu hình mã hóa và mật khẩu nếu có
                if (!string.IsNullOrEmpty(password))
                {
                    zip.Password = password;
                    zip.Encryption = EncryptionAlgorithm.WinZipAes256; // Chuẩn mã hóa cao cấp
                }

                // Cấu hình Encode để không bị lỗi font tiếng Việt/Ký tự lạ
                zip.AlternateEncoding = Encoding.UTF8;
                zip.AlternateEncodingUsage = ZipOption.Always;

                // Thêm toàn bộ thư mục gốc vào (toàn bộ file và sub-folder)
                zip.AddDirectory(sourceDir);

                // Thêm file CONTACT ADMIN.url động vào Zip
                AddContactEntry(zip);

                // Gắn Comment mô tả (Đa ngôn ngữ + Password info)
                zip.Comment = GenerateMetadata(sourceDir, password);

                // Đăng ký sự kiện cập nhật tiến trình
                zip.SaveProgress += (s, e) =>
                {
                    if (e.EventType == ZipProgressEventType.Saving_BeforeWriteEntry)
                    {
                        if (ct.IsCancellationRequested) e.Cancel = true;
                        
                        double pct = (double)e.EntriesSaved / e.EntriesTotal * 100;
                        ProgressChanged?.Invoke(pct);
                        LogReceived?.Invoke(LanguageService.Get("txa_log_zip_entry", e.CurrentEntry.FileName));
                    }
                };

                // Tiến hành lưu file Zip
                zip.Save(outputZipPath);
            }

            // --- LƯU Ý: Không xóa folder gốc sau khi nén theo yêu cầu người dùng ---
            // CleanupSourceDir(sourceDir);

        }, ct);

        LogReceived?.Invoke(LanguageService.Get("txa_log_zip_ok", Path.GetFileName(outputZipPath)));
        return outputZipPath;
    }

    /// <summary>
    /// Nén chỉ các tệp đặc biệt (Clipboard, Shortcut, Info) dư thừa trong thư mục
    /// </summary>
    public async Task CompressSpecialAsync(string sourceDir, string? password = null, CancellationToken ct = default)
    {
        if (!Directory.Exists(sourceDir)) return;
        
        string outputZipPath = Path.Combine(sourceDir, "TXA_RESTORE_BUNDLE.zip");
        
        LogReceived?.Invoke("[ZIP] Creating special restore bundle...");

        await Task.Run(() =>
        {
            if (File.Exists(outputZipPath)) File.Delete(outputZipPath);

            using (ZipFile zip = new ZipFile())
            {
                if (!string.IsNullOrEmpty(password))
                {
                    zip.Password = password;
                    zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                }
                zip.AlternateEncoding = Encoding.UTF8;
                zip.AlternateEncodingUsage = ZipOption.Always;

                // Thêm Clipboard if exists
                string cbPath = Path.Combine(sourceDir, "Clipboard_History.txt");
                if (File.Exists(cbPath)) zip.AddFile(cbPath, "");

                // Thêm Shortcuts if exists
                string scPath = Path.Combine(sourceDir, "Shortcuts");
                if (Directory.Exists(scPath)) zip.AddDirectory(scPath, "Shortcuts");

                // Thêm Info if exists
                string infoPath = Path.Combine(sourceDir, "TXA_BACKUP_INFO.txt");
                if (File.Exists(infoPath)) zip.AddFile(infoPath, "");

                // Add contact
                AddContactEntry(zip);

                zip.Save(outputZipPath);
                
                // Cleanup ONLY special files
                try { if (File.Exists(cbPath)) File.Delete(cbPath); } catch {}
                try { if (Directory.Exists(scPath)) Directory.Delete(scPath, true); } catch {}
                try { if (File.Exists(infoPath)) File.Delete(infoPath); } catch {}
            }
        });
        
        LogReceived?.Invoke("[ZIP] Special restore bundle created.");
    }

    /// <summary>
    /// Giải nén file ZIP vào thư mục đích
    /// </summary>
    public async Task<bool> DecompressAsync(string zipPath, string targetDir, string? password = null)
    {
        if (!File.Exists(zipPath)) return false;
        if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

        return await Task.Run(() =>
        {
            try
            {
                using (ZipFile zip = ZipFile.Read(zipPath))
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        zip.Password = password;
                    }
                    zip.ExtractAll(targetDir, ExtractExistingFileAction.OverwriteSilently);
                }
                return true;
            }
            catch (BadPasswordException)
            {
                throw new Exception("PASSWORD_INVALID");
            }
            catch (Exception ex)
            {
                LogReceived?.Invoke($"[ZIP ERROR] {ex.Message}");
                return false;
            }
        });
    }
    #endregion

    #region Private Helpers
    private void AddContactEntry(ZipFile zip)
    {
        try
        {
            string urlContent = "[InternetShortcut]\r\nURL=https://fb.com/vlog.txa.2311\r\n";
            zip.AddEntry("CONTACT ADMIN.url", urlContent, Encoding.UTF8);
            LogReceived?.Invoke(LanguageService.Get("txa_log_zip_shortcut"));
        }
        catch { }
    }

    private void CleanupSourceDir(string sourceDir)
    {
        try
        {
            LogReceived?.Invoke(LanguageService.Get("txa_log_zip_cleaning"));
            if (Directory.Exists(sourceDir))
            {
                Directory.Delete(sourceDir, true);
            }
        }
        catch (Exception ex)
        {
            LogReceived?.Invoke($"{LanguageService.Get("txa_log_error", ex.Message)}");
        }
    }

    private string GenerateMetadata(string sourceDir, string? password)
    {
        // Thu thập thông tin từ THƯ MỤC GỐC trước khi nén (vì sourceDir sắp bị xóa)
        var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
        long totalSize = 0;
        foreach (var f in files) try { totalSize += new FileInfo(f).Length; } catch { }

        var sb = new StringBuilder();
        sb.AppendLine(LanguageService.Get("txa_info_header", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
        sb.AppendLine("------------------------------------------------------");
        sb.AppendLine(LanguageService.Get("txa_info_user", Environment.UserName));
        sb.AppendLine(LanguageService.Get("txa_info_machine", Environment.MachineName));
        sb.AppendLine(LanguageService.Get("txa_info_files", files.Length));
        sb.AppendLine(LanguageService.Get("txa_info_data", TXAFormat.FileSize(totalSize)));
        
        if (!string.IsNullOrEmpty(password))
            sb.AppendLine(LanguageService.Get("txa_zip_password_label", password));

        sb.AppendLine("Software: TXA Backup Premium 1.0");
        sb.AppendLine(LanguageService.Get("txa_info_copyright", DateTime.Now.Year));
        sb.AppendLine("Contact: https://fb.com/vlog.txa.2311");
        
        return sb.ToString();
    }
    #endregion
}
