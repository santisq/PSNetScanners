using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

internal sealed class Cancellation : IDisposable
{
    private readonly CancellationTokenSource _cts;

    internal Task Task { get; }

    internal Cancellation(int timeout)
    {
        _cts = new CancellationTokenSource(timeout);
        Task = Task.Delay(Timeout.Infinite, _cts.Token);
    }

    internal void Cancel() => _cts.Cancel();

    internal CancellationTokenRegistration Register(Action action) =>
        _cts.Token.Register(action);

    public void Dispose()
    {
        _cts.Dispose();
    }
}