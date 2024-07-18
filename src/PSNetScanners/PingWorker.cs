using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

internal sealed class PingWorker : WorkerBase<string, Output, PingResult>
{
    protected override CancellationToken Token { get => _cancellation.Token; }

    protected override Task Worker { get; }

    private readonly PingAsyncOptions _options;

    private readonly Cancellation _cancellation;

    internal PingWorker(PingAsyncOptions options)
        : base(options.ThrottleLimit)
    {
        _cancellation = new Cancellation();
        _options = options;
        Worker = Task.Run(Start, Token);
    }

    internal override void Cancel() => _cancellation.Cancel();

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
                    cancelTask: _cancellation.Task));

                if (tasks.Count == _throttle)
                {
                    Task<PingResult> result = await WaitOne(tasks);
                    await ProcessTaskAsync(result);
                }
            }
        }

        while (tasks.Count > 0)
        {
            Task<PingResult> result = await WaitOne(tasks);
            await ProcessTaskAsync(result);
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
