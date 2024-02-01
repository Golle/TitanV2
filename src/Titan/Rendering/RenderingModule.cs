using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Titan.Application;
using Titan.Rendering.D3D12;
using Titan.Rendering.Vulkan;

namespace Titan.Rendering;

public record AdapterConfig(uint DeviceId, uint VendorId);

[JsonSerializable(typeof(RenderingConfig))]
public record RenderingConfig : IConfiguration, IDefault<RenderingConfig>, IPersistable<RenderingConfig>
{
#if DEBUG
    private const bool DefaultDebug = true;
#else
    private const bool DefaultDebug = false;
#endif

    public bool Debug { get; init; }
    public AdapterConfig? Adapter { get; init; }

    public static RenderingConfig Default => new()
    {
        Debug = DefaultDebug
    };

    public static JsonTypeInfo<RenderingConfig> TypeInfo => TitanSerializationContext.Default.RenderingConfig;

    public static string Filename => "rendering.conf";
}

internal sealed class RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddPersistedConfig(RenderingConfig.Default);
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
