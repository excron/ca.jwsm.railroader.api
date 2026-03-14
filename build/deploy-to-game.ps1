param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Railroader"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$modsRoot = Join-Path $repoRoot "..\ca.jwsm.railroader.mods"
$modsRoot = [System.IO.Path]::GetFullPath($modsRoot)

$apiHostProject = Join-Path $repoRoot "host\ca.jwsm.railroader.api.host.csproj"
$apiInfoPath = Join-Path $repoRoot "host\info.json"
$apiOutputDir = Join-Path $repoRoot "host\bin\Release"

$couplerProject = Join-Path $modsRoot "couplerforces\ca.jwsm.railroader.mods.couplerforces.csproj"
$couplerInfoPath = Join-Path $modsRoot "couplerforces\info.json"
$couplerOutputDir = Join-Path $modsRoot "couplerforces\bin\Release"

$gameModsDir = Join-Path $GameDir "Mods"
$apiInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.api"
$couplerInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.couplerforces"

if (-not (Test-Path $gameModsDir)) {
    throw "Game Mods directory not found: $gameModsDir"
}

dotnet build $apiHostProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
dotnet build $couplerProject -c Release -p:GAME_DIR="$GameDir" | Out-Host

New-Item -ItemType Directory -Force -Path $apiInstallDir | Out-Null
New-Item -ItemType Directory -Force -Path $couplerInstallDir | Out-Null

Copy-Item $apiInfoPath -Destination (Join-Path $apiInstallDir "info.json") -Force
Get-ChildItem $apiOutputDir -File -Filter "ca.jwsm.railroader.api*.dll" | Copy-Item -Destination $apiInstallDir -Force

Copy-Item $couplerInfoPath -Destination (Join-Path $couplerInstallDir "info.json") -Force
Copy-Item (Join-Path $couplerOutputDir "ca.jwsm.railroader.mods.couplerforces.dll") -Destination $couplerInstallDir -Force

Write-Host "Deployed API package to: $apiInstallDir"
Write-Host "Deployed Coupler Forces package to: $couplerInstallDir"
