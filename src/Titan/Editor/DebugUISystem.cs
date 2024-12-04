using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using Titan.Assets;
using Titan.Core.Maths;
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

    private static UISliderState _sliderState;
    private static UISliderStyle _sliderStyle;

    public static UIImageStyle _testStyle;
    private static UIID _sliderID = UIID.Create();

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

        _sliderStyle = new()
        {
            AssetHandle = _uiSprite,
            SliderSize = new(32, 32),
            BackgroundIndexCenter = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderCenter,
            BackgroundIndexLeft = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderLeft,
            BackgroundIndexRight = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderRight,
            BackgroundIndexEmptyRight = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderEmptyRight,
            BackgroundIndexEmptyCenter = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderEmptyCenter,
            BackgroundIndexEmptyLeft = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderEmptyLeft,
            SliderIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderBlob,
            SliderSelectedIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderBlob
        };
    }

    private static float _rand;
    [System]
    public static void Update(UIManager ui)
    {
        _testStyle.NinePatch = UIImageStyleNinePatch.FromValue(24);
        //var timer = Stopwatch.StartNew();
        ////ui.Image(new(900, 200), new(200, 150), _testStyle);
        //timer.Stop();
        var timer = TimeSpan.FromMilliseconds(123.123);
        _rand = Random.Shared.Next(100,1999);
        Span<byte> textbuffer = stackalloc byte[32];
        _rand.TryFormat(textbuffer, out var bytesWritten);
        ui.Text(new(900, 700), textbuffer[..bytesWritten], _font, Color.Green);
        //ui.Text(new(900, 700), "Hey"u8, _font);
        
        return;

        ui.Slider(_sliderID, new Vector2(1000, 300), new SizeF(200, 32), ref _sliderState, _sliderStyle);
        
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(AssetsManager assetsManager)
    {
        assetsManager.Unload(ref _uiSprite);

    }
}
