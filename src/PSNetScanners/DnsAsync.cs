using System.Net;
using System.Threading.Tasks;

namespace PSNetScanners;

public enum Status
{
    Success,
    Timeout,
    Error
}

public abstract class DnsResult
{
    public string HostName { get; } = string.Empty;

    public Status Status { get; }

    public bool IsSuccess { get; }

    protected DnsResult(string hostname) =>
        (HostName, IsSuccess) = (hostname, true);

    protected DnsResult(Status status) => Status = status;

    public override string ToString() => HostName;
}

public class DnsSuccess(string hostname) :
    DnsResult(hostname)
{

}

public class DnsFailure(Status status, string error) :
    DnsResult(status)
{
    public string ErrorMessage { get; } = error;
}

public sealed class DnsAsync
{
    private readonly Task _cancellation;

    internal DnsAsync(Cancellation cancellation) =>
        _cancellation = cancellation.Task;

    internal async Task<DnsResult> GetHostEntryAsync(
        string host,
        Cancellation cancellation)
    {
        Task task = Task.WhenAny(Dns.GetHostEntryAsync(host), _cancellation);
        if (task == cancellation.Task)
        {
            return new DnsFailure(Status.Timeout, _cancellation.Exception.Message);
        }

        if (task.Status is not TaskStatus.RanToCompletion)
        {
            return new DnsFailure(Status.Error, task.Exception.Message);
        }

        IPHostEntry entry = await (Task<IPHostEntry>)task;
        return new DnsSuccess(entry.HostName);
    }
}
