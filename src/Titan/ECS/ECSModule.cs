using Titan.Application;
using Titan.Core.Memory;
using Titan.ECS.Archetypes;

namespace Titan.ECS;

public record ECSConfig(uint MaxEntities, uint MaxArchetypes, uint MaxChunks, uint PreAllocatedChunks, uint MaxCommandComponentSize, uint MaxCommands) : IConfiguration, IDefault<ECSConfig>
{
    public const uint DefaultMaxEntities = 100_000;
    public const uint DefaultMaxChunks = 50_000;
    public const uint DefaultMaxArchetypes = 1024;
    public const uint DefaultPreAllocatedChunks = 1024;

    public static readonly uint DefaultMaxCommandComponentsSize = MemoryUtils.MegaBytes(16);
    public static readonly uint DefaultMaxCommands = 50_000;
    public static ECSConfig Default => new(DefaultMaxEntities, DefaultMaxArchetypes, DefaultMaxChunks, DefaultPreAllocatedChunks, DefaultMaxCommandComponentsSize, DefaultMaxCommands);
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
