using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Ids;

namespace Titan.ECS;

[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay("ID: {Id} Size: {Size}")]
public readonly struct ComponentType(uint id, uint size)
{
    public readonly uint Id = id;
    public readonly uint Size = size;
    public bool Contains(in ComponentType type) => (Id % type.Id) == 0; // not sure if this is correct, add tests when we have it working.

    public static unsafe ComponentType GetNext<T>() where T : unmanaged, IComponent
    {
        var id = (uint)IdGenerator<ComponentType, ulong, PrimeNumberIncrement>.GetNext();
        return new(id, (uint)sizeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ulong(in ComponentType type) => type.Id;
#if DEBUG
    public override string ToString() => $"Id: {Id}";
#endif
}
