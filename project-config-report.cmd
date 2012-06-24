@setlocal enableextensions
@cd /d "%~dp0"
@echo off
rem For every csproj file, find the things that are wrong with it and add it to a text file
if exist FixUpReport.txt del FixUpReport.txt
for /R %%i in (*.csproj) do echo %%i >> FixUpReport.txt && lib\sed\sed -n -f lib\sed\FixUp_Report.sed %%i >> FixUpReport.txt
start FixUpReport.txt