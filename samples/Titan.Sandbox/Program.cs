using System.Diagnostics;
using System.Numerics;
using Titan;
using Titan.Application;
using Titan.Assets;
using Titan.Audio;
using Titan.Audio.Resources;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.ECS;
using Titan.ECS.Components;
using Titan.Input;
using Titan.Materials;
using Titan.Meshes;
using Titan.Rendering;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Sandbox;
using Titan.Systems;
using Titan.UI;
using Titan.UI.Resources;
using Titan.UI.Widgets;
using Titan.Windows;
using static Titan.Assets.EngineAssetsRegistry;

using var _ = Logger.Start<ConsoleLogger>(10_000);

var appConfig = new AppConfig("Titan.Sandbox", "0.0.1")
{
    EnginePath = EngineHelper.GetEngineFolder("Titan.sln"),
    ContentPath = EngineHelper.GetContentPath("Titan.Sandbox.csproj", "Assets")
};

App.Create(appConfig)
    .AddModule<GameModule>()
    //.AddConfig(ECSConfig.Default with
    //{
    //    MaxEntities = 1_000_000,
    //    MaxCommands = 1_000_000
    //})
    .AddPersistedConfig(new WindowConfig(1920, 1080, true)
    {
        KeepCursorInside = false
    })
    .AddPersistedConfig(new RenderingConfig
    {
#if DEBUG
        Debug = true
#endif
    })
    .AddPersistedConfig(AudioConfig.Default with
    {
        Channels = 64,
        Format = AudioFormat.Default
    })
    .AddRegistry<SandboxRegistry>()
    .Build()
    .Run();

namespace Titan.Sandbox
{

    [Component]
    public partial struct Selected { }
    internal class GameModule : IModule
    {
        public static bool Build(IAppBuilder builder, AppConfig config)
        {
            builder
                //.AddSystems<TheGameLightSystem>()
                .AddSystemsAndResource<EntityTestSystem>()
                .AddSystems<TheAudioThing>()
                .AddSystems<InputStuff>()
                ;

            return true;
        }
    }

    internal unsafe partial struct InputStuff
    {

        [System]
        public static void Update(in Window window, in InputState state)
        {
            if (state.IsKeyReleased(KeyCode.Escape))
            {
                window.Close();
            }
        }
    }

    [UnmanagedResource]
    internal unsafe partial struct EntityTestSystem
    {
        private Entity _entity;
        private bool _done;

        private AssetHandle<TextureAsset> BookTexture;
        private AssetHandle<MeshAsset> BookMesh;

        [System(SystemStage.Init)]
        public static void LoadResources(ref EntityTestSystem system, AssetsManager assetsManager)
        {
            system.BookMesh = assetsManager.LoadMesh(EngineAssetsRegistry.Meshes.Book);
            system.BookTexture = assetsManager.LoadTexture(Textures.BookTexture);
        }

        [System]
        public static void TransformFunction(in EntityTestSystem sys, ReadOnlySpan<Entity> entities, Span<Transform3D> transforms, ReadOnlySpan<TransformRect> rects, IMemoryManager memoryManager, in EntityManager entityManager)
        {
            foreach (ref var transform in transforms)
            {
                //transform.Position += Vector3.One * 0.1f;
            }
        }

