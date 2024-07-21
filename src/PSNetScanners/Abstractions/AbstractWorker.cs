using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners.Abstractions;

internal abstract class WorkerBase(int throttle, Cancellation cancellation)
    : IDisposable
{
    protected CancellationToken Token { get => _cancellation.Token; }

    protected abstract Task Worker { get; }

    internal string Source { get; } = Dns.GetHostName();

    protected readonly Cancellation _cancellation = cancellation;

    protected readonly int _throttle = throttle;

    protected bool _disposed;

    protected abstract Task Start();

    internal void Cancel()
    {
        _cancellation.Cancel();
        Wait();
    }

    internal void Wait() => Worker.GetAwaiter().GetResult();

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
