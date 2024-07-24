---
external help file: PSNetScanners.dll-Help.xml
Module Name: PSNetScanners
online version:
schema: 2.0.0
---

# Test-PingAsync

## SYNOPSIS

Parallel ICMP scanner.

## SYNTAX

```powershell
Test-PingAsync
    [-Target] <String[]>
    [-BufferSize <Int32>
    [-ResolveDns] [-Ttl <Int32>]
    [-DontFragment]
    [-ThrottleLimit <Int32>]
    [-ConnectionTimeout <Int32>]
    [<CommonParameters>]
```

## DESCRIPTION

`Test-PingAsync` is a PowerShell cmdlet that ICMP echo-requests in parallel using [`Ping.SendPingAsync` Method](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping.sendpingasync). In essence, it's like `Test-Connection` with less options but faster.

## EXAMPLES

### Example 1: Send parallel echo-requests to remote hosts

```powershell
Test-PingAsync google.com, github.com

Source           Destination      Address             Latency Status       DnsResult
------           -----------      -------             ------- ------       ---------
DESKTOP-9HJU1AK  google.com       142.251.133.206        8 ms Success
DESKTOP-9HJU1AK  github.com       20.201.28.151         37 ms Success
```

### Example 2: Attempt to resolve DNS name for multiple hosts

```powershell
PS ..\PSNetScanners> $result = Test-PingAsync 8.8.8.8, 8.8.4.4, 1.1.1.1 -ResolveDns
PS ..\PSNetScanners> $result

Source           Destination      Address             Latency Status       DnsResult
------           -----------      -------             ------- ------       ---------
DESKTOP-9HJU1AK  8.8.8.8          8.8.8.8                8 ms Success      dns.google
DESKTOP-9HJU1AK  8.8.4.4          8.8.4.4                8 ms Success      dns.google
DESKTOP-9HJU1AK  1.1.1.1          1.1.1.1                8 ms Success      one.one.one.one

PS ..\PSNetScanners> $result[0].DnsResult

Status      : Success
HostName    : dns.google
AddressList : {8.8.8.8, 8.8.4.4}
Aliases     : {}
```

### Example 3: Specify a connection timeout for each request

```powershell
PS ..\PSNetScanners> 1..20 | ForEach-Object { "192.168.1.$_" } | Test-PingAsync -ConnectionTimeout 200

Source           Destination      Address             Latency Status       DnsResult
------           -----------      -------             ------- ------       ---------
DESKTOP-9HJU1AK  192.168.1.1      192.168.1.1            0 ms Success
DESKTOP-9HJU1AK  192.168.1.5      192.168.1.5            0 ms Success
DESKTOP-9HJU1AK  192.168.1.14     192.168.1.14           3 ms Success
DESKTOP-9HJU1AK  192.168.1.7      192.168.1.7          137 ms Success
DESKTOP-9HJU1AK  192.168.1.3      192.168.1.3          204 ms Success
DESKTOP-9HJU1AK  192.168.1.2      *                         * TimedOut
DESKTOP-9HJU1AK  192.168.1.4      *                         * TimedOut
DESKTOP-9HJU1AK  192.168.1.6      *                         * TimedOut
DESKTOP-9HJU1AK  192.168.1.8      *                         * TimedOut
DESKTOP-9HJU1AK  192.168.1.9      *                         * TimedOut
DESKTOP-9HJU1AK  192.168.1.10     *                         * TimedOut
DESKTOP-9HJU1AK  192.168.1.11     *                         * TimedOut
DESKTOP-9HJU1AK  192.168.1.12     *                         * TimedOut
...
...
```

## PARAMETERS

### -Target

Specifies one or more remote computers, Uris or Ip addresses to test connectivity.

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

Specifies the size, in bytes, of the buffer sent with this command.

> [!NOTE]
>
> The default value is 32.

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

Specifies a timeout __in milliseconds__ for each async task.

> [!NOTE]
>
> - If a task is not completed after this timeout, the status will be `TimedOut`.
> - The default value for this parameter is `4000` (4 seconds).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: timeout, to, ct

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DontFragment

This parameter sets the Don't Fragment flag in the IP header. See [`PingOptions.DontFragment` Property](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.pingoptions.dontfragment#system-net-networkinformation-pingoptions-dontfragment) for more information.

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

Causes the cmdlet to attempt to resolve the DNS name of the target.

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

Specifies the maximum number of async tasks to run in parallel.

> [!NOTE]
>
> The default value for `-ThrottleLimit` is `50`.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: tl

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Ttl

Sets the maximum number of hops that an ICMP request message can be sent. The default value is controlled by the operating system. The default value for Windows 10 and higher is 128 hops.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

## OUTPUTS

### PSNetScanners.PingResult

## NOTES

## RELATED LINKS

[`Ping.SendPingAsync` Method](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.ping.sendpingasync)

[`PingOptions.DontFragment` Property](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation.pingoptions.dontfragment#system-net-networkinformation-pingoptions-dontfragment)

[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216)
