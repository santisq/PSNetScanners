using System;

namespace PSNetScanners.Ping;

public sealed class PingResultException : Exception
{
    private readonly PingResult _context;

    internal PingResultException(PingResult pingResult, Exception exception)
        : base(
            $"Failed to resolve host '{pingResult.Destination}'. {exception.Message}",
            exception)
    {
        _context = pingResult;
    }

    internal PingResult GetContext() => _context;
}
