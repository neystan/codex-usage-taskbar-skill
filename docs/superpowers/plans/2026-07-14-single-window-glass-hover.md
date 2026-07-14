# Single-Window Glass Hover Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the taskbar quota widget content-sized, near-transparent, and immune to quota-refresh hover races.

**Architecture:** Replace the WPF `Popup` with an in-window detail panel. The main window is content-sized and keeps its bottom-right edge aligned with the work area whenever its detail panel changes visibility. One hover state owned by the main window controls the panel; refreshes change only quota text.

**Tech Stack:** .NET 9, WPF, MSTest.

## Global Constraints

- Windows taskbar widget remains a topmost, non-taskbar WPF window.
- No fixed display widths for the status or detail wrappers.
- Quota refresh occurs every 10 seconds and must not change detail visibility.

---

### Task 1: Define single-window hover state

**Files:**
- Modify: `tests/CodexUsageTaskbar.Tests/HoverStateTests.cs`
- Modify: `src/CodexUsageTaskbar/Views/HoverState.cs`

- [ ] Write a failing `ShouldCollapse_WhenMainWindowIsNotHovered` test.
- [ ] Run `dotnet test CodexUsageTaskbar.sln --no-restore --filter HoverState` and confirm it fails because `ShouldCollapse` does not exist.
- [ ] Implement `ShouldCollapse(bool mainWindowHovered) => !mainWindowHovered`.
- [ ] Re-run the filtered test and confirm it passes.

### Task 2: Move details into the main WPF window

**Files:**
- Modify: `src/CodexUsageTaskbar/MainWindow.xaml`
- Modify: `src/CodexUsageTaskbar/MainWindow.xaml.cs`

- [ ] Replace the `Popup` with a collapsed `DetailPanel` in the root grid.
- [ ] Use `SizeToContent="WidthAndHeight"`; make status and detail borders content-sized with transparent fills and white glass outlines.
- [ ] Open the panel on main-window hover and close it only after the pointer leaves the main window for 360ms.
- [ ] Reposition on `SizeChanged` so the status remains pinned to the taskbar edge.

### Task 3: Verify and deploy

**Files:**
- Publish: `dist/`

- [ ] Run `dotnet test CodexUsageTaskbar.sln --no-restore`.
- [ ] Stop the launcher and widget processes, publish both projects, then reinstall the scheduled launcher task.
- [ ] Confirm `CodexUsageTaskbarLauncher` is running and both processes exist.
