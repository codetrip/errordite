@ECHO OFF
setlocal

set TARGETENVIRONMENT=%1
set MSBUILDTARGET=%2
set AUTO=true

if "%TARGETENVIRONMENT%" neq "" goto DEPLOYMENTMENU

set AUTO=false
:ENVIRONMENTMENU
SET TARGETENVIRONMENT="**UNDEFINED**"
cls
echo.
echo ASOS Sitecore - Server Deployment Environment Menu
echo.
echo. Environment Selection Menu
echo.
echo	A. Dev
echo	B. Test
echo	C. Production
echo	0. ABORT DEPLOYMENT
echo.
choice /C ABC0 /M "Please select you target environment:"

if "%ERRORLEVEL%" == "4" goto DOABORT
if "%ERRORLEVEL%" == "1" (
	set TARGETENVIRONMENT=dev
)
if "%ERRORLEVEL%" == "2" (
	set TARGETENVIRONMENT=test
)
if "%ERRORLEVEL%" == "3" (
	set TARGETENVIRONMENT=production
)
goto DEPLOYMENTMENU


:DEPLOYMENTMENU
if "%MSBUILDTARGET%" neq "" goto STARTDEPLOY

set AUTO=false
cls
echo.
echo Errordite Install Menu
echo.
echo. Component Selection Menu (Target Environment is '%TARGETENVIRONMENT%')
echo.
echo	A. WEB: Errordite Web
echo	B. WEB: Errordite Receive Web
echo	C. SVC: Errordite Receive Svc
echo	D. SVC: Errordite Notifications Svc
echo	E. SVC: Errordite Events Svc
echo	F. SVC: Errordite Tasks
echo	G. All Web
echo	H. All Services
echo	0. ABORT DEPLOYMENT
echo.
choice /C ABCDEFGH0 /M "Please select the component you wish to deploy:"

if "%ERRORLEVEL%" == "9" goto DOABORT
if "%ERRORLEVEL%" == "1" (
	set MSBUILDTARGET=InstallWeb
	set DEPLOYDISPLAYNAME=WEB: Errordite Web
)
if "%ERRORLEVEL%" == "2" (
	set MSBUILDTARGET=InstallReceiveWeb
	set DEPLOYDISPLAYNAME=WEB: Errordite Receive Web
)
if "%ERRORLEVEL%" == "3" (
	set MSBUILDTARGET=InstallReceiveService
	set DEPLOYDISPLAYNAME=SVC: Errordite Receive Svc
)
if "%ERRORLEVEL%" == "4" (
	set MSBUILDTARGET=InstallNotificationsService
	set DEPLOYDISPLAYNAME=SVC: Errordite Notifications Svc
)
if "%ERRORLEVEL%" == "5" (
	set MSBUILDTARGET=InstallEventsService
	set DEPLOYDISPLAYNAME=SVC: Errordite Events Svc
)
if "%ERRORLEVEL%" == "6" (
	set MSBUILDTARGET=InstallTasks
	set DEPLOYDISPLAYNAME=SVC: Errordite Tasks
)
if "%ERRORLEVEL%" == "7" (
	set MSBUILDTARGET=ALLWEB
	set DEPLOYDISPLAYNAME=Install All Web
)
if "%ERRORLEVEL%" == "8" (
	set MSBUILDTARGET=ALLSERVICES
	set DEPLOYDISPLAYNAME=Install All Services
)

pushd %MSBUILDPROJDIR%
echo.
echo Deploying '%DEPLOYDISPLAYNAME%'...
choice /C YN /M "Are you sure?"
if "%ERRORLEVEL%" neq "1" goto DEPLOYMENTMENU 

:STARTDEPLOY

if "%MSBUILDTARGET%" == "ALLWEB" (

	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallWeb.log /v:diag /t:InstallWeb /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallReceiveWeb.log /v:diag /t:InstallReceiveWeb /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallWeb_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallWeb_Gen.log /v:diag /t:InstallWeb /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallReceiveWeb_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallReceiveWeb_Gen.log /v:diag /t:InstallReceiveWeb /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true

) else if "%MSBUILDTARGET%" == "ALLSERVICES" (

	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallReceiveService.log /v:diag /t:InstallReceiveService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallNotificationsService.log /v:diag /t:InstallNotificationsService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallEventsService.log /v:diag /t:InstallEventsService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	REM %windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallScheduledTasks.log /v:diag /t:InstallScheduledTasks /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true

	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallReceiveService_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallReceiveService_Gen.log /v:diag /t:InstallReceiveService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallNotificationsService_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallNotificationsService_Gen.log /v:diag /t:InstallNotificationsService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallEventsService_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallEventsService_Gen.log /v:diag /t:InstallEventsService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	REM %windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallScheduledTasks_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallEventsService_Gen.log /v:diag /t:InstallScheduledTasks /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true

) else (

	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_%MSBUILDTARGET%.log /v:diag /t:%MSBUILDTARGET% /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\%MSBUILDTARGET%_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_%MSBUILDTARGET%.log /v:diag /t:%MSBUILDTARGET% /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true

)

REM If there were no errors that report success and return to the deployment menu

if "%ERRORLEVEL%" == "0" (
	echo.
	echo.
	echo ****************************
	echo *** DEPLOYMENT SUCCEEDED ***
	echo ****************************
	if "%AUTO%"=="true" exit /b 0
	pause
	popd
	goto DEPLOYMENTMENU
) else (
	echo.
	echo.
	echo *************************
	echo *** DEPLOYMENT FAILED ***
	echo *************************
	echo.
	echo Please review the MSBuild log file 'Deploy_%MSBUILDTARGET%.log' for more information.
	echo.
	pause
	endlocal
	exit /b 1 
)

:DOABORT
echo.
echo **************************
echo *** Deployment Aborted ***
echo **************************
echo.
pause
endlocal
exit /b 0
