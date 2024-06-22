using Titan.Events;

namespace Titan.ECS.Events;

[Event]
public partial record struct EntityDestroyedEvent(Entity Entity);
