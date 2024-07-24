---
external help file: PSNetScanners.dll-Help.xml
Module Name: PSNetScanners
online version:
schema: 2.0.0
---

# Test-TcpAsync

## SYNOPSIS

Parallel TCP scanner.

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

`Test-TcpAsync` is a PowerShell cmdlet that tests TCP connectivity in parallel using [`TcpClient.ConnectAsync` Method](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connectasync). In essence, it's like `Test-NetConnection` with `-Port` but faster.

## EXAMPLES

### Example 1: Test TCP connectivity on multiple ports for a specified host

```powershell
PS ..\> Test-TcpAsync google.com 20, 25, 80, 443

Source           Destination      Client             Port Status
------           -----------      ------             ---- ------
DESKTOP-1234567  google.com       142.251.134.14       80 Opened
DESKTOP-1234567  google.com       142.251.134.14      443 Opened
DESKTOP-1234567  google.com                            20 TimedOut
DESKTOP-1234567  google.com                            25 TimedOut
```

### Example 2: Test TCP connectivity on multiple ports for multiple hosts

```powershell
PS ..\> Test-TcpAsync google.com, github.com 20, 25, 80, 443

Source           Destination      Client             Port Status
------           -----------      ------             ---- ------
DESKTOP-1234567  google.com       142.251.134.14      443 Opened
DESKTOP-1234567  google.com       142.251.134.14       80 Opened
DESKTOP-1234567  github.com       20.201.28.151        80 Opened
DESKTOP-1234567  github.com       20.201.28.151       443 Opened
DESKTOP-1234567  google.com                            20 TimedOut
DESKTOP-1234567  google.com                            25 TimedOut
DESKTOP-1234567  github.com                            20 TimedOut
DESKTOP-1234567  github.com                            25 TimedOut
```

### Example 3: Specify a timeout for TCP connectivity

```powershell
PS ..\> $result = Test-TcpAsync github.com 20, 80 -ConnectionTimeout 30000
PS ..\> $result

Source           Destination      Client             Port Status
------           -----------      ------             ---- ------
DESKTOP-1234567  github.com       20.201.28.151        80 Opened
DESKTOP-1234567  github.com                            20 Closed

PS ..\> $result.Details

Message         : A connection attempt failed because the connected party did not properly respond after a period of
                  time, or established connection failed because connected host has failed to respond.
SocketErrorCode : TimedOut
ErrorCode       : 10060
NativeErrorCode : 10060
TargetSite      : Void ThrowException(System.Net.Sockets.SocketError, System.Threading.CancellationToken)
Data            : {}
InnerException  :
HelpLink        :
Source          : System.Net.Sockets
HResult         : -2147467259
StackTrace      :    at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error,
                  CancellationToken cancellationToken)
                     at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTa
                  skSource.GetResult(Int16 token)
                     at System.Threading.Tasks.ValueTask.ValueTaskSourceAsTask.<>c.<.cctor>b__4_0(Object state)
                  --- End of stack trace from previous location ---
                     at System.Net.Sockets.TcpClient.CompleteConnectAsync(Task task)
                     at PSNetScanners.TcpResult.CreateAsync(TcpInput input, Cancellation cancellation, Int32 timeout)
                  in D:\PSNetScanners\src\PSNetScanners\TcpResult.cs:line 64
```

### Example 4: Use a CSV as input

```powershell
PS ..\> Import-Csv targets.csv | Test-TcpAsync

Source           Destination      Client             Port Status
------           -----------      ------             ---- ------
DESKTOP-1234567  google.com       142.251.133.206      80 Opened
DESKTOP-1234567  google.com       142.251.133.206     443 Opened
DESKTOP-1234567  github.com       20.201.28.151       443 Opened
DESKTOP-1234567  github.com       20.201.28.151        80 Opened
DESKTOP-1234567  amazon.com       52.94.236.248       443 Opened
DESKTOP-1234567  cisco.com        72.163.4.185        443 Opened
DESKTOP-1234567  cisco.com        72.163.4.185         80 Opened
DESKTOP-1234567  amazon.com       52.94.236.248        80 Opened
```

> [!TIP]
>
> Both parameters `-Target` and `-Port` take value from pipeline by property name, if your CSV headers match the parameters names or their aliases, you can use a CSV as input for this cmdlet.

## PARAMETERS

### -Target

Specifies one or many remote computers, Uris or Ip addresses to test connectivity.

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

Specifies one or many TCP ports to test connectivity to the specified targets.

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

Specifies a timeout __in milliseconds__ for each async task.

> [!NOTE]
>
> - If a task is not completed after this timeout, the status will be `TimedOut`.
> - If your `-ConnectionTimeout` is greater than the maximum timeout of [`TcpClient`](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient) and the connection fails, the status will be `Closed`.
> - In both cases of a connectivity failure, the `.Details` property will be populated with a `SocketException`.
> - The default value for this parameter is `4000` (4 seconds).

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

### PSNetScanners.TcpResult

## NOTES

## RELATED LINKS

[`TcpClient.ConnectAsync` Method](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.connectasync)

[`TcpClient`](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient)

[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216)
