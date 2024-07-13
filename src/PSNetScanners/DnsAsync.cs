using System;
using System.Net;
using System.Threading.Tasks;

namespace PSNetScanners;

internal static class DnsAsync
{
    internal static async Task<DnsResult> GetHostEntryAsync(
        string host,
        Cancellation cancellation)
    {
        if (cancellation.IsCancellationRequested)
        {
            return new DnsFailure(DnsStatus.Cancelled);
        }

        Task<IPHostEntry> taskEntry = Dns.GetHostEntryAsync(host);
        Task task = await Task.WhenAny(taskEntry, cancellation.Task);

        if (task == cancellation.Task)
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
