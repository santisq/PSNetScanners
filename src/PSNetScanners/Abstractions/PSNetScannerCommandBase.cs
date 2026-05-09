using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace PSNetScanners.Abstractions;

public abstract class PSNetScannerCommandBase<TInput> : PSCmdlet, IDisposable
{
    private IWorker<TInput>? _worker;

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

    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition)
        => Debug.Assert(condition);

    internal abstract IWorker<TInput> CreateWorker();

    protected void Enqueue(TInput input)
    {
        Assert(_worker is not null);
        _worker.Enqueue(input);
    }

    protected abstract void EnqueueAllTasks();

    protected override void BeginProcessing()
    {
        _worker = CreateWorker();
    }

    protected override void ProcessRecord()
    {
        Assert(_worker is not null);

        try
        {
            EnqueueAllTasks();
            while (_worker.TryTake(out Output data)) Process(data);
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            _worker.Cancel();
            throw;
        }
    }

    protected override void EndProcessing()
    {
        Assert(_worker is not null);

        try
        {
            _worker.CompleteAdding();
            foreach (Output data in _worker.GetOutput()) Process(data);
            _worker.Wait();
        }
        catch (Exception _) when (_ is PipelineStoppedException or FlowControlException)
        {
            _worker.Cancel();
            throw;
        }
    }

    protected override void StopProcessing()
    {
        Assert(_worker is not null);
        _worker.Cancel();
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

    public void Dispose()
    {
        Assert(_worker is not null);
        _worker.Dispose();
        GC.SuppressFinalize(this);
    }
}
