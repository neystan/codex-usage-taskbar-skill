param(
    [Parameter(Mandatory = $true)] [string]$LauncherPath
)

$ErrorActionPreference = 'Stop'
$launcher = (Resolve-Path -LiteralPath $LauncherPath).Path
$userId = "$env:USERDOMAIN\$env:USERNAME"
$action = New-ScheduledTaskAction -Execute $launcher
$trigger = New-ScheduledTaskTrigger -AtLogOn -User $userId
$principal = New-ScheduledTaskPrincipal -UserId $userId -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -ExecutionTimeLimit (New-TimeSpan -Days 365)
Register-ScheduledTask -TaskName 'CodexUsageTaskbarLauncher' -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Description 'Starts the Codex quota taskbar widget while Codex is running.' -Force | Out-Null
Start-ScheduledTask -TaskName 'CodexUsageTaskbarLauncher'
