@echo off

set WorkDir=%~dp0

echo Current folder:
echo %WorkDir%

echo.
set /p Answer=Do you really want to clean up the current folder (y/[n])?
if /i "%Answer%" neq "y" goto end

echo.
echo Deleting temporary files...
for /r "%WorkDir%" %%d in (*.user, OnBoardFlash.*) do (
  if exist "%%d" (
    echo %%d
    del /f /q "%%d"
  )
)

del /f /q /s     *.suo 2>nul
del /f /q /s /ah *.suo 2>nul

echo.
echo Deleting temporary folders...
for /d /r "%WorkDir%" %%d in (bin, obj, .vs, DOTNETMF_FS_EMULATION) do (
  if exist "%%d" (
    echo %%d
    cmd /c rd /s /q "%%d"
  )
)

echo.
pause

:end