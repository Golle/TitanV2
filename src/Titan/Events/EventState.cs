using Titan.Core;
using Titan.Resources;

namespace Titan.Events;

[UnmanagedResource]
internal partial struct EventState(uint stride, uint maxEvents, TitanBuffer buffer)
{
    public readonly uint Stride = stride;
    public readonly uint MaxEvents = maxEvents;
    public readonly TitanBuffer Buffer = buffer;

    public InternalEventState Current;
    public InternalEventState Previous;
}


internal unsafe struct InternalEventState(byte* data)
{
    public readonly byte* Data = data;
    public volatile uint Count;
}
