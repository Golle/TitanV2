using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Titan.Core.Strings;

namespace Titan.Systems;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SystemDescriptor
{
    public StringRef Name;
    public SystemStage Stage;
    public SystemExecutionType ExecutionType;
    public delegate*<ref SystemInitializer, void> Init;
    public delegate*<void*, void> Execute;
}