        [System]
        public static void RunMe(ref EntityTestSystem sys, in EntityManager entityManager, AssetsManager assetsManager, MaterialsManager materialsManager, MeshManager meshManager) => sys.InstanceMethod(entityManager, assetsManager, materialsManager, meshManager);
        private void InstanceMethod(in EntityManager entityManager, AssetsManager assetsManager, in MaterialsManager materialsManager, in MeshManager meshManager)
        {
            if (_done)
            {
                return;
            }

            if (_entity.IsValid)
            {
                //entityManager.DestroyEntity(_entity);
                //entityManager.RemoveComponent<TransformRect>(_entity);
                //_entity = default;
                _done = true;
            }
            else if (assetsManager.IsLoaded(BookMesh) && assetsManager.IsLoaded(BookTexture))
            {
                _entity = entityManager.CreateEntity();
                entityManager.AddComponent(_entity, Transform3D.Create(Vector3.UnitZ * 10 + Vector3.UnitY * -3));
                entityManager.AddComponent<TransformRect>(_entity);

                var bookMaterial = materialsManager.CreateMaterial(new()
                {
                    Color = Color.White,
                    AlbedoTexture = assetsManager.Get(BookTexture)
                });
                entityManager.AddComponent(_entity, new Mesh
                {
                    //TODO(Jens): Replace this mesh thing with a Create mesh
                    MeshIndex = assetsManager.Get(BookMesh),
                    MaterialIndex = bookMaterial
                });


                for (var z = 0; z < 5; ++z)
                    for (var j = 0; j < 5; ++j)
                        for (var i = 0; i < 5; ++i)
                        {
                            var ent = entityManager.CreateEntity();

                            entityManager.AddComponent(ent, new Selected());
                            entityManager.AddComponent(ent, Transform3D.Create(Vector3.UnitZ * 10 * (j + 1) + Vector3.UnitY * 10 * (z + 1) + Vector3.UnitX * 10 * (i + 1)));

                            entityManager.AddComponent(ent, new Mesh
                            {
                                //TODO(Jens): Replace this mesh thing with a Create mesh
                                MeshIndex = assetsManager.Get(BookMesh),
                                MaterialIndex = bookMaterial
                            });
                        }

                entityManager.AddComponent(_entity, new Selected());

                {
                    var lightEntity = entityManager.CreateEntity();
                    entityManager.AddComponent(lightEntity, Transform3D.Create(Vector3.UnitY * 10));
                    entityManager.AddComponent(lightEntity, new Light
                    {
                        Active = true,
                        Color = Color.White,
                        Direction = -Vector3.UnitY,
                        Intensity = 0,
                        Radius = 100f,
                        LightType = LightType.Point
                    });

                }
                {
                    var lightEntity = entityManager.CreateEntity();
                    entityManager.AddComponent(lightEntity, Transform3D.Create(Vector3.UnitZ * 10));
                    entityManager.AddComponent(lightEntity, new Light()
                    {
                        Active = true,
                        Color = new ColorRGB(0.4f, 0.8f, 1f),
                        Direction = -Vector3.UnitY,
                        Intensity = 0,
                        Radius = 100f,
                        LightType = LightType.Point
                    });

                }

                //for (var i = 0; i < 1000; ++i)
                //{
                //    var entity = entityManager.CreateEntity();
                //    entityManager.AddComponent<Transform3D>(entity);
                //    entityManager.AddComponent<TransformRect>(entity);
                //}
            }
        }
    }

    internal partial struct TheAudioThing
    {
        private static UITextBoxStyle _textboxStyle;
        private static UICheckboxStyle _checkboxStyle;

        private static UISliderState _slider = new() { Value = 0.5f };
        private static UISliderState _slider2;
        private static UISliderState _slider3;
        private static UISliderStyle _sliderStyle;

        private static UIRadioState _radio;
        private static UIRadioStyle _radioStyle;

        private static UISelectBoxState _selectBox;
        private static UISelectBoxStyle _selectBoxStyle;

        private static UIProgressBarState _progressBar;
        private static UIProgressBarStyle _progressBarStyle;

        private static Inline4<UICheckboxState> _checkboxStates;
        private static AssetHandle<SpriteAsset> _sprite2;

        private static Inline8<AssetHandle<AudioAsset>> _uiEffects;
        private static AssetHandle<AudioAsset> _music;
        private static AssetHandle<FontAsset> _font;
        private static AssetHandle<FontAsset> _font2;
        private static Inline32<byte> _text;

        public static Inline8<UIID> Ids;
        public static Inline3<UIID> RadioIds;
        public static UIID SelectBoxID = UIID.Create();
        public static UIID SliderID = UIID.Create();
        public static UIID SliderID2 = UIID.Create();
        public static UIID SliderID3 = UIID.Create();
        private static bool _playing = false;

        private static string[] Modes = [];

