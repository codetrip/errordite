net stop Errordite.Services$Reception
net stop Errordite.Services$Events
net stop Errordite.Services$Notifications

robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Reception" *.* /s
robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Events" *.* /s
robocopy "%CD%\core\Errordite.Services\bin\Debug" "%CD%\services\Notifications" *.* /s

net start Errordite.Services$Reception
net start Errordite.Services$Events
net start Errordite.Services$Notifications

