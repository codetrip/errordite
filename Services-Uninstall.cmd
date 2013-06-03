net stop Errordite.Services$Receive
net stop Errordite.Services$Events
net stop Errordite.Services$Notifications

"%CD%\Services\Receive\Errordite.Services.exe" uninstall -instance:Receive
"%CD%\Services\Events\Errordite.Services.exe" uninstall -instance:Events
"%CD%\Services\Notifications\Errordite.Services.exe" uninstall -instance:Notifications