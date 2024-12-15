using System.Runtime.CompilerServices;

namespace Titan.Events;

public readonly unsafe struct EventReader<T> where T : unmanaged, IEvent
{
    private readonly EventState* _state;
    internal EventReader(EventState* state)
    {
        _state = state;
    }

    public bool HasEvents => EventCount != 0;
    /// <summary>
    /// Returns the events in the event queue
    /// <remarks>This will return the count of all events in the queue, not just the type.</remarks>
    /// TODO(Jens): Add support for tracking each type, so we can use this property to check if a system should run or not.
    /// </summary>
    public uint EventCount => _state->Previous.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(_state);
    public ref struct Enumerator
    {
        private readonly EventState* _state;
        private readonly int _count;
        private int _next;

        internal Enumerator(EventState* state)
        {
            _state = state;
            _next = -1;
            _count = (int)state->Previous.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (++_next < _count)
            {
                var header = (EventSystem.EventHeader*)(_state->Previous.Data + _state->Stride * _next);
                if (header->Id == T.Id)
                {
                    return true;
                }
            }

            return false;
        }

        public readonly ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref *(T*)((EventSystem.EventHeader*)(_state->Previous.Data + _state->Stride * _next))->GetDataStart();
        }
    }
}
