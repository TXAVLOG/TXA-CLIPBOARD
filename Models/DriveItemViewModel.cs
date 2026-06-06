using System.Windows.Media;

namespace TXABackupTool.Models;

public class DriveItemViewModel
{
    public string Label { get; set; } = "";
    public string Used { get; set; } = "—";
    public string Total { get; set; } = "";
    public string Pct { get; set; } = "—";
    public double PctValue { get; set; }
    public Brush ColorBrush { get; set; } = Brushes.Transparent;
    public string UsageTooltip => $"{Used} {Total}  ({Pct})";
}
