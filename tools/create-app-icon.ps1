$ErrorActionPreference = "Stop"

$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$sourceFile = Join-Path $root "resources\dashboard\favicon.ico"
$outDir = Join-Path $root "resources"
$outFile = Join-Path $outDir "app.ico"

if (-not (Test-Path $sourceFile)) {
    throw "zashboard favicon not found: $sourceFile"
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null
Copy-Item -LiteralPath $sourceFile -Destination $outFile -Force

Write-Host "Synced zashboard icon to $outFile"
