using System;

namespace PSNetScanners.Ping;

public sealed class PingResultException : Exception
{
    internal PingResultException(PingResult pingResult, Exception exception)
        : base(
            $"Failed to resolve host '{pingResult.Destination}'. {exception.Message}",
            exception)
    { }
}
