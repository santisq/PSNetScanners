using System;
using System.Net;
using System.Net.Sockets;

namespace PSNetScanners.Dns;

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
    internal static DnsFailure Timeout { get; } = new(
        DnsStatus.Timeout,
        new SocketException(11001));

    public Exception Exception { get; }

    internal DnsFailure(DnsStatus status, Exception exception)
        : base(status)
    {
        Exception = exception;
    }

    public override string ToString() => Exception.Message;
}
