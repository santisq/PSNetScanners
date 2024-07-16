using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

internal sealed class Cancellation : IDisposable
{
    private readonly CancellationTokenSource _cts;

    internal CancellationToken Token { get => _cts.Token; }

    internal Task Task { get; }

    internal Cancellation()
    {
        _cts = new CancellationTokenSource();
        Task = Task.Delay(Timeout.Infinite, _cts.Token);
    }

    internal void Cancel() => _cts.Cancel();

    public void Dispose() => _cts.Dispose();
}
