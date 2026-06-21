@echo off
rem *** Uninstall the UDP Relay Service. Run elevated.
sc stop "UDP_Relay_Service"
sc delete "UDP_Relay_Service"
echo Done.
pause
