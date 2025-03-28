using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Platform.Win32;

namespace Titan.Core.Memory;

public static unsafe class MemoryUtils
{
    public const ulong KiloByte = 1024;
    public const ulong MegaByte = KiloByte * KiloByte;
    public const ulong GigaByte = MegaByte * KiloByte;

    private const uint OneKiloByte = 1024u;

    public static void InitArray<T>(in TitanArray<T> array, byte value = 0) where T : unmanaged
        => Init(array.AsPointer(), array.Length * sizeof(T), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Init(void* mem, long sizeInBytes, byte value = 0)
        => Init(mem, (nuint)sizeInBytes, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Init(void* mem, int sizeInBytes, byte value = 0)
        => Init(mem, (nuint)sizeInBytes, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Init(void* mem, uint sizeInBytes, byte value = 0)
        => Init(mem, (nuint)sizeInBytes, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Init(void* mem, nuint sizeInBytes, byte value = 0)
    {
        Debug.Assert(sizeInBytes < uint.MaxValue, $"Can't clear memory that has a size bigger size than {uint.MaxValue} bytes.");
        Unsafe.InitBlockUnaligned(mem, value, (uint)sizeInBytes);

        //NOTE(Jens): Should we have a platform layer for this? is ZeroMemory/SecureZeroMemory faster on Windows?
        //https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/aa366877(v=vs.85)
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(Span<T> dst, T* src, int length) where T : unmanaged
    {
        Debug.Assert(length >= 0);
        Copy(dst, src, (uint)length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(Span<T> dst, T* src, uint length) where T : unmanaged
    {
        Debug.Assert(length <= dst.Length);
        fixed (T* pDst = dst)
        {
            Copy(pDst, src, (uint)sizeof(T) * length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(void* dst, in ReadOnlySpan<byte> src, uint length)
    {
        Debug.Assert(length <= src.Length);
        fixed (byte* pSrc = src)
        {
            Copy(dst, pSrc, length);
        }
    }

    public static void Copy<T>(Span<T> dst, in ReadOnlySpan<T> src) where T : unmanaged
    {
        Debug.Assert(dst.Length >= src.Length);
        fixed (T* pDst = dst)
        fixed (T* pSrc = src)
        {
            Copy(pDst, pSrc, src.Length * sizeof(T));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy<T>(void* dst, in ReadOnlySpan<T> src) where T : unmanaged
    {
        fixed (T* pSrc = src)
        {
            Copy(dst, pSrc, src.Length * sizeof(T));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(void* dst, in ReadOnlySpan<byte> src)
    {
        fixed (byte* pSrc = src)
        {
            Copy(dst, pSrc, src.Length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(void* dst, void* src, int sizeInBytes)
    {
        Debug.Assert(sizeInBytes >= 0, $"{nameof(sizeInBytes)} >= 0");
        Copy(dst, src, (nuint)sizeInBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(void* dst, void* src, uint sizeInBytes)
        => Copy(dst, src, (nuint)sizeInBytes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(void* dst, void* src, nuint sizeInBytes)
    {
        Debug.Assert(sizeInBytes < uint.MaxValue, $"Can't copy memory that has a size bigger size than {uint.MaxValue} bytes.");
        Unsafe.CopyBlockUnaligned(dst, src, (uint)sizeInBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(TitanBuffer dst, TitanBuffer src, int size)
        => Copy(dst.AsPointer(), src.AsPointer(), size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Copy(TitanBuffer dst, TitanBuffer src, uint size)
        => Copy(dst.AsPointer(), src.AsPointer(), size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* AsPointer<T>(in T value) where T : unmanaged
        => (T*)Unsafe.AsPointer(ref Unsafe.AsRef(in value));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T ToRef<T>(T* ptr) where T : unmanaged
    {
        Debug.Assert(ptr != null);
        return ref *ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T** AddressOf<T>(in T* ptr) where T : unmanaged
    {
        //NOTE(Jens): any risk with doing it like this?
        fixed (T** pptr = &ptr)
        {
            return pptr;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GigaBytes(uint size) => MegaBytes(size) * OneKiloByte;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint MegaBytes(uint size) => KiloBytes(size) * OneKiloByte;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint KiloBytes(uint size) => size * OneKiloByte;

    /// <summary>
    /// Aligns to 8 bytes.
    /// </summary>
    /// <param name="size">The size of the memory block</param>
    /// <returns>The 8 bytes aligned size</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Align(nuint size)
        => size & ~(nuint)7u;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Align(uint size)
        => size & ~7u;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AlignToUpper(int size)
    {
        Debug.Assert(size >= 0);
        return AlignToUpper((uint)size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AlignToUpper(uint size)
    {
        var alignedSize = Align(size);
        return alignedSize < size ? alignedSize + 8u : alignedSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Align(uint size, uint alignment)
        => (uint)Align((nuint)size, alignment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Align(ulong size, uint alignment)
        => (uint)Align((nuint)size, alignment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AlignNotPowerOf2(uint size, uint alignment)
        => (size + alignment - 1) / alignment * alignment;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint Align(nuint size, uint alignment)
    {
        Debug.Assert(nuint.IsPow2(alignment));
        var mask = alignment - 1u;
        var alignedMemory = size & ~mask;
        return alignedMemory;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AlignToUpper(uint size, uint alignment)
        => (uint)AlignToUpper((nuint)size, alignment);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AlignToUpper(ulong size, uint alignment)
    {
        var alignedMemory = Align(size, alignment);
        return alignedMemory < size ? alignedMemory + alignment : alignedMemory;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint AlignToUpper(nuint size, uint alignment)
    {
        var alignedMemory = Align(size, alignment);
        return alignedMemory < size ? alignedMemory + alignment : alignedMemory;
    }

    public static bool Equals<T>(TitanArray<T> lhs, TitanArray<T> rhs) where T : unmanaged
    {
        Debug.Assert(GlobalConfiguration.Platform == Platforms.Windows, "The equals have only been implemented for Windows.");
        if (lhs.Length != rhs.Length)
        {
            return false;
        }

        var size = (uint)sizeof(T);
        var length = lhs.Length;
        return MSVCRT.memcmp(lhs.AsPointer(), rhs.AsPointer(), length * size) == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint SizeOf<T>() where T : unmanaged => (uint)sizeof(T);

    /// <summary>
    /// This should only be used where other allocators can't be used. 
    /// </summary>
    /// <remarks>Uses <see cref="NativeMemory"/> to allocate memory.</remarks>
    /// <param name="size">The bytes</param>
    /// <returns>Pointer to memory allocated</returns>
    internal static void* GlobalAlloc(nuint size)
        => NativeMemory.Alloc(size);

    internal static void GlobalFree(void* ptr)
        => NativeMemory.Free(ptr);
}
