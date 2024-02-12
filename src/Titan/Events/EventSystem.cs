using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Systems;

namespace Titan.Events;

internal unsafe partial class EventSystem : IEventSystem
{
    private UnmanagedResource<EventState> _eventState;
    private IMemoryManager? _memoryManager;

    public bool Init(IMemoryManager memoryManager, EventConfig config, UnmanagedResource<EventState> eventState)
    {
        var eventsPerFrame = config.MaxEventsPerFrame;
        var eventMaxSize = config.MaxEventSize;
        var eventIdSize = sizeof(ushort);
        var stride = eventIdSize + eventMaxSize;
        var totalSize = (uint)(stride * eventsPerFrame * 2);

        Logger.Trace<EventSystem>($"Events per frame = {eventsPerFrame} Event Max Size = {eventMaxSize} Stride = {stride} Total Size = {totalSize} bytes");

        if (!memoryManager.TryAllocBuffer(out var events, totalSize))
        {
            Logger.Error<EventSystem>($"Failed to allocate events buffer Size = {totalSize} bytes");
            return false;
        }
        ref var state = ref eventState.AsRef;

        state = new EventState(eventMaxSize, eventsPerFrame, events)
        {
            Current = new(events.AsPointer()), // Start of the buffer
            Previous = new(events.AsPointer() + stride * eventsPerFrame)// Set the event pointer to the middle of the buffer
        };

        _memoryManager = memoryManager;
        _eventState = eventState;

        return true;
    }

    public void Shutdown()
    {
        ref var state = ref _eventState.AsRef;
        var tempEvents = state.Events;
        _memoryManager?.FreeBuffer(ref tempEvents);
        state = default;
    }


    public EventWriter CreateWriter()
        => new(_eventState.AsPointer);

    public EventReader<T> CreateReader<T>() where T : unmanaged, IEvent
        => new(_eventState.AsPointer);


    public static void Write<T>(in T @event, EventState* eventState) where T : unmanaged, IEvent
    {
        ref var state = ref eventState->Current;
        var offset = Interlocked.Increment(ref state.Count) - 1;

        var eventHeader = (EventHeader*)(state.Data + offset * eventState->Stride);
        eventHeader->Id = T.Id;
        MemoryUtils.Copy(eventHeader->GetDataStart(), MemoryUtils.AsPointer(in @event), sizeof(T));
    }

    internal struct EventHeader
    {
        public ushort Id;
        private byte _dataStart;
        public byte* GetDataStart() => (byte*)Unsafe.AsPointer(ref _dataStart);
    }

    [System(SystemStage.First, SystemExecutionType.Inline)]
    internal static void Update(EventState* eventState)
    {
        (eventState->Current, eventState->Previous) = (eventState->Previous, eventState->Current);
        eventState->Current.Count = 0;

        //TODO(Jens): Swap the current with previous, reset current
        //Logger.Info<EventSystem>($"State: {eventState->A}");
    }
}
