using System.Threading.Tasks;
using PSNetScanners.Abstractions;

namespace PSNetScanners.Ping;

internal sealed class PingWorker(PingAsyncOptions options)
    : WorkerBase<string, PingResult>(options.ThrottleLimit)
{
    protected override Task<PingResult> CreateAsync(string destination)
        => PingResult.CreateAsync(Source, destination, options, Cancellation);
}
