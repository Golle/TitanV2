using System.Diagnostics;
using System.Numerics;
using Titan;
using Titan.Application;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS;
using Titan.ECS.Components;
using Titan.Graphics.Pipeline;
using Titan.Graphics.Rendering;
using Titan.Graphics.Rendering.D3D12;
using Titan.Graphics.Resources;
using Titan.Input;
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
        Debug = true
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
                .AddSystems<ATestSystem>()
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
            assetsManager.Load<ShaderInfo>(EngineAssetsRegistry.GBufferShader);

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
                entityManager.AddComponent<Transform3D>(_entity);
                entityManager.AddComponent<TransformRect>(_entity);
                entityManager.AddComponent(_entity, new Mesh3D
                {
                    Asset = assetsManager.Load<MeshAsset>(EngineAssetsRegistry.Book)
                });

                //for (var i = 0; i < 1000; ++i)
                //{
                //    var entity = entityManager.CreateEntity();
                //    entityManager.AddComponent<Transform3D>(entity);
                //    entityManager.AddComponent<TransformRect>(entity);
                //}
            }
        }
    }

    internal partial struct ATestSystem
    {
        private static AssetHandle<MeshAsset> _assetHandle;

        [System(SystemStage.Update, SystemExecutionType.Inline)]
        public static void Update(in InputState inputState)
        {
            if (inputState.IsKeyDown(KeyCode.W))
            {
                Logger.Info<ATestSystem>("Moving forward!");
            }

            if (inputState.IsKeyDown(KeyCode.A))
            {
                Logger.Info<ATestSystem>("Turning left");
            }

            if (inputState.IsKeyDown(KeyCode.D))
            {
                Logger.Info<ATestSystem>("Turning right");
            }

            if (inputState.IsKeyDown(KeyCode.S))
            {
                Logger.Info<ATestSystem>("Moving backwards");
            }
        }

        [System]
        public static void LoadModelTest(AssetsManager assetsManager)
        {
            if (_assetHandle.IsInvalid)
            {
                _assetHandle = assetsManager.Load<MeshAsset>(SandboxRegistry.TileLowRed);
            }
        }
    }
}
