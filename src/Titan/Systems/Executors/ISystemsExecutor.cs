using Titan.Core;
using Titan.Core.Threading;

namespace Titan.Systems.Executors;

internal interface ISystemsExecutor
{
    static abstract void Run(IJobSystem jobSystem, TitanArray<SystemNode> nodes);
}
