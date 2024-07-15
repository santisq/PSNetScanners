using System;
using System.Net;
using System.Threading.Tasks;

namespace PSNetScanners;

internal static class DnsAsync
{
    internal static async Task<DnsResult> GetHostEntryAsync(
        string host,
        TaskOptions options)
    {
        if (options.Cancellation.IsCancellationRequested)
        {
            return new DnsFailure(DnsStatus.Cancelled);
        }

        Task<IPHostEntry> taskEntry = Dns.GetHostEntryAsync(host);
        Task delayTask = options.GetTimeoutDelay();
        Task task = await Task.WhenAny(taskEntry, options.CancelTask, delayTask);

        if (task == options.CancelTask || task == delayTask)
        {
            return new DnsFailure(DnsStatus.Timeout);
        }

        try
        {
            IPHostEntry entry = await taskEntry;
            return new DnsSuccess(entry);
        }
        catch (Exception exception)
        {
            return new DnsFailure(DnsStatus.Error, exception);
        }
    }
}
