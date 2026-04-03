@echo off
echo ==========================================
echo  SortDemo Installer Build Script
echo ==========================================
echo.

REM Step 1: Publish as self-contained single file
echo [1/2] Publishing SortDemo...
dotnet publish ..\SortDemo\SortDemo.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
if errorlevel 1 (
    echo ERROR: dotnet publish failed.
    pause
    exit /b 1
)
echo      Published to Installer\publish\

REM Step 2: Build installer with Inno Setup (if installed)
where iscc >nul 2>nul
if %errorlevel%==0 (
    echo [2/2] Building installer with Inno Setup...
    iscc SortDemo.iss
    if errorlevel 1 (
        echo ERROR: Inno Setup compilation failed.
        pause
        exit /b 1
    )
    echo.
    echo ==========================================
    echo  Installer built: Output\SortDemoSetup.exe
    echo ==========================================
) else (
    echo [2/2] Inno Setup not found - skipping installer.
    echo      You can distribute Installer\publish\SortDemo.exe directly.
    echo      Or install Inno Setup from https://jrsoftware.org/isinfo.php
    echo      and run this script again to create a setup wizard.
)
echo.
pause
