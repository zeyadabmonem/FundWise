@echo off
title FundWise AI Launcher
color 0A

echo ========================================================
echo                FundWise AI - Launcher
echo ========================================================
echo.

echo [1/2] Starting ASP.NET Core Backend API on http://localhost:5207 ...
start "FundWise Backend API" cmd /k "cd /d %~dp0 && dotnet run --project src\FundWise.API\FundWise.API.csproj --launch-profile http"

echo Waiting 5 seconds for Backend API to initialize...
timeout /t 5 /nobreak >nul

echo [2/2] Launching Flutter App on Chrome...
start "FundWise Flutter Mobile App" cmd /k "cd /d %~dp0mobile && C:\flutter\bin\flutter.bat run -d chrome --no-pub"

echo.
echo ========================================================
echo  SUCCESS: FundWise AI Services Launched!
echo.
echo  1. Backend Swagger API: http://localhost:5207/swagger
echo  2. Flutter App: Chrome window will open automatically
echo.
echo  Test Credentials:
echo  - Email:    zeyad@fundwise.ai
echo  - Password: FundWise@2026
echo ========================================================
echo.
pause
