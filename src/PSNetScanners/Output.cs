using System.Management.Automation;

namespace PSNetScanners;

internal record struct Output(Type Type, object Data)
{
    internal static Output CreateSuccess(object Data) =>
        new(Type.Success, Data);

    internal static Output CreateError(ErrorRecord error) =>
        new(Type.Error, error);
}
