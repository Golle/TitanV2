using Titan.Application;

namespace Titan.Materials;
internal class MaterialsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddSystemsAndResource<MaterialsSystem>();

        return true;
    }
}
