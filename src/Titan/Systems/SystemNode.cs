using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Strings;
using Titan.Core.Threading;

namespace Titan.Systems;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct SystemNode
{
    public JobDescriptor JobDescriptor;
    public TitanArray<ushort> Dependencies;
    public readonly bool HasDependencies => !Dependencies.IsEmpty;

#if DEBUG
    public SystemDescriptor SystemDescriptor;
#endif
}
