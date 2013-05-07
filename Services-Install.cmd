if exist "%CD%\services" RD /S /Q "%CD%\services"

mkdir "services"
mkdir "services\Receive"
mkdir "services\Events"
mkdir "services\Notifications"

robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Receive" *.* /s
robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Events" *.* /s
robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Notifications" *.* /s

"%CD%\services\Receive\Errordite.Services.exe" install -instance:Receive
"%CD%\services\Events\Errordite.Services.exe" install -instance:Events
"%CD%\services\Notifications\Errordite.Services.exe" install -instance:Notifications

net start Errordite.Services$Receive
net start Errordite.Services$Events
net start Errordite.Services$Notifications