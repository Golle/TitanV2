using Titan.Core.Logging;
using Titan.Systems;

namespace Titan.Windows.Win32;

/// <summary>
/// The Windows events will be processed/read as the last thing in the game loop, we do this so we have the latest information in the next frame minimizing any input latency.
/// <remarks>The event system that this system publish events to will be swapped at the start of the frame</remarks>
/// </summary>
internal class Win32MessagePumpSystem
{
    [System(SystemStage.Last)]
    public static void Update(ref Win32MessageQueue queue)
    {
        if (!queue.HasEvents())
        {
            return;
        }

        if (queue.TryReadEvent(out var @event))
        {
            Logger.Info<Win32MessagePumpSystem>($"Read event of ID {@event.Id}");
        }
    }
}
