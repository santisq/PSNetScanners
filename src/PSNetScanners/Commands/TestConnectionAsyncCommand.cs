using System;
using System.Management.Automation;

namespace PSNetScanners;

[Cmdlet(VerbsDiagnostic.Test, "ConnectionAsync")]
[OutputType(typeof(PingResult))]
public sealed class TestConnectionAsyncCommand : PSNetScannerCommandBase, IDisposable
{
    [Parameter]
    [ValidateRange(200, int.MaxValue)]
    public int? TaskTimeoutMilliseconds { get; set; }

    [Parameter]
    [ValidateRange(1, 65500)]
    public int BufferSize { get; set; } = 32;

    private PingWorker? _worker;

    protected override void BeginProcessing()
    {
        _worker = new PingWorker(
            BufferSize,
            TaskTimeoutMilliseconds,
            ThrottleLimit);
    }

    protected override void ProcessRecord()
    {
        if (_worker is null)
        {
            return;
        }

        try
        {
            foreach (string addr in Target)
            {
                _worker.Enqueue(addr);
            }

            while (_worker.TryTake(out Output data))
            {
                Process(data);
            }
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            StopHandle(_worker);
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
            StopHandle(_worker);
            throw;
        }
    }

    public void Dispose()
    {
        _worker?.Dispose();
        GC.SuppressFinalize(this);
    }
}
