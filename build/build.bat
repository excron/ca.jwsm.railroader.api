@echo off
setlocal

set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set DOTNET_CLI_HOME=%ROOT_DIR%
set DOTNET_CLI_TELEMETRY_OPTOUT=1
set DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
set DOTNET_NOLOGO=1
set APPDATA=%SCRIPT_DIR%appdata

dotnet build "%ROOT_DIR%\ca.jwsm.railroader.api.sln" --configfile "%SCRIPT_DIR%NuGet.Config" -m:1 -p:RestoreDisableParallel=true -p:UseSharedCompilation=false %*
exit /b %ERRORLEVEL%
