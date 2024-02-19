using Titan.Application.Events;
using Titan.Core.Logging;
using Titan.Events;
using Titan.Systems;
using Titan.Windows.Win32.Events;

namespace Titan.Windows.Win32;
internal unsafe partial struct Win32MessagePump
{
    [System(SystemStage.Last)]
    public static void Update(ref Window window, ref Win32MessageQueue queue, EventWriter writer)
    {
        // Pump the messages and put them on the queue (This is something we might want to do async, maybe start a thread in CreateWindow)

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
                case EventTypes.Close:
                    Logger.Info<Win32WindowSystem>("Close message received");
                    writer.Send(new EngineShutdownEvent());
                    break;
                case EventTypes.Quit:
                    Logger.Info<Win32WindowSystem>("Quit message received!");
                    break;
                default:
                    Logger.Warning<Win32WindowSystem>($"Win32 Message not handled. Id = {@event.Id}");
                    break;
            }
        }
    }
}

