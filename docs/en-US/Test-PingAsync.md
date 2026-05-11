---
external help file: PSNetScanners.dll-Help.xml
Module Name: PSNetScanners
online version:
schema: 2.0.0
---

# Test-PingAsync

## SYNOPSIS

Sends ICMP echo requests (pings) to one or more targets in parallel.

## SYNTAX

```powershell
Test-PingAsync
    [-Target] <String[]>
    [-BufferSize <Int32>]
    [-ResolveDns]
    [-Ttl <Int32>]
    [-DontFragment]
    [-ThrottleLimit <Int32>]
    [-ConnectionTimeout <Int32>]
    [<CommonParameters>]
```

## DESCRIPTION

The `Test-PingAsync` cmdlet sends ICMP echo requests to multiple targets concurrently using the [.NET `Ping.SendPingAsync` method](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping.sendpingasync).

It provides a faster, lightweight alternative to `Test-Connection` when you need high-performance parallel scanning with basic options.

> [!IMPORTANT]
>
> This cmdlet __never throws terminating errors__. It uses a result-oriented pattern — any errors (DNS resolution failures, etc.) are captured in the `.Error` property of the returned object.

## EXAMPLES

### Example 1: Ping multiple hosts in parallel

```powershell
PS ..\> Test-PingAsync google.com, github.com

Source       Destination      Address             Latency Status
------       -----------      -------             ------- ------
DESKTOP-XYZ  google.com       142.251.128.46         9 ms Success
DESKTOP-XYZ  github.com       4.228.31.150          41 ms Success
```

### Example 2: Resolve DNS names for the targets

```powershell
PS ..\> $result = Test-PingAsync 8.8.8.8, 8.8.4.4, 1.1.1.1 -ResolveDns
PS ..\> $result.DnsResult

Status      : Success
HostName    : dns.google
AddressList : {8.8.8.8, 8.8.4.4}
Aliases     : {}

Status      : Success
HostName    : dns.google
AddressList : {8.8.8.8, 8.8.4.4}
Aliases     : {}

Status      : Success
HostName    : one.one.one.one
AddressList : {1.1.1.1, 1.0.0.1}
Aliases     : {}
```

### Example 3: Specify a per-request timeout

```powershell
PS ..\> $range = 1..10 | ForEach-Object { "192.168.1.$_" }
PS ..\> $range | Test-PingAsync -ConnectionTimeout 200

Source       Destination      Address             Latency Status
------       -----------      -------             ------- ------
DESKTOP-XYZ  192.168.1.4      192.168.1.4            0 ms Success
DESKTOP-XYZ  192.168.1.1      192.168.1.1           35 ms Success
DESKTOP-XYZ  192.168.1.2      192.168.1.2          314 ms Success
DESKTOP-XYZ  192.168.1.3      *                         * TimedOut
DESKTOP-XYZ  192.168.1.5      *                         * TimedOut
DESKTOP-XYZ  192.168.1.6      *                         * TimedOut
DESKTOP-XYZ  192.168.1.7      *                         * TimedOut
DESKTOP-XYZ  192.168.1.8      *                         * TimedOut
DESKTOP-XYZ  192.168.1.9      *                         * TimedOut
DESKTOP-XYZ  192.168.1.10     *                         * TimedOut
```

### Example 4: Handling resolution failures

```powershell
PS ..\> $result = Test-PingAsync google.com, doesnotexist.xy
PS ..\> $result

Source       Destination      Address             Latency Status
------       -----------      -------             ------- ------
DESKTOP-XYZ  google.com       142.251.128.238       11 ms Success
DESKTOP-XYZ  doesnotexist.xy  *                         * Unknown

PS ..\> $result[1].Error

TargetSite     :
Message        : Failed to resolve host 'doesnotexist.xy'. No such host is known.
Data           : {}
InnerException : System.Net.Sockets.SocketException (11001): No such host is known.
                    at System.Net.NameResolutionPal.ProcessResult(SocketError errorCode, GetAddrInfoExContext* context)
                    at System.Net.NameResolutionPal.GetAddressInfoExCallback(Int32 error, Int32 bytes, NativeOverlapped* overlapped)
                 --- End of stack trace from previous location ---
                    at System.Net.NetworkInformation.Ping.<>c.<<SendPingAsync>b__56_0>d.MoveNext()
                 --- End of stack trace from previous location ---
                    at System.Net.NetworkInformation.Ping.SendPingAsyncInternal[TArg](TArg getAddressArg, Func`3 getAddress, Int32 timeout, Byte[] buffer, PingOptions options, CancellationToken cancellationToken)
HelpLink       :
Source         :
HResult        : -2146233088
StackTrace     :
```

> [!IMPORTANT]
>
> This cmdlet does not throw exceptions. All errors are captured in the `.Error` property.

## PARAMETERS

### -Target

Specifies one or more targets to test. You can use hostnames, FQDNs, IPv4/IPv6 addresses, or URIs.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: ComputerName, HostName, Host, Server, Address

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -BufferSize

Specifies the size, in bytes, of the data buffer sent with the ICMP request.

> [!NOTE]
>
> The default value is 32 bytes.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: bfs

Required: False
Position: Named
Default value: 32
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectionTimeout

Specifies the timeout (in milliseconds) for each individual ping request.

> [!NOTE]
>
> - If a request does not complete within this time, its status becomes `TimedOut`.
> - Default value is __4000__ (4 seconds).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: timeout, to, ct

Required: False
Position: Named
Default value: 4000
Accept pipeline input: False
Accept wildcard characters: False
```

### -DontFragment

Sets the _Don't Fragment_ flag in the IP header.

See [`PingOptions.DontFragment`](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.pingoptions.dontfragment#system-net-networkinformation-pingoptions-dontfragment) for more information.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ResolveDns

Attempts to resolve the DNS hostname for each target and includes the result in the `DnsResult` property.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: dns

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ThrottleLimit

Limits the maximum number of concurrent ping requests.

> [!NOTE]
>
> The default value __50__.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: tl

Required: False
Position: Named
Default value: 50
Accept pipeline input: False
Accept wildcard characters: False
```

### -Ttl

Sets the Time to Live (TTL) value for the ICMP packets (maximum number of hops).

> [!NOTE]
>
> Must be between __1__ and __255__. Default is __128__.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 128
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

## OUTPUTS

### PSNetScanners.Ping.PingResult

## NOTES

- DNS resolution occurs automatically when a hostname is provided.
- Designed for performance — exceptions are never thrown.
- Always check `.Status`, `.Success` and `.Error` on each result object.

## RELATED LINKS

[__`Ping` Class__](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping)

[__`Ping.SendPingAsync` Method__](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping.sendpingasync)
