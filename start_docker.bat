@echo off
REM Author: Min Thu Khaing

set "CONTAINER_NAME=sql_server_dev"
set "DB_PASSWORD=Apple0097@"

REM Check if container exists (running or stopped)
for /f "delims=" %%i in ('docker ps -aq -f name=%CONTAINER_NAME%') do set CONTAINER_ID=%%i

if "%CONTAINER_ID%"=="" (
    echo Container "%CONTAINER_NAME%" not found. Creating a new one...
    docker run --name %CONTAINER_NAME% ^
        -e "ACCEPT_EULA=Y" ^
        -e "MSSQL_SA_PASSWORD=%DB_PASSWORD%" ^
        -p 1433:1433 ^
        -d mcr.microsoft.com/mssql/server:latest
    echo Container "%CONTAINER_NAME%" created and started.
) else (
    REM Check if it's already running
    for /f "delims=" %%i in ('docker ps -q -f name=%CONTAINER_NAME%') do set RUNNING_ID=%%i

    if "%RUNNING_ID%"=="" (
        echo Container "%CONTAINER_NAME%" exists but is stopped. Starting it...
        docker start %CONTAINER_NAME%
        echo Container "%CONTAINER_NAME%" started.
    ) else (
        echo Container "%CONTAINER_NAME%" is already running.
    )
)

pause
