using System;
using System.Collections.Generic;

namespace PSNetScanners;

internal interface IWorker<TInput> : IDisposable
{
    string Source { get; }
    void Enqueue(TInput input);
    bool TryTake(out object result);
    void Cancel();
    void CompleteAdding();
    void Wait();
    IEnumerable<object> EnumerateOutput();
}
