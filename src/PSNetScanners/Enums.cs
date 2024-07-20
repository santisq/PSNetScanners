namespace PSNetScanners;

internal enum Type
{
    Success,
    Error
}

public enum DnsStatus
{
    Success,
    Timeout,
    Error
}

public enum TcpStatus
{
    Opened,
    TimedOut,
    Closed
}
