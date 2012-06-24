@REM This file:
@REM - copies the instruction files to a temp directory
@REM - applies the replacement instruction file(s) to the temp location of the XML config instruction file(s)
@REM - applies the replacement instruction file(s) to the output config file location
@REM - applies the modified config instruction file(s) to the output config file location

setlocal
set env=%1
set tempDir=%2
set instructionsPath=%3
set configFileChangerPath=%4
set configFileLocation=%5
if not exist %tempDir%\rep md %tempDir%\rep
if not exist %tempDir%\xml md %tempDir%\xml
copy /Y %instructionsPath%\InstructionReplacements.inst %tempDir%\rep
copy /Y %instructionsPath%\XmlConfig.inst %tempDir%\xml

@echo Apply replacements to temp xml instruction location
%configFileChangerPath%\Harmony.Utils.ConfigFileChanger.exe /M:Deploy /A:+ /Env:%env% /Inst:%tempDir%\rep /Config:%tempDir%\xml

@echo Apply replacements to config file location
%configFileChangerPath%\Harmony.Utils.ConfigFileChanger.exe /M:Deploy /A:+ /Env:%env% /Inst:%tempDir%\rep /Config:%configFileLocation%

@echo Apply xml changes to config file location
%configFileChangerPath%\Harmony.Utils.ConfigFileChanger.exe /M:Deploy /A:+ /Env:%env% /Inst:%tempDir%\xml /Config:%configFileLocation%