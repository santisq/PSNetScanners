using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PSNetScanners;

public sealed class PingResult
{
    private string? _displayAddress;

    private IPAddress? _address;

    public string Source { get; }

    public string Destination { get; }

    public IPAddress? Address
    {
        get => _address ??= Status is IPStatus.Success ? Reply?.Address : null;
    }

    public string DisplayAddress
    {
        get => _displayAddress ??= Address?.ToString() ?? "*";
    }

    public long? Latency { get => Reply?.RoundtripTime; }

    public IPStatus Status { get => Reply?.Status ?? IPStatus.TimedOut; }

    public DnsResult? DnsResult { get; private set; }

    public PingReply? Reply { get; private set; }

    private PingResult(string source, string target)
    {
        Source = source;
        Destination = target;
    }

    internal static async Task<PingResult> CreateAsync(
        string source,
        string target,
        TaskOptions pingOptions)
    {
        return new PingResult(source, target)
        {
            Reply = await PingAsync(target, pingOptions),
            DnsResult = await DnsAsync.GetHostEntryAsync(target, pingOptions)
        };
    }

    private static async Task<PingReply?> PingAsync(
        string target,
        TaskOptions pingOptions)
    {
        if (pingOptions.Cancellation.IsCancellationRequested)
        {
            return null;
        }

        using Ping ping = new();

        Task<PingReply> pingTask = ping.SendPingAsync(
            hostNameOrAddress: target,
            timeout: pingOptions.TaskTimeout,
            buffer: pingOptions.Buffer);

        Task task = await Task.WhenAny(pingTask, pingOptions.CancelTask);

        if (task == pingOptions.CancelTask)
        {
            return null;
        }

        PingReply reply = await pingTask;
        return reply;
    }
}
