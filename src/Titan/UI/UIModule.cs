using Titan.Application;

namespace Titan.UI;

public record UIConfig : IConfiguration, IDefault<UIConfig>
{
    public const uint DefaultMaxElements = 10 * 1024;
    public uint MaxElements { get; init; }

    public static UIConfig Default => new()
    {
        MaxElements = DefaultMaxElements
    };
}
internal class UIModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<UISystem>()
            .AddAssetLoader<Resources.FontLoader>()
            .AddAssetLoader<Resources.SpriteLoader>()
            ;

        return true;
    }
}
