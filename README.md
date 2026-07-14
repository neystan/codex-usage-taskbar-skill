# Codex Usage Taskbar Skill

这是一个 Windows Codex 用量任务栏组件的 Skill 包。它读取本机 Codex 的 5 小时与周额度，在任务栏旁显示剩余额度；支持悬停详情、手动刷新、托盘菜单和随 Codex 自动启动。

仓库只保留两类可交付内容：

- `codex-taskbar-usage-widget/SKILL.md`：让 Codex 按既定体验自行设计、创建或改造组件的规则；
- `codex-taskbar-usage-widget/source/`：已经实现好的同款 WPF 源码，供 Codex 直接复用。

## 方式一：安装 Skill 后自行设计

把整个 `codex-taskbar-usage-widget` 文件夹复制到本机：

```text
%USERPROFILE%\.codex\skills\codex-taskbar-usage-widget\
```

重启 Codex 或新建任务，然后直接提出需求，例如：

> 为我设计一个任务栏旁的 Codex 用量组件，保持玻璃风格，但把状态条改成圆角胶囊。

Codex 会遵循 Skill 中的数据读取、安全、刷新、悬停和自动启动约束来实现你的版本。

## 方式二：直接复用同款源码

将 `codex-taskbar-usage-widget/source/` 复制到你的工作区，再让 Codex 直接基于该目录修改：

> 使用这个 source 文件夹中的 WPF 项目，直接构建并运行同款 Codex Usage Taskbar；只在我明确提出时改变功能或外观。

源码需要 Windows 与 .NET 9 SDK。构建命令：

```powershell
cd codex-taskbar-usage-widget\source
dotnet build CodexUsageTaskbar.sln -c Release
```

构建完成后运行 `src\CodexUsageTaskbar\bin\Release\net9.0-windows\CodexUsageTaskbar.exe`。如需安装自动启动任务：

```powershell
powershell -ExecutionPolicy Bypass -File .\install-autostart.ps1 -LauncherPath .\src\CodexUsageTaskbar.Launcher\bin\Release\net9.0\CodexUsageTaskbar.Launcher.exe
```
