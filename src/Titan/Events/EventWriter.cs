using System.Runtime.CompilerServices;
using static Titan.Events.EventSystem;

namespace Titan.Events;
public readonly unsafe struct EventReader<T> where T : unmanaged, IEvent
{
    private readonly EventState* _state;
    internal EventReader(EventState* state)
    {
        _state = state;
    }

    public bool HasEvents => EventCount != 0;
    public uint EventCount => _state->Previous.Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(_state);
    public ref struct Enumerator
    {
        private readonly EventState* _state;
        private int _next;
        internal Enumerator(EventState* state)
        {
            _state = state;
            _next = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            var count = _state->Previous.Count;
            while (++_next < count)
            {
                var header = (EventHeader*)(_state->Previous.Data + _state->Stride * _next);
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
            get => ref *(T*)((EventHeader*)(_state->Previous.Data + _state->Stride * _next))->GetDataStart();
        }
    }



}
public readonly unsafe struct EventWriter
{
    private readonly EventState* _state;
    internal EventWriter(EventState* state)
    {
        _state = state;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Send<T>(in T @event) where T : unmanaged, IEvent => EventSystem.Write(@event, _state);
}
