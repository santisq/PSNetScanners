using System;
using System.Management.Automation;

namespace PSNetScanners;

public abstract class PSNetScannerCommandBase : PSCmdlet
{
    [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
    public string[] Address { get; set; } = null!;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    public int ThrottleLimit { get; set; } = 50;

    internal static void StopHandle(WorkerBase worker)
    {
        worker.Cancel();
        worker.Wait();
    }

    internal void Process(Output output)
    {
        switch (output.Type)
        {
            case Type.Success:
                WriteObject(output.Data);
                break;

            case Type.Error:
                WriteError((ErrorRecord)output.Data);
                break;
        }
    }
}
