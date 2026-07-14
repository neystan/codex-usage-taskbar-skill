param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

function Assert-Exists {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Expected path is missing: $Path"
    }
}

$requiredPaths = @(
    '.gitignore',
    'docs/INSTALL.md',
    '.agents/plugins/marketplace.json',
    'plugins/codex-taskbar-usage-widget/.codex-plugin/plugin.json',
    'plugins/codex-taskbar-usage-widget/skills/codex-taskbar-usage-widget/SKILL.md',
    'scripts/sync-plugin-template.ps1',
    'scripts/package-release.ps1'
)

foreach ($relativePath in $requiredPaths) {
    Assert-Exists (Join-Path $RepositoryRoot $relativePath)
}

Write-Output 'Distribution layout checks passed.'
