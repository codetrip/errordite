@setlocal enableextensions
@cd /d "%~dp0"
call "%VS100COMNTOOLS%..\..\VC\vcvarsall.bat"

"%FrameworkDir%\%FrameworkVersion%\msbuild.exe" "%CD%\core\Errordite.Build\Projects\GenerateMinifiedResources.proj" /logger:FileLogger,Microsoft.Build.Engine;Package.log /p:SourcePath=%CD% /p:BuildNumber=1.0.1.0 /p:Targets="Build" /m 
pause
if NOT %ERRORLEVEL% == 0 pause