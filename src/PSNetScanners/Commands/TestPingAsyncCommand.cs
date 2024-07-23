using System;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Text;
using PSNetScanners.Abstractions;
using PSNetScanners.Dbg;

namespace PSNetScanners.Commands;

[Cmdlet(VerbsDiagnostic.Test, "PingAsync")]
[OutputType(typeof(PingResult))]
[Alias("pingasync")]
public sealed class TestPingAsyncCommand : PSNetScannerCommandBase, IDisposable
{
    [Parameter]
    [ValidateRange(1, 65500)]
    [Alias("bfs")]
    public int BufferSize { get; set; } = 32;

    [Parameter]
    [Alias("dns")]
    public SwitchParameter ResolveDns { get; set; }

    [Parameter]
    public int Ttl { get; set; } = 128;

    [Parameter]
    public SwitchParameter DontFragment { get; set; }

    private PingWorker? _worker;

    protected override void BeginProcessing()
    {
        PingAsyncOptions options = new()
        {
            PingOptions = new PingOptions(Ttl, DontFragment.IsPresent),
            Buffer = Encoding.ASCII.GetBytes(new string('A', BufferSize)),
            TaskTimeout = ConnectionTimeout,
            ThrottleLimit = ThrottleLimit,
            ResolveDns = ResolveDns.IsPresent
        };

        _worker = new PingWorker(options);
    }

    protected override void ProcessRecord()
    {
        Debug.Assert(_worker is not null);

        try
        {
            foreach (string address in Target)
            {
                _worker.Enqueue(address);

                if (_worker.TryTake(out Output data))
                {
                    Process(data);
                }
            }
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            _worker.Cancel();
            throw;
        }
    }

    protected override void EndProcessing()
    {
        Debug.Assert(_worker is not null);

        try
        {
            _worker.CompleteAdding();
            foreach (Output data in _worker.GetOutput())
            {
                Process(data);
            }
            _worker.Wait();
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            _worker.Cancel();
            throw;
        }
    }

    protected override void StopProcessing() => _worker?.Cancel();

    public void Dispose()
    {
        _worker?.Dispose();
        GC.SuppressFinalize(this);
    }
}
