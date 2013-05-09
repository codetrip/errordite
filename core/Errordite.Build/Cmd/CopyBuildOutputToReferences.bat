if exist %2obj\%3\%4.dll xcopy %2obj\%3\%4.dll %1\..\lib\Errordite\%3\ /Y /R
if exist %2obj\%3\%4.pdb xcopy %2obj\%3\%4.pdb %1\..\lib\Errordite\%3\ /Y /R
if exist %2obj\%3\%4.exe xcopy %2obj\%3\%4.exe %1\..\lib\Errordite\%3\ /Y /R