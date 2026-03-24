param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Railroader",
    [switch]$IncludeCouplerForces,
    [switch]$IncludeLocomotiveControl,
    [switch]$IncludeMapLoader,
    [string]$WebPublishDir = "",
    [switch]$PublishWebProxy
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$modsRoot = Join-Path $repoRoot "..\ca.jwsm.railroader.mods"
$modsRoot = [System.IO.Path]::GetFullPath($modsRoot)
$webRoot = Join-Path $repoRoot "..\ca.jwsm.railroader.web"
$webRoot = [System.IO.Path]::GetFullPath($webRoot)

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
dotnet build $webViewProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
if ($IncludeCouplerForces) {
    dotnet build $couplerProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
}
if ($IncludeLocomotiveControl) {
    dotnet build $locomotiveControlProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
}
if ($IncludeMapLoader) {
    dotnet build $mapLoaderProject -c Release -p:GAME_DIR="$GameDir" | Out-Host
}

Reset-InstallDir -Path $apiInstallDir
Reset-InstallDir -Path $webViewInstallDir -PreserveFiles @("Settings.local.json")
if ($IncludeCouplerForces) {
    Reset-InstallDir -Path $couplerInstallDir -PreserveFiles @("Settings.xml")
}
if ($IncludeLocomotiveControl) {
    Reset-InstallDir -Path $locomotiveControlInstallDir
}
if ($IncludeMapLoader) {
    Reset-InstallDir -Path $mapLoaderInstallDir
}

if (Test-Path $legacyDpuInstallDir) {
    Remove-Item $legacyDpuInstallDir -Force -Recurse
}

Copy-Item $apiInfoPath -Destination (Join-Path $apiInstallDir "info.json") -Force
Get-ChildItem $apiOutputDir -File -Filter "ca.jwsm.railroader.api*.dll" | Copy-Item -Destination $apiInstallDir -Force
Get-ChildItem $apiWebOutputDir -File -Filter "ca.jwsm.railroader.api.web*.dll" | Copy-Item -Destination $apiInstallDir -Force

Copy-Item $webViewInfoPath -Destination (Join-Path $webViewInstallDir "info.json") -Force
Get-ChildItem $webViewOutputDir -File -Filter "ca.jwsm.railroader.mods.webview*.dll" | Copy-Item -Destination $webViewInstallDir -Force
Get-ChildItem $webViewOutputDir -File -Filter "ca.jwsm.railroader.api*.dll" | Copy-Item -Destination $webViewInstallDir -Force
if (Test-Path $webViewSettingsExamplePath) {
    Copy-Item $webViewSettingsExamplePath -Destination (Join-Path $webViewInstallDir "Settings.example.json") -Force
}
if (Test-Path $webViewSettingsLocalPath) {
    Copy-Item $webViewSettingsLocalPath -Destination (Join-Path $webViewInstallDir "Settings.local.json") -Force
}

if ($IncludeCouplerForces) {
    Copy-Item $couplerInfoPath -Destination (Join-Path $couplerInstallDir "info.json") -Force
    Copy-Item (Join-Path $couplerOutputDir "ca.jwsm.railroader.mods.couplerforces.dll") -Destination $couplerInstallDir -Force
}

if ($IncludeLocomotiveControl) {
    Copy-Item $locomotiveControlInfoPath -Destination (Join-Path $locomotiveControlInstallDir "info.json") -Force
    Copy-Item (Join-Path $locomotiveControlOutputDir "ca.jwsm.railroader.mods.locomotivecontrol.dll") -Destination $locomotiveControlInstallDir -Force
}

if ($IncludeMapLoader) {
    Copy-Item $mapLoaderInfoPath -Destination (Join-Path $mapLoaderInstallDir "info.json") -Force
    Get-ChildItem $mapLoaderOutputDir -File -Filter "*.dll" | Copy-Item -Destination $mapLoaderInstallDir -Force
}

if (-not [string]::IsNullOrWhiteSpace($WebPublishDir)) {
    if (-not (Test-Path $WebPublishDir)) {
        throw "Web publish directory not found: $WebPublishDir"
    }

    $webPublicDir = Join-Path $webRoot "public"
    $webFiles = @("index.php", "styles.css", "app.js")
    if ($PublishWebProxy) {
        $webFiles += "proxy.php"
    }

    foreach ($fileName in $webFiles) {
        Copy-Item (Join-Path $webPublicDir $fileName) -Destination (Join-Path $WebPublishDir $fileName) -Force
    }
}

Write-Host "Deployed API package to: $apiInstallDir"
Write-Host "Deployed WebView package to: $webViewInstallDir"
Write-Host "Retired legacy DPU package folder: $legacyDpuInstallDir"
if ($IncludeCouplerForces) {
    Write-Host "Deployed Coupler Forces package to: $couplerInstallDir"
}
if ($IncludeLocomotiveControl) {
    Write-Host "Deployed Locomotive Control package to: $locomotiveControlInstallDir"
}
if ($IncludeMapLoader) {
    Write-Host "Deployed Map Mod Loader package to: $mapLoaderInstallDir"
}
if (-not [string]::IsNullOrWhiteSpace($WebPublishDir)) {
    Write-Host "Published web client assets to: $WebPublishDir"
    if (-not $PublishWebProxy) {
        Write-Host "Skipped proxy.php publish; existing server-local proxy was preserved."
    }
}
