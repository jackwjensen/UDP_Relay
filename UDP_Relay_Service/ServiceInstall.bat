@echo off
rem *** Install the UDP Relay Service. Run elevated, from the folder containing the exe.
sc create "UDP_Relay_Service" binPath= "%~dp0UDP_Relay_Service.exe" start= auto DisplayName= "UDP Relay Service"
sc description "UDP_Relay_Service" "Relays UDP broadcast/unicast packets between a local network and a remote endpoint."
echo Done. Start it with ServiceStart.bat (or: sc start UDP_Relay_Service).
pause
