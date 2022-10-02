# PowerShell Network Scanners

## DESCRIPTION
Two PowerShell scripts designed to scan Network IP Ranges or hostname using [`Runspace`](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.runspaces.runspace?view=powershellsdk-7.0.0) for faster execution. And two standalone functions using async techniques for ICMP and TCP scanning.

| Name | Description |
| --- | --- |
| [__`Network-IPScanner`__](https://github.com/santysq/Network-IPScanner/blob/main/Network-IPScanner.ps1) | Sends ICMP echo requests using the [__Ping Class__](https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping?view=net-6.0) and the [__Dns Class__](https://docs.microsoft.com/en-us/dotnet/api/system.net.dns?view=net-6.0) for DNS resolution in parallel with _Runspaces_. |
| [__`Network-TCPScanner`__](https://github.com/santysq/Network-IPScanner/blob/main/Network-TCPScanner.ps1) | Sends TCP connection requests using the [__TcpClient Class__](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-6.0) in parallel with _Runspaces_. |
| [__`Test-ICMPConnectionAsync`__](https://github.com/santysq/PowerShell-Network-Scanners/blob/main/Test-ICMPConnectionAsync.ps1) | Standalone function, uses [`SendPingAsync(...)` Method](https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping.sendpingasync?view=net-6.0) for the async echo requests and [`GetHostEntryAsync(...)` Method](https://docs.microsoft.com/en-us/dotnet/api/system.net.dns.gethostentryasync?view=net-6.0#system-net-dns-gethostentryasync(system-net-ipaddress)) for async DNS resolutions. |
| [__`Network-TCPConnectionAsync`__](https://github.com/santysq/PowerShell-Network-Scanners/blob/main/Network-TCPScanner.ps1) &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; | Standalone function, uses [`ConnectAsync(...)` Method](https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connectasync?view=net-6.0#system-net-sockets-tcpclient-connectasync(system-net-ipaddress-system-int32)) to send the async TCP connection requests. |

## OUTPUT

### Network-IPScanner

- __```System.Management.Automation.PSCustomObject```__

```powershell
Name        TypeNameOfValue
----        ---------------
Ping        System.Int32
Source      System.String
Address     System.Net.IPAddress
Destination System.String
Latency     System.String
Status      System.Net.NetworkInformation.IPStatus
```

### Network-TCPScanner

- __```System.Management.Automation.PSCustomObject```__

```powershell
Name         TypeNameOfValue
----         ---------------
Source       System.String
Destination  System.String
Port         System.Int32
Success      System.Boolean
```

## REQUIREMENTS

- Requires __.NET Framework 4.5+__ if running __Windows PowerShell__ / __.NET Core 1.0+__ if running __PowerShell Core__.

## MEASUREMENTS

Below are the measurements scanning a 254 IP range with `Test-ICMPConnection` default script parameters:

```powershell
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

### Network-IPScanner

```powershell
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

### Network-TCPScanner

```powershell
Source          Destination  Port Success
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
