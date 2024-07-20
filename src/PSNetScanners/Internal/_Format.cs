using System.ComponentModel;
using System.Management.Automation;

namespace PSNetScanners.Internal;

#pragma warning disable IDE1006

[EditorBrowsable(EditorBrowsableState.Never)]
public static class _Format
{
    [Hidden, EditorBrowsable(EditorBrowsableState.Never)]
    public static string FormatLatency(long latency) =>
        string.Format("{0} ms", latency);
}
