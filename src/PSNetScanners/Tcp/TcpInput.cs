using System.Net;
using System.Net.Sockets;

namespace PSNetScanners.Tcp;

public readonly record struct TcpInput
{
    internal string Target { get; }

    internal int Port { get; }

    internal AddressFamily AddressFamily { get; }

    internal TcpInput(string target, int port)
    {
        Target = target;
        Port = port;
        AddressFamily = IPAddress.TryParse(target, out IPAddress ip)
            ? ip.AddressFamily
            : AddressFamily.InterNetwork;
    }
}
