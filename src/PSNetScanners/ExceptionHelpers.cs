using System;
using System.Management.Automation;

namespace PSNetScanners;

internal static class ExceptionHelpers
{
    internal static ErrorRecord CreateProcessing(this Exception exception, object context) =>
        new(exception, errorId: "ProcessingTaskFailure", ErrorCategory.ConnectionError, context);
}
