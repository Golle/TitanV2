using System.Runtime.CompilerServices;

namespace Titan.Events;

public static class EventReaderExtensions
{
    /// <summary>
    /// Helper methods to support checking only if the reader contains events. This is a workaround for HasEvents returning true if there are ANY events in the queue.
    /// </summary>
    /// <returns>True if queue contains events of type <see cref="T"/> </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Any<T>(this EventReader<T> reader) where T : unmanaged, IEvent
    {
        foreach (ref readonly var _ in reader)
        {
            return true;
        }

        return false;
    }
}
