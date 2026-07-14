namespace CodexUsageTaskbar.Views;

public static class HoverState
{
    public static bool ShouldCollapse(bool mainWindowHovered) => !mainWindowHovered;

    public static bool ShouldDisplay(bool pointerInsideWindow) => pointerInsideWindow;
}
