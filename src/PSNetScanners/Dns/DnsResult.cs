namespace PSNetScanners.Dns;

public abstract class DnsResult(DnsStatus status)
{
    public DnsStatus Status { get; } = status;
}
