@echo off
:: =========================================================
::  CreateShortcut.bat
::  Creates a Desktop shortcut for CleanC PC Manager (.exe)
:: =========================================================

set "SCRIPT_DIR=%~dp0"
echo Creating Desktop shortcut for CleanC PC Manager...

set "TARGET_EXE=%SCRIPT_DIR%publish\CleanC.UI.exe"
set "WORKING_DIR=%SCRIPT_DIR%publish"

if not exist "%TARGET_EXE%" (
    if exist "%SCRIPT_DIR%CleanC.UI.exe" (
        set "TARGET_EXE=%SCRIPT_DIR%CleanC.UI.exe"
        set "WORKING_DIR=%SCRIPT_DIR%"
    )
)

powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $desk = [Environment]::GetFolderPath('Desktop'); $sc = $ws.CreateShortcut((Join-Path $desk 'CleanC PC Manager.lnk')); $sc.TargetPath = '%TARGET_EXE%'; $sc.WorkingDirectory = '%WORKING_DIR%'; $sc.Description = 'CleanC - Windows 11 Native PC Manager (WinUI 3)'; $sc.IconLocation = '%TARGET_EXE%,0'; $sc.Save()"

if %errorlevel% equ 0 (
    echo [OK] Shortcut created successfully on your Desktop!
) else (
    echo [FAIL] Could not create shortcut.
)

pause
