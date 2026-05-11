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
        PingResult result = new(source, destination);
        (PingOptions opt, int _, int timeout, byte[] buffer, bool resolveDns) = options;

        try
        {
            using System.Net.NetworkInformation.Ping ping = new();
            result.Reply = await ping.SendPingAsync(destination, timeout, buffer, opt).NoContext();
            if (resolveDns) await SetDnsAsync(result, timeout, cancellation).NoContext();
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

    private static async Task SetDnsAsync(
        PingResult result,
        int timeout,
        Cancellation cancellation)
    {
        try
        {
            Task<IPHostEntry> dns = System.Net.Dns.GetHostEntryAsync(result.Destination);
            Task timeoutTask = cancellation.GetTimeoutTask(timeout);
            Task any = await Task.WhenAny(dns, timeoutTask, cancellation.Task).NoContext();
            result.DnsResult = any == dns ? new DnsSuccess(await dns.NoContext()) : DnsFailure.Timeout;
        }
        catch (Exception exception)
        {
            result.DnsResult = new DnsFailure(DnsStatus.Error, exception);
        }
    }
}
