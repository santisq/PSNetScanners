using System.Net;

namespace PSNetScanners.Dns;

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
