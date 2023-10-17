@echo off
rem *** Install the service.
rem sc create "UDP_Relay_Service" binPath= %CD%\UDP_Relay_Service.exe DisplayName= "UDP Relay Service" start= delayed-auto
rem sc failure "UDP_Relay_Service" reset= 0 actions= restart/30/restart/30/restart/3000
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%
installutil.exe /install "%~dp0UDP_Relay_Service.exe"
pause