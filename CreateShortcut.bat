@echo off
:: =========================================================
::  CreateShortcut.bat
::  Creates a Desktop shortcut for CleanC PC Manager (.exe)
:: =========================================================

set "SCRIPT_DIR=%~dp0"
echo Creating Desktop shortcut for CleanC PC Manager...

powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $desk = [Environment]::GetFolderPath('Desktop'); $sc = $ws.CreateShortcut((Join-Path $desk 'CleanC PC Manager.lnk')); $sc.TargetPath = '%SCRIPT_DIR%publish\CleanC.UI.exe'; $sc.WorkingDirectory = '%SCRIPT_DIR%publish'; $sc.Description = 'CleanC - Windows 11 Native PC Manager (WinUI 3)'; $sc.IconLocation = '%SCRIPT_DIR%publish\CleanC.UI.exe,0'; $sc.Save()"

if %errorlevel% equ 0 (
    echo [OK] Shortcut created successfully on your Desktop!
) else (
    echo [FAIL] Could not create shortcut.
)

pause
