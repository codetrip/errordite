net stop Errordite.Services$Receive
net stop Errordite.Services$Events
net stop Errordite.Services$Notifications

robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Receive" *.* /s
robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Events" *.* /s
robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Notifications" *.* /s

net start Errordite.Services$Receive
net start Errordite.Services$Events
net start Errordite.Services$Notifications

