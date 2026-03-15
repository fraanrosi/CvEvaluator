@echo off
REM =============================================
REM  CvEvaluator — Start frontend + backend
REM  Usage: double-click or run start-devfrom CMD/PowerShell
REM =============================================

where wt >nul 2>nul
if %errorlevel%==0 (
    echo Starting with Windows Terminal tabs...
    wt -w 0 new-tab --title "API" -d "%~dp0BackEnd" cmd /k "dotnet run --project CvEvaluator.Api --launch-profile \"API https\"" ; new-tab --title "Frontend" -d "%~dp0FrontEnd\cv-evaluator-ui" cmd /k "npm start"
) else (
    echo Starting in separate windows...
    start "CvEvaluator API" cmd /c "cd /d %~dp0BackEnd && dotnet run --project CvEvaluator.Api --launch-profile "API https""
    start "CvEvaluator UI" cmd /c "cd /d %~dp0FrontEnd\cv-evaluator-ui && npm start"
)

echo.
echo ============================================
echo   Frontend : http://localhost:4200
echo   API      : https://localhost:7184
echo ============================================
