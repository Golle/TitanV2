namespace Titan.Events;

public interface IEvent
{
    static abstract ushort Id { get; }
}
