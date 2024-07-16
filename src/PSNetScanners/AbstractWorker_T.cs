using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PSNetScanners;

internal abstract class WorkerBase<TInput, TOutput>(int throttle) : WorkerBase(throttle)
{
    protected virtual BlockingCollection<TInput> InputQueue { get; } = [];

    protected virtual BlockingCollection<TOutput> OutputQueue { get; } = [];

    internal void Enqueue(TInput item) => InputQueue.Add(item, Token);

    internal void CompleteAdding() => InputQueue.CompleteAdding();

    internal virtual IEnumerable<TOutput> GetOutput() => OutputQueue.GetConsumingEnumerable(Token);

    internal bool TryTake(out TOutput result) => OutputQueue.TryTake(out result, 0, Token);

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            InputQueue.Dispose();
            OutputQueue.Dispose();
        }

        _disposed = true;
    }
}
