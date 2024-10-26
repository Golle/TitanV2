using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Titan;
using Titan.Application;
using Titan.Assets;
using Titan.Audio;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.ECS;
using Titan.ECS.Components;
using Titan.Input;
using Titan.Rendering;
using Titan.Rendering.D3D12.Renderers;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Sandbox;
using Titan.Systems;
using Titan.Windows;

using var _ = Logger.Start<ConsoleLogger>(10_000);

var appConfig = new AppConfig("Titan.Sandbox", "0.0.1")
{
    EnginePath = EngineHelper.GetEngineFolder("Titan.sln"),
    ContentPath = EngineHelper.GetContentPath("Titan.Sandbox.csproj", "Assets")
};

App.Create(appConfig)
    .AddModule<GameModule>()
    .AddPersistedConfig(new WindowConfig(1024, 768, true, true))
    .AddPersistedConfig(new RenderingConfig
    {
#if DEBUG
        Debug = true
#endif
    })
    .AddPersistedConfig(new AudioConfig
    {
        Format = AudioFormat.Default,
        Channels = 64
    })
    .AddRegistry<SandboxRegistry>()
    .Build()
    .Run();

namespace Titan.Sandbox
{
    internal class GameModule : IModule
    {
        public static bool Build(IAppBuilder builder, AppConfig config)
        {
            builder
                //.AddSystems<TheGameLightSystem>()
                .AddSystemsAndResource<EntityTestSystem>()
                ;

            return true;
        }
    }

    [UnmanagedResource]
    internal unsafe partial struct EntityTestSystem
    {
        private Entity _entity;
        private bool _done;

        [System]
        public static void TransformFunction(in EntityTestSystem sys, ReadOnlySpan<Entity> entities, Span<Transform3D> transforms, ReadOnlySpan<TransformRect> rects, IMemoryManager memoryManager, in EntityManager entityManager)
        {
            foreach (ref var transform in transforms)
            {
                transform.Position += Vector3.One * 0.1f;
            }
        }

        public static void DrawText(in D3D12TextRenderer renderer)
        {
            renderer.DrawText(Vector2.Zero, "This is my text"u8);
        }

        [System]
        public static void RunMe(ref EntityTestSystem sys, in EntityManager entityManager, AssetsManager assetsManager) => sys.InstanceMethod(entityManager, assetsManager);
        private void InstanceMethod(in EntityManager entityManager, AssetsManager assetsManager)
        {
            if (_done)
            {
                return;
            }

            if (_entity.IsValid)
            {
                //entityManager.DestroyEntity(_entity);
                //entityManager.RemoveComponent<TransformRect>(_entity);
                _entity = default;
                _done = true;
            }
            else
            {
                _entity = entityManager.CreateEntity();
                entityManager.AddComponent(_entity, Transform3D.Create(Vector3.Zero));
                entityManager.AddComponent<TransformRect>(_entity);
                entityManager.AddComponent(_entity, new Mesh
                {
                    Asset = assetsManager.Load<MeshAsset>(EngineAssetsRegistry.Book),
                    TextureAsset = assetsManager.Load<TextureAsset>(EngineAssetsRegistry.BookTexture),
                });


                
                {
                    var lightEntity = entityManager.CreateEntity();
                    entityManager.AddComponent(lightEntity, Transform3D.Create(Vector3.UnitY * 10));
                    entityManager.AddComponent(lightEntity, new Light()
                    {
                        Color = Color.White,
                        Direction = -Vector3.UnitY,
                        Intensity = 100f,
                        Radius = 100f,
                        LightType = LightType.Point
                    });

                }
                {
                    var lightEntity = entityManager.CreateEntity();
                    entityManager.AddComponent(lightEntity, Transform3D.Create(Vector3.UnitZ * 10));
                    entityManager.AddComponent(lightEntity, new Light()
                    {
                        Color = new ColorRGB(0.4f, 0.8f,1f),
                        Direction = -Vector3.UnitY,
                        Intensity = 100f,
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
