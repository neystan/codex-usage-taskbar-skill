using System.Diagnostics;

var widgetPath = args.FirstOrDefault() ?? Path.Combine(AppContext.BaseDirectory, "CodexUsageTaskbar.exe");
if (!File.Exists(widgetPath)) return;

Process? widget = null;
while (true)
{
    while (!ShouldShow()) await Task.Delay(TimeSpan.FromSeconds(1));
    widget = Process.Start(new ProcessStartInfo(widgetPath) { UseShellExecute = true });

    while (ShouldShow() && widget is { HasExited: false }) await Task.Delay(TimeSpan.FromSeconds(1));
    if (widget is { HasExited: false }) widget.Kill(entireProcessTree: true);
    widget?.Dispose();
    widget = null;
}

static bool ShouldShow() => Process.GetProcessesByName("ChatGPT").Length > 0
    && File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex", ".codex-global-state.json"));
