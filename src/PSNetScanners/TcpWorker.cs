using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PSNetScanners;

internal sealed class TcpWorker : WorkerBase<TcpInput, Output, TcpResult>
{
    protected override Task Worker { get; }

    private readonly int _timeout;

    internal TcpWorker(int throttle, int timeout)
        : base(throttle, new Cancellation())
    {
        _timeout = timeout;
        Worker = Task.Run(Start, Token);
    }

    protected override async Task Start()
    {
        List<Task<TcpResult>> tasks = [];
        while (!InputQueue.IsCompleted)
        {
            if (InputQueue.TryTake(out TcpInput input, 0, Token))
            {
                tasks.Add(TcpResult.CreateAsync(
                    input: input,
                    cancelTask: _cancellation.Task,
                    timeout: _timeout));
            }

            if (tasks.Count == _throttle)
            {
                await ProcessOneAsync(tasks);
            }
        }

        while (tasks.Count > 0)
        {
            await ProcessOneAsync(tasks);
        }

        OutputQueue.CompleteAdding();
    }

    protected override async Task ProcessTaskAsync(Task<TcpResult> task)
    {
        try
        {
            TcpResult result = await task;
            OutputQueue.Add(Output.CreateSuccess(result), Token);
        }
        catch (Exception exception)
        {
            ErrorRecord error = exception.CreateProcessing(task);
            OutputQueue.Add(Output.CreateError(error), Token);
        }
    }
}
