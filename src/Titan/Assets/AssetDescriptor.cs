using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Assets.Types;
using Titan.Core.Ids;

namespace Titan.Assets;
[DebuggerDisplay("Id = {Value}")]
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public readonly struct AssetId
{
    public readonly uint Value;
    private AssetId(uint value) => Value = value;
    public static AssetId GetNext() => new(IdGenerator<AssetId, uint, SimpleValueIncrement<uint>>.GetNext());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator uint(in AssetId id) => id.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(in AssetId id) => unchecked((int)id.Value);
}

[DebuggerDisplay("Id = {Value}")]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct RegistryId
{
    public readonly byte Value;
    private RegistryId(byte value) => Value = value;
    public static RegistryId GetNext() => new(IdGenerator<RegistryId, byte, SimpleValueIncrement<byte>>.GetNext());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator byte(in RegistryId id) => id.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator sbyte(in RegistryId id) => unchecked((sbyte)id.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(in RegistryId id) => id.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator uint(in RegistryId id) => id.Value;
}

public struct AssetDescriptor
{
    public AssetId Id;
    public RegistryId RegistryId;

    public AssetType Type;
    public FileDescriptor File;
    public AssetDependencies Dependencies;
    private AssetDescriptorUnion _descriptors;
    [UnscopedRef]
    public ref Texture2DDescriptor Texture2D => ref _descriptors.Texture2D;
    [UnscopedRef]
    public ref MeshDescriptor Mesh => ref _descriptors.Mesh;
    [UnscopedRef]
    public ref ShaderDescriptor Shader => ref _descriptors.Shader;
    [UnscopedRef]
    public ref AudioDescriptor Audio => ref _descriptors.Audio;

    [UnscopedRef]
    public ref FontDescriptor Font => ref _descriptors.Font;

    /// <summary>
    /// All supported built in Asset descriptors
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    private struct AssetDescriptorUnion
    {
        [FieldOffset(0)]
        public Texture2DDescriptor Texture2D;
        [FieldOffset(0)]
        public MeshDescriptor Mesh;
        [FieldOffset(0)]
        public ShaderDescriptor Shader;
        [FieldOffset(0)]
        public AudioDescriptor Audio;
        [FieldOffset(0)]
        public FontDescriptor Font;
    }
}
