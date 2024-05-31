namespace Titan.ECS;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class ComponentAttribute : Attribute;

public interface IComponent
{
    static abstract ComponentId ComponentId { get; }
}
