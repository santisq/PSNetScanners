using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using PSNetScanners.Abstractions;
using PSNetScanners.Dns;

namespace PSNetScanners.Ping;

public sealed class PingResult : ResultBase
{
    internal string LatencyAsString
    {
        get => field ??= Status is IPStatus.Success ? $"{Latency} ms" : "*";
    }

    public IPAddress? Address
    {
        get => field ??= Status is IPStatus.Success ? Reply?.Address : null;
    }

    public string DisplayAddress
    {
        get => field ??= Address?.ToString() ?? "*";
    }

    public long? Latency
    {
        get => field ??= Reply?.RoundtripTime ?? 0;
    }

    public IPStatus? Status
    {
        get => field ??= Reply?.Status ?? IPStatus.Unknown;
        private set;
    }

    public DnsResult? DnsResult { get; private set; }

    public PingReply? Reply { get; private set; }

    public override bool Success { get => Status == IPStatus.Success; }

    private PingResult(string source, string destination)
        : base(source, destination)
    { }

    internal static async Task<PingResult> CreateAsync(
        string source,
        string destination,
        PingAsyncOptions options,
        Cancellation cancellation)
    {
        using System.Net.NetworkInformation.Ping ping = new();
        PingResult result = new(source, destination);
        (PingOptions opt, int _, int timeout, byte[] buffer, bool resolveDns) = options;
        Task<PingReply> pingTask = ping.SendPingAsync(destination, timeout, buffer, opt);

        try
        {
            if (!resolveDns)
            {
                result.Reply = await pingTask.NoContext();
                return result;
            }

            Task<DnsResult> dnsTask = GetDnsAsync(destination, options, cancellation);
            Task any = await Task
                .WhenAny(pingTask, cancellation.Task, dnsTask)
                .NoContext();

            if (any != dnsTask && any != pingTask)
            {
                result.Status = IPStatus.TimedOut;
                result.DnsResult = DnsFailure.Timeout;
                return result;
            }

            result.Reply = await pingTask.NoContext();
            result.DnsResult = await dnsTask.NoContext();
        }
        catch (PingException exception)
        {
            result.Error = new PingResultException(result, exception.InnerException);
        }
        catch (Exception exception)
        {
            result.Error = new PingResultException(result, exception);
        }

        return result;
    }

    private static async Task<DnsResult> GetDnsAsync(
        string destination,
        PingAsyncOptions options,
        Cancellation cancellation)
    {
        Task<IPHostEntry> dns = System.Net.Dns.GetHostEntryAsync(destination);
        Task timeout = cancellation.GetTimeoutTask(options.TaskTimeout);
        Task result = await Task
            .WhenAny(dns, timeout)
            .NoContext();

        if (result == timeout) return DnsFailure.Timeout;

        try
        {
            IPHostEntry entry = await dns.NoContext();
            return new DnsSuccess(entry);
        }
        catch (Exception exception)
        {
            return new DnsFailure(DnsStatus.Error, exception);
        }
    }
}
