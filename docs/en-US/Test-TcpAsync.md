---
external help file: PSNetScanners.dll-Help.xml
Module Name: PSNetScanners
online version:
schema: 2.0.0
---

# Test-TcpAsync

## SYNOPSIS

Tests TCP port connectivity to one or more targets in parallel.

## SYNTAX

```powershell
Test-TcpAsync
    [-Target] <String[]>
    [-Port] <Int32[]>
    [-ThrottleLimit <Int32>]
    [-ConnectionTimeout <Int32>]
    [<CommonParameters>]
```

## DESCRIPTION

The `Test-TcpAsync` cmdlet tests TCP connectivity to one or more targets across multiple ports using the [.NET `TcpClient.ConnectAsync` method](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connectasync).

It is a fast, parallel alternative to `Test-NetConnection -Port`.

> [!IMPORTANT]
>
> This cmdlet __never throws terminating errors__. It uses a result-oriented pattern — any errors (timeouts, refused connections, DNS issues, etc.) are captured in the `.Error` property of the returned object.

## EXAMPLES

### Example 1: Test multiple ports on a single host

```powershell
PS ..\> Test-TcpAsync google.com 20, 25, 80, 443

Source       Destination      Client             Port Status
------       -----------      ------             ---- ------
DESKTOP-XYZ  google.com       142.251.128.142      80 Opened
DESKTOP-XYZ  google.com       142.251.128.142     443 Opened
DESKTOP-XYZ  google.com                            25 TimedOut
DESKTOP-XYZ  google.com                            20 TimedOut
```

### Example 2: Test multiple ports on multiple hosts

```powershell
PS ..\> Test-TcpAsync google.com, github.com 20, 25, 80, 443

Source       Destination      Client             Port Status
------       -----------      ------             ---- ------
DESKTOP-XYZ  google.com       142.251.134.14      443 Opened
DESKTOP-XYZ  google.com       142.251.134.14       80 Opened
DESKTOP-XYZ  github.com       20.201.28.151        80 Opened
DESKTOP-XYZ  github.com       20.201.28.151       443 Opened
DESKTOP-XYZ  google.com                            20 TimedOut
DESKTOP-XYZ  google.com                            25 TimedOut
DESKTOP-XYZ  github.com                            20 TimedOut
DESKTOP-XYZ  github.com                            25 TimedOut
```

### Example 3: Specify a custom timeout

```powershell
PS ..\> $result = Test-TcpAsync github.com 20, 80 -ConnectionTimeout 30000
PS ..\> $result

Source       Destination      Client             Port Status
------       -----------      ------             ---- ------
DESKTOP-XYZ  github.com       20.201.28.151        80 Opened
DESKTOP-XYZ  github.com                            20 Closed

PS ..\> $result[1].Error

Message         : A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.
SocketErrorCode : TimedOut
ErrorCode       : 10060
NativeErrorCode : 10060
TargetSite      : Void ThrowException(System.Net.Sockets.SocketError, System.Threading.CancellationToken)
Data            : {}
InnerException  :
HelpLink        :
Source          : System.Net.Sockets
HResult         : -2147467259
StackTrace      :    at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
                     at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource.GetResult(Int16 token)
                     at System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c.<.cctor>b__4_0(Object state)
                  --- End of stack trace from previous location ---
                     at System.Net.Sockets.TcpClient.CompleteConnectAsync(Task task)
                     at PSNetScanners.Tcp.TcpResult.CreateAsync(String source, TcpInput input, Cancellation cancellation, Int32 timeout) in D:\pwsh\PSNetScanners\src\PSNetScanners\Tcp\TcpResult.cs:line 57
```

### Example 4: Import targets and ports from CSV

```powershell
PS ..\> Import-Csv targets.csv | Test-TcpAsync

Source       Destination      Client             Port Status
------       -----------      ------             ---- ------
DESKTOP-XYZ  google.com       142.251.133.206      80 Opened
DESKTOP-XYZ  google.com       142.251.133.206     443 Opened
DESKTOP-XYZ  github.com       20.201.28.151       443 Opened
DESKTOP-XYZ  github.com       20.201.28.151        80 Opened
DESKTOP-XYZ  amazon.com       52.94.236.248       443 Opened
DESKTOP-XYZ  cisco.com        72.163.4.185        443 Opened
DESKTOP-XYZ  cisco.com        72.163.4.185         80 Opened
DESKTOP-XYZ  amazon.com       52.94.236.248        80 Opened
```

> [!TIP]
>
> The cmdlet accepts pipeline input by property name. Any column name that matches the parameter name or any of its aliases will bind automatically.

## PARAMETERS

### -Target

Specifies one or more targets to test. Accepts hostnames, FQDNs, IPv4/IPv6 addresses, or URIs.

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

### -Port

Specifies one or more TCP ports to test.

```yaml
Type: Int32[]
Parameter Sets: (All)
Aliases: p

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ConnectionTimeout

Specifies the timeout (in milliseconds) for each connection attempt.

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

### CommonParameters

This cmdlet supports the common parameters. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Int32[]

### System.String[]

## OUTPUTS

### PSNetScanners.Tcp.TcpResult

## NOTES

- DNS resolution occurs automatically when a hostname is provided.
- This cmdlet is optimized for high-performance parallel scanning.
- Always check the `Status` and `.Error` properties for detailed failure information.

## RELATED LINKS

[__`TcpClient` Class__](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient)

[__`TcpClient.ConnectAsync` Method__](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connectasync)
