@echo off
echo The best way to launch this script is with the Developer Command Prompt.
echo Type "Developer Command Prompt" in the Windows Start Menu to get it listed.
echo Drag and drop is supported.
echo.

set WorkDir=%~dp0
pushd "%WorkDir%"
MSBuild.exe /p:Configuration=Release Liersch.Json_net20.csproj
MSBuild.exe /p:Configuration=Release Liersch.Json_net35.csproj
MSBuild.exe /p:Configuration=Release Liersch.Json_net45.csproj
::MSBuild.exe /p:Configuration=Release Liersch.Json_netmf.csproj
popd

echo.
pause