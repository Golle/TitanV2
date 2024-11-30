using Titan.Application;
using Titan.Editor;
using Titan.UI.Resources;

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
            .AddAssetLoader<FontLoader>()
            .AddAssetLoader<SpriteLoader>()

            //NOTE(Jens): Maybe add a compile time flag here, we don't want debug UI in release builds.
            .AddModule<EditorModule>()
            ;

        return true;
    }
}
