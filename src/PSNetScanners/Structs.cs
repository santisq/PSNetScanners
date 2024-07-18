using System.Management.Automation;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PSNetScanners;

internal record struct PingAsyncOptions(
    PingOptions PingOptions,
    int ThrottleLimit,
    int TaskTimeout,
    byte[] Buffer,
    bool ResolveDns);

internal record struct Output(Type Type, object Data)
{
    internal static Output CreateSuccess(object Data) =>
        new(Type.Success, Data);

    internal static Output CreateError(ErrorRecord error) =>
        new(Type.Error, error);
}

internal readonly record struct TcpInput
{
    internal string Source { get; }

    internal string Target { get; }

    internal int Port { get; }

    internal AddressFamily AddressFamily { get; }

    internal TcpInput(string source, string target, int port)
    {
        Source = source;
        Target = target;
        Port = port;
        AddressFamily = IPAddress.TryParse(target, out IPAddress ip)
            ? ip.AddressFamily
            : AddressFamily.InterNetwork;
    }

    internal void Deconstruct(
        out string source,
        out string target,
        out int port)
    {
        source = Source;
        target = Target;
        port = Port;
    }

}
