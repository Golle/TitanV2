using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core.Logging;
using Titan.Resources;
using Titan.Windows.Win32.Events;

namespace Titan.Windows.Win32;

[UnmanagedResource]
internal unsafe partial struct Win32MessageQueue
{
    public const int Win32MaxEventCount = 1024;
    private Win32Events _events;
    private volatile int _head;
    private volatile int _tail;
    private volatile int _eventCount;

    public readonly int EventCount => _eventCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasEvents() => _tail != _head;
    public void Push<T>(in T @event) where T : unmanaged, IWin32Event
    {

        Debug.Assert(sizeof(T) <= Win32Event.Win32EventMaxSize);

        if (_eventCount >= Win32MaxEventCount - 1)
        {
            Logger.Warning<Win32MessageQueue>($"Win32 Message queue is full. The event of type {typeof(T).Name} will be discarded. Id = {T.Id}");
            return;
        }

        //NOTE(Jens): This might not be needed since we'll have a single producer and single consumer of Win32 Events.
        while (true) // Add iteration check?
        {
            var current = _head;
            var index = Interlocked.CompareExchange(ref _head, (current + 1) % Win32MaxEventCount, current);
            // Some other thread updated the counter, do another lap
            if (index != current)
            {
                continue;
            }

            ref var e = ref _events[index];
            e.Id = T.Id;
            *(T*)e.DataStartPtr = @event;
            Interlocked.Increment(ref _eventCount);
            break;
        }
    }

    public bool TryReadEvent(out Win32Event @event)
    {
        Unsafe.SkipInit(out @event);
        while (HasEvents())
        {
            var current = _tail;
            var index = Interlocked.CompareExchange(ref _tail, (current + 1) % Win32MaxEventCount, current);
            if (index != current)
            {
                continue;
            }

            @event = _events[index];
            Interlocked.Decrement(ref _eventCount);
            return true;
        }
        return false;
    }

    [InlineArray(Win32MaxEventCount)]
    private struct Win32Events
    {
        private Win32Event _event;
    }
}
