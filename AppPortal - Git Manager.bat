@echo off
title CyberFrost Git Manager
cd /d "%~dp0"

:MENU
cls
echo =============================================
echo       CyberFrost Git Manager
echo =============================================
echo  1. Login / Setup Token
echo  2. Check Profile
echo  3. Status
echo  4. Add all files (git add .)
echo  5. Commit
echo  6. Push ke GitHub
echo  7. Pull dari GitHub
echo  8. Branch Manager
echo  9. Remote Setup (init + add URL)
echo  0. Exit
echo =============================================
echo.
set /p cmd="Pilih [0-9]: "
if "%cmd%"=="1" goto login
if "%cmd%"=="2" goto profile
if "%cmd%"=="3" goto status
if "%cmd%"=="4" goto add
if "%cmd%"=="5" goto commit
if "%cmd%"=="6" goto push
if "%cmd%"=="7" goto pull
if "%cmd%"=="8" goto branch
if "%cmd%"=="9" goto remote
if "%cmd%"=="0" goto end
goto MENU

:login
cls
echo ===== Login GitHub =====
set /p GIT_USER="Username: "
set /p GIT_EMAIL="Email: "
set /p GIT_TOKEN="Personal Access Token: "
if "%GIT_TOKEN%"=="" goto login
git config --global user.name "%GIT_USER%"
git config --global user.email "%GIT_EMAIL%"
>nul curl -s -H "Authorization: token %GIT_TOKEN%" https://api.github.com/user -o "%TEMP%\gh_test.json"
findstr /C:"\"login\"" "%TEMP%\gh_test.json" >nul
if %errorlevel% equ 0 (
    echo Login berhasil!
    setx GITHUB_TOKEN "%GIT_TOKEN%" >nul
    git config --global credential.helper store
    echo https://%GIT_USER%:%GIT_TOKEN%@github.com > "%USERPROFILE%\.git-credentials"
) else ( echo Token tidak valid! )
del "%TEMP%\gh_test.json" 2>nul
pause
goto MENU

:profile
cls
echo ===== Git Profile =====
git config --global --list | findstr "user\."
if exist "%USERPROFILE%\.git-credentials" ( echo Token: Tersimpan ) else ( echo Token: Belum di-set )
pause
goto MENU

:status
cls
if not exist ".git" ( echo Belum ada repo. & pause & goto MENU )
echo ===== Status =====
git status
pause
goto MENU

:add
cls
if not exist ".git" ( echo Belum ada repo. & pause & goto MENU )
echo ===== Git Add / Unstage / Clean =====
echo 1. Add all files
echo 2. Add specific file
echo 3. Unstage all (git reset)
echo 4. Unstage specific file
echo 5. Hapus .md + .bat dari tracking (gitignore cleanup)
echo 0. Back
set /p A="Pilih: "
if "%A%"=="1" git add . && echo All files added.
if "%A%"=="2" set /p F="File path: " && if not "%F%"=="" git add "%F%" && echo Added.
if "%A%"=="3" git reset -- . && echo All files unstaged.
if "%A%"=="4" set /p F="File path: " && if not "%F%"=="" git reset -- "%F%" && echo Unstaged: %F%
if "%A%"=="5" powershell -Command "git ls-files '*.md' | ForEach-Object { git rm --cached --ignore-unmatch $_ }; git ls-files '*.bat' | ForEach-Object { git rm --cached --ignore-unmatch $_ }" && echo .md + .bat removed from tracking.
if "%A%"=="0" goto MENU
pause
goto MENU

:commit
cls
if not exist ".git" ( echo Belum ada repo. & pause & goto MENU )
echo ===== Commit =====
echo Files to commit:
git status -s
set /p MSG="Pesan commit: "
if "%MSG%"=="" goto MENU
git commit -m "%MSG%"
if %errorlevel% neq 0 echo Commit gagal. Coba menu 4 (add) dulu.
pause
goto MENU

:push
cls
if not exist ".git" ( echo Belum ada repo. & pause & goto MENU )
:: Cek apakah sudah ada commit
git rev-parse HEAD >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Belum ada commit! Jalankan Menu 5 (Commit) dulu.
    pause
    goto MENU
)
for /f %%i in ('git rev-parse --abbrev-ref HEAD 2^>nul') do set BRANCH=%%i
if "%BRANCH%"=="" set BRANCH=main
echo Push ke origin/%BRANCH%
set /p CONFIRM="Lanjutkan? (y/n): "
if not "%CONFIRM%"=="y" goto MENU
git push origin %BRANCH%
if %errorlevel% neq 0 ( echo Gagal. Cek remote/token. ) else ( echo Push OK! )
pause
goto MENU

:pull
cls
if not exist ".git" ( echo Belum ada repo. & pause & goto MENU )
for /f %%i in ('git rev-parse --abbrev-ref HEAD 2^>nul') do set BRANCH=%%i
if "%BRANCH%"=="" set BRANCH=main
git pull origin %BRANCH%
pause
goto MENU

:branch
cls
if not exist ".git" ( echo Belum ada repo. & pause & goto MENU )
echo ===== Branches =====
git branch -a 2>nul || echo Belum ada branch.
echo 1. Create  2. Switch  3. Delete  0. Back
set /p B="Pilih: "
if "%B%"=="1" set /p BN="Branch: " && if not "%BN%"=="" git checkout -b %BN%
if "%B%"=="2" set /p BN="Branch: " && if not "%BN%"=="" git checkout %BN% 2>nul || git checkout --track origin/%BN% 2>nul
if "%B%"=="3" set /p BN="Branch: " && if not "%BN%"=="" git branch -D %BN%
if "%B%"=="0" goto MENU
pause
goto MENU

:remote
cls
if not exist ".git" ( git init >nul 2>&1 & git checkout -b main >nul 2>&1 & echo Repo siap. )
echo ===== Remote Setup =====
git remote -v 2>nul
if %errorlevel% neq 0 echo (belum ada remote)
set /p RURL="URL GitHub (https://github.com/user/repo.git): "
if "%RURL%"=="" goto MENU
git remote remove origin 2>nul
git remote add origin "%RURL%"
if %errorlevel% equ 0 ( echo Remote added! ) else ( echo Gagal. )
pause
goto MENU

:end
timeout /t 1 >nul
