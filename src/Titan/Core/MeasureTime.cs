using System.Diagnostics;
using Titan.Core.Logging;

namespace Titan.Core;

/// <summary>
/// Create a new instance of the MeasureTime struct. Messageformat should include a '{0}' to get the time in milliseconds.
/// </summary>
/// <param name="messageFormat">The format of the message to print to Trace. This should include  a '{0}'</param>
public readonly struct MeasureTime<T>(string messageFormat) : IDisposable
{
    private readonly Stopwatch _timer = Stopwatch.StartNew();
    public void Dispose()
    {
        _timer.Stop();
        
        Logger.Trace<T>(string.Format(messageFormat, _timer.Elapsed.TotalMilliseconds));
    }
}
