using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PSNetScanners.Abstractions;

internal abstract class WorkerBase<TInput, TResult>(int throttle) : WorkerBase, IWorker<TInput>
{
    private bool _disposed;

    protected virtual BlockingCollection<TInput> InputQueue { get; } = [];

    protected virtual BlockingCollection<Output> OutputQueue { get; } = [];

    internal void Enqueue(TInput item) => InputQueue.Add(item, Token);

    internal void CompleteAdding() => InputQueue.CompleteAdding();

    internal virtual IEnumerable<Output> GetOutput() => OutputQueue.GetConsumingEnumerable(Token);

    internal bool TryTake(out Output result) => OutputQueue.TryTake(out result, 0, Token);

    string IWorker<TInput>.Source { get => Source; }

    void IWorker<TInput>.Enqueue(TInput input) => Enqueue(input);

    bool IWorker<TInput>.TryTake(out Output result) => TryTake(out result);

    void IWorker<TInput>.Cancel() => Cancel();

    void IWorker<TInput>.CompleteAdding() => CompleteAdding();

    void IWorker<TInput>.Wait() => Wait();

    IEnumerable<Output> IWorker<TInput>.GetOutput() => GetOutput();

    protected override async Task Start()
    {
        List<Task<TResult>> tasks = [];

        while (!InputQueue.IsCompleted)
        {
            if (InputQueue.TryTake(out TInput input, 0, Token))
            {
                tasks.Add(CreateAsync(input));
                if (tasks.Count == throttle)
                    await ProcessOneAsync(tasks).NoContext();
            }
        }

        while (tasks.Count > 0)
            await ProcessOneAsync(tasks).NoContext();

        OutputQueue.CompleteAdding();
    }

    private async Task ProcessOneAsync(List<Task<TResult>> tasks)
    {
        Task<TResult> task = await Task.WhenAny(tasks).NoContext();
        tasks.Remove(task);
        await ProcessTaskAsync(task).NoContext();
    }

    protected abstract Task ProcessTaskAsync(Task<TResult> task);

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
