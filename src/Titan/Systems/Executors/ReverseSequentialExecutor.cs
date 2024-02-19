using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Threading;

namespace Titan.Systems.Executors;

internal class ReverseSequentialExecutor : ISystemsExecutor
{
    [SkipLocalsInit]
    public static void Run(IJobSystem jobSystem, TitanArray<SystemNode> nodes)
    {
        var count = (int)nodes.Length;
        if (count == 0)
        {
            return;
        }
        for (var i = count - 1; i >= 0; --i)
        {
            ref var node = ref nodes[i];
            if (node.ExecutionType is SystemExecutionType.Check or SystemExecutionType.InlineCheck)
            {
                //TODO(Jens): Enable this when we have support for check conditions.
                //if (!node.ShouldRun())
                //{
                //    continue;
                //}
            }
            node.Execute();
        }
    }
}
