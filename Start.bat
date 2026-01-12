@echo off
set DOTNET_WATCH_RESTART_ON_RUDE_EDIT=1
set DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=1
start "Server" cmd /k "cd /D %~dp0\Server && dotnet watch"