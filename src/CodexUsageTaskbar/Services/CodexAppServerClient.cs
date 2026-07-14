using System.Diagnostics;
using System.IO;
using System.Text.Json;
using CodexUsageTaskbar.Models;

namespace CodexUsageTaskbar.Services;

public sealed class CodexAppServerClient : ICodeUsageClient
{
    public static async Task<string?> ReadNextProtocolLineAsync(TextReader reader, CancellationToken cancellationToken)
    {
        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null || !string.IsNullOrWhiteSpace(line)) return line;
        }
    }

    public static QuotaSnapshot ParseRateLimits(string json, DateTimeOffset updatedAt)
    {
        using var document = JsonDocument.Parse(json);
        var limits = document.RootElement.GetProperty("rateLimits");
        QuotaWindow? fiveHour = null;
        QuotaWindow? weekly = null;

        foreach (var name in new[] { "primary", "secondary" })
        {
            if (!limits.TryGetProperty(name, out var window) || window.ValueKind == JsonValueKind.Null)
                continue;

            var duration = window.GetProperty("windowDurationMins").GetInt32();
            var quota = new QuotaWindow(
                (int)Math.Round(window.GetProperty("usedPercent").GetDouble()),
                DateTimeOffset.FromUnixTimeSeconds(window.GetProperty("resetsAt").GetInt64()));
            if (duration <= 360) fiveHour = quota;
            else weekly = quota;
        }

        if (fiveHour is null && weekly is null)
            throw new InvalidOperationException("Codex app-server did not return quota windows.");

        return new QuotaSnapshot(fiveHour, weekly, updatedAt, false);
    }

    public async Task<QuotaSnapshot> RefreshAsync(CancellationToken cancellationToken)
    {
        var executable = FindCodexExecutable() ?? throw new FileNotFoundException("未找到 Codex 可执行文件。");
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(executable, "app-server --listen stdio://")
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            },
        };
        process.Start();
        await using var input = process.StandardInput;
        using var output = process.StandardOutput;

        await input.WriteLineAsync("""{"id":1,"method":"initialize","params":{"clientInfo":{"name":"codex-usage-taskbar","title":"Codex Usage Taskbar","version":"1.0.0"},"capabilities":{"optOutNotificationMethods":["thread/started"]}}}""");
        await input.FlushAsync(cancellationToken);

        while (true)
        {
            var line = await ReadNextProtocolLineAsync(output, cancellationToken);
            if (line is null) break;
            using var message = JsonDocument.Parse(line);
            var root = message.RootElement;
            if (!root.TryGetProperty("id", out var id)) continue;
            if (id.GetInt32() == 1)
            {
                await input.WriteLineAsync("""{"method":"initialized","params":{}}""");
                await input.WriteLineAsync("""{"id":2,"method":"account/rateLimits/read","params":{}}""");
                await input.FlushAsync(cancellationToken);
            }
            else if (id.GetInt32() == 2)
            {
                if (root.TryGetProperty("error", out var error))
                    throw new InvalidOperationException(error.ToString());
                return ParseRateLimits(root.GetProperty("result").GetRawText(), DateTimeOffset.UtcNow);
            }
        }
        throw new InvalidOperationException("Codex app-server ended before returning quota data.");
    }

    private static string? FindCodexExecutable()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin");
        return Directory.Exists(root)
            ? Directory.EnumerateDirectories(root).Select(path => Path.Combine(path, "codex.exe"))
                .Where(File.Exists).OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault()
            : null;
    }
}
