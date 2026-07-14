using System.Windows;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace CodexUsageTaskbar;

public partial class App : System.Windows.Application
{
    private Mutex? instanceMutex;
    private Forms.NotifyIcon? trayIcon;
    private MainWindow? mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        instanceMutex = new Mutex(false, "CodexUsageTaskbar.Instance");
        try
        {
            if (!instanceMutex.WaitOne(0))
            {
                Shutdown();
                return;
            }
        }
        catch (AbandonedMutexException) { }

        base.OnStartup(e);
        mainWindow = new MainWindow();
        mainWindow.Show();
        trayIcon = new Forms.NotifyIcon
        {
            Icon = Drawing.SystemIcons.Information,
            Text = "Codex 剩余额度（双击显示）",
            Visible = true,
            ContextMenuStrip = new Forms.ContextMenuStrip(),
        };
        trayIcon.ContextMenuStrip.Items.Add("显示", null, (_, _) => mainWindow.ShowWidget());
        trayIcon.ContextMenuStrip.Items.Add("退出工具", null, (_, _) => Shutdown());
        trayIcon.DoubleClick += (_, _) => mainWindow.ShowWidget();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        instanceMutex?.ReleaseMutex();
        instanceMutex?.Dispose();
        trayIcon?.Dispose();
        base.OnExit(e);
    }
}
