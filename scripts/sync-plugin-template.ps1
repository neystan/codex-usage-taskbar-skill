param(
    [string]$Destination
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$defaultDestination = Join-Path $repositoryRoot 'plugins\codex-taskbar-usage-widget\assets\source-template'
$explicitDestination = $PSBoundParameters.ContainsKey('Destination')

if (-not $explicitDestination) {
    $Destination = $defaultDestination
}

if (Test-Path -LiteralPath $Destination) {
    $existingFiles = Get-ChildItem -LiteralPath $Destination -Force
    if ($explicitDestination -and $existingFiles.Count -gt 0) {
        throw "Destination is not empty: $Destination"
    }
    if (-not $explicitDestination) {
        Remove-Item -LiteralPath $Destination -Recurse -Force
    }
}

New-Item -ItemType Directory -Force $Destination | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'CodexUsageTaskbar.sln') -Destination $Destination
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'README.md') -Destination $Destination
New-Item -ItemType Directory -Force (Join-Path $Destination 'docs') | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'docs\INSTALL.md') -Destination (Join-Path $Destination 'docs\INSTALL.md')
New-Item -ItemType Directory -Force (Join-Path $Destination 'scripts') | Out-Null
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'scripts\install-autostart.ps1') -Destination (Join-Path $Destination 'scripts\install-autostart.ps1')

foreach ($directory in @('src', 'tests')) {
    $source = Join-Path $repositoryRoot $directory
    $target = Join-Path $Destination $directory
    & robocopy $source $target /E /XD bin obj | Out-Null
    if ($LASTEXITCODE -gt 7) {
        throw "robocopy failed for $directory with exit code $LASTEXITCODE"
    }
}

Write-Output "Source template synchronized to $Destination"
