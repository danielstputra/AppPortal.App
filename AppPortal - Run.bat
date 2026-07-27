@echo off
title AppPortal — Environment Launcher
cd /d "%~dp0"

set "PROJECT_DIR=%~dp0Frontend\src\Web"

:menu
cls
echo.
echo  =============================================
echo      A P P   P O R T A L
echo      Select Environment
echo  =============================================
echo.
echo     1   Development    [hot reload, mock API]
echo     2   Local          [debug, localhost API]
echo     3   Staging        [pre-production]
echo     4   Production     [live deployment]
echo.
echo     0   Exit
echo.
echo  =============================================
echo.

set /p env_num="Enter number (0-4) and press Enter: "

if "%env_num%"=="1" set "env_name=Development" & set "env_urls=https://localhost:5001;http://localhost:5000" & set "env_config=Debug"
if "%env_num%"=="2" set "env_name=Local"       & set "env_urls=https://localhost:5001;http://localhost:5000" & set "env_config=Debug"
if "%env_num%"=="3" set "env_name=Staging"     & set "env_urls=https://+:443;http://+:80"                    & set "env_config=Release"
if "%env_num%"=="4" set "env_name=Production"  & set "env_urls=https://+:443;http://+:80"                    & set "env_config=Release"

if "%env_num%"=="0" exit /b 0

if not defined env_name (
    echo.
    echo  Invalid choice. Please try again.
    timeout /t 2 >nul
    goto menu
)

if not exist "%PROJECT_DIR%\node_modules" (
    echo.
    echo  Configuring Tailwind CSS...
    cd /d "%PROJECT_DIR%"
    call npm install
    if errorlevel 1 (
        echo [ERROR] npm install failed.
        pause
        exit /b 1
    )
)

echo  Building Tailwind CSS...
cd /d "%PROJECT_DIR%"
call npm run build:css
if errorlevel 1 (
    echo [WARN] Tailwind CSS build had issues, continuing anyway...
)

cls
echo.
echo  ===============================================
echo    APPPORTAL  -  %env_name% MODE
echo  ===============================================
echo.
echo    Environment  : %env_name%
echo    URLs         : %env_urls%
echo    Configuration: %env_config%
echo.
echo  ===============================================
echo.

cd /d "%PROJECT_DIR%"

set ASPNETCORE_ENVIRONMENT=%env_name%
set ASPNETCORE_URLS=%env_urls%

if /i "%env_config%"=="Release" (
    dotnet run --no-launch-profile --configuration Release
) else (
    dotnet watch run --no-launch-profile
)

set "exit_code=%ERRORLEVEL%"

echo.
if %exit_code% NEQ 0 (
    echo [ERROR] Application exited with code %exit_code%
    pause
) else (
    echo  Application closed.
    timeout /t 3 >nul
)

exit /b %exit_code%
