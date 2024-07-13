using System;
using System.Net;

namespace PSNetScanners;

public enum DnsStatus
{
    Success,
    Timeout,
    Cancelled,
    Error
}

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
    public Exception? Exception { get; }

    internal DnsFailure(DnsStatus status, Exception? error = null)
        : base(status)
    {
        Exception = error;
    }

    public override string ToString() => Exception?.Message ?? Status.ToString();
}
