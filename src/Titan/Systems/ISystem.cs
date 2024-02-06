namespace Titan.Systems;

public interface ISystem
{
    static abstract int GetSystems(Span<SystemDescriptor> descriptors);
}
