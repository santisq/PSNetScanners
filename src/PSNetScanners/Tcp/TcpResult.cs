using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using PSNetScanners.Abstractions;

namespace PSNetScanners.Tcp;

public sealed class TcpResult : ResultBase
{
    private readonly static SocketException s_timeoutException = new(10060);

    internal string? ClientString
    {
        get => field ??= Client is IPEndPoint ip
            ? ip.Address.ToString()
            : Client?.ToString();
    }

    public int Port { get; }

    public EndPoint? Client { get; private set; }

    public TcpStatus Status { get; private set; }

    public Exception? Error { get; private set; }

    public override bool Success { get => Status == TcpStatus.Opened; }

    private TcpResult(string source, TcpInput input) : base(source, input.Target)
        => Port = input.Port;

    private static TcpResult CreateSuccess(string source, TcpInput input, EndPoint client)
        => new(source, input)
        {
            Status = TcpStatus.Opened,
            Client = client
        };

    private static TcpResult CreateTimeout(string source, TcpInput input)
        => new(source, input)
        {
            Status = TcpStatus.TimedOut,
            Error = s_timeoutException
        };

    private static TcpResult CreateError(string source, TcpInput input, Exception exception)
        => new(source, input)
        {
            Status = TcpStatus.Closed,
            Error = exception
        };

    internal static async Task<TcpResult> CreateAsync(
        string source,
        TcpInput input,
        Cancellation cancellation,
        int timeout)
    {
        try
        {
            using TcpClient tcp = new(input.AddressFamily);
            Task tcpTask = tcp.ConnectAsync(input.Target, input.Port);
            Task result = await Task
                .WhenAny(tcpTask, cancellation.Task, cancellation.GetTimeoutTask(timeout))
                .NoContext();

            if (result == tcpTask)
            {
                await tcpTask.NoContext();
                return CreateSuccess(source, input, tcp.Client.RemoteEndPoint);
            }

            return CreateTimeout(source, input);
        }
        catch (Exception exception)
        {
            return CreateError(source, input, exception);
        }
    }
}
