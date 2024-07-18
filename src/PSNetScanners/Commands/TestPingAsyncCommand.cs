using System;
using System.Management.Automation;
using System.Net.NetworkInformation;
using System.Text;

namespace PSNetScanners;

[Cmdlet(VerbsDiagnostic.Test, "PingAsync")]
[OutputType(typeof(PingResult))]
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
            TaskTimeout = ConnectionTimeout ?? 4000,
            ThrottleLimit = ThrottleLimit,
            ResolveDns = ResolveDns.IsPresent
        };

        _worker = new PingWorker(options);
    }

    protected override void ProcessRecord()
    {
        if (_worker is null)
        {
            return;
        }

        try
        {
            foreach (string address in Target)
            {
                _worker.Enqueue(address);
            }

            while (_worker.TryTake(out Output data))
            {
                Process(data);
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
        if (_worker is null)
        {
            return;
        }

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
