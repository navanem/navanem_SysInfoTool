@echo off
REM Lance BgLight silencieusement et met a jour le fond d'ecran.
REM Adapter le chemin de l'EXE selon le deploiement (ex. C:\ProgramData\BgLight\BgLight.exe).

set "BGLIGHT_EXE=%ProgramData%\BgLight\BgLight.exe"

if not exist "%BGLIGHT_EXE%" (
    echo BgLight.exe introuvable: %BGLIGHT_EXE%
    exit /b 1
)

start "" /b "%BGLIGHT_EXE%" /outputPath="%ProgramData%\BgLight\wallpaper_info.bmp" /fontSize=11 /position=TopLeft
exit /b 0
