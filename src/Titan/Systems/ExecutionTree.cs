using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Memory;
using Titan.Core.Threading;

namespace Titan.Systems;

[InlineArray((int)SystemStage.Count)]
internal struct SystemStageCollection
{
    private Stage _;
    public readonly unsafe struct Stage(TitanArray<SystemNode> nodes, delegate*<IJobSystem, TitanArray<SystemNode>, void> executor)
    {
        public uint Count => nodes.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(IJobSystem jobSystem) => executor(jobSystem, nodes);
    }
}

internal struct ExecutionTree(IMemoryManager memoryManager, SystemStageCollection stages, TitanArray<SystemNode> nodes, TitanArray<ushort> dependencies) : IDisposable
{
    //NOTE(Jens): This struct is basically redundant, this information should be stored inside the scheduler. and the cleanup should be handled by the scheduler. Refactor later.
    public void Run(IJobSystem jobSystem)
    {
        var i = 0;
        foreach (ref var stage in stages)
        {
            //Logger.Info<ExecutionTree>($"Execute stage: {(SystemStage)i++} System Count: {stage.Count}");
            stage.Execute(jobSystem);
        }
    }

    public void Dispose()
    {
        memoryManager.FreeArray(ref dependencies);
        memoryManager.FreeArray(ref nodes);
    }
}
