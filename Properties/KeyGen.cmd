@echo off
echo The best way to launch this script is with the Developer Command Prompt.
echo Type "Developer Command Prompt" in the Windows Start Menu to get it listed.
echo Drag and drop is supported.
echo.

set WorkDir=%~dp0
pushd "%WorkDir%"
sn -k 1024 KeyPrivate.snk
sn -p KeyPrivate.snk KeyPublic.snk
popd

echo.
pause