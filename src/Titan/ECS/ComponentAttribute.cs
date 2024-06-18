namespace Titan.ECS;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class ComponentAttribute : Attribute;

public interface IComponent
{
    static abstract ComponentType Type { get; }
}

public sealed class EntityConfigAttribute: Attribute
{
    public Type[] Not { get; init; } = [];
    public Type[] With { get; init; } = [];
}
