using System;
using System.Collections.Generic;
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
        Status = reply?.Status ?? IPStatus.TimedOut;
        Address = Status is IPStatus.Success ? reply?.Address : null;
        Latency = reply?.RoundtripTime ?? 0;
        DisplayAddress = Address?.ToString() ?? "*";
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

        Task<IPHostEntry> dnsTask = Dns.GetHostEntryAsync(destination);
        List<Task> tasks = [pingTask, cancellation.Task, dnsTask];
        Task result = await WaitOneAsync(options, cancellation, tasks);

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
            dns: await GetDnsResult(dnsTask),
            reply: await pingTask);
    }

    private static async Task<Task> WaitOneAsync(
        PingAsyncOptions options,
        Cancellation cancellation,
        List<Task> tasks)
    {
        if (options.TaskTimeout == 4000)
        {
            return await Task.WhenAny(tasks);
        }

        tasks.Add(Task.Delay(options.TaskTimeout, cancellation.Token));
        return await Task.WhenAny(tasks);
    }

    private static async Task<DnsResult> GetDnsResult(
        Task<IPHostEntry> dnsTask)
    {
        try
        {
            IPHostEntry entry = await dnsTask;
            return new DnsSuccess(entry);
        }
        catch (Exception exception)
        {
            return new DnsFailure(DnsStatus.Error, exception);
        }
    }
}
