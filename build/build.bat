@echo off
powershell -executionpolicy remotesigned -File build.ps1
if not %errorlevel% == 0 pause
