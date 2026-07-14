using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CodexUsageTaskbar.Models;
using CodexUsageTaskbar.Services;
using CodexUsageTaskbar.Views;
using Forms = System.Windows.Forms;

namespace CodexUsageTaskbar;

public partial class MainWindow : Window
{
    private readonly QuotaRefreshService refreshService = new(new CodexAppServerClient());
    private readonly QuotaViewModel viewModel = new();
    private readonly DispatcherTimer pointerTimer = new() { Interval = TimeSpan.FromMilliseconds(75) };

    public MainWindow()
    {
        InitializeComponent();
        pointerTimer.Tick += (_, _) => SyncDetailWithPointer();
        refreshService.SnapshotUpdated += (_, snapshot) => Dispatcher.BeginInvoke(() => Apply(snapshot), DispatcherPriority.Background);
    }

    public void ShowWidget()
    {
        Show();
        PositionWidget();
        Activate();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ShowWidget();
        refreshService.Start();
        pointerTimer.Start();
    }

    private void Apply(QuotaSnapshot snapshot)
    {
        viewModel.Apply(snapshot);
        StatusText.Text = viewModel.StatusText;
        BatteryFill.Width = 16 * (viewModel.FiveHourRemaining ?? 0) / 100.0;
        BatteryFill.Background = viewModel.BatteryBrush;
        FiveHourValue.Text = snapshot.FiveHour is null ? "--" : $"{snapshot.FiveHour.RemainingPercent}%";
        WeeklyValue.Text = snapshot.Weekly is null ? "--" : $"{snapshot.Weekly.RemainingPercent}%";
        FiveHourMeta.Text = CompactMeta(snapshot.FiveHour);
        WeeklyMeta.Text = CompactMeta(snapshot.Weekly);
        var updateLabel = snapshot.IsStale ? "缓存" : "更新";
        UpdatedDetail.Text = $"{updateLabel} {TimeZoneInfo.ConvertTime(snapshot.UpdatedAt, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")):HH:mm}";
    }

    private static string CompactMeta(QuotaWindow? window)
    {
        if (window is null) return "暂未提供";
        var reset = TimeZoneInfo.ConvertTime(window.ResetsAt, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
        return $"已用 {window.UsedPercent}% · {reset:MM/dd HH:mm}";
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => PositionWidget();

    private void PositionWidget()
    {
        Left = SystemParameters.WorkArea.Right - ActualWidth;
        Top = SystemParameters.WorkArea.Bottom - ActualHeight;
    }

    private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        DetailPanel.Visibility = Visibility.Visible;
    }

    private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) => SyncDetailWithPointer();

    private void SyncDetailWithPointer()
    {
        if (!IsVisible || ActualWidth <= 0 || ActualHeight <= 0) return;

        var cursor = Forms.Cursor.Position;
        var windowPoint = PointFromScreen(new System.Windows.Point(cursor.X, cursor.Y));
        var pointerInsideWindow = new Rect(0, 0, ActualWidth, ActualHeight).Contains(windowPoint);
        DetailPanel.Visibility = HoverState.ShouldDisplay(pointerInsideWindow)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
    private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await refreshService.RefreshNowAsync();
    private void HideButton_Click(object sender, RoutedEventArgs e) { DetailPanel.Visibility = Visibility.Collapsed; Hide(); }
    protected override void OnClosed(EventArgs e)
    {
        pointerTimer.Stop();
        refreshService.Dispose();
        base.OnClosed(e);
    }
}
