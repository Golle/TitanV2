using Titan.Application;

namespace Titan.RenderingV3;

public readonly record struct CommandListHandle(int Index);

internal class RenderingV3Module : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {

        builder
            .AddResource<D3D12Context>()
            .AddSystems<D3D12Backend>();

        return true;
    }
}
