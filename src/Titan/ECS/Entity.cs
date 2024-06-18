using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.ECS;

[DebuggerDisplay("Id: {IdNoVersion} Version: {Version}")]
[StructLayout(LayoutKind.Explicit, Pack = 1)]
public readonly struct Entity
{
    public static readonly unsafe ComponentType Type = new(1, (uint)sizeof(Entity));
    
    internal Entity(uint id, byte version)
    {
        Id = id;
        Version = version;
    }

    [FieldOffset(0)]
    public readonly uint Id;

    [FieldOffset(3)]
    public readonly byte Version;
    public uint IdNoVersion
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id & 0x00ffffff;
    }

    public bool IsValid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Id != 0;
    }

    public static readonly Entity Invalid = default;

    public static implicit operator uint(in Entity entity) => entity.Id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Entity lh, in Entity rh) => lh.Id == rh.Id;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Entity lh, in Entity rh) => lh.Id != rh.Id;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => (int)Id;

    public override bool Equals(object? obj) => throw new InvalidOperationException("Don't use object equals for Entity");
}

