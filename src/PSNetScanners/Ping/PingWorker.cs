using System.Management.Automation;
using System.Threading.Tasks;
using PSNetScanners.Abstractions;

namespace PSNetScanners.Ping;

internal sealed class PingWorker(PingAsyncOptions options)
    : WorkerBase<string, PingResult>(options.ThrottleLimit)
{
    protected override Task<PingResult> CreateAsync(string destination)
        => PingResult.CreateAsync(Source, destination, options, Cancellation);

    protected override async Task ProcessTaskAsync(Task<PingResult> task)
    {
        try
        {
            PingResult result = await task.NoContext();
            OutputQueue.Add(Output.CreateSuccess(result), Token);
        }
        catch (PingResultException exception)
        {
            ErrorRecord error = exception.CreateProcessing(exception.GetContext());
            OutputQueue.Add(Output.CreateError(error), Token);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Cancellation.Dispose();
    }
}
