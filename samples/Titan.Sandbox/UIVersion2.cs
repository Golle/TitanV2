using System.Diagnostics;
using System.Numerics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Systems;
using Titan.UI;
using Titan.UI.Resources;
using Titan.UI2;

namespace Titan.Sandbox;
internal partial struct UIVersion2
{

    private static AssetHandle<SpriteAsset> Sprite;
    [System(SystemStage.Init)]
    public static void Init(AssetsManager assetsManager)
    {
        Sprite = assetsManager.Load<SpriteAsset>(EngineAssetsRegistry.Sprites.DebugUiStyle.Asset);
    }

    private static UISliderState2 _state;
    private static UICheckboxState2 _checkboxState;

    private static Inline32<char> Text1;
    private static Inline32<char> Text2;

    private static int Text1Count;
    private static int Text2Count;
    [System]
    public static void Update1(UIManager uiManager)
    {
        {
            var context = uiManager.GiveMe();
            context.Begin(1);

            //context.Box(new(1000, 100), new(200, 400), Color.Magenta);
            //context.Sprite(new(300, 300), new(200, 200), Sprite, EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.Button01);

            if (context.Button(1, "My button", Vector2.One * 400, new(100, 100), Color.White, Color.Black))
            {
                Logger.Error<UIVersion2>("Rolf!");
            }

            context.Label(new Vector2(600, 600), new SizeF(300, 300), "This is sparta", Color.White);

            context.Slider(23, new(600, 260), new(220, 50), Color.White, ref _state);

            context.End();
        }
    }

    [System]
    public static void Update2(UIManager uiManager)
    {
        {
            var context = uiManager.GiveMe();
            context.Begin(2);
            if (context.Button(2, "My button", new(430, 460), new(100, 100), Color.White, Color.Black))
            {
                Logger.Error<UIVersion2>("Rolf!");
            }

            context.Checkbox(1212, new(100, 500), new(64, 64), Color.White, ref _checkboxState, context.Style.Checkbox with
            {
                CheckmarkIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.CheckboxCross
            });
            context.TextBox(63, new(700, 100), new(300, 40), Text1, ref Text1Count);
            context.TextBox(64, new(700, 150), new(300, 36), Text2, ref Text2Count);

            context.End();
        }
    }

    [System]
    public static void Update(UIManager uiManager)
    {
        var context = uiManager.GiveMe();
        {
            context.Begin(3);
            if (context.Button(3, "My button", new(470, 400), new(100, 100), Color.White, Color.Black))
            {
                Logger.Error<UIVersion2>("Rolf!");
            }


            //NOTE(Jens): I like this design, but it will be hard to create the Selected element.. not sure how to do that.
            var timer = Stopwatch.StartNew();
            //if (context.SelectBox(543, new(800, 700), new(140, 32), out var state))
            //{
            //    context.SelectBoxItem(ref state, "Item1");
            //    context.SelectBoxItem(ref state, "Item2");
            //    context.SelectBoxItem(ref state, "Item3");
            //    context.SelectBoxItem(ref state, "Item4");
            //    context.SelectBoxItem(ref state, "Item5");
            //    context.SelectBoxItem(ref state, "Item6");
            //    context.SelectBoxItem(ref state, "Item7");
            //    context.SelectBoxItem(ref state, "Item8");
            //    context.SelectBoxItem(ref state, "Item9");
            //    context.SelectBoxItem(ref state, "Item10");
            //}

            timer.Stop();
            //Logger.Error($"Elapsed: {timer.Elapsed.Microseconds} microseconds");
            context.End();
        }
    }
}
