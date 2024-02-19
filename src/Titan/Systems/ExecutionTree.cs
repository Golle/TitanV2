using Titan.Core;
using Titan.Core.Memory;
using Titan.Core.Threading;

namespace Titan.Systems;

internal struct ExecutionTree(IMemoryManager memoryManager, SystemStageCollection stages, TitanArray<SystemNode> nodes, TitanArray<ushort> dependencies) : IDisposable
{
    //NOTE(Jens): This struct is basically redundant, this information should be stored inside the scheduler. and the cleanup should be handled by the scheduler. Refactor later.

    public void PreInit(IJobSystem jobSystem) => stages[(int)SystemStage.PreInit].Execute(jobSystem);
    public void Init(IJobSystem jobSystem) => stages[(int)SystemStage.Init].Execute(jobSystem);
    public void Update(IJobSystem jobSystem)
    {
        for (var i = SystemStage.First; i <= SystemStage.Last; ++i)
        {
            stages[(int)i].Execute(jobSystem);
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
