using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TXABackupTool.Services;

namespace TXABackupTool.Helpers;

/// <summary>
/// TXATooltip — Attached property cho tooltip có hỗ trợ i18n key ${key}
/// Tự động re-resolve khi ngôn ngữ thay đổi.
/// </summary>
public static class TXATooltip
{
    // Track tất cả elements đang dùng TXATooltip để refresh khi đổi ngôn ngữ
    private static readonly List<WeakReference<FrameworkElement>> _tracked = new();
    private static readonly Dictionary<FrameworkElement, string> _rawTexts = new();

    static TXATooltip()
    {
        // Hook vào event ngôn ngữ thay đổi để refresh toàn bộ tooltips
        LanguageService.LanguageChanged += OnLanguageChanged;
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(TXATooltip),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public static string GetText(DependencyObject obj) => (string)obj.GetValue(TextProperty);
    public static void SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;
        string raw = (e.NewValue as string ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(raw))
        {
            element.ToolTip = null;
            _rawTexts.Remove(element);
            return;
        }

        // Lưu raw text để re-resolve khi ngôn ngữ đổi
        _rawTexts[element] = raw;
        _tracked.Add(new WeakReference<FrameworkElement>(element));

        ApplyTooltip(element, raw);
    }

    private static void ApplyTooltip(FrameworkElement element, string raw)
    {
        string resolved = raw;

        // Hỗ trợ ${key} interpolation
        if (raw.StartsWith("${") && raw.EndsWith("}"))
        {
            string key = raw.Substring(2, raw.Length - 3).Trim();
            resolved = LanguageService.Get(key);
        }

        element.ToolTip = string.IsNullOrWhiteSpace(resolved) ? null : resolved;
    }

    private static void OnLanguageChanged()
    {
        // Re-resolve tất cả tooltips khi ngôn ngữ thay đổi
        var dead = new List<WeakReference<FrameworkElement>>();

        foreach (var weakRef in _tracked)
        {
            if (weakRef.TryGetTarget(out var element))
            {
                if (_rawTexts.TryGetValue(element, out var raw))
                {
                    try
                    {
                        // Phải chạy trên UI thread
                        element.Dispatcher.Invoke(() => ApplyTooltip(element, raw));
                    }
                    catch { }
                }
            }
            else
            {
                dead.Add(weakRef);
            }
        }

        // Dọn các ref chết
        foreach (var d in dead) _tracked.Remove(d);
    }
}
