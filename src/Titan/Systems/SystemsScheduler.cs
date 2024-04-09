using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Events;
using Titan.Resources;
using Titan.Services;

namespace Titan.Systems;

[UnmanagedResource]
internal unsafe partial struct SystemsScheduler
{
    private SystemStageCollection _stages;
    private TitanArray<SystemNode> _nodes;
    private TitanArray<ushort> _dependencies;

    public bool Init(IMemoryManager memoryManager, EventSystem eventSystem, IReadOnlyList<SystemDescriptor> systems, UnmanagedResourceRegistry unmanaged, ServiceRegistry services)
    {
        // Make this configurable. This is during init phase, so it will always be the same for each run when the game is published. Adjust accordingly.
        var systemInitializerSize = MemoryUtils.MegaBytes(2);
        if (!memoryManager.TryCreateBumpAllocator(out var allocator, systemInitializerSize))
        {
            Logger.Error<SystemsScheduler>("Failed to create the bump allocator.");
            return false;
        }
        Span<uint> mutable = stackalloc uint[64];
        Span<uint> readOnly = stackalloc uint[64];

        var systemsCount = systems.Count;
        var builder = new ExecutionTreeBuilder(ref allocator, (uint)systemsCount);
        foreach (var descriptor in systems)
        {
            var initializer = new SystemInitializer(unmanaged, services, eventSystem, mutable, readOnly);
            descriptor.Init(ref initializer);

            builder.AddSystem(descriptor, mutable[..initializer.MutableCount], readOnly[..initializer.ReadOnlyCount]);
            Logger.Trace<SystemsScheduler>($"Initialized system {descriptor.Name.GetString()}. Mutable = {initializer.MutableCount} ReadOnly = {initializer.ReadOnlyCount}");
        }

        if (!builder.TryBuild(ref _stages, ref _nodes, ref _dependencies, memoryManager))
        {
            Logger.Error<SystemsScheduler>($"Failed to build the {nameof(SystemsScheduler)}");
            return false;
        }

        return true;
    }

    public void Shutdown(IMemoryManager memoryManager)
    {
        memoryManager.FreeArray(ref _dependencies);
        memoryManager.FreeArray(ref _nodes);
    }

    public void StartupSystems(IJobSystem jobSystem) => _stages[(int)SystemStage.Startup].Execute(jobSystem);
    public void PreInitSystems(IJobSystem jobSystem) => _stages[(int)SystemStage.PreInit].Execute(jobSystem);

    public void InitSystems(IJobSystem jobSystem) => _stages[(int)SystemStage.Init].Execute(jobSystem);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateSystems(IJobSystem jobSystem)
    {
        for (var i = SystemStage.First; i <= SystemStage.Last; ++i)
        {
            _stages[(int)i].Execute(jobSystem);
        }
    }

    public void ShutdownSystems(IJobSystem jobSystem) => _stages[(int)SystemStage.Shutdown].Execute(jobSystem);

    public void PostShutdownSystems(IJobSystem jobSystem) => _stages[(int)SystemStage.PostShutdown].Execute(jobSystem);

    public void EndOfLifeSystems(IJobSystem jobSystem) => _stages[(int)SystemStage.EndOfLife].Execute(jobSystem);
}
