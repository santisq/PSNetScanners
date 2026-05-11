using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Text;
using PSNetScanners.Abstractions;
using PSNetScanners.Ping;

namespace PSNetScanners.Commands;

[Cmdlet(VerbsDiagnostic.Test, "PingAsync")]
[OutputType(typeof(PingResult))]
[Alias("pingasync")]
public sealed class TestPingAsyncCommand : PSNetScannerCommandBase<string>
{
    [Parameter]
    [ValidateRange(1, 65500)]
    [Alias("bfs")]
    public int BufferSize { get; set; } = 32;

    [Parameter]
    [Alias("dns")]
    public SwitchParameter ResolveDns { get; set; }

    [Parameter]
    [ValidateRange(1, 255)]
    public int Ttl { get; set; } = 128;

    [Parameter]
    public SwitchParameter DontFragment { get; set; }

    protected override void EnqueueTasks()
    {
        foreach (string address in Target)
        {
            Enqueue(address);
            WriteCompleted();
        }
    }

    internal override IWorker<string> CreateWorker()
    {
        PingAsyncOptions options = new()
        {
            PingOptions = new PingOptions()
            {
                DontFragment = DontFragment.IsPresent,
                Ttl = Ttl
            },
            Buffer = Encoding.ASCII.GetBytes(new string('A', BufferSize)),
            TaskTimeout = ConnectionTimeout,
            ThrottleLimit = ThrottleLimit,
            ResolveDns = ResolveDns.IsPresent
        };

        return new PingWorker(options);
    }
}
