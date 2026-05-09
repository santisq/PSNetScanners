namespace PSNetScanners.Abstractions;

public abstract class ResultBase(string source, string destination)
{
    public string Source { get; } = source;

    public string Destination { get; } = destination;

    public abstract bool Success { get; }
}
