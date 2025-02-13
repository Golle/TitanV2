namespace Titan.ECS;



[AttributeUsage(AttributeTargets.Struct)]
public sealed class ComponentAttribute(bool isTag = false) : Attribute
{
    /// <summary>
    /// IsTag property excludes the component from tracking dependencies. This is a read only component and will not be tracked for dependencies.
    /// </summary>
    public bool IsTag { get; } = isTag;
}

public interface IComponent
{
    static abstract ComponentType Type { get; }
    static abstract bool IsTag { get; }
}

public sealed class EntityConfigAttribute: Attribute
{
    public Type[] Not { get; init; } = [];
    public Type[] With { get; init; } = [];
}
