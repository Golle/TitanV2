using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Strings;
using Titan.Core.Threading;
using Titan.Events;
using Titan.Resources;
using Titan.Services;

namespace Titan.Systems;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SystemDescriptor
{
    public StringRef Name;
    public SystemStage Stage;
    public SystemExecutionType ExecutionType;
    public delegate*<ref SystemInitializer, void> Init;
    public delegate*<void*, void> Execute;
}

internal sealed unsafe class SystemsScheduler : ISystemsScheduler
{
    private ExecutionTree _executionTree;
    private IJobSystem? _jobSystem;
    public bool Init(IMemoryManager memoryManager, IJobSystem jobSystem, IEventSystem eventSystem, ImmutableArray<SystemDescriptor> systems, IUnmanagedResources unmanaged, IManagedServices services)
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
        // This should be per stage, but keep it simple now.
        var systemsCount = systems.Length;
        var builder = new ExecutionTreeBuilder(ref allocator, (uint)systemsCount);
        foreach (var descriptor in systems)
        {
            var initializer = new SystemInitializer(unmanaged, services, eventSystem, mutable, readOnly);
            descriptor.Init(ref initializer);

            builder.AddSystem(descriptor, mutable[..initializer.MutableCount], readOnly[..initializer.ReadOnlyCount]);
            Logger.Trace<SystemsScheduler>($"Initialized system {descriptor.Name.GetString()}. Mutable = {initializer.MutableCount} ReadOnly = {initializer.ReadOnlyCount}");
        }

        if (!builder.TryBuild(out _executionTree, memoryManager))
        {
            Logger.Error<SystemsScheduler>($"Failed to build the {nameof(ExecutionTree)}");
            return false;
        }

        _jobSystem = jobSystem;
        return true;
    }


    public void Shutdown()
    {
        _executionTree.Dispose();
    }

    public void Execute()
    {
        Debug.Assert(_jobSystem != null);
        _executionTree.Run(_jobSystem);
    }
}