        [System(SystemStage.Init)]
        public static void Init(AssetsManager assetsManager, in Window window)
        {
            UIID.Create(RadioIds);

            UICheckboxState.Create(_checkboxStates);

            _music = assetsManager.Load<AudioAsset>(Audios.BackgroundMusic);
            _uiEffects[0] = assetsManager.Load<AudioAsset>(Audios.Click1);
            _uiEffects[1] = assetsManager.Load<AudioAsset>(Audios.Click2);
            _uiEffects[2] = assetsManager.Load<AudioAsset>(Audios.Click3);
            _uiEffects[3] = assetsManager.Load<AudioAsset>(Audios.Click4);
            _uiEffects[4] = assetsManager.Load<AudioAsset>(Audios.Click5);

            _font = assetsManager.Load<FontAsset>(Fonts.CutiveMonoRegular);
            _font2 = assetsManager.Load<FontAsset>(Fonts.SyneMonoRegular);

            _sprite2 = assetsManager.Load<SpriteAsset>(SandboxRegistry.Sprites.UiStyleOrange.Asset);

            _checkboxStyle = new UICheckboxStyle
            {
                CheckboxAsset = assetsManager.Load<SpriteAsset>(SandboxRegistry.Sprites.UiStyleOrange.Asset),
                UncheckedIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Checkbox1,
                CheckedIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Checkbox1Checked,
                HoverIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Checkbox1Down
            };

            _textboxStyle = new()
            {
                DefaultIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Input1,
                SelectedIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Input1Selected,
                FontAsset = assetsManager.Load<FontAsset>(Fonts.CutiveMonoRegular),
                SpriteAsset = assetsManager.Load<SpriteAsset>(SandboxRegistry.Sprites.UiStyleOrange.Asset)
            };

            _sliderStyle = new()
            {
                AssetHandle = assetsManager.Load<SpriteAsset>(SandboxRegistry.Sprites.UiStyleOrange.Asset),
                SliderIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Slider1Blob,
                SliderSelectedIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Slider1BlobBlack,
                BackgroundIndexCenter = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Slider1,
                SliderSize = new(32, 32)
            };

            _radioStyle = new()
            {
                AssetHandle = assetsManager.Load<SpriteAsset>(SandboxRegistry.Sprites.UiStyleOrange.Asset),
                Radio = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Radio1,
                RadioSelected = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Radio1Selected,
                RadioHover = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Radio1Hover
            };
            _selectBoxStyle = new()
            {
                SpriteHandle = assetsManager.Load<SpriteAsset>(SandboxRegistry.Sprites.UiStyleOrange.Asset),
                FontHandle = assetsManager.Load<FontAsset>(Fonts.SyneMonoRegular),
                BackgroundIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Input1,
                HoverIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Input1Selected,
            };

            _progressBarStyle = new()
            {
                AssetHandle = assetsManager.Load<SpriteAsset>(SandboxRegistry.Sprites.UiStyleOrange.Asset),
                BackgroundIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Progress1,
                BarIndex = SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Progress1Bar
            };

            _timer = Stopwatch.StartNew();

            var modes = EngineSettings.GraphicAdapters[0].Outputs[0].GetModes();
            Modes = new string[modes.Length];
            for (var i = 0; i < modes.Length; ++i)
            {
                Modes[i] = new string(modes[i].GetDescription());

                if (modes[i].Width == window.Width && modes[i].Height == window.Height)
                {
                    _screenStates[1].SelectedIndex = (byte)i;
                }
            }
        }


        private static Inline64<byte> FpsCounter;
        public static int frameCount;
        public static int fpsSize;
        private static Stopwatch _timer;



        private static Inline2<UISelectBoxState> _screenStates;
        
        [System]
        public static void ScreenSize(UIManager ui, in Window window)
        {
            const int offset = 900;
            ui.SelectBox(1001, new Vector2(100, offset + 80), new(250, 33), ["Window", "Borderless Fullscreen"], ref _screenStates[0], _selectBoxStyle);
            if (_screenStates[0].SelectedIndex == 0)
            {
                ui.SelectBox(1002, new Vector2(100, offset), new(250, 33), Modes, ref _screenStates[1], _selectBoxStyle);
            }

            if (ui.Button(1003, new Vector2(380, offset + 82), new SizeF(100, 30), Color.Magenta))
            {
                ref readonly var mode = ref EngineSettings.GraphicAdapters[0].Outputs[0].Modes[_screenStates[1].SelectedIndex];
                window.Resize(mode.Width, mode.Height);
            }
        }

        [System]
        public static void Update(AssetsManager assetsManager, AudioManager audioManager, in InputState inputState, in UIManager ui)
        {

            frameCount++;

            if (_timer.Elapsed.TotalSeconds > 1f)
            {
                var fps = frameCount / _timer.Elapsed.TotalSeconds;

                fps.TryFormat(FpsCounter, out fpsSize);
                //Logger.Info<TitanApp>($"FPS: {fps}");
                frameCount = 0;
                _timer.Restart();
            }



            //if (ui.Button(new(100, 200), new(200, 100), Color.Green))
            //{
            //    Logger.Error("button 1 pressed");
            //}

            //if (ui.Button(new(350, 200), new(200, 100), Color.White with { A = 0.6f }))
            //{
            //    Logger.Error("button 2 pressed");
            //    audioManager.PlayOnce(_music, new(Loop: true));
            //}

            //if (ui.Button(new(600, 200), new(200, 100), Color.FromRGB(0xbd8c2a)))
            //{
            //    Logger.Error("button 3 pressed");
            //    audioManager.PlayOnce(_uiEffects[1]);

            //}

            if (assetsManager.IsLoaded(_font) && assetsManager.IsLoaded(_font2))
            {
                ref readonly var font = ref assetsManager.Get(_font);
                ref readonly var font2 = ref assetsManager.Get(_font2);

                if (fpsSize > 0)
                {
                    ui.Text(new(200, 700), FpsCounter.AsSpan().Slice(0, fpsSize), font);
                }
                ui.Text(new(200, 500), "The Big Red Tulip"u8, font);
                ui.Text(new(200, 600), "The Quick Brown Fox"u8, font2);

                //ui.Text(new(110, 220), "Click"u8, font);

                //ui.Image(new(300, 400), _sprite, index);
                int y = 350;
                int x = 200;
                for (var i = 0; i < 4; ++i)
                {
                    ui.Checkbox(new(x + 50 * i, y - 10 * i), new(36, 36), ref _checkboxStates[i], _checkboxStyle);
                }

                var state = new UICheckboxState();

                if (state)
                {
                    // do something
                }

                if (_checkboxStates[3])
                {
                    ui.TextBox(Ids[4], new Vector2(100, 50), new SizeF(49 * 4, 8 * 4), _text.AsSpan(), _textboxStyle);
                }

                //ui.Image(new (20, 120), _sprite2, SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Slider1);
                //ui.Image(new (20, 150), _sprite2, SandboxRegistry.Sprites.UiStyleOrange.SpriteIndex.Slider1Blob);


                ui.Slider(SliderID, new(20, 120), new(200, 36), ref _slider, _sliderStyle);
                ui.Slider(SliderID2, new(20, 220), new(200, 36), ref _slider2, _sliderStyle);
                ui.Slider(SliderID3, new(20, 320), new(200, 36), ref _slider3, _sliderStyle);


                int min = 43;
                int max = 120;
                Inline32<byte> text = default;
                var value = Math.Round(_slider.Value * (max - min)) + min;
                value.TryFormat(text.AsSpan(), out var size);
                ui.Text(new(240, 110), text.AsReadOnlySpan()[..size], font2, Color.Green);

                for (var i = 0; i < 3; ++i)
                {
                    ui.Radio(RadioIds[i], i, new(500, 36 * i + 30), new(32, 32), ref _radio, _radioStyle);
                }

                ui.SelectBox(SelectBoxID, new(600, 200), new SizeF(144, 33), ["My first", "My second", "My third"], ref _selectBox, _selectBoxStyle);

                _progressBar.Value = _slider.Value;
                ui.ProgressBar(new Vector2(600, 400), new SizeF(46, 8) * 5.0f, ref _progressBar, _progressBarStyle);
            }

            //audioManager.PlayOnce(_music, new PlaybackSettings());
            if (!_playing && assetsManager.IsLoaded(_music) && inputState.IsKeyPressed(KeyCode.M))
            {
                audioManager.PlayOnce(_music, new(Loop: true));
                _playing = true;
            }

            PlaySound(KeyCode.One, audioManager, inputState);
            PlaySound(KeyCode.Two, audioManager, inputState);
            PlaySound(KeyCode.Three, audioManager, inputState);
            PlaySound(KeyCode.Four, audioManager, inputState);
            PlaySound(KeyCode.Five, audioManager, inputState);
            PlaySound(KeyCode.Six, audioManager, inputState);
            PlaySound(KeyCode.Seven, audioManager, inputState);

            static void PlaySound(KeyCode code, in AudioManager audioManager, in InputState inputState)
            {
                if (inputState.IsKeyPressed(code))
                {
                    audioManager.PlayOnce(_uiEffects[(int)code - 49]);
                }
            }
        }

        [System]
        public static void UpdateScale(ReadOnlySpan<Selected> _, Span<Transform3D> transforms)
        {
            var scale = _slider.Value;
            foreach (ref var transform in transforms)
            {
                transform.Scale = Vector3.One * scale;
                transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 2 * MathF.PI * _slider2.Value);
            }
        }

        [System]
        public static void UpdateLights(Span<Light> lights)
        {
            for (var i = 0; i < lights.Length; ++i)
            {
                lights[i].Active = _checkboxStates[i];
                lights[i].Color.R = _slider3.Value;
            }
        }

    }
    internal partial struct TheGameLightSystem
    {
        //[System]
        public static void Update(in InputState inputState, Span<Light> lights)
        {
            ColorRGB color = Color.White;

            if (inputState.IsKeyDown(KeyCode.One))
            {
                color = Color.Red;
            }
            else if (inputState.IsKeyDown(KeyCode.Two))
            {
                color = Color.Green;
            }
            else if (inputState.IsKeyDown(KeyCode.Three))
            {
                color = Color.Blue;
            }

            foreach (ref var light in lights)
            {
                light.Color = color;
            }
        }
    }
}
