using System.Management.Automation;
using System.Net;
using PSNetScanners.Abstractions;
using PSNetScanners.Tcp;

namespace PSNetScanners.Commands;

[Cmdlet(VerbsDiagnostic.Test, "TcpAsync")]
[OutputType(typeof(TcpResult))]
[Alias("tcpasync")]
public sealed class TestTcpAsyncCommand : PSNetScannerCommandBase<TcpInput>
{
    [Parameter(
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        Position = 1)]
    [ValidateRange(IPEndPoint.MinPort, IPEndPoint.MaxPort)]
    [Alias("p")]
    public int[] Port { get; set; } = null!;

    protected override void EnqueueAllTasks()
    {
        foreach (string address in Target)
            foreach (int port in Port)
                Enqueue(new TcpInput(address, port));
    }

    internal override IWorker<TcpInput> CreateWorker()
        => new TcpWorker(ThrottleLimit, ConnectionTimeout);
}
