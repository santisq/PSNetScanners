using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PSNetScanners;

public sealed class TcpResult
{
    public string Source { get; }

    public string Destination { get; }

    public int Port { get; }

    public TcpStatus Status { get; }

    public Exception? Details { get; }

    private TcpResult(
        TcpInput input,
        TcpStatus status,
        Exception? details = null)
    {
        (Source, Destination, Port) = input;
        Status = status;
        Details = details;
    }

    private static TcpResult CreateSuccess(TcpInput input) =>
        new(input, TcpStatus.Opened);

    private static TcpResult CreateTimeout(TcpInput input) =>
            new(input, TcpStatus.TimedOut, new TimeoutException());

    private static TcpResult CreateError(TcpInput input, Exception exception) =>
        new(input, TcpStatus.Closed, exception);

    internal static async Task<TcpResult> CreateAsync(
        TcpInput input,
        Cancellation cancellation,
        int timeout)
    {
        try
        {
            using TcpClient tcp = new(input.AddressFamily);
            Task tcpTask = tcp.ConnectAsync(input.Target, input.Port);
            Task result = await Task.WhenAny(
                tcpTask,
                cancellation.Task,
                cancellation.GetTimeoutTask(timeout));

            if (result == tcpTask)
            {
                await tcpTask;
                return CreateSuccess(input);
            }

            return CreateTimeout(input);
        }
        catch (Exception exception)
        {
            return CreateError(input, exception);
        }
    }
}
