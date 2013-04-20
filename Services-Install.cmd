if exist "%CD%\services" RD /S /Q "%CD%\services"

mkdir "services"
mkdir "services\Reception"
mkdir "services\Events"
mkdir "services\Notifications"

robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Reception" *.* /s
robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Events" *.* /s
robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Notifications" *.* /s

"%CD%\services\Reception\Errordite.Services.exe" install -instance:Reception
"%CD%\services\Events\Errordite.Services.exe" install -instance:Events
"%CD%\services\Notifications\Errordite.Services.exe" install -instance:Notifications

net start Errordite.Services$Reception
net start Errordite.Services$Events
net start Errordite.Services$Notifications