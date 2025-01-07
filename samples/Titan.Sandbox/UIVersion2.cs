using System.Numerics;
using Titan.Assets;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Systems;
using Titan.UI;
using Titan.UI.Resources;

namespace Titan.Sandbox;
internal partial struct UIVersion2
{

    private static AssetHandle<SpriteAsset> Sprite;
    [System(SystemStage.Init)]
    public static void Init(AssetsManager assetsManager)
    {
        Sprite = assetsManager.Load<SpriteAsset>(EngineAssetsRegistry.Sprites.DebugUiStyle.Asset);
    }


    [System]
    public static void Update1(UIManager uiManager)
    {
        {
            var context = uiManager.GiveMe();
            context.Begin(1);

            //context.Box(new(1000, 100), new(200, 400), Color.Magenta);
            //context.Sprite(new(300, 300), new(200, 200), Sprite, EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.Button01);

            if (context.Button(1, "My button"u8, Vector2.One * 400, new(100, 100), Color.White))
            {
                Logger.Error<UIVersion2>("Rolf!");
            }


            context.End();
        }
    }

    [System]
    public static void Update2(UIManager uiManager)
    {
        {
            var context = uiManager.GiveMe();
            context.Begin(2);
            if (context.Button(2, "My button"u8, new(430, 460), new(100, 100), Color.White))
            {
                Logger.Error<UIVersion2>("Rolf!");
            }

            context.End();
        }
    }

    [System]
    public static void Update(UIManager uiManager)
    {
        {

            var context = uiManager.GiveMe();
            context.Begin(3);
            if (context.Button(3, "My button"u8, new(470,400), new(100, 100), Color.White))
            {
                Logger.Error<UIVersion2>("Rolf!");
            }

            context.End();
        }


    }
}
