using System;
using System.Management.Automation;
using System.Threading.Tasks;
using PSNetScanners.Abstractions;

namespace PSNetScanners.Tcp;

internal sealed class TcpWorker(int throttle, int timeout)
    : WorkerBase<TcpInput, TcpResult>(throttle)
{
    protected override Task<TcpResult> CreateAsync(TcpInput destination)
        => TcpResult.CreateAsync(Source, destination, Cancellation, timeout);

    protected override async Task ProcessTaskAsync(Task<TcpResult> task)
    {
        try
        {
            TcpResult result = await task.NoContext();
            OutputQueue.Add(Output.CreateSuccess(result), Token);
        }
        catch (Exception exception)
        {
            ErrorRecord error = exception.CreateProcessing(task);
            OutputQueue.Add(Output.CreateError(error), Token);
        }
    }
}
