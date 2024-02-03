using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS;

namespace Titan.Modules;

public record ECSConfig(uint MaxEntities) : IConfiguration, IDefault<ECSConfig>
{
    public const uint DefaultMaxEntities = 10_000;
    public static ECSConfig Default => new(DefaultMaxEntities);
}

internal sealed class ECSModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddService<IEntityManager, EntityManager>(new EntityManager());
        return true;
    }

    public static bool Init(IApp app)
    {
        var entityManager = app.GetService<EntityManager>();
        var memorySystem = app.GetService<IMemorySystem>();
        var config = app.GetConfigOrDefault<ECSConfig>();

        if (!entityManager.Init(memorySystem, config))
        {
            Logger.Error<ECSModule>($"Failed to init the {nameof(EntityManager)}.");
            return false;
        }

        return true;
    }

    public static bool Shutdown(IApp app)
    {
        app.GetService<EntityManager>()
            .Shutdown();

        return true;
    }
}
