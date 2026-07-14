param(
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.-]+)?$')]
    [string]$Version = '0.1.0',
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
if (-not $PSBoundParameters.ContainsKey('OutputDirectory')) {
    $OutputDirectory = Join-Path $repositoryRoot 'artifacts'
}

dotnet test (Join-Path $repositoryRoot 'CodexUsageTaskbar.sln')
if ($LASTEXITCODE -ne 0) { throw 'Solution tests failed.' }

$stage = Join-Path ([System.IO.Path]::GetTempPath()) ("CodexUsageTaskbar-$Version-" + [guid]::NewGuid())
$packageName = "CodexUsageTaskbar-win-x64-$Version"
$packageRoot = Join-Path $stage $packageName
New-Item -ItemType Directory -Force $packageRoot | Out-Null

try {
    dotnet publish (Join-Path $repositoryRoot 'src\CodexUsageTaskbar\CodexUsageTaskbar.csproj') -c Release -r win-x64 --self-contained true -o $packageRoot
    if ($LASTEXITCODE -ne 0) { throw 'Widget publish failed.' }
    dotnet publish (Join-Path $repositoryRoot 'src\CodexUsageTaskbar.Launcher\CodexUsageTaskbar.Launcher.csproj') -c Release -r win-x64 --self-contained true -o $packageRoot
    if ($LASTEXITCODE -ne 0) { throw 'Launcher publish failed.' }

    foreach ($requiredFile in @('CodexUsageTaskbar.exe', 'CodexUsageTaskbar.Launcher.exe')) {
        if (-not (Test-Path -LiteralPath (Join-Path $packageRoot $requiredFile))) {
            throw "Publish output is missing $requiredFile"
        }
    }

    Copy-Item -LiteralPath (Join-Path $repositoryRoot 'scripts\install-autostart.ps1') -Destination $packageRoot
    Copy-Item -LiteralPath (Join-Path $repositoryRoot 'README.md') -Destination $packageRoot
    Copy-Item -LiteralPath (Join-Path $repositoryRoot 'docs\INSTALL.md') -Destination $packageRoot
    New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
    $zipPath = Join-Path $OutputDirectory "$packageName.zip"
    $shaPath = "$zipPath.sha256"
    Remove-Item -LiteralPath $zipPath, $shaPath -Force -ErrorAction SilentlyContinue
    Compress-Archive -LiteralPath $packageRoot -DestinationPath $zipPath -Force
    if (-not (Test-Path -LiteralPath $zipPath)) { throw 'Release ZIP was not created.' }
    $hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
    Set-Content -LiteralPath $shaPath -Value "$hash *$(Split-Path -Leaf $zipPath)" -Encoding utf8
    Write-Output "Release ZIP: $zipPath"
    Write-Output "SHA-256: $shaPath"
}
finally {
    Remove-Item -LiteralPath $stage -Recurse -Force -ErrorAction SilentlyContinue
}
