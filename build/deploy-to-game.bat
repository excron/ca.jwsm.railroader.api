@echo off
setlocal
powershell -ExecutionPolicy Bypass -File "%~dp0deploy-to-game.ps1" %*
