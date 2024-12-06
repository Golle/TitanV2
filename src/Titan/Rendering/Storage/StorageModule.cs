using Titan.Application;

namespace Titan.Rendering.Storage;
internal class StorageModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<MeshStorage>()
            .AddSystemsAndResource<LightStorage>()
            ;

        return true;
    }
}
