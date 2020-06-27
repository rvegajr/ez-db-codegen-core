@ECHO OFF
SETLOCAL
SET "sourcedir=%~dp0"
SET "filename1=%sourcedir%\GitVersion.yml"
SET searchStr="next-version: "

FINDSTR /B Root: "%filename1%" 
::returns the correct line - works well
FOR /f "tokens=*" %%i IN (
 'FINDSTR /B Root: "%filename1%"
') do set "root=%%i"

echo %root%
::echos "Root:" - instead of the line content

FOR /F "usebackqdelims=" %%G IN ("%filename1%") DO (
 if "%%G"=="%searchStr%" (
  ECHO(x=Hello
 ) ELSE (
 ECHO(%%G
 )
)

GOTO :EOF