using Titan.Core.Logging;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows.Win32.Events;

namespace Titan.Windows.Win32;

/// <summary>
/// The Windows events will be processed/read as the last thing in the game loop, we do this so we have the latest information in the next frame minimizing any input latency.
/// <remarks>The event system that this system publish events to will be swapped at the start of the frame</remarks>
/// </summary>
internal partial struct Win32MessagePumpSystem
{
    [System(SystemStage.Last)]
    public static void Update(ref Win32MessageQueue queue, IWindow window)
    {
        if (!queue.HasEvents())
        {
            return;
        }

        var count = queue.EventCount;
        for (var i = 0; i < count; ++i)
        {
            if (!queue.TryReadEvent(out var @event))
            {
                break;
            }
            
            switch (@event.Id)
            {
                case EventTypes.KeyDown:
                    ref readonly var keyDownEvent = ref @event.As<Win32KeyDownEvent>();
                    Logger.Info($"Key Down: {keyDownEvent.Code} (Repeat = {keyDownEvent.Repeat})");
                    break;
                case EventTypes.KeyUp:
                    ref readonly var keyUpEvent = ref @event.As<Win32KeyUpEvent>();
                    Logger.Info($"Key Down: {keyUpEvent.Code}");
                    break;
                default:
                    Logger.Warning<Win32MessagePumpSystem>($"Win32 Message not handled. Id = {@event.Id}");
                    break;
            }
        }
    }
}
