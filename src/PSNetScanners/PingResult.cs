using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PSNetScanners;

public sealed class PingResult
{
    private IPStatus? _status;

    private IPAddress? _ip;

    private long? _latency;

    private string? _displayAddress;

    internal readonly string _displayLatency;

    public string Source { get; }

    public string Destination { get; }

    public IPAddress? Address
    {
        get => _ip ??= Status is IPStatus.Success ? Reply?.Address : null;
    }

    public string DisplayAddress
    {
        get => _displayAddress ??= Address?.ToString() ?? "*";
    }

    public long Latency
    {
        get => _latency ??= Reply?.RoundtripTime ?? 0;
    }

    public IPStatus Status
    {
        get => _status ??= Reply?.Status ?? IPStatus.TimedOut;
    }

    public DnsResult? DnsResult { get; }

    public PingReply? Reply { get; }

    private PingResult(
        string source,
        string destination,
        DnsResult? dns = null,
        PingReply? reply = null)
    {
        Source = source;
        Destination = destination;
        DnsResult = dns;
        Reply = reply;
        _displayLatency = Status is IPStatus.Success ? $"{Latency} ms" : "*";
    }

    internal static async Task<PingResult> CreateAsync(
        string source,
        string destination,
        PingAsyncOptions options,
        Cancellation cancellation)
    {
        using Ping ping = new();
        Task<PingReply> pingTask = ping.SendPingAsync(
            hostNameOrAddress: destination,
            timeout: options.TaskTimeout,
            buffer: options.Buffer,
            options: options.PingOptions);

        if (!options.ResolveDns)
        {
            return new PingResult(
                source: source,
                destination: destination,
                reply: await pingTask);
        }

        Task<DnsResult> dnsTask = GetDnsAsync(destination, options, cancellation);
        Task result = await Task.WhenAny(pingTask, cancellation.Task, dnsTask);

        if (result != dnsTask && result != pingTask)
        {
            return new PingResult(
                source: source,
                destination: destination,
                dns: DnsFailure.CreateTimeout());
        }

        return new PingResult(
            source: source,
            destination: destination,
            dns: await dnsTask,
            reply: await pingTask);
    }

    private static async Task<DnsResult> GetDnsAsync(
        string destination,
        PingAsyncOptions options,
        Cancellation cancellation)
    {
        Task<IPHostEntry> dns = Dns.GetHostEntryAsync(destination);
        Task timeout = cancellation.GetTimeoutTask(options.TaskTimeout);
        Task result = await Task.WhenAny(dns, timeout);

        if (result == timeout)
        {
            return DnsFailure.CreateTimeout();
        }

        try
        {
            IPHostEntry entry = await dns;
            return new DnsSuccess(entry);
        }
        catch (Exception exception)
        {
            return new DnsFailure(DnsStatus.Error, exception);
        }
    }
}
