using System;
using System.Net;
using System.Net.Sockets;

namespace PSNetScanners;

public abstract class DnsResult(DnsStatus status)
{
    public DnsStatus Status { get; } = status;
}

public sealed class DnsSuccess : DnsResult
{
    public string HostName { get => _entry.HostName; }

    public IPAddress[] AddressList { get => _entry.AddressList; }

    public string[] Aliases { get => _entry.Aliases; }

    private readonly IPHostEntry _entry;

    internal DnsSuccess(IPHostEntry entry) : base(DnsStatus.Success)
    {
        _entry = entry;
    }

    public override string ToString() => HostName;
}

public class DnsFailure : DnsResult
{
    public Exception Exception { get; }

    internal DnsFailure(DnsStatus status, Exception exception)
        : base(status)
    {
        Exception = exception;
    }

    internal static DnsFailure CreateTimeout() =>
        new(DnsStatus.Timeout, new SocketException(11001));

    public override string ToString() => Exception.Message;
}
