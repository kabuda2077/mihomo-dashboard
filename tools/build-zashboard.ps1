param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$sourceRoot = Join-Path $repoRoot 'dashboard-src'
$dashboardDir = Join-Path $repoRoot 'resources\dashboard'

if (-not (Test-Path (Join-Path $sourceRoot 'package.json'))) {
    throw "dashboard-src is missing. Restore the zashboard source before building."
}

$requiredFiles = @(
    'src\hostBootstrap.ts',
    'src\views\CorePage.vue',
    'src\router\index.ts',
    'src\constant\index.ts'
)

foreach ($relativePath in $requiredFiles) {
    $path = Join-Path $sourceRoot $relativePath
    if (-not (Test-Path $path)) {
        throw "dashboard source check failed: missing $relativePath"
    }
}

if ($SkipBuild) {
    Write-Host 'dashboard source check completed.'
    return
}

Push-Location $sourceRoot
try {
    pnpm install --frozen-lockfile
    pnpm run build
}
finally {
    Pop-Location
}

if (Test-Path $dashboardDir) {
    Remove-Item -LiteralPath $dashboardDir -Recurse -Force
}

New-Item -ItemType Directory -Path $dashboardDir | Out-Null
Copy-Item -Path (Join-Path $sourceRoot 'dist\*') -Destination $dashboardDir -Recurse -Force

& (Join-Path $repoRoot 'tools\create-app-icon.ps1')
