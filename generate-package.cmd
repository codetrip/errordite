@setlocal enableextensions
@cd /d "%~dp0"
call "%VS100COMNTOOLS%..\..\VC\vcvarsall.bat"

"%FrameworkDir%\%FrameworkVersion%\msbuild.exe" "%CD%\core\Errordite.Build\Projects\Package.proj" /logger:FileLogger,Microsoft.Build.Engine;Package.log /p:Configuration=Release /p:Targets="Package" /p:Platform="Any Cpu" /p:Branch=trunk /p:ErrorditeBuildDestination=%ErrorditeBuildDestination% /m 

pause
if NOT %ERRORLEVEL% == 0 pause