using System.Net;
using System.Net.Sockets;

namespace PSNetScanners.Tcp;

public readonly record struct TcpInput
{
    internal readonly string Target;
    internal readonly int Port;
    internal readonly AddressFamily AddressFamily = AddressFamily.InterNetwork;

    internal TcpInput(string target, int port)
    {
        Target = target;
        Port = port;
        if (IPAddress.TryParse(target, out IPAddress ip)) AddressFamily = ip.AddressFamily;
    }
}
