using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Core.Threading;
using Titan.Systems.Executors;

namespace Titan.Systems;

file enum DependencyType
{
    None,
    OneWay,
    TwoWay,
}
internal unsafe ref struct ExecutionTreeBuilder
{
    private ref BumpAllocator _allocator;
    private readonly TitanArray<SystemInstance> _systems;
    private uint _count;
    private uint _totalDependencies;
    private fixed int _stageCounter[(int)SystemStage.Count];
    public ExecutionTreeBuilder(ref BumpAllocator allocator, uint systemCount)
    {
        allocator.Reset(true);
        _systems = allocator.AllocateArray<SystemInstance>(systemCount);
        _allocator = ref allocator;
    }

    public void AddSystem(in SystemDescriptor descriptor, Span<uint> mutableResources, Span<uint> readOnlyResources)
    {
        ref var system = ref _systems[_count++];
        system.Descriptor = descriptor;
        system.Mutable = _allocator.AllocateArray<uint>(mutableResources.Length);
        system.ReadOnly = _allocator.AllocateArray<uint>(readOnlyResources.Length);

        mutableResources.CopyTo(system.Mutable.AsSpan());
        readOnlyResources.CopyTo(system.ReadOnly.AsSpan());
        _totalDependencies += system.Mutable.Length;
        _totalDependencies += system.ReadOnly.Length;
        _stageCounter[(int)system.Descriptor.Stage]++;
    }

    public bool TryBuild(ref SystemStageCollection stages, ref TitanArray<SystemNode> nodes, ref TitanArray<ushort> dependencies, IMemoryManager memoryManager)
    {
        var systemCount = _count;

        _systems.AsSpan().Sort(SortSystems);

        if (!memoryManager.TryAllocArray(out nodes, systemCount))
        {
            Logger.Error($"Failed to allocate memory. Type = {nameof(SystemNode)} Count = {systemCount} Size = {systemCount * sizeof(SystemNode)} bytes", typeof(ExecutionTreeBuilder));
            return false;
        }

        if (!memoryManager.TryAllocArray(out dependencies, _totalDependencies))
        {
            Logger.Error($"Failed to allocate memory. Type = {nameof(UInt16)} Size = {systemCount * sizeof(ushort)}", typeof(ExecutionTreeBuilder));
            return false;
        }

        {

            var depenciesOffset = 0u;
            Span<ushort> tempDependencies = stackalloc ushort[(int)systemCount]; // temp array for storing dependencies
            for (var outer = 0; outer < systemCount; ++outer)
            {
                var dependencyCount = 0;
                ref var node = ref nodes[outer];
                ref var outerSystem = ref _systems[outer];
                node.JobDescriptor = JobDescriptor.Create(outerSystem.Descriptor.Execute, null, false);
                node.ExecutionType = outerSystem.Descriptor.ExecutionType;
#if DEBUG
                node.SystemDescriptor = outerSystem.Descriptor;
#endif

                for (var inner = 0; inner < systemCount; ++inner)
                {
                    // Don' compare with self
                    if (outer == inner)
                    {
                        continue;
                    }

                    ref var innerSystem = ref _systems[inner];

                    // don't check for dependencies between stages. that could potentially cause a deadlock and doesn't really make any sense anyway.
                    if (outerSystem.Descriptor.Stage != innerSystem.Descriptor.Stage)
                    {
                        continue;
                    }

                    var dependencyType = CheckDependency(outerSystem, innerSystem);
                    if (dependencyType is DependencyType.OneWay or DependencyType.TwoWay)
                    {
                        if (IsCircular(nodes, inner, outer))
                        {
                            Logger.Warning<SystemsScheduler>($"System {outer} has a circular dependency to {inner}. The system will not be added to the dependency list and will be executed before the other system");
                        }
                        else
                        {
                            tempDependencies[dependencyCount++] = (ushort)inner;
                        }
                    }
                }

                if (dependencyCount > 0)
                {

                    node.Dependencies = dependencies.Slice(depenciesOffset, (uint)dependencyCount);
                    tempDependencies[..dependencyCount].CopyTo(node.Dependencies.AsSpan());

                    depenciesOffset += (uint)dependencyCount;
#if DEBUG
                    //Logger.Trace<SystemsScheduler>($"{system.Stage}: System {system.Id} has {dependencyCount} dependencies");
                    //for (var a = 0; a < system.DependenciesCount; ++a)
                    //{
                    //    Logger.Trace<SystemsScheduler>($"\tDependency {systems[system.Dependencies[a]].Id}");
                    //}
#endif
                }
            }
        }

        var offset = 0u;
        for (var i = 0; i < (int)SystemStage.Count; ++i)
        {
            var numberOfNodes = (uint)_stageCounter[i];
            Logger.Trace($"Stage {(SystemStage)i}. Systems Count = {numberOfNodes}", typeof(ExecutionTreeBuilder));

            //NOTE(Jens): Startup and EndOfLife are excuted in the order they are registered. This is to allow certain things to happen in order without having dependencies.
            delegate*<IJobSystem, TitanArray<SystemNode>, void> executor = (SystemStage)i switch
            {
                SystemStage.Startup => &SequentialExecutor.Run,
                SystemStage.EndOfLife => &ReverseSequentialExecutor.Run,
                _ => &OrderedSystemsExecutor.Run
            };
            var stageNodes = nodes.Slice(offset, numberOfNodes);
            // The dependency index is in the full array of systems, this is an optimization to adjust the index to the current stage. 
            AdjustDependencyIndices(stageNodes, offset);
            stages[i] = new SystemStageCollection.Stage((SystemStage)i, stageNodes, executor);

            offset += numberOfNodes;
        }

        return true;

        static void AdjustDependencyIndices(TitanArray<SystemNode> nodes, uint offset)
        {
            foreach (ref var node in nodes.AsSpan())
            {
                for (var i = 0; i < node.Dependencies.Length; ++i)
                {
                    node.Dependencies[i] -= (ushort)offset;
                }
            }
        }

        static DependencyType CheckDependency(in SystemInstance inner, in SystemInstance outer)
        {
            // Check if the inner systems ReadOnly references has a match in the outer systems Mutable resources. in this case it's a one way dependency, Inner depends on Outer.
            if (inner.ReadOnly.ContainsAny(outer.Mutable))
            {
                return DependencyType.OneWay;
            }

            //Check if both systems are mutating the same resource, in that case it's a TwoWay dependency. And the priority will determine the order they are run.
            if (inner.Mutable.ContainsAny(outer.Mutable))
            {
                return DependencyType.TwoWay;
            }

            // No dependencies between these systems.
            return DependencyType.None;
        }
    }

    private static bool IsCircular(in TitanArray<SystemNode> nodes, int current, int id)
    {
        ref readonly var node = ref nodes[current];

        foreach (var dependency in node.Dependencies.AsReadOnlySpan())
        {
            if (dependency == id)
            {
                return true;
            }
            if (IsCircular(nodes, dependency, id))
            {
                return true;
            }
        }
        return false;
    }


    private static int SortSystems(SystemInstance x, SystemInstance y)
    {
        var stageDiff = x.Descriptor.Stage - y.Descriptor.Stage;
        if (stageDiff != 0)
        {
            return stageDiff;
        }

        //TODO(Jens): Add priority  check, we currently don't support priority but we will eventually.
        return 1;
    }

    private struct SystemInstance
    {
        public SystemDescriptor Descriptor;
        public TitanArray<uint> Mutable;
        public TitanArray<uint> ReadOnly;
    }
}
