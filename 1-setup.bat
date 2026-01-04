@echo off
echo ========================================
echo Video Thumbnail Changer - Setup
echo ========================================
echo.

REM Vérifie Python
python --version >nul 2>&1
if errorlevel 1 (
    echo Erreur: Python n'est pas installé ou pas dans le PATH
    echo Télécharge Python depuis https://www.python.org
    pause
    exit /b 1
)

echo [1/3] Installation des dépendances Python...
pip install -r requirements.txt
if errorlevel 1 (
    echo Erreur lors de l'installation des dépendances
    pause
    exit /b 1
)

echo [2/3] Vérification de FFmpeg...
ffmpeg -version >nul 2>&1
if errorlevel 1 (
    echo.
    echo Attention: FFmpeg n'est pas détecté dans le PATH
    echo.
    echo Options:
    echo 1. Télécharger FFmpeg: https://ffmpeg.org/download.html
    echo 2. Ajouter FFmpeg au PATH
    echo 3. Ou placer ffmpeg.exe dans le même dossier que le script
    echo.
    pause
) else (
    echo FFmpeg trouvé !
)

echo [3/3] Setup terminé!
echo.
echo Pour lancer l'application:
echo   python video_thumbnail_changer.py
echo.
pause
