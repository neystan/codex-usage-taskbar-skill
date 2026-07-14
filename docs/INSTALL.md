# 安装与复用

## 使用插件让 Codex 定制

克隆团队仓库后，在仓库根目录运行：

```powershell
codex plugin marketplace add .
codex plugin add codex-taskbar-usage-widget@personal
```

随后新建一个 Codex 任务，说明你需要的窗口样式、刷新频率或自动启动行为。Skill 会优先从插件中附带的 `assets/source-template` 复制经过验证的源码；只有在需求明显不同的时候才创建新的实现。

## 直接复用源码

从仓库根目录运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\sync-plugin-template.ps1 -Destination C:\work\MyCodexUsageTaskbar
cd C:\work\MyCodexUsageTaskbar
dotnet test CodexUsageTaskbar.sln
```

目标目录必须不存在或为空，脚本不会覆盖已有项目。

## 直接运行发布包

从 GitHub Releases 下载 `CodexUsageTaskbar-win-x64-<版本>.zip`，解压后运行 `CodexUsageTaskbar.Launcher.exe`。该包为 self-contained `win-x64`，无需安装 .NET。

如需随 Codex 自动启动：

```powershell
powershell -ExecutionPolicy Bypass -File .\install-autostart.ps1 -LauncherPath .\CodexUsageTaskbar.Launcher.exe
```

下载后用同目录 `.sha256` 文件中的 SHA-256 校验 ZIP 完整性。

