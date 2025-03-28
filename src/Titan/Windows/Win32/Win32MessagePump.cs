using Titan.Application.Events;
using Titan.Audio.Events;
using Titan.Core.Logging;
using Titan.Events;
using Titan.Input;
using Titan.Systems;
using Titan.Windows.Win32.Events;

namespace Titan.Windows.Win32;
internal partial struct Win32MessagePump
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
                    writer.Send(new KeyDownEvent(keyDownEvent.Code, keyDownEvent.Repeat));
                    break;
                case EventTypes.KeyUp:
                    
                    ref readonly var keyUp = ref @event.As<Win32KeyUpEvent>();
                    writer.Send(new KeyUpEvent(keyUp.Code));
                    break;
                case EventTypes.CharacterTyped:
                    ref readonly var charTypedEvent = ref @event.As<Win32CharacterTypedEvent>();
                    writer.Send(new CharacterTypedEvent(charTypedEvent.Character));
                    break;
                case EventTypes.Close:
                    Logger.Trace<Win32WindowSystem>("Close message received");
                    writer.Send(new EngineShutdownEvent());
                    break;
                case EventTypes.Quit:
                    Logger.Trace<Win32WindowSystem>("Quit message received!");
                    break;
                case EventTypes.LostFocus:
                    writer.Send(new WindowLostFocusEvent());
                    break;
                case EventTypes.GainedFocus:
                    writer.Send(new WindowGainedFocusEvent());
                    break;
                case EventTypes.AudioDeviceArrival:
                case EventTypes.AudioDeviceRemoveComplete:
                    // Maybe we need a more granular approach later, but for now this will do.
                    writer.Send(new AudioDeviceChangedEvent());
                    break;
                case EventTypes.Resize:
                    ref readonly var resizeEvent = ref @event.As<Win32ResizeEvent>();
                    writer.Send(new WindowResizeEvent(resizeEvent.Width, resizeEvent.Height));
                    break;
                case EventTypes.MouseWheelDelta:
                    ref readonly var wheelDeltaEvent = ref @event.As<Win32MouseWheelEvent>();
                    writer.Send(new MouseWheelDeltaEvent(wheelDeltaEvent.Delta));
                    break;
                default:
                    Logger.Warning<Win32WindowSystem>($"Win32 Message not handled. Id = {@event.Id}");
                    break;
            }
        }
    }
}

