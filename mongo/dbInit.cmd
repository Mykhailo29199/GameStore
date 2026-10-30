@ECHO OFF

ECHO Local Northwind db init

SETLOCAL ENABLEDELAYEDEXPANSION

FOR /r data %%i IN (*.csv) DO (
	ECHO Importing %%~ni...
	"C:\Program Files\MongoDB\Tools\100\bin\mongoimport.exe" --host 127.0.0.1 --port 27017 -d Northwind --collection %%~ni --type csv --file %%~fi --headerline --drop
	ECHO Importing %%~ni done
)
ENDLOCAL

ECHO Done!