using Titan.Core.Memory;
using Titan.Core.Strings;

namespace Titan.Resources;

internal readonly unsafe struct UnmanagedResourceDescriptor(uint id, uint size, uint alignedSize, StringRef name)
{
    public readonly uint Id = id;
    public readonly uint Size = size;
    public readonly uint AlignedSize = alignedSize;
    public readonly StringRef Name = name;
    public static UnmanagedResourceDescriptor Create<T>() where T : unmanaged, IResource
        => new(T.Id, (uint)sizeof(T), MemoryUtils.AlignToUpper(sizeof(T)), StringRef.Create(typeof(T).Name));
}
