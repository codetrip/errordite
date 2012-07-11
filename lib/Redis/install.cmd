@echo off
@setlocal enableextensions
@cd /d "%~dp0"

sc create Redis displayname= Redis binpath= "\"%CD%\redis-service.exe\" \"%CD%\redis.conf\"" start= auto
sc description Redis "Provides advanced key-value data storage (64-bit). It is often referred to as a data structure server since keys can contain strings, hashes, lists, sets and sorted sets."
sc config "Redis" obj= "%COMPUTERNAME%\errordite_redis" password= "Err0rD1t3_RED$"

IF EXIST "%CD%\data" GOTO DIREXISTS
MD "%CD%\data"

:DATAEXISTS

IF EXIST "%CD%\logs" GOTO LOGSEXISTS
MD "%CD%\logs"

:LOGSEXISTS