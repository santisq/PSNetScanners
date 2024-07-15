using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSNetScanners;

internal sealed class PingWorker : IDisposable
{
    private readonly BlockingCollection<string> _inputQueue = [];

    private readonly BlockingCollection<Output> _outputQueue = [];

    private readonly Task _worker;

    private CancellationToken Token { get => _cancellation.Token; }

    private readonly TaskOptions _options;

    private readonly Cancellation _cancellation;

    internal PingWorker(
        int bufferSize,
        TimeSpan? pingTimeout,
        int timeoutSeconds)
    {
        _cancellation = new Cancellation(timeoutSeconds);
        _options = new TaskOptions
        {
            Buffer = Encoding.ASCII.GetBytes(new string('A', bufferSize)),
            Timeout = pingTimeout ?? TimeSpan.FromMilliseconds(500),
            Cancellation = _cancellation
        };

        _worker = Task.Run(Start, Token);
    }

    internal void Cancel() => _cancellation.Cancel();

    internal void Wait() => _worker.GetAwaiter().GetResult();

    internal void Enqueue(string item) => _inputQueue.Add(item, Token);

    internal void CompleteAdding() => _inputQueue.CompleteAdding();

    internal IEnumerable<Output> GetOutput() => _outputQueue.GetConsumingEnumerable(Token);

    internal void ThrowIfCancellationRequested() => _cancellation.ThrowIfCancellationRequested();

    private async Task Start()
    {
        string source = Dns.GetHostName();
        List<Task<PingResult>> tasks = [];

        while (!_inputQueue.IsCompleted)
        {
            if (_inputQueue.TryTake(out string host, 0, Token))
            {
                tasks.Add(PingResult.CreateAsync(
                    source: source,
                    target: host,
                    pingOptions: _options));
            }
        }

        while (tasks.Count > 0)
        {
            Task<PingResult> task = await Task.WhenAny(tasks);
            tasks.Remove(task);

            try
            {
                PingResult result = await task;
                _outputQueue.Add(Output.CreateSuccess(result), Token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                ErrorRecord error = exception.CreateProcessing(this);
                _outputQueue.Add(Output.CreateError(error), Token);
            }
        }

        _outputQueue.CompleteAdding();
    }

    public void Dispose()
    {
        _inputQueue.Dispose();
        _outputQueue.Dispose();
        _cancellation.Dispose();
        GC.SuppressFinalize(this);
    }
}
