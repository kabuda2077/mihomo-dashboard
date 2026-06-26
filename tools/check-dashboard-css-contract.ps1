param()

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$sourceRoot = Join-Path $repoRoot 'dashboard-src'
$mainCssPath = Join-Path $sourceRoot 'src\assets\main.css'
$desktopCssPath = Join-Path $sourceRoot 'src\assets\styles\dashboard-desktop.css'

if (-not (Test-Path -LiteralPath $mainCssPath)) {
    throw "CSS contract check failed: missing src\assets\main.css"
}

if (-not (Test-Path -LiteralPath $desktopCssPath)) {
    throw "CSS contract check failed: missing src\assets\styles\dashboard-desktop.css"
}

$mainCss = Get-Content -LiteralPath $mainCssPath -Raw
$desktopCss = Get-Content -LiteralPath $desktopCssPath -Raw

$imports = [regex]::Matches($mainCss, "@import\s+['""]([^'""]+)['""]\s*;") |
    ForEach-Object { $_.Groups[1].Value }

if (-not $imports -or $imports[-1] -ne './styles/dashboard-desktop.css') {
    throw "CSS contract check failed: dashboard-desktop.css must be the last import in src\assets\main.css"
}

$requiredSelectors = @(
    '.settings-section-label',
    '.dashboard-section-title',
    '.settings-grid',
    '.setting-item',
    '.setting-panel-row',
    '.dashboard-input',
    '.dashboard-action-btn',
    '.dashboard-note',
    '.dashboard-log-block',
    '.core-status-box',
    '.core-top-button',
    '.toggle',
    '.ctrls-bar'
)

$missing = @()
foreach ($selector in $requiredSelectors) {
    if ($desktopCss -notmatch [regex]::Escape($selector)) {
        $missing += $selector
    }
}

if ($missing.Count -gt 0) {
    throw "CSS contract check failed: missing desktop selectors: $($missing -join ', ')"
}

Write-Host 'dashboard desktop CSS contract check completed.'
