using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PSNetScanners;

public sealed class PingResult
{
    public string Source { get; }

    public string Destination { get; }

    public IPAddress? Address { get; }

    public string DisplayAddress { get; }

    public long Latency { get; }

    public IPStatus Status { get; }

    public DnsResult DnsResult { get; }

    public PingReply? Reply { get; }

    private PingResult(
        string source,
        string destination,
        DnsResult dns,
        PingReply? reply = null)
    {
        Source = source;
        Destination = destination;
        DnsResult = dns;
        Reply = reply;
        Status = reply?.Status ?? IPStatus.TimedOut;
        Address = Status is IPStatus.Success ? reply?.Address : IPAddress.None;
        Latency = reply?.RoundtripTime ?? 0;
        DisplayAddress = Address?.ToString() ?? "*";
    }

    internal static async Task<PingResult> CreateAsync(
        string source,
        string destination,
        TaskOptions options)
    {
        using Ping ping = new();
        Task<IPHostEntry> dnsTask = Dns.GetHostEntryAsync(destination);
        Task<PingReply> pingTask = ping.SendPingAsync(
            destination,
            options.TaskTimeout,
            options.Buffer);

        Task result = options.TaskTimeout == 4000
            ? await Task.WhenAny(options.CancelTask, dnsTask, pingTask)
            : await Task.WhenAny(
                options.CancelTask, dnsTask, pingTask,
                Task.Delay(options.TaskTimeout));

        if (result != dnsTask && result != pingTask)
        {
            return new PingResult(
                source: source,
                destination: destination,
                dns: DnsFailure.CreateTimeout());
        }

        DnsResult dnsResult;
        try
        {
            IPHostEntry entry = await dnsTask;
            dnsResult = new DnsSuccess(entry);
        }
        catch (Exception exception)
        {
            dnsResult = new DnsFailure(DnsStatus.Error, exception);
        }

        return new PingResult(
            source: source,
            destination: destination,
            dns: dnsResult,
            reply: await pingTask);
    }
}
