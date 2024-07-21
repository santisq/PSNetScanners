using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PSNetScanners.Abstractions;

internal abstract class WorkerBase<TInput, TResult>(int throttle, Cancellation cancellation)
    : WorkerBase(throttle, cancellation)
{
    protected virtual BlockingCollection<TInput> InputQueue { get; } = [];

    protected virtual BlockingCollection<Output> OutputQueue { get; } = [];

    internal void Enqueue(TInput item) => InputQueue.Add(item, Token);

    internal void CompleteAdding() => InputQueue.CompleteAdding();

    internal virtual IEnumerable<Output> GetOutput() => OutputQueue.GetConsumingEnumerable(Token);

    internal bool TryTake(out Output result) => OutputQueue.TryTake(out result, 0, Token);

    protected async Task ProcessOneAsync(
        List<Task<TResult>> tasks)
    {
        Task<TResult> task = await Task.WhenAny(tasks);
        tasks.Remove(task);
        await ProcessTaskAsync(task);
    }

    protected abstract Task ProcessTaskAsync(Task<TResult> task);

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _cancellation.Cancel();
            InputQueue.Dispose();
            OutputQueue.Dispose();
        }

        _disposed = true;
    }
}
