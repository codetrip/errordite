@setlocal enableextensions
@cd /d "%~dp0"
call "%VS100COMNTOOLS%..\..\VC\vcvarsall.bat"

"%FrameworkDir%\%FrameworkVersion%\msbuild.exe" "%CD%\core\Errordite.Build\Projects\Build.proj" /logger:FileLogger,Microsoft.Build.Engine;Package.log /p:Configuration=Debug /p:Targets="Build" /p:Platform="Any Cpu" /p:Branch=trunk /m 

if NOT %ERRORLEVEL% == 0 pause