using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PSNetScanners.Dbg;

internal static class Debug
{
    [Conditional("DEBUG")]
    public static void Assert([DoesNotReturnIf(false)] bool condition) =>
        System.Diagnostics.Debug.Assert(condition);
}
