using Titan.Application;
using Titan.ECS.Archetypes;

namespace Titan.ECS;

public record ECSConfig(uint MaxEntities, uint PreAllocatedChunks) : IConfiguration, IDefault<ECSConfig>
{
    public const uint DefaultMaxEntities = 10_000;
    public const uint DefaultPreAllocatedChunks = 1024;
    public static ECSConfig Default => new(DefaultMaxEntities, DefaultPreAllocatedChunks);
}

internal sealed class ECSModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<EntitySystem>()
            .AddSystemsAndResource<ComponentSystem>();
        return true;
    }
}
