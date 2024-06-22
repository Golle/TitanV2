using System.Diagnostics;
using System.Numerics;
using Titan;
using Titan.Application;
using Titan.Assets;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS;
using Titan.ECS.Components;
using Titan.Graphics.Rendering;
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
                .AddResource<Res1>()
                .AddResource<Res2>()
                .AddResource<Res3>()
                ;
                
            return true;
        }
    }

    [UnmanagedResource]
    internal partial struct Res1
    {
        public int A;
    }
    [UnmanagedResource]
    internal partial struct Res2
    {
        public int A;
    }
    [UnmanagedResource]
    internal partial struct Res3
    {
        public int A;
    }

    [UnmanagedResource]
    internal unsafe partial struct EntityTestSystem
    {
        private Entity _entity;
        private bool _done;


        [System]
        public static void CircularA(Res1* res, in Res2 res2) { }
        [System]
        public static void CircularB(Res2* res, in Res3 res3) { }
        [System]
        public static void CircularC(Res3* res, in Res1 res1) { }


        [System]
        public static void TransformFunction(in EntityTestSystem sys, ReadOnlySpan<Entity> entities, Span<Transform3D> transforms, ReadOnlySpan<TransformRect> rects, IMemoryManager memoryManager, in EntityManager entityManager)
        {
            foreach (ref var transform in transforms)
            {
                transform.Position += Vector3.One * 0.1f;
            }
        }

        [System]
        public static void RunMe(ref EntityTestSystem sys, in EntityManager entityManager) => sys.InstanceMethod(entityManager);
        private void InstanceMethod(in EntityManager entityManager)
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
                entityManager.AddComponent<Transform3D>(_entity);
                entityManager.AddComponent<TransformRect>(_entity);

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
        public static void LoadModelTest(IAssetsManager assetsManager)
        {
            if (_assetHandle.IsInvalid)
            {
                _assetHandle = assetsManager.Load<MeshAsset>(SandboxRegistry.TileLowRed);
            }
        }
    }
}
