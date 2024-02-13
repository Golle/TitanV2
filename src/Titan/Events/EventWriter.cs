using System.Runtime.CompilerServices;

namespace Titan.Events;

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
