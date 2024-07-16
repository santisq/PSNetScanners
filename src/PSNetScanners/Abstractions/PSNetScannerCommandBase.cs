using System.Management.Automation;

namespace PSNetScanners;

public abstract class PSNetScannerCommandBase : PSCmdlet
{
    [Parameter(
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        Position = 0)]
    [Alias(["Address", "Host"])]
    public string[] Target { get; set; } = null!;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    [Alias("ttl")]
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
