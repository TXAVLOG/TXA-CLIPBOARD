using System;
using System.Globalization;
using System.Windows.Data;

namespace TXABackupTool.Converters;

/// <summary>
/// IValueConverter: value=pct(0-100), parameter=maxWidth string → pixel width
/// IMultiValueConverter: values=[value, maximum, actualWidth] → pixel width
/// </summary>
public class PercentToWidthConverter : IValueConverter, IMultiValueConverter
{
    // IValueConverter — for manual use with ConverterParameter
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double pct && parameter is string maxStr && double.TryParse(maxStr, out double max))
            return Math.Max(0, Math.Min(max, pct / 100.0 * max));
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();

    // IMultiValueConverter — for ProgressBar template: [value, maximum, actualWidth]
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 3
            && values[0] is double val
            && values[1] is double max && max > 0
            && values[2] is double width)
        {
            return Math.Max(0, Math.Min(width, val / max * width));
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
