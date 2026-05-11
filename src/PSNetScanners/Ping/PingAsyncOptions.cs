using System.Net.NetworkInformation;

namespace PSNetScanners.Ping;

internal record struct PingAsyncOptions(
    PingOptions PingOptions,
    int ThrottleLimit,
    int TaskTimeout,
    byte[] Buffer,
    bool ResolveDns);
