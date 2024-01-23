using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Titan.Core.IO;

[DebuggerDisplay("{Handle},nb")]
public readonly struct NativeFileHandle
{
    public readonly nuint Handle;
    internal NativeFileHandle(nuint handle) => Handle = handle;
    public bool IsValid() => Handle != 0;
    public bool IsInvalid() => Handle == 0;
    public static readonly NativeFileHandle Invalid = new(0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator nuint(in NativeFileHandle handle) => handle.Handle;

#if DEBUG
    public override string ToString() => Handle.ToString();
#endif
}

