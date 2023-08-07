using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PSNetScanners;

[Cmdlet(VerbsDiagnostic.Test, "ConnectionAsync")]
public sealed class TestConnectionAsyncCommand : PSCmdlet
{
    private readonly List<Task> _tasks = new();

    private byte[] _buffer;

    private readonly PingOptions _pingOptions = new();

    private TimeSpan? _timeOut;

    [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
    public string[] Address { get; set; } = null!;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    public int BufferSize { get; set; } = 32;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    public double? TimeOutSeconds { get; set; }

    [Parameter]
    public SwitchParameter DontFragment { get; set; }

    protected override void BeginProcessing()
    {
        _buffer = Enumerable.Repeat((byte)'A', BufferSize).ToArray();
        _pingOptions.DontFragment = DontFragment.IsPresent;

        if (TimeOutSeconds is not null)
        {
            _timeOut = TimeSpan.FromSeconds((double)TimeOutSeconds);
        }
    }

    protected override void ProcessRecord()
    {
        foreach (string addr in Address)
        {
            Ping ping = new();
            ping.PingCompleted += (sender, e) =>
            {
                // e.
            };
        }
    }
}