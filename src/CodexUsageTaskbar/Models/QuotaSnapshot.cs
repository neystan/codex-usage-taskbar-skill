namespace CodexUsageTaskbar.Models;

public sealed record QuotaWindow(int UsedPercent, DateTimeOffset ResetsAt)
{
    public int RemainingPercent => Math.Clamp(100 - UsedPercent, 0, 100);
}

public sealed record QuotaSnapshot(
    QuotaWindow? FiveHour,
    QuotaWindow? Weekly,
    DateTimeOffset UpdatedAt,
    bool IsStale);
