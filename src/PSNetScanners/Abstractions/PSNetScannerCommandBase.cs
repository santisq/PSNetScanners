using System.Management.Automation;

namespace PSNetScanners.Abstractions;

public abstract class PSNetScannerCommandBase : PSCmdlet
{
    [Parameter(
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        Position = 0)]
    [Alias([
        "ComputerName",
        "HostName",
        "Host",
        "Server",
        "Address"])]
    public string[] Target { get; set; } = null!;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    [Alias("tl")]
    public int ThrottleLimit { get; set; } = 50;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    [Alias(["timeout", "to", "ct"])]
    public int ConnectionTimeout { get; set; } = 4000;

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
