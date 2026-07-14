using CodexUsageTaskbar.Views;

namespace CodexUsageTaskbar.Tests;

[TestClass]
public class HoverStateTests
{
    [DataTestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public void ShouldCollapse_OnlyWhenPointerLeavesTheSingleWindow(bool windowHovered, bool expected)
    {
        Assert.AreEqual(expected, HoverState.ShouldCollapse(windowHovered));
    }

    [DataTestMethod]
    [DataRow(false, false)]
    [DataRow(true, true)]
    public void ShouldDisplay_TracksTheActualPointerLocation(bool pointerInsideWindow, bool expected)
    {
        Assert.AreEqual(expected, HoverState.ShouldDisplay(pointerInsideWindow));
    }
}
