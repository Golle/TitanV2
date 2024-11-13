using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[SkipLocalsInit]
[StructLayout(LayoutKind.Sequential)]
public struct HWND
{
    public nint Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nint(HWND hwnd) => hwnd.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HWND(int hwnd) => new() { Value = hwnd };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HWND(nint hwnd) => new() { Value = hwnd };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HWND(nuint hwnd) => new() { Value = (nint)hwnd };
    public bool IsValid => Value != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => Value.ToString();

    public static readonly HWND HWND_TOP = 0;
    public static readonly HWND HWND_BOTTOM = 1;
    public static readonly HWND HWND_TOPMOST = -1;
    public static readonly HWND HWND_NOTOPMOST = -2;

}
