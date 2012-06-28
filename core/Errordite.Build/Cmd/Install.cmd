@ECHO OFF
setlocal

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
cls
echo.
echo Errordite Install Menu
echo.
echo. Component Selection Menu (Target Environment is '%TARGETENVIRONMENT%')
echo.
echo	A. WEB: Errordite Web
echo	B. WEB: Errordite Reception Web
echo	C. SVC: Errordite Reception Svc
echo	D. SVC: Errordite Notifications Svc
echo	E. SVC: Errordite Events Svc
echo	F. Full Install
echo	0. ABORT DEPLOYMENT
echo.
choice /C ABCDEF0 /M "Please select the component you wish to deploy:"

if "%ERRORLEVEL%" == "7" goto DOABORT
if "%ERRORLEVEL%" == "1" (
	set MSBUILDTARGET=InstallWeb
	set DEPLOYDISPLAYNAME=WEB: Errordite Web
)
if "%ERRORLEVEL%" == "2" (
	set MSBUILDTARGET=InstallReceptionWeb
	set DEPLOYDISPLAYNAME=WEB: Errordite Reception Web
)
if "%ERRORLEVEL%" == "3" (
	set MSBUILDTARGET=InstallReceptionService
	set DEPLOYDISPLAYNAME=SVC: Errordite Reception Svc
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
	set MSBUILDTARGET=ALL
	set DEPLOYDISPLAYNAME=Full Install
)

pushd %MSBUILDPROJDIR%
echo.
echo Deploying '%DEPLOYDISPLAYNAME%'...
choice /C YN /M "Are you sure?"
if "%ERRORLEVEL%" neq "1" goto DEPLOYMENTMENU

if "%MSBUILDTARGET%" == "ALL" (
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallWeb.log /v:diag /t:InstallWeb /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallReceptionWeb.log /v:diag /t:InstallReceptionWeb /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallReceptionService.log /v:diag /t:InstallReceptionService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallNotificationsService.log /v:diag /t:InstallNotificationsService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallEventsService.log /v:diag /t:InstallEventsService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true

	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallWeb_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallWeb_Gen.log /v:diag /t:InstallWeb /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallReceptionWeb_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallReceptionWeb_Gen.log /v:diag /t:InstallReceptionWeb /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallReceptionService_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallReceptionService_Gen.log /v:diag /t:InstallReceptionService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallNotificationsService_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallNotificationsService_Gen.log /v:diag /t:InstallNotificationsService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallEventsService_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_InstallEventsService_Gen.log /v:diag /t:InstallEventsService /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
) else (
	echo.
	%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\InstallMaster.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_%MSBUILDTARGET%.log /v:diag /t:%MSBUILDTARGET% /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true

	if "%ERRORLEVEL%" == "0" (
		echo.
		echo ****************************
		echo *** RUNNING GENERATED BUILD %MSBUILDTARGET%_Gen.proj ***
		echo ****************************
		echo.
		%windir%\Microsoft.NET\Framework64\v4.0.30319\msbuild "%CD%\Errordite.Install\%MSBUILDTARGET%_Gen.proj" /logger:FileLogger,Microsoft.Build.Engine;Deploy_%MSBUILDTARGET%.log /v:diag /t:%MSBUILDTARGET% /p:TargetEnvironment=%TARGETENVIRONMENT% /p:Install=true
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
)

rem If there were no errors that report success and return to the deployment menu
if "%ERRORLEVEL%" == "0" (
	echo.
	echo.
	echo ****************************
	echo *** DEPLOYMENT SUCCEEDED ***
	echo ****************************
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
