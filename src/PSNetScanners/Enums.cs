namespace PSNetScanners;

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
