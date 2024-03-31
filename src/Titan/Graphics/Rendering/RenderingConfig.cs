using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Titan.Graphics.Rendering;

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
    public bool VSync { get; init; }
    public bool AllowTearing { get; init; }
    public AdapterConfig? Adapter { get; init; }
    public static RenderingConfig Default => new()
    {
        Debug = DefaultDebug,
        VSync = true,
        AllowTearing = false
    };

    public static JsonTypeInfo<RenderingConfig> TypeInfo => TitanSerializationContext.Default.RenderingConfig;

    public static string Filename => "rendering.conf";
}
