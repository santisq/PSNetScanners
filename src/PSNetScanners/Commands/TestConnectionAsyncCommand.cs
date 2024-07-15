using System;
using System.Management.Automation;

namespace PSNetScanners;

[Cmdlet(VerbsDiagnostic.Test, "ConnectionAsync")]
public sealed class TestConnectionAsyncCommand : PSCmdlet, IDisposable
{
    [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
    public string[] Address { get; set; } = null!;

    [Parameter]
    [ValidateRange(1, int.MaxValue)]
    public int TimeoutSeconds { get; set; }

    [Parameter]
    public TimeSpan? TaskTimeout { get; set; }

    [Parameter]
    [ValidateRange(1, 65500)]
    public int BufferSize { get; set; } = 32;

    private PingWorker? _worker;

    protected override void BeginProcessing()
    {
        TaskTimeout.ValidateTimeout(this);
        _worker = new PingWorker(BufferSize, TaskTimeout, TimeoutSeconds);
    }

    protected override void ProcessRecord()
    {
        if (_worker is null)
        {
            return;
        }

        try
        {
            foreach (string addr in Address)
            {
                _worker.Enqueue(addr);
            }
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            _worker.Cancel();
            _worker.Wait();
            throw;
        }
        catch (OperationCanceledException exception)
        {
            _worker.Wait();
            exception.WriteTimeoutError(this);
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
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            _worker.Cancel();
            _worker.Wait();
            throw;
        }
        catch (OperationCanceledException exception)
        {
            _worker.Wait();
            exception.WriteTimeoutError(this);
        }
    }

    private void Process(Output output)
    {
        switch (output.Type)
        {
            case Type.Success:
                WriteObject((PingResult)output.Data);
                break;

            case Type.Error:
                WriteError((ErrorRecord)output.Data);
                break;
        }
    }

    protected override void StopProcessing() => _worker?.Cancel();

    public void Dispose()
    {
        _worker?.Dispose();
        GC.SuppressFinalize(this);
    }
}
