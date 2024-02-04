using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Windows.Win32.Events;

[StructLayout(LayoutKind.Sequential, Size = Win32EventMaxSize, Pack = 1)]
internal struct Win32Event
{
    public const int Win32EventMaxSize = 32;
    public int Id;
    public readonly unsafe void* DataStartPtr => (byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in this)) + sizeof(uint);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe ref readonly T As<T>() where T : unmanaged, IWin32Event
    {
        Debug.Assert(T.Id == Id, $"Trying to cast the Win32Event to the wrong type. Event = {Id} TargetId = {T.Id}");
        return ref *(T*)DataStartPtr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Is<T>() where T : unmanaged, IWin32Event => Id == T.Id;
}
