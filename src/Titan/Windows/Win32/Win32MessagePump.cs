using System.Runtime.CompilerServices;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Input;
using Titan.Platform.Win32;
using Titan.Windows.Win32.Events;

namespace Titan.Windows.Win32;

internal sealed unsafe class Win32MessagePump : IService
{
    private Win32MessageQueue* _queue;
    private IMemorySystem? _memorySystem;

    public bool Init(IMemorySystem memorySystem)
    {
        _queue = memorySystem.Alloc<Win32MessageQueue>();
        if (_queue == null)
        {
            Logger.Error<Win32MessagePump>($"Failed to allocate memory for {nameof(Win32MessageQueue)}. Size = {sizeof(Win32MessageQueue)}");
            return false;
        }

        return true;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool OnMessage(HWND hwnd, WindowMessage message, nuint wParam, nuint lParam)
    {
        if (message == WindowMessage.WM_CLOSE)
        {
            User32.PostQuitMessage(0);
            return true;
        }

        switch (message)
        {
            case WindowMessage.WM_KEYDOWN:
                var win32KeyDownEvent = new Win32KeyDownEvent(KeyCode.A, false);
                _queue->Push(win32KeyDownEvent);
                break;
        }
        return false;
    }


    public bool TryReadEvent(out Win32Event @event) 
        => _queue->TryReadEvent(out @event);

    public void Update()
    {
        var count = _queue->EventCount;
        for (var i = 0; i < count; ++i)
        {
            if (!_queue->TryReadEvent(out var @event))
            {
                break;
            }

            Logger.Info<Win32MessagePump>($"REad event Id = {@event.Id}");
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
                    Logger.Warning<Win32MessagePump>($"Win32 Message not handled. Id = {@event.Id}");
                    break;
            }
        }
    }

    public void Shutdown()
    {
        _memorySystem?.Free(_queue);
        _memorySystem = null;
    }
}

internal interface IWin32Event
{
    static abstract int Id { get; }
}
