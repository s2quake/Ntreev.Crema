@echo off
powershell -executionpolicy remotesigned -File "%~dp0\build.ps1"
if not %errorlevel% == 0 pause
