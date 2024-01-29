using Titan.Application;
using Titan.Rendering.D3D12;
using Titan.Rendering.Vulkan;

namespace Titan.Rendering;


public record RenderingConfig : IConfiguration, IDefault<RenderingConfig>
{
#if DEBUG
    private const bool DefaultDebug = true;
#else
    private const bool DefaultDebug = false;
#endif

    public bool Debug { get; init; }

    public static RenderingConfig Default => new ()
    {
        Debug = DefaultDebug
    };
}

internal sealed class RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        if (GlobalConfiguration.Platform == Platforms.Windows)
        {
            builder.AddModule<D3D12Module>();
        }
        else if (GlobalConfiguration.Platform == Platforms.Linux)
        {
            builder.AddModule<VulkanModule>();
        }

        return true;
    }

    public static bool Init(IApp app) => true;
    public static bool Shutdown(IApp app) => true;
}
