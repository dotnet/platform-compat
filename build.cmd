@echo off

set MSBuild="%VSINSTALLDIR%\MSBuild\15.0\Bin\MSBuild.exe"
if exist %MSBuild% goto build
echo ERROR: You need Visual Studio 2017 to build.
exit /B -1

:build

setlocal
set OutDir="%~dp0bin"

if not exist %OutDir% mkdir %OutDir%
%MSBuild% /nologo /m /v:m /nr:false /flp:logfile=bin\msbuild.log;verbosity=normal /t:Restore /t:Build /p:OutDir=%OutDir% %*
