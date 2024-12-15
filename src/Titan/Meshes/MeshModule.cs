using Titan.Application;

namespace Titan.Meshes;
internal class MeshModule :IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<MeshSystem>();

        return true;
    }
}
