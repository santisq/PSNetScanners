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

    protected virtual BlockingCollection<TInput> InputQueue { get; } = [];

    protected virtual BlockingCollection<object> OutputQueue { get; } = [];

    internal void Enqueue(TInput item) => InputQueue.Add(item, Token);

    internal void CompleteAdding() => InputQueue.CompleteAdding();

    internal virtual IEnumerable<object> EnumerateOutput() => OutputQueue.GetConsumingEnumerable(Token);

    internal bool TryTake(out object result) => OutputQueue.TryTake(out result, 0, Token);

    string IWorker<TInput>.Source { get => Source; }

    void IWorker<TInput>.Enqueue(TInput input) => Enqueue(input);

    bool IWorker<TInput>.TryTake(out object result) => TryTake(out result);

    void IWorker<TInput>.Cancel() => Cancel();

    void IWorker<TInput>.CompleteAdding() => CompleteAdding();

    void IWorker<TInput>.Wait() => Wait();

    IEnumerable<object> IWorker<TInput>.EnumerateOutput() => EnumerateOutput();

    protected override async Task Start()
    {
        List<Task<TResult>> tasks = new(throttle);

        foreach (TInput input in InputQueue.GetConsumingEnumerable(Token))
        {
            if (tasks.Count == throttle || tasks.Any(t => t.IsCompleted))
                await ProcessOne(tasks);

            tasks.Add(CreateAsync(input));
        }

        while (tasks.Count > 0)
            await ProcessOne(tasks);

        OutputQueue.CompleteAdding();
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
