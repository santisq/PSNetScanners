using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using PSNetScanners.Abstractions;

namespace PSNetScanners.Tcp;

public sealed class TcpResult : ResultBase
{
    private readonly static SocketException s_timeoutException = new(10060);

    internal string? ClientString { get; set; }

    public int Port { get; }

    public EndPoint? Client
    {
        get;
        private set
        {
            field = value;
            ClientString = value is IPEndPoint ip ? ip.Address.ToString() : value?.ToString();
        }
    }

    public TcpStatus Status { get; private set; } = TcpStatus.Opened;

    public override bool Success { get => Status == TcpStatus.Opened; }

    private TcpResult(string source, TcpInput input)
        : base(source, input.Target)
    {
        Port = input.Port;
    }

    internal static async Task<TcpResult> CreateAsync(
        string source,
        TcpInput input,
        Cancellation cancellation,
        int timeout)
    {
        Task cancellationTask = cancellation.Task;
        Task timeOutTask = cancellation.GetTimeoutTask(timeout);
        TcpResult tcpResult = new(source, input);
        using TcpClient tcp = new(input.AddressFamily);

        try
        {
            Task tcpTask = tcp.ConnectAsync(input.Target, input.Port);
            Task any = await Task.WhenAny(tcpTask, cancellationTask, timeOutTask).NoContext();

            if (any == tcpTask)
            {
                await tcpTask.NoContext();
                tcpResult.Client = tcp.Client.RemoteEndPoint;
                return tcpResult;
            }

            tcpResult.Status = TcpStatus.TimedOut;
            tcpResult.Error = s_timeoutException;
            return tcpResult;
        }
        catch (Exception exception)
        {
            tcpResult.Status = TcpStatus.Closed;
            tcpResult.Error = exception;
            return tcpResult;
        }
    }
}
