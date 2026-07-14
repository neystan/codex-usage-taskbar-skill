using CodexUsageTaskbar.Models;

namespace CodexUsageTaskbar.Services;

public sealed class QuotaRefreshService(ICodeUsageClient client) : IDisposable
{
    private readonly PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
    private readonly CancellationTokenSource cancellation = new();
    public QuotaSnapshot? Current { get; private set; }
    public event EventHandler<QuotaSnapshot>? SnapshotUpdated;

    public void Start() => _ = RefreshLoopAsync();

    public async Task RefreshNowAsync()
    {
        try
        {
            Current = await client.RefreshAsync(cancellation.Token);
        }
        catch when (Current is not null)
        {
            Current = Current with { IsStale = true };
        }
        if (Current is not null) SnapshotUpdated?.Invoke(this, Current);
    }

    private async Task RefreshLoopAsync()
    {
        await RefreshNowAsync();
        while (await timer.WaitForNextTickAsync(cancellation.Token)) await RefreshNowAsync();
    }

    public void Dispose() { cancellation.Cancel(); timer.Dispose(); cancellation.Dispose(); }
}
