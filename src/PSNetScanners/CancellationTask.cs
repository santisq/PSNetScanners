using System;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

public sealed class Cancellation : IDisposable
{
    private readonly CancellationTokenSource _cts;

    internal bool IsCancellationRequested { get => _cts.IsCancellationRequested; }

    internal Task Task { get; }

    public Cancellation(int? timeout)
    {
        _cts = new CancellationTokenSource(timeout ?? -1);
        Task = Task.Delay(Timeout.Infinite, _cts.Token);
    }

    public void Cancel() => _cts.Cancel();

    internal CancellationTokenRegistration Register(Action action) =>
        _cts.Token.Register(action);

    internal void ThrowIfCancellationRequested() => _cts.Token.ThrowIfCancellationRequested();

    public void Dispose() => _cts.Dispose();
}
