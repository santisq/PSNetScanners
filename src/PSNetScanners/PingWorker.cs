using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PSNetScanners;

internal sealed class PingWorker : WorkerBase<string, Output, PingResult>
{
    protected override Task Worker { get; }

    private readonly PingAsyncOptions _options;

    internal PingWorker(PingAsyncOptions options)
        : base(options.ThrottleLimit, new Cancellation())
    {
        _options = options;
        Worker = Task.Run(Start, Token);
    }

    protected override async Task Start()
    {
        List<Task<PingResult>> tasks = [];

        while (!InputQueue.IsCompleted)
        {
            if (InputQueue.TryTake(out string host, 0, Token))
            {
                tasks.Add(PingResult.CreateAsync(
                    source: Source,
                    destination: host,
                    options: _options,
                    cancellation: _cancellation));

                if (tasks.Count == _throttle)
                {
                    await ProcessOneAsync(tasks);
                }
            }
        }

        while (tasks.Count > 0)
        {
            await ProcessOneAsync(tasks);
        }

        OutputQueue.CompleteAdding();
    }

    protected override async Task ProcessTaskAsync(Task<PingResult> task)
    {
        try
        {
            PingResult result = await task;
            OutputQueue.Add(Output.CreateSuccess(result), Token);
        }
        catch (PingException exception)
        {
            ErrorRecord error = exception.InnerException.CreateProcessing(task);
            OutputQueue.Add(Output.CreateError(error), Token);
        }
        catch (Exception exception)
        {
            ErrorRecord error = exception.CreateProcessing(task);
            OutputQueue.Add(Output.CreateError(error), Token);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _cancellation.Dispose();
    }
}
