@setlocal enableextensions
@cd /d "%~dp0"
@echo off
rem For every csproj file, edit it in place using the FixUp_Fix.sed sed script
for /R %%i in (*.csproj) do lib\sed\sed -i -f lib\sed\FixUp_Fix.sed %%i
del sed*