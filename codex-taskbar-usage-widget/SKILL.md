---
name: codex-taskbar-usage-widget
description: Build, repair, restyle, or run a Windows taskbar-adjacent Codex usage widget that displays live remaining 5-hour and weekly quota. Use when a user asks for a Codex remaining-limit battery indicator, a compact hover detail panel, live local Usage data, taskbar auto-start/auto-stop behavior, or fixes for stale data, UI freezes, and hover tracking.
---

# Codex Usage Taskbar Widget

Maintain a standalone WPF taskbar widget; never edit Codex binaries or copy account credentials.

## Reuse first

Prefer the verified WPF project in this Skill folder's sibling `source` directory. Copy that source into the user's requested workspace before customizing it, and never overwrite a non-empty destination. It includes the solution, WPF widget, launcher, and autostart installer. Create a fresh implementation only when the user explicitly asks for a different stack or the supplied source cannot meet the requested behavior.

## Data and refresh

1. Locate the newest `%LOCALAPPDATA%\OpenAI\Codex\bin\*\codex.exe`.
2. Start a short-lived `codex.exe app-server --listen stdio://` child process.
3. Send JSONL `initialize`, `initialized`, then `account/rateLimits/read`.
4. Map a window of `windowDurationMins <= 360` to 5H and the longer window to weekly. Display remaining quota as `100 - usedPercent`.
5. Run the first refresh at launch and then every 10 seconds. Cache the last successful snapshot; if a request fails, label the timestamp `缓存 HH:mm` rather than showing a separate warning row.
6. Never call `StreamReader.EndOfStream` while waiting for app-server output: it can synchronously block the WPF UI. Await `ReadLineAsync` in a loop and stop only on `null`.
7. Serialize or coalesce manual and periodic refreshes if they can overlap. Apply UI snapshots with background dispatcher priority.

## Window and interaction

- Use one transparent, topmost WPF window with `SizeToContent="WidthAndHeight"`; put the status strip and detail panel in the same visual tree. Do not use a separate `Popup` for hover details.
- Align the window bottom-right to `SystemParameters.WorkArea` after every `SizeChanged`, so expansion grows upward from the taskbar.
- Keep status and detail borders content-sized; do not set fixed outer widths. Use nearly transparent fills, pale glass outlines, and compact text.
- Put `更新` and `隐藏` in the detail header's right side. Keep the 5H and weekly rows to one line each.
- Detect hover from actual cursor position every ~75 ms, transformed with `Window.PointFromScreen`, rather than depending only on WPF `MouseEnter`/`MouseLeave`. This remains reliable when refreshed text changes the auto-sized window.
- Hide the widget only through its explicit button or tray action; ignore pointer polling while the window is hidden.

## Lifecycle

- Use a single-instance mutex and a notification-area icon with `显示` and `退出工具` actions.
- Register a current-user scheduled launcher. It starts the widget only while the ChatGPT/Codex process and `~\.codex\.codex-global-state.json` are present, and stops it after Codex exits.
- Before publishing, stop the scheduled launcher and both widget processes to avoid locked assemblies. Publish the widget and launcher, reinstall the task, then verify both processes are alive.

## Validation and diagnosis

- Add a failing unit test before changing parsing, refresh coordination, or pure hover-state logic; run the full test suite before deployment.
- If clicking `更新` stalls the UI, first inspect the app-server reader for synchronous calls such as `EndOfStream`; do not reduce refresh frequency until the UI-thread block is ruled out.
- If hover fails around a refresh, verify that refresh code changes only snapshot text and that cursor tracking runs independently of the refresh task.
- Convert reset timestamps to `China Standard Time` for Chinese displays.
