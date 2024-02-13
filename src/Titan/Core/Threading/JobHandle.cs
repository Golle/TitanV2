using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core.Threading;

[StructLayout(LayoutKind.Explicit, Size = 4, Pack = 1)]
#pragma warning disable CS0660, CS0661
public readonly struct JobHandle
#pragma warning restore CS0660, CS0661
{
    [FieldOffset(0)]
    internal readonly uint Value;
    [FieldOffset(3)]
    internal readonly byte Version;

    internal int ValueWithoutVersion
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)(Value & 0x00ffffff);
    }

    internal JobHandle(int value, byte version)
    {
        Value = (uint)value;
        Version = version;
    }
    public bool IsValid => Value > 0;
    public static readonly JobHandle Invalid = default;

    public static bool operator ==(in JobHandle lhs, in JobHandle rhs) => lhs.Value == rhs.Value;
    public static bool operator !=(in JobHandle lhs, in JobHandle rhs) => lhs.Value != rhs.Value;
}
