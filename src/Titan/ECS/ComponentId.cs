using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Core.Ids;

namespace Titan.ECS;

[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay("ID: {Value}")]
public readonly struct ComponentId(ulong value)
{
    public readonly ulong Value = value;
    public static ComponentId FromULong(ulong value) => new(value);
    public bool Contains(in ComponentId id) => (Value % id.Value) == 0; // not sure if this is correct, add tests when we have it working.

    public static implicit operator ComponentId(ulong value) => new(value);
    public static implicit operator ulong(ComponentId component) => component.Value;
    public static ComponentId GetNext() => IdGenerator<ComponentId, ulong, PrimeNumberIncrement>.GetNext();


#if DEBUG
    public override string ToString() => $"Id: {Value}";
#endif
}
