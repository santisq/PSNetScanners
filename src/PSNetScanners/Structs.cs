using System.Management.Automation;
using System.Net.NetworkInformation;

namespace PSNetScanners;

internal record struct PingAsyncOptions(
    PingOptions PingOptions,
    int ThrottleLimit,
    int TaskTimeout,
    byte[] Buffer,
    bool ResolveDns);

internal record struct Output(Type Type, object Data)
{
    internal static Output CreateSuccess(PingResult Data) =>
        new(Type.Success, Data);

    internal static Output CreateError(ErrorRecord error) =>
        new(Type.Error, error);
}
