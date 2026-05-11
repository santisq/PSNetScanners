using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSNetScanners.Abstractions;

internal abstract class WorkerBase<TInput, TResult>(int throttle)
    : WorkerBase, IWorker<TInput>
    where TResult : notnull
{
    private bool _disposed;

    protected BlockingCollection<TInput> InputQueue { get; } = [];

    protected BlockingCollection<object> OutputQueue { get; } = [];

    void IWorker<TInput>.Enqueue(TInput input) => InputQueue.Add(input, Token);

    bool IWorker<TInput>.TryTake(out object result) => OutputQueue.TryTake(out result, 0, Token);

    void IWorker<TInput>.CompleteAdding() => InputQueue.CompleteAdding();

    IEnumerable<object> IWorker<TInput>.EnumerateOutput() => OutputQueue.GetConsumingEnumerable(Token);

    void IWorker<TInput>.Wait() => Wait();

    void IWorker<TInput>.CancelAndWait()
    {
        Cancel();
        Wait();
    }

    protected override async Task Start()
    {
        List<Task<TResult>> tasks = new(throttle);
        try
        {
            foreach (TInput input in InputQueue.GetConsumingEnumerable(Token))
            {
                if (tasks.Count == throttle || tasks.Any(t => t.IsCompleted))
                    await ProcessOne(tasks);

                if (!Token.IsCancellationRequested)
                    tasks.Add(CreateAsync(input));
            }

            while (tasks.Count > 0 && !Token.IsCancellationRequested)
                await ProcessOne(tasks);
        }
        catch(OperationCanceledException)
        {
            if (tasks.Count > 0)
                await Task.WhenAll(tasks).NoContext();
        }
        finally
        {
            OutputQueue.CompleteAdding();
        }
    }

    private async Task ProcessOne(List<Task<TResult>> tasks)
    {
        Task<TResult> task = await Task.WhenAny(tasks).NoContext();
        tasks.Remove(task);
        TResult result = await task.NoContext();
        OutputQueue.Add(result, Token);
    }

    protected abstract Task<TResult> CreateAsync(TInput destination);

    protected override void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            InputQueue.Dispose();
            OutputQueue.Dispose();
        }

        _disposed = true;
    }
}
