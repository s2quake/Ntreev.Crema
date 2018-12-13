@echo off
powershell -executionpolicy remotesigned -File "%~dp0\gen-ver.ps1"
if not %errorlevel% == 0 pause
