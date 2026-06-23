@echo off
REM Runs BgLight silently and updates the desktop wallpaper.
REM Adjust the EXE path to your deployment (e.g. C:\ProgramData\BgLight\BgLight.exe).

set "BGLIGHT_EXE=%ProgramData%\BgLight\BgLight.exe"

if not exist "%BGLIGHT_EXE%" (
    echo BgLight.exe not found: %BGLIGHT_EXE%
    exit /b 1
)

start "" /b "%BGLIGHT_EXE%" /outputPath="%ProgramData%\BgLight\wallpaper_info.bmp" /fontSize=11 /position=TopRight
exit /b 0
