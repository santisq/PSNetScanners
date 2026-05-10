using System;
using System.Collections.Generic;

namespace PSNetScanners;

internal interface IWorker<TInput> : IDisposable
{
    void Enqueue(TInput input);
    bool TryTake(out object result);
    void Cancel();
    void CompleteAdding();
    void Wait();
    IEnumerable<object> EnumerateOutput();
}
