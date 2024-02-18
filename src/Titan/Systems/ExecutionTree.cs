using System.Reflection.PortableExecutable;
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

    public void PreInit(IJobSystem jobSystem) => stages[(int)SystemStage.PreInit].Execute(jobSystem);
    public void Init(IJobSystem jobSystem) => stages[(int)SystemStage.Init].Execute(jobSystem);
    public void Update(IJobSystem jobSystem)
    {
        foreach (ref var stage in stages[(int)SystemStage.PreUpdate..(int)SystemStage.PostUpdate])
        {
            stage.Execute(jobSystem);
        }
    }

    public void Shutdown(IJobSystem jobSystem) => stages[(int)SystemStage.Shutdown].Execute(jobSystem);
    public void PostShutdown(IJobSystem jobSystem) => stages[(int)SystemStage.PostShutdown].Execute(jobSystem);

    public void Dispose()
    {
        memoryManager.FreeArray(ref dependencies);
        memoryManager.FreeArray(ref nodes);
    }
}
