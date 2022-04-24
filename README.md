# PowerShell Network Scanners

## DESCRIPTION
Two PowerShell scripts designed to scan Network IP Ranges or hostnames using [`Runspace`](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.runspaces.runspace?view=powershellsdk-7.0.0) for faster execution.

- [__Network-IPScanner__](https://github.com/santysq/Network-IPScanner/blob/main/Network-IPScanner.ps1): Sends ICMP echo requests in parallel. `Test-ICMPConnection` function uses [`System.Net.NetworkInformation.Ping`](https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping?view=net-6.0) to send the echo requests and [`Dns.GetHostEntry` method](https://docs.microsoft.com/en-us/dotnet/api/system.net.dns.gethostentry?view=net-6.0) for IP to hostname DNS resolution.
- [__Network-TCPScanner__](https://github.com/santysq/Network-IPScanner/blob/main/Network-TCPScanner.ps1): Sends TCP connection requests in parallel. `Test-Port` function uses [`System.Net.Sockets.TCPClient`](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-6.0) and it's [`.ConnectAsync(...)` Method](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connectasync?view=net-6.0#system-net-sockets-tcpclient-connectasync(system-net-ipaddress-system-int32)) to send the TCP connection requests.

## OUTPUT

### Network-IPScanner:

- __```System.Management.Automation.PSCustomObject```__

```
Name        TypeNameOfValue
----        ---------------
Ping        System.Int32
Source      System.String
Address     System.Net.IPAddress
Destination System.String
Latency     System.String
Status      System.Net.NetworkInformation.IPStatus
```

### Network-TCPScanner:

- __```System.Management.Automation.PSCustomObject```__

```
Name         TypeNameOfValue
----         ---------------
Source       System.String
Destionation System.String
Port         System.Int32
Success      System.Boolean
```

## REQUIREMENTS
- Requires __.NET Framework 4.5+__ if running __Windows PowerShell__ / __.NET Core 1.0+__ if running __PowerShell Core__.


## MEASUREMENTS

Below are the measurements scanning a 254 IP range with `Test-ICMPConnection` default script parameters:

```
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 4
Milliseconds      : 600
Ticks             : 46007194
TotalDays         : 5.32490671296296E-05
TotalHours        : 0.00127797761111111
TotalMinutes      : 0.0766786566666667
TotalSeconds      : 4.6007194
TotalMilliseconds : 4600.7194
```

## SAMPLE OUTPUT

### Network-IPScanner:

```
Ping Source Address       Destination Latency   Status
---- ------ -------       ----------- -------   ------
   1 moon   192.168.1.1   _gateway    3 ms     Success
   1 moon   192.168.1.2   *           113 ms   Success
   1 moon   192.168.1.3   *           214 ms   Success
   1 moon   192.168.1.4   *           591 ms   Success
   1 moon   192.168.1.5   moon        0 ms     Success
   1 moon   192.168.1.6   *           *       TimedOut
   1 moon   192.168.1.7   *           *       TimedOut
   1 moon   192.168.1.8   *           *       TimedOut
   1 moon   192.168.1.9   *           *       TimedOut
```

### Network-TCPScanner:

```
Source          Destionation Port Success
------          ------------ ---- -------
myHostName      google.com     80    True
myHostName      google.com    443    True
myHostName      google.com   8080   False
myHostName      google.com    125   False
myHostName      cisco.com     443    True
myHostName      cisco.com      80    True
myHostName      cisco.com     125   False
myHostName      cisco.com    8080   False
myHostName      amazon.com    443    True
myHostName      amazon.com     80    True
myHostName      amazon.com    125   False
myHostName      amazon.com   8080   False
```
