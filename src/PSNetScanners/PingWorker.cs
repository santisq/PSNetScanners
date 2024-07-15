using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
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

    private readonly int _throttle;

    internal PingWorker(int bufferSize, int? taskTimeout, int throttle)
    {
        _cancellation = new Cancellation();
        _throttle = throttle;
        _options = new TaskOptions
        {
            Buffer = Encoding.ASCII.GetBytes(new string('A', bufferSize)),
            Cancellation = _cancellation,
            TaskTimeout = taskTimeout ?? 4000
        };

        _worker = Task.Run(Start, Token);
    }

    internal void Cancel() => _cancellation.Cancel();

    internal void Wait() => _worker.GetAwaiter().GetResult();

    internal void Enqueue(string item) => _inputQueue.Add(item, Token);

    internal void CompleteAdding() => _inputQueue.CompleteAdding();

    internal IEnumerable<Output> GetOutput() => _outputQueue.GetConsumingEnumerable(Token);

    internal bool TryTake(out Output result) => _outputQueue.TryTake(out result, 0, Token);

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
                    destination: host,
                    options: _options));

                if (tasks.Count == _throttle)
                {
                    await ProcessOne(tasks);
                }
            }
        }

        while (tasks.Count > 0)
        {
            await ProcessOne(tasks);
        }

        _outputQueue.CompleteAdding();

        async Task ProcessOne(List<Task<PingResult>> tasks)
        {
            Task<PingResult> task = await Task.WhenAny(tasks);
            tasks.Remove(task);

            try
            {
                PingResult result = await task;
                _outputQueue.Add(Output.CreateSuccess(result), Token);
            }
            catch (PingException exception)
            {
                ErrorRecord error = exception.InnerException.CreateProcessing(task);
                _outputQueue.Add(Output.CreateError(error), Token);
            }
            catch (Exception exception)
            {
                ErrorRecord error = exception.CreateProcessing(task);
                _outputQueue.Add(Output.CreateError(error), Token);
            }
        }
    }

    public void Dispose()
    {
        _inputQueue.Dispose();
        _outputQueue.Dispose();
        _cancellation.Dispose();
        GC.SuppressFinalize(this);
    }
}
