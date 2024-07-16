using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

internal sealed class PingWorker : WorkerBase<string, Output>
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
        string source = Dns.GetHostName();
        List<Task<PingResult>> tasks = [];

        while (!InputQueue.IsCompleted)
        {
            if (InputQueue.TryTake(out string host, 0, Token))
            {
                tasks.Add(PingResult.CreateAsync(
                    source: source,
                    destination: host,
                    options: _options,
                    cancelTask: _cancellation.Task));

                if (tasks.Count == _throttle)
                {
                    await ProcessOne(tasks);
                }
            }
        }

        while (tasks.Count > 0)
        {
            await ProcessOne(tasks);
        }

        OutputQueue.CompleteAdding();

        async Task ProcessOne(List<Task<PingResult>> tasks)
        {
            Task<PingResult> task = await Task.WhenAny(tasks);
            tasks.Remove(task);

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
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _cancellation.Dispose();
    }
}
