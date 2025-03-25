using System.Diagnostics;
using Titan.Core.Logging;

namespace Titan.Core;

/// <summary>
/// Create a new instance of the MeasureTime struct. Messageformat should include a '{0}' to get the time in milliseconds.
/// </summary>
/// <param name="messageFormat">The format of the message to print to Trace. This should include  a '{0}'</param>
public readonly struct MeasureTime<T>(string messageFormat) : IDisposable
{
    private readonly long _timer = Stopwatch.GetTimestamp();
    public void Dispose()
    {
        var diff = Stopwatch.GetElapsedTime(_timer);
        Logger.Trace<T>(string.Format(messageFormat, diff.TotalMilliseconds));
    }
}
