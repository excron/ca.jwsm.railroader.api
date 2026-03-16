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

$locomotiveControlProject = Join-Path $modsRoot "locomotivecontrol\ca.jwsm.railroader.mods.locomotivecontrol.csproj"
$locomotiveControlInfoPath = Join-Path $modsRoot "locomotivecontrol\info.json"
$locomotiveControlOutputDir = Join-Path $modsRoot "locomotivecontrol\bin\Release"

$mapLoaderProject = Join-Path $modsRoot "mapmodloader\ca.jwsm.railroader.mods.mapmodloader.csproj"
$mapLoaderInfoPath = Join-Path $modsRoot "mapmodloader\info.json"
$mapLoaderOutputDir = Join-Path $modsRoot "mapmodloader\bin\Release"

$gameModsDir = Join-Path $GameDir "Mods"
$apiInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.api"
$couplerInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.couplerforces"
$locomotiveControlInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.locomotivecontrol"
$legacyDpuInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.dpu"
$mapLoaderInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.mapmodloader"

if (-not (Test-Path $gameModsDir)) {
    throw "Game Mods directory not found: $gameModsDir"
}

dotnet build $apiHostProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
dotnet build $couplerProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
dotnet build $locomotiveControlProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
dotnet build $mapLoaderProject -c Release -p:GAME_DIR="$GameDir" | Out-Host

New-Item -ItemType Directory -Force -Path $apiInstallDir | Out-Null
New-Item -ItemType Directory -Force -Path $couplerInstallDir | Out-Null
New-Item -ItemType Directory -Force -Path $locomotiveControlInstallDir | Out-Null
New-Item -ItemType Directory -Force -Path $mapLoaderInstallDir | Out-Null

if (Test-Path $legacyDpuInstallDir) {
    Remove-Item $legacyDpuInstallDir -Force -Recurse
}

Get-ChildItem $mapLoaderInstallDir -Force | Remove-Item -Force -Recurse

Copy-Item $apiInfoPath -Destination (Join-Path $apiInstallDir "info.json") -Force
Get-ChildItem $apiOutputDir -File -Filter "ca.jwsm.railroader.api*.dll" | Copy-Item -Destination $apiInstallDir -Force

Copy-Item $couplerInfoPath -Destination (Join-Path $couplerInstallDir "info.json") -Force
Copy-Item (Join-Path $couplerOutputDir "ca.jwsm.railroader.mods.couplerforces.dll") -Destination $couplerInstallDir -Force

Copy-Item $locomotiveControlInfoPath -Destination (Join-Path $locomotiveControlInstallDir "info.json") -Force
Copy-Item (Join-Path $locomotiveControlOutputDir "ca.jwsm.railroader.mods.locomotivecontrol.dll") -Destination $locomotiveControlInstallDir -Force

Copy-Item $mapLoaderInfoPath -Destination (Join-Path $mapLoaderInstallDir "info.json") -Force
Get-ChildItem $mapLoaderOutputDir -File -Filter "*.dll" | Copy-Item -Destination $mapLoaderInstallDir -Force

Write-Host "Deployed API package to: $apiInstallDir"
Write-Host "Deployed Coupler Forces package to: $couplerInstallDir"
Write-Host "Deployed Locomotive Control package to: $locomotiveControlInstallDir"
Write-Host "Retired legacy DPU package folder: $legacyDpuInstallDir"
Write-Host "Deployed Map Mod Loader package to: $mapLoaderInstallDir"