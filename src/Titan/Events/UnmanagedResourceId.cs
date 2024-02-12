using Titan.Core.Ids;

namespace Titan.Events;

public struct EventId
{
    public static ushort GetNext() => IdGenerator<EventId, ushort, SimpleValueIncrement<ushort>>.GetNext();
}
