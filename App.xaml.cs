using System;
using System.Linq;
using System.Threading;
using System.Windows;
using TXABackupTool.Services;

namespace TXABackupTool;

public partial class App : Application
{
    #region Fields
    private const string MutexName = "Global\\TXA_BACKUP_TOOL_SINGLE_INSTANCE_2026";
    private static Mutex? _mutex;
    
    private static readonly string LogFullDir = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TXABackupTool",
        "Logs"
    );
    private static readonly string BootLogPath = System.IO.Path.Combine(LogFullDir, "startup_debug.log");
    private static string? _pendingFileArgument;
    #endregion

    private static bool _isExiting = false;

    static App()
    {
        // Xóa log cũ khi khởi chạy mới để tránh file quá lớn và dễ theo dõi
        TXALogger.ClearLogs(LogType.Startup);
        TXALogger.Log(LogType.Startup, "--- AGGRESSIVE DEBUG START ---");
    }

    public App()
    {
        TXALogger.Log(LogType.Startup, "App Instance Created");
        
        this.DispatcherUnhandledException += (s, e) => {
            HandleFatalException(e.Exception, "WPF DISPATCHER");
            e.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) => {
            HandleFatalException(e.ExceptionObject as Exception, "APP DOMAIN");
        };
    }

    private void HandleFatalException(Exception? ex, string source)
    {
        if (_isExiting) return;
        _isExiting = true;

        string error = ex?.ToString() ?? "Unknown error";
        TXALogger.Log(LogType.Error, $"FATAL ({source}): {error}");
        
        // Show as SystemModal to ensure it's visible even if the UI is hanging
        MessageBox.Show($"FATAL SYSTEM ERROR ({source}):\n\n{ex?.Message}\n\nReview startup_debug.log in LocalAppData for details.", 
            "TXA Global Crash Reporter", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
        
        Environment.Exit(1);
    }

    #region Lifecycle Events
    protected override void OnStartup(StartupEventArgs e)
    {
        TXALogger.Log(LogType.Startup, "Entering OnStartup");
        
        // ── Single Instance Check ──
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            TXALogger.Log(LogType.Startup, "Another instance detected. Shutting down.");
            bool isVi = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi";
            string title = isVi ? "⚠️ ỨNG DỤNG ĐÃ CHẠY" : "⚠️ APP ALREADY RUNNING";
            string msg = isVi 
                ? "TXA Backup Tool đang chạy ở một tiến trình khác.\nKhông thể mở 2 cửa sổ cùng lúc." 
                : "TXA Backup Tool is already running in another process.\nCannot open two windows at the same time.";
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            Shutdown();
            return;
        }
        
        try 
        {
            TXALogger.Log(LogType.Startup, "Calling DependencyChecker...");
            var missing = DependencyChecker.CheckMissingDependencies();
            TXALogger.Log(LogType.Startup, $"DependencyChecker returned. Missing count: {missing.Count}");

            if (missing.Any())
            {
                string files = string.Join("\n", missing.Select(f => "- " + f));
                TXALogger.Log(LogType.Startup, $"Missing items detected: {files}");

                bool isVi = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi";
                string title = isVi ? "Lỗi khởi động" : "Startup Error";
                string msg = isVi 
                    ? $"⚠️ PHÁT HIỆN THIẾU FILE HỆ THỐNG:\n\n{files}\n\nVui lòng cài đặt lại ứng dụng hoặc copy đầy đủ file."
                    : $"⚠️ SYSTEM FILES MISSING:\n\n{files}\n\nPlease reinstall or copy all required files.";

                TXALogger.Log(LogType.Startup, $"Missing dependencies: {files}");
                TXALogger.Log(LogType.Startup, "Showing missing dependency MessageBox");
                MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Error);
                TXALogger.Log(LogType.Startup, "Shutting down due to missing dependencies");
                Shutdown();
                return;
            }

            // Handle command line arguments (TXA FILE ASSOCIATION ENGINE)
            if (e.Args.Length > 0)
            {
                var filePath = e.Args[0];
                if (System.IO.File.Exists(filePath))
                {
                    _pendingFileArgument = filePath;
                }
            }

            TXALogger.Log(LogType.Startup, "Calling base.OnStartup...");
            base.OnStartup(e);
            TXALogger.Log(LogType.Startup, "base.OnStartup completed");
            
            // Execute argument handling after window load
            if (!string.IsNullOrEmpty(_pendingFileArgument) && MainWindow.DataContext is ViewModels.MainViewModel mvm)
            {
                mvm.ImportLanguageFile(_pendingFileArgument);
            }
            
            // Xác định ngôn ngữ cần load từ Settings hoặc Hệ điều hành
            var settings = SettingsService.Load();
            string? langPath = settings.LastLanguagePath;

            if (string.IsNullOrEmpty(langPath) || !System.IO.File.Exists(langPath))
            {
                // Fallback: Tìm file theo ngôn ngữ hệ thống
                LanguageService.GetAvailableLanguages();
                bool isVi = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi";
                var target = isVi 
                    ? LanguageService.AvailableLanguages.FirstOrDefault(l => l.Name.Contains("Việt")) 
                    : LanguageService.AvailableLanguages.FirstOrDefault(l => l.Name.Contains("English"));
                
                langPath = target?.Path ?? LanguageService.AvailableLanguages.FirstOrDefault()?.Path;
            }

            if (!string.IsNullOrEmpty(langPath) && System.IO.File.Exists(langPath))
            {
                TXALogger.Log(LogType.Startup, $"Loading language from: {langPath}");
                LanguageService.LoadLanguage(langPath);
            }
            else
            {
                TXALogger.Log(LogType.Startup, "No language file found, using default dictionaries.");
            }
        }
        catch (Exception ex)
        {
            TXALogger.Log(LogType.Error, $"CRITICAL STARTUP EXCEPTION: {ex.GetType().Name} - {ex.Message}", ex.StackTrace);
            
            // Always show message first in case Logger fails
            string errorMsg = $"CRITICAL STARTUP ERROR:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            MessageBox.Show(errorMsg, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            
            Shutdown();
        }
    }
    #endregion
}
