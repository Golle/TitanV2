using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Strings;
using Titan.Resources;
using Titan.Services;

namespace Titan.Systems;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SystemDescriptor
{
    public StringRef Name;
    public delegate*<SystemInitializer, void> Init;
    public delegate*<void> Execute;
}

internal sealed unsafe class SystemsScheduler : ISystemsScheduler
{
    private ImmutableArray<SystemDescriptor> _TEMPsystems;
    public bool Init(IMemorySystem memorySystem, ImmutableArray<SystemDescriptor> systems, IUnmanagedResources unmanaged, IManagedServices services)
    {
        Span<uint> mutable = stackalloc uint[64];
        Span<uint> readOnly = stackalloc uint[64];

        for (var i = 0; i < systems.Length; ++i)
        {
            var descriptor = systems[i];

            var initializer = new SystemInitializer(unmanaged, services, mutable, readOnly);
            descriptor.Init(initializer);

            Logger.Trace<SystemsScheduler>($"Initialized system {descriptor.Name.GetString()}. Mutable = {initializer.MutableCount} ReadOnly = {initializer.ReadOnlyCount}");
        }

        _TEMPsystems = systems;

        return true;
    }


    public void Shutdown()
    {

    }

    public void Execute()
    {
        //NOTE(Jens): This is just temporary to get the execution up and running.
        foreach (var systemDescriptor in _TEMPsystems)
        {
            systemDescriptor.Execute();
        }
    }
}
