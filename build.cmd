@echo off

set MSBuild="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
set OutDir="%~dp0bin"

if not exist %OutDir% mkdir %OutDir%
%MSBuild% /nologo /m /v:m /nr:false /flp:logfile=bin\msbuild.log;verbosity=normal /t:Restore /t:Build /p:OutDir=%OutDir% %* 