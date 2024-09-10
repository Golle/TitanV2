using System.Runtime.InteropServices;
using Titan.Core.Strings;
using Titan.ECS.Archetypes;

namespace Titan.Systems;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SystemDescriptor
{
    public StringRef Name;
    public SystemStage Stage;
    public SystemExecutionType ExecutionType;
    public int Order;
    public delegate*<ref SystemInitializer, void> Init;
    public delegate*<void*, void> Execute;
    public delegate*<CachedQuery*> GetQuery;
}
