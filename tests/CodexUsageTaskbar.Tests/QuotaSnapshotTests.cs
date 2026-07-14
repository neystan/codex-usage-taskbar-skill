using CodexUsageTaskbar.Models;
using CodexUsageTaskbar.Services;

namespace CodexUsageTaskbar.Tests;

[TestClass]
public sealed class QuotaSnapshotTests
{
    [TestMethod]
    public async Task ReadNextProtocolLineAsync_SkipsBlankLinesWithoutPollingEndOfStream()
    {
        using var reader = new StringReader("\r\n\n{\"id\":2}\r\n");

        var line = await CodexAppServerClient.ReadNextProtocolLineAsync(reader, CancellationToken.None);

        Assert.AreEqual("{\"id\":2}", line);
    }

    [TestMethod]
    public void RemainingPercent_ClampsAndSubtractsUsedPercent()
    {
        var window = new QuotaWindow(75, DateTimeOffset.UnixEpoch);

        Assert.AreEqual(25, window.RemainingPercent);
    }

    [DataTestMethod]
    [DataRow(-5, 100)]
    [DataRow(150, 0)]
    public void RemainingPercent_ClampsInvalidUsage(int usedPercent, int expectedRemaining)
    {
        var window = new QuotaWindow(usedPercent, DateTimeOffset.UnixEpoch);

        Assert.AreEqual(expectedRemaining, window.RemainingPercent);
    }

    [TestMethod]
    public void ParseRateLimits_MapsFiveHourAndWeeklyWindowsByDuration()
    {
        const string json = """
            {"rateLimits":{"primary":{"usedPercent":24,"windowDurationMins":300,"resetsAt":1000},"secondary":{"usedPercent":75,"windowDurationMins":10080,"resetsAt":2000}}}
            """;

        var snapshot = CodexAppServerClient.ParseRateLimits(json, DateTimeOffset.UnixEpoch);

        Assert.AreEqual(76, snapshot.FiveHour!.RemainingPercent);
        Assert.AreEqual(25, snapshot.Weekly!.RemainingPercent);
    }
}
