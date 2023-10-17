@echo off
rem *** Uninstall the service.
rem sc delete "UDP_Relay_Service"
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%
installutil.exe /uninstall "%~dp0UDP_Relay_Service.exe"
pause
