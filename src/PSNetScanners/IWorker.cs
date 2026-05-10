using System;
using System.Collections.Generic;

namespace PSNetScanners;

internal interface IWorker<TInput> : IDisposable
{
    void Enqueue(TInput input);
    bool TryTake(out object result);
    void CompleteAdding();
    IEnumerable<object> EnumerateOutput();
    void Wait();
    void CancelAndWait();
}
