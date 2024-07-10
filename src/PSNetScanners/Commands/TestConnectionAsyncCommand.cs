using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

[Cmdlet(VerbsDiagnostic.Test, "ConnectionAsync")]
public sealed class TestConnectionAsyncCommand : PSCmdlet
{
    private readonly List<Task> _tasks = new();

    private byte[] _buffer = null!;

    private int _taskCount;

    private readonly PingOptions _pingOptions = new();

    private readonly BlockingCollection<PingReply> _output = new();

    [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
    public string[] Address { get; set; } = null!;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    public int BufferSize { get; set; } = 32;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    public int TimeOutSeconds { get; set; } = 10;

    [Parameter]
    public SwitchParameter DontFragment { get; set; }

    protected override void BeginProcessing()
    {
        _buffer = Enumerable.Repeat((byte)'A', BufferSize).ToArray();
        _pingOptions.DontFragment = DontFragment.IsPresent;
    }

    protected override void ProcessRecord()
    {
        foreach (string addr in Address)
        {
            Ping ping = new();
            ping.SendPingAsync(
                hostNameOrAddress: addr,
                timeout: TimeOutSeconds * 1000,
                buffer: _buffer,
                options: _pingOptions);

            ping.Disposed += (sender, e) => Interlocked.Decrement(ref _taskCount);

            ping.PingCompleted += (sender, e) =>
            {
                _output.Add(e.Reply);
                ((AutoResetEvent)e.UserState).Set();
            };
        }
    }
}