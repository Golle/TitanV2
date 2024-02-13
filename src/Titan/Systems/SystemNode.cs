using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Threading;

namespace Titan.Systems;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct SystemNode
{
    public JobDescriptor JobDescriptor;
    public TitanArray<ushort> Dependencies;
    public readonly bool HasDependencies => !Dependencies.IsEmpty;
    public SystemExecutionType ExecutionType;
#if DEBUG
    public SystemDescriptor SystemDescriptor;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe void Execute() => JobDescriptor.Callback(JobDescriptor.Context);
}
