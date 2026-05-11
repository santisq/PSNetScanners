using System;
using System.Net.Sockets;

namespace PSNetScanners.Dns;

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
