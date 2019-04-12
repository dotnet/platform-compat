@echo off
powershell -ExecutionPolicy ByPass %~dp0eng\common\Build.ps1 -test %*
exit /b %ErrorLevel%