using System;
using System.Management.Automation;
using System.Net;

namespace PSNetScanners;

[Cmdlet(VerbsDiagnostic.Test, "TcpAsync")]
[OutputType(typeof(TcpResult))]
[Alias("tcpasync")]
public sealed class TestTcpAsyncCommand : PSNetScannerCommandBase, IDisposable
{
    [Parameter(
        Mandatory = true,
        ValueFromPipelineByPropertyName = true,
        Position = 1)]
    [ValidateRange(IPEndPoint.MinPort, IPEndPoint.MaxPort)]
    [Alias("p")]
    public int[] Port { get; set; } = null!;

    private TcpWorker? _worker;

    protected override void BeginProcessing()
    {
        _worker = new TcpWorker(
            throttle: ThrottleLimit,
            timeout: ConnectionTimeout ?? 4000);
    }

    protected override void ProcessRecord()
    {
        Dbg.Assert(_worker is not null);

        try
        {
            foreach (string address in Target)
            {
                foreach (int port in Port)
                {
                    _worker.Enqueue(new TcpInput(
                        source: _worker.Source,
                        target: address,
                        port: port));
                }
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
        Dbg.Assert(_worker is not null);

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
