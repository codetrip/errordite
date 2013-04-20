net stop Errordite.Services$Reception
net stop Errordite.Services$Events
net stop Errordite.Services$Notifications

"%CD%\Services\Reception\Errordite.Services.exe" uninstall -instance:Reception
"%CD%\Services\Events\Errordite.Services.exe" uninstall -instance:Events
"%CD%\Services\Notifications\Errordite.Services.exe" uninstall -instance:Notifications