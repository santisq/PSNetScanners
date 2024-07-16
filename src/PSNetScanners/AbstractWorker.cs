using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

internal abstract class WorkerBase(int throttle) : IDisposable
{
    protected abstract CancellationToken Token { get; }

    protected abstract Task Worker { get; }

    protected readonly int _throttle = throttle;

    protected bool _disposed;

    protected abstract Task Start();

    internal abstract void Cancel();

    internal void Wait() => Worker.GetAwaiter().GetResult();

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
