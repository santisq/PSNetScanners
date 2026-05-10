using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using PSNetScanners.Abstractions;
using PSNetScanners.Dns;

namespace PSNetScanners.Ping;

public sealed class PingResult : ResultBase
{
    public IPAddress? Address { get; private set; }

    public string DisplayAddress { get; private set; } = "*";

    public long Latency { get; private set; }

    internal string LatencyAsString { get; private set; } = "*";

    public IPStatus? Status { get; private set; } = IPStatus.Unknown;

    public DnsResult? DnsResult { get; private set; }

    public PingReply? Reply
    {
        get;
        private set
        {
            field = value;
            if (value is null) return;

            Status = value.Status;
            if (Status != IPStatus.Success) return;

            Latency = value.RoundtripTime;
            LatencyAsString = $"{Latency} ms";
            Address = value.Address;
            DisplayAddress = Address.ToString();
        }
    }

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
            result.Reply = await pingTask.NoContext();
            if (!resolveDns) return result;

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
