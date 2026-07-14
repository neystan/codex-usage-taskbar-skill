using CodexUsageTaskbar.Models;

namespace CodexUsageTaskbar.Services;

public interface ICodeUsageClient
{
    Task<QuotaSnapshot> RefreshAsync(CancellationToken cancellationToken);
}
