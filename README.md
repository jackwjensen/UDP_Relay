# UDP_Relay
UDP Relay catches local UDP broardcast messages and relays them to external server. Can also be set up to return answers from the external server.

### Use cases
Sensors, alarms or other devices that broardcast a signal on LAN, but the receiving application is not located on the same LAN. Set up UDP Relay on a computer in the same LAN as the broardcasting device, and use UDP Relay to forward to the external application.

## Use:
Use either the UDP Relay Console or the UDP Relay Service. The UDP Relay Console should be cross platform. The UDP Relay Service is a Windows Service.

Settings like IP addresses and port numbers are saved in XML files.

## Test:
Use the 3 console applications for testing: TestSender, UDP Relay Console and TestReceiver. They can be used/adapted to test if UDP Relay can be used in your situation.

## Logging:
Logging is set up as injectable so you have options for which logger you want to use.
