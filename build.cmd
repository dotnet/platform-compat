@echo off

for /d %%F in ("%ProgramFiles(x86)%\Microsoft Visual Studio\2017\*") DO (
 set MSBuild="%%F\MSBuild\15.0\Bin\MSBuild.exe"
 goto validate
)

:validate

if exist %MSBuild% goto build
echo ERROR: You need Visual Studio 2017 to build.
exit

:build

set OutDir="%~dp0bin"

if not exist %OutDir% mkdir %OutDir%
%MSBuild% /nologo /m /v:m /nr:false /flp:logfile=bin\msbuild.log;verbosity=normal /t:Restore /t:Build /p:OutDir=%OutDir% %*
exit
