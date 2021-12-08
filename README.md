# Network-IPScanner

### DESCRIPTION
PowerShell utilty to scan IP ranges or hostnames using [`Runspace`](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.runspaces.runspace?view=powershellsdk-7.0.0) for faster execution.

`Pinger` function uses [`System.Net.NetworkInformation.Ping`](https://docs.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping?view=net-6.0) to send the echo request messages (ICMP) and [`Dns.GetHostEntry` method](https://docs.microsoft.com/en-us/dotnet/api/system.net.dns.gethostentry?view=net-6.0) for IP to hostname DNS resolution.

### OUTPUT

- __``` System.Collections.ObjectModel.Collection`1 ```__

| Name | Type |
|---|---|
| Ping     | `System.Int32`
| Source   | `System.String` |
| Address  | `System.String` |
| HostName | `System.String` |
| Latency  | `System.String` |
| Status   | `System.Net.NetworkInformation.IPStatus` |

### REQUIREMENTS
- Tested and compatible with __PowerShell v5.1__ and __PowerShell Core__.

### INIT VARAIBLE
- __`$list`__ should be defined as an `array` containing the list of hostnames or IP Addresses to scan. For example:

```
$list = 1..254 | ForEach-Object {
    "192.168.1.$_"
}
```

### MEASUREMENTS

Below are the measurements scanning a 254 IP range with `Pinger` default parameters:

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

### SAMPLE OUTPUT

```
Ping Source Address       HostName Latency   Status
---- ------ -------       -------- -------   ------
   1 moon   192.168.1.1   _gateway 3 ms     Success
   1 moon   192.168.1.2   *        113 ms   Success
   1 moon   192.168.1.3   *        214 ms   Success
   1 moon   192.168.1.4   *        591 ms   Success
   1 moon   192.168.1.5   moon     0 ms     Success
   1 moon   192.168.1.6   *        *       TimedOut
   1 moon   192.168.1.7   *        *       TimedOut
   1 moon   192.168.1.8   *        *       TimedOut
   1 moon   192.168.1.9   *        *       TimedOut
```
