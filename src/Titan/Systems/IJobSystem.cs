using System.Runtime.InteropServices;
using Titan.Core.Strings;

namespace Titan.Systems;


public ref struct SystemInitializer
{
    
}


[StructLayout(LayoutKind.Sequential)]
public unsafe struct SystemDescriptor
{
    public StringRef Name;
    public delegate*<SystemInitializer, void> Init;
    public delegate*<void> Execute;
}

public interface IJobSystem
{
    static abstract int GetJobs(Span<SystemDescriptor> descriptors);
}
