# Codex Usage Taskbar

Windows 原生 Codex 额度任务栏组件：状态条显示 5 小时剩余额度，悬停可查看 5 小时与周额度、刷新时间和操作按钮。

这个仓库同时提供两种团队使用方式：

- 安装 `codex-taskbar-usage-widget` 插件，让 Codex 从规范或随插件附带的源码模板创建适合本机的版本；
- 直接复制源码模板，或从 GitHub Releases 下载 self-contained `win-x64` 发布包。

完整安装、源码复用和发布包说明见 [docs/INSTALL.md](docs/INSTALL.md)。

## 开发

```powershell
dotnet test CodexUsageTaskbar.sln
powershell -ExecutionPolicy Bypass -File .\scripts\sync-plugin-template.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\package-release.ps1 -Version 0.1.0
```

发布脚本会先运行全部测试，再在 `artifacts/` 中生成 self-contained `win-x64` ZIP 与 SHA-256 文件。该目录不会提交到 Git；将来在 GitHub 中创建 Release 时上传这两个文件即可。

