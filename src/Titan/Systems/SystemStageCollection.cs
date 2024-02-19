using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Threading;

namespace Titan.Systems;

[InlineArray((int)SystemStage.Count)]
internal struct SystemStageCollection
{
    private Stage _;
    public readonly unsafe struct Stage(SystemStage stage, TitanArray<SystemNode> nodes, delegate*<IJobSystem, TitanArray<SystemNode>, void> executor)
    {
        public readonly SystemStage Name = stage;
        public uint Count => nodes.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(IJobSystem jobSystem) => executor(jobSystem, nodes);
    }
}
