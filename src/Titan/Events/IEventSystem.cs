namespace Titan.Events;

public interface IEventSystem : IService
{
    EventWriter CreateWriter();
    EventReader<T> CreateReader<T>() where T : unmanaged, IEvent;
}
