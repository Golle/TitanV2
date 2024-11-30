using System.Diagnostics;
using System.Linq.Expressions;
using Titan.Assets;
using Titan.Resources;
using Titan.Systems;
using Titan.UI;
using Titan.UI.Resources;
using Titan.UI.Widgets;

namespace Titan.Editor;

internal partial struct DebugUISystem
{
    private static AssetHandle<FontAsset> _font;
    private static AssetHandle<SpriteAsset> _uiSprite;

    public static UIImageStyle _testStyle;

    [System(SystemStage.Init)]
    public static void Init(AssetsManager assetsManager)
    {
        Debug.Assert(_uiSprite.IsInvalid);
        _uiSprite = assetsManager.Load<SpriteAsset>(EngineAssetsRegistry.Sprites.DebugUiStyle.Asset);
        _font = assetsManager.Load<FontAsset>(EngineAssetsRegistry.Fonts.CutiveMonoRegular);
        _testStyle = new UIImageStyle
        {
            Sprite = _uiSprite,
            NinePatch = UIImageStyleNinePatch.FromValue(8),
            Index = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.FirstNinePatch,
            IsNinePatch = true
        };

    }

    [System]
    public static void Update(UIManager ui)
    {
        _testStyle.NinePatch = UIImageStyleNinePatch.FromValue(24);
        var timer = Stopwatch.StartNew();
        ui.Image(new(900, 200), new(200, 150), _testStyle);
        timer.Stop();

        Span<byte> textbuffer = stackalloc byte[32];
        timer.Elapsed.TotalMilliseconds.TryFormat(textbuffer, out var bytesWritten);
        ui.Text(new(900, 700), textbuffer[..bytesWritten], _font);
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(AssetsManager assetsManager)
    {
        assetsManager.Unload(ref _uiSprite);

    }
}
