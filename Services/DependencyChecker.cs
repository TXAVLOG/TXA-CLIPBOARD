using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TXABackupTool.Services;

public static class DependencyChecker
{
    public static List<string> CheckMissingDependencies()
    {
        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TXABackupTool", "Logs", "startup_debug.log");
        void Log(string msg) { try { File.AppendAllText(logPath, $"[{DateTime.Now:HH:mm:ss.fff}] [DEP] {msg}\n"); } catch { } }

        var missing = new List<string>();
        string appDir = AppContext.BaseDirectory;
        Log($"Scanning dir: {appDir}");
        
        string[] criticalDlls = {
            "TXABackupTool.dll",
            "TXABackupTool.runtimeconfig.json"
        };

        foreach (var dll in criticalDlls)
        {
            Log($"Checking: {dll}");
            if (!File.Exists(Path.Combine(appDir, dll)))
            {
                Log($"MISSING: {dll}");
                missing.Add(dll);
            }
        }

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var references = assembly.GetReferencedAssemblies();
            Log($"Scanning {references.Length} references...");
            foreach (var r in references)
            {
                string name = r.Name ?? "";
                if (name.StartsWith("System") || 
                    name.StartsWith("Microsoft") || 
                    name.StartsWith("Presentation") || 
                    name.StartsWith("WindowsBase") || 
                    name.StartsWith("mscorlib") || 
                    name.StartsWith("netstandard") || 
                    name.StartsWith("UIAutomation") || 
                    name.Contains("DirectWrite") || 
                    name == "Windows") 
                    continue;

                Log($"Loading external ref: {name}");
                try { Assembly.Load(r); }
                catch (FileNotFoundException) { Log($"DLL NOT FOUND: {name}"); missing.Add(r.Name + ".dll"); }
                catch (Exception ex) { Log($"Load error ({name}): {ex.Message}"); }
            }
        }
        catch (Exception ex) { Log($"Reference scan fatal error: {ex.Message}"); }

        return missing.Distinct().ToList();
    }
}
