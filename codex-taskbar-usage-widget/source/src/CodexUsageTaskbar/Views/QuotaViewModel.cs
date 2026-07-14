using System.Windows.Media;
using CodexUsageTaskbar.Models;

namespace CodexUsageTaskbar.Views;

public sealed class QuotaViewModel
{
    public static string BatteryColorFor(int remaining) => remaining switch
    {
        > 50 => "#22C55E",
        > 20 => "#EAB308",
        _ => "#EF4444",
    };

    public void Apply(QuotaSnapshot snapshot)
    {
        FiveHourRemaining = snapshot.FiveHour?.RemainingPercent;
        WeeklyRemaining = snapshot.Weekly?.RemainingPercent;
        BatteryBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(BatteryColorFor(FiveHourRemaining ?? 0)));
        StatusText = $"{FiveHourRemaining?.ToString() ?? "--"}({WeeklyRemaining?.ToString() ?? "--"})%";
    }

    public int? FiveHourRemaining { get; private set; }
    public int? WeeklyRemaining { get; private set; }
    public System.Windows.Media.Brush BatteryBrush { get; private set; } = System.Windows.Media.Brushes.Gray;
    public string StatusText { get; private set; } = "--(--)%";
}
