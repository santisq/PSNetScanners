using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners.Abstractions;

internal abstract class WorkerBase : IDisposable
{
    internal string Source { get; } = System.Net.Dns.GetHostName();

    protected CancellationToken Token { get => Cancellation.Token; }

    protected Task Worker { get; }

    protected Cancellation Cancellation { get; } = new();

    protected abstract Task Start();

    protected WorkerBase() => Worker = Task.Run(Start, Token);

    internal void Cancel()
    {
        Cancellation.Cancel();
        Wait();
    }

    internal void Wait() => Worker.GetAwaiter().GetResult();

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        Cancellation.Dispose();
        GC.SuppressFinalize(this);
    }
}
