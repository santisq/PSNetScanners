using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

internal sealed class Cancellation : IDisposable
{
    private readonly CancellationTokenSource _cts;

    internal CancellationToken Token { get => _cts.Token; }

    internal bool IsCancellationRequested { get => _cts.IsCancellationRequested; }

    internal Task Task { get; }

    internal Cancellation(int timeout)
    {
        _cts = new CancellationTokenSource(timeout > 0 ? timeout * 1000 : -1);
        Task = Task.Delay(Timeout.Infinite, _cts.Token);
    }

    internal void Cancel() => _cts.Cancel();

    internal void ThrowIfCancellationRequested() => _cts.Token.ThrowIfCancellationRequested();

    public void Dispose() => _cts.Dispose();
}
