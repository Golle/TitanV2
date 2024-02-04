using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core.Strings;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
#pragma warning disable CS0660, CS0661
public readonly struct StringRef
#pragma warning restore CS0660, CS0661
{
    public static readonly StringRef Empty = default;
    internal readonly uint Id;
    internal StringRef(uint id) => Id = id;
    public int Length => StringReferences.GetLength(this);
    public static StringRef Create(string str) => StringReferences.GetOrCreate(str);
    public static StringRef Create(ReadOnlySpan<char> str) => StringReferences.GetOrCreate(new string(str));
    public string GetString() => StringReferences.GetString(this);
    public int GetUTF8String(Span<byte> buffer) => StringReferences.GetUTF8String(this, buffer);
    public bool IsValid => Id > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(StringRef lh, StringRef rh) => lh.Id == rh.Id;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(StringRef lh, StringRef rh) => lh.Id != rh.Id;

#if DEBUG
    //NOTE(Jens): We only allow implicit calls to ToString in debug mode. 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => GetString();
#else
    public override string ToString() => string.Empty;

#endif
}


/// <summary>
/// Container for all managed strings that are used by StringRef. This is to make it possible to store a string in unmanaged memory
/// </summary>
internal static class StringReferences
{
    private const uint MaxStrings = 1000;
    private static readonly string[] Strings = new string[MaxStrings];
    private static uint Next = 1;
    static StringReferences()
        => Strings[0] = string.Empty;

    public static StringRef GetOrCreate(string value)
    {
        lock (Strings)
        {
            for (var i = 1u; i < Next; ++i)
            {
                if (Strings[i] == value)
                {
                    return new StringRef(i);
                }
            }

            Debug.Assert(Next + 1 < MaxStrings);
            //NOTE(Jens): We skip the first one so we can use 0 as invalid
            var index = ++Next;
            Strings[index] = value;
            return new StringRef(index);
        }
    }

    public static string GetString(in StringRef str)
    {
        Debug.Assert(str.Id < MaxStrings);
        Debug.Assert(str.IsValid);
        Debug.Assert(Strings[str.Id] != null);
        return Strings[str.Id];
    }

    public static int GetUTF8String(in StringRef str, Span<byte> value)
    {
        // not implemented
        return -1;
    }

    public static int GetLength(in StringRef str)
    {
        Debug.Assert(str.Id < MaxStrings);
        Debug.Assert(str.IsValid);
        Debug.Assert(Strings[str.Id] != null);
        return Strings[str.Id].Length;
    }
}
