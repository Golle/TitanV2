using System.Runtime.InteropServices;

namespace Titan.Assets;

/// <summary>
/// Internal structure for <see cref="IAssetRegistry"/> to idenfity dependencies
/// </summary>
/// <param name="index">The start index in the underlying array</param>
/// <param name="count">The number of dependencies</param>
[StructLayout(LayoutKind.Explicit, Size = 4)]
public readonly struct AssetDependencies(uint index, byte count)
{
    [FieldOffset(0)]
    private readonly uint _index = index;
    public uint Index => _index & 0x00ffffff;

    [FieldOffset(3)]
    public readonly byte Count = count;
}
