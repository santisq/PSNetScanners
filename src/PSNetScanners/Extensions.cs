using System;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PSNetScanners;

internal static class Extensions
{
    internal static ConfiguredTaskAwaitable NoContext(this Task task) => task.ConfigureAwait(false);

    internal static ConfiguredTaskAwaitable<T> NoContext<T>(this Task<T> task) => task.ConfigureAwait(false);
}
