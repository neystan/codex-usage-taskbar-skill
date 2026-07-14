using CodexUsageTaskbar.Models;

namespace CodexUsageTaskbar.Services;

public static class ChineseQuotaFormatter
{
    private static readonly TimeZoneInfo Beijing = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

    public static string ResetText(QuotaWindow? window)
    {
        if (window is null) return "暂未提供";
        var reset = TimeZoneInfo.ConvertTime(window.ResetsAt, Beijing);
        return $"下次重置：{reset:MM月dd日 HH:mm}";
    }

    public static string Detail(string title, QuotaWindow? window) => window is null
        ? $"{title}：暂未提供"
        : $"{title}：已用 {window.UsedPercent}% · 剩余 {window.RemainingPercent}% · {ResetText(window)}";
}
