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
$apiWebOutputDir = Join-Path $repoRoot "web\bin\Release\net48"

$couplerProject = Join-Path $modsRoot "couplerforces\ca.jwsm.railroader.mods.couplerforces.csproj"
$couplerInfoPath = Join-Path $modsRoot "couplerforces\info.json"
$couplerOutputDir = Join-Path $modsRoot "couplerforces\bin\Release"

$locomotiveControlProject = Join-Path $modsRoot "locomotivecontrol\ca.jwsm.railroader.mods.locomotivecontrol.csproj"
$locomotiveControlInfoPath = Join-Path $modsRoot "locomotivecontrol\info.json"
$locomotiveControlOutputDir = Join-Path $modsRoot "locomotivecontrol\bin\Release"

$webViewProject = Join-Path $modsRoot "webview\ca.jwsm.railroader.mods.webview.csproj"
$webViewInfoPath = Join-Path $modsRoot "webview\info.json"
$webViewOutputDir = Join-Path $modsRoot "webview\bin\Release"
$webViewSettingsExamplePath = Join-Path $modsRoot "webview\Settings.example.json"
$webViewSettingsLocalPath = Join-Path $modsRoot "webview\Settings.local.json"

$mapLoaderProject = Join-Path $modsRoot "compat\mapmodloader\ca.jwsm.railroader.mods.compat.mapmodloader.csproj"
$mapLoaderInfoPath = Join-Path $modsRoot "compat\mapmodloader\info.json"
$mapLoaderOutputDir = Join-Path $modsRoot "compat\mapmodloader\bin\Release"

$gameModsDir = Join-Path $GameDir "Mods"
$apiInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.api"
$couplerInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.couplerforces"
$locomotiveControlInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.locomotivecontrol"
$webViewInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.webview"
$legacyDpuInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.dpu"
$mapLoaderInstallDir = Join-Path $gameModsDir "ca.jwsm.railroader.mods.compat.mapmodloader"

if (-not (Test-Path $gameModsDir)) {
    throw "Game Mods directory not found: $gameModsDir"
}

function Reset-InstallDir {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [string[]]$PreserveFiles = @()
    )

    New-Item -ItemType Directory -Force -Path $Path | Out-Null

    $preserve = @{}
    foreach ($file in $PreserveFiles) {
        if ([string]::IsNullOrWhiteSpace($file)) { continue }
        $preserve[$file] = $true
    }

    Get-ChildItem $Path -Force -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_.PSIsContainer) {
            Remove-Item $_.FullName -Force -Recurse -ErrorAction SilentlyContinue
            return
        }

        if ($preserve.ContainsKey($_.Name)) {
            return
        }

        Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
    }
}

dotnet build $apiHostProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
dotnet build $couplerProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
dotnet build $locomotiveControlProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
dotnet build $webViewProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
dotnet build $mapLoaderProject -c Release -p:GAME_DIR="$GameDir" | Out-Host

Reset-InstallDir -Path $apiInstallDir
Reset-InstallDir -Path $couplerInstallDir -PreserveFiles @("Settings.xml")
Reset-InstallDir -Path $locomotiveControlInstallDir
Reset-InstallDir -Path $webViewInstallDir -PreserveFiles @("Settings.local.json")
Reset-InstallDir -Path $mapLoaderInstallDir

if (Test-Path $legacyDpuInstallDir) {
    Remove-Item $legacyDpuInstallDir -Force -Recurse
}

Copy-Item $apiInfoPath -Destination (Join-Path $apiInstallDir "info.json") -Force
Get-ChildItem $apiOutputDir -File -Filter "ca.jwsm.railroader.api*.dll" | Copy-Item -Destination $apiInstallDir -Force
Get-ChildItem $apiWebOutputDir -File -Filter "ca.jwsm.railroader.api.web*.dll" | Copy-Item -Destination $apiInstallDir -Force

Copy-Item $couplerInfoPath -Destination (Join-Path $couplerInstallDir "info.json") -Force
Copy-Item (Join-Path $couplerOutputDir "ca.jwsm.railroader.mods.couplerforces.dll") -Destination $couplerInstallDir -Force

Copy-Item $locomotiveControlInfoPath -Destination (Join-Path $locomotiveControlInstallDir "info.json") -Force
Copy-Item (Join-Path $locomotiveControlOutputDir "ca.jwsm.railroader.mods.locomotivecontrol.dll") -Destination $locomotiveControlInstallDir -Force

Copy-Item $webViewInfoPath -Destination (Join-Path $webViewInstallDir "info.json") -Force
Get-ChildItem $webViewOutputDir -File -Filter "ca.jwsm.railroader.mods.webview*.dll" | Copy-Item -Destination $webViewInstallDir -Force
Get-ChildItem $webViewOutputDir -File -Filter "ca.jwsm.railroader.api*.dll" | Copy-Item -Destination $webViewInstallDir -Force
if (Test-Path $webViewSettingsExamplePath) {
    Copy-Item $webViewSettingsExamplePath -Destination (Join-Path $webViewInstallDir "Settings.example.json") -Force
}
if (Test-Path $webViewSettingsLocalPath) {
    Copy-Item $webViewSettingsLocalPath -Destination (Join-Path $webViewInstallDir "Settings.local.json") -Force
}

Copy-Item $mapLoaderInfoPath -Destination (Join-Path $mapLoaderInstallDir "info.json") -Force
Get-ChildItem $mapLoaderOutputDir -File -Filter "*.dll" | Copy-Item -Destination $mapLoaderInstallDir -Force

Write-Host "Deployed API package to: $apiInstallDir"
Write-Host "Deployed Coupler Forces package to: $couplerInstallDir"
Write-Host "Deployed Locomotive Control package to: $locomotiveControlInstallDir"
Write-Host "Deployed WebView package to: $webViewInstallDir"
Write-Host "Retired legacy DPU package folder: $legacyDpuInstallDir"
Write-Host "Deployed Map Mod Loader package to: $mapLoaderInstallDir"
