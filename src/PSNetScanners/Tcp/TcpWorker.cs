using System.Threading.Tasks;
using PSNetScanners.Abstractions;

namespace PSNetScanners.Tcp;

internal sealed class TcpWorker(int throttle, int timeout)
    : WorkerBase<TcpInput, TcpResult>(throttle)
{
    protected override Task<TcpResult> CreateAsync(TcpInput destination)
        => TcpResult.CreateAsync(Source, destination, Cancellation, timeout);
}
