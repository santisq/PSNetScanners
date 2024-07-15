using System;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PSNetScanners;

internal record struct TaskOptions(
    Cancellation Cancellation,
    TimeSpan Timeout,
    byte[] Buffer)
{
    internal readonly int TaskTimeout { get => Timeout.Milliseconds; }

    internal readonly Task GetTimeoutDelay() => Task.Delay(Timeout);

    internal readonly Task CancelTask { get => Cancellation.Task; }
}

internal record struct Output(Type Type, object Data)
{
    internal static Output CreateSuccess(PingResult Data) =>
        new(Type.Success, Data);

    internal static Output CreateError(ErrorRecord error) =>
        new(Type.Error, error);
}
