using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Titan.Core.Memory.Allocators;

/// <summary>
/// The general purpose allocator will allow any size allocations (8 byte aligns).
/// It uses best-fit (O(N)) and a free list to decide where to put the allocation, making it slower than a more restrictive allocator.
/// Uses this for temporary(or permanent) big buffers that last several frames or use this as the base for other Allocators like Linear/Stack/Pool that is smaller than 4kb.
/// </summary>
public unsafe struct GeneralAllocator : IAllocator
{
    private const uint MinBlockSize = 64;
    private static readonly uint HeaderSize = (uint)sizeof(Header);

    private VirtualMemoryBlock _memoryBlock;
    private Header* _allocations;
    private Header* _lastAllocation;

    internal GeneralAllocator(VirtualMemoryBlock memoryBlock)
    {
        //NOTE(Jens): we can add a initial size if needed, but we would probably need a create function with a bool return type then.
        _memoryBlock = memoryBlock;
    }

    public TitanBuffer AllocBuffer(uint size, bool initialize = false)
    {
        var mem = Alloc(size, initialize);
        return new(mem, size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TitanArray<T> AllocArray<T>(uint count, bool initialize = false) where T : unmanaged
    {
        var mem = Alloc<T>(count, initialize);
        return new(mem, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAllocArray<T>(out TitanArray<T> array, uint count, bool initialize = false) where T : unmanaged
    {
        Unsafe.SkipInit(out array);
        var mem = Alloc<T>(count, initialize);
        if (mem == null)
        {
            return false;
        }
        array = new TitanArray<T>(mem, count);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAllocBuffer(out TitanBuffer buffer, uint size, bool initialize = false)
    {
        Unsafe.SkipInit(out buffer);
        var mem = Alloc(size, initialize);
        if (mem == null)
        {
            return false;
        }
        buffer = new TitanBuffer(mem, size);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Alloc<T>(uint count, bool initialize = true) where T : unmanaged => (T*)Alloc((uint)sizeof(T) * count, initialize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Alloc<T>(bool initialize = true) where T : unmanaged => (T*)Alloc((uint)sizeof(T), initialize);

    public void* Alloc(uint size, bool initialize = true)
    {
        Debug.Assert(size > 0);
        Debug.Assert(_memoryBlock.MaxSize > 0);
        var totalSize = MemoryUtils.AlignToUpper(size) + HeaderSize;
        
        var node = GetFreeNode(totalSize);
        Debug.Assert(node != null);

        var remainingSize = node->BlockSize - totalSize;
        // if the remaining size is greater than MinBlockSize, split the node into 2. 
        if (remainingSize > MinBlockSize)
        {
            var header = (Header*)((byte*)node + totalSize);
            header->State = AllocationState.Free;
            header->Previous = node;
            header->Next = node->Next;
            header->BlockSize = remainingSize;
            node->Next = header;
            if (header->Next != null)
            {
                header->Next->Previous = header;
            }
            node->BlockSize = totalSize;
            if (node == _lastAllocation)
            {
                _lastAllocation = header;
            }
        }
        node->State = AllocationState.Allocated;
        var data = (void*)(node + 1);
        Debug.Assert((nuint)data == MemoryUtils.Align((nuint)data), "The data pointer returned should be aligned at 8 bytes. Not sure why this happened.");

        if (initialize)
        {
            MemoryUtils.Init(data, size);
        }
        
        AssertCircularMemmory();
        return data;
    }

    private Header* GetFreeNode(uint totalSize)
    {
        // todo insert a count, and assert if it expires. should not have to run more than 2 laps.
        while (true)
        {
            //NOTE(Jens): this is a "first fit" implementation, should change it to a "best fit" when it works.
            var header = _allocations;
            while (header != null)
            {
                if (header->State == AllocationState.Free && header->BlockSize >= totalSize)
                {
                    return header;
                }
                header = header->Next;
            }
            Expand(totalSize);
        }
    }

    private void Expand(nuint requestedBlockSize)
    {
        if (_allocations == null)
        {
            // first allocation
            _memoryBlock.Resize(requestedBlockSize); // set the size to the requested size (aligned with the page size)
            _allocations = (Header*)_memoryBlock.Mem;
            _allocations->Next = null;
            _allocations->Previous = null;
            _allocations->BlockSize = _memoryBlock.Size;
            _allocations->State = AllocationState.Free;
            _lastAllocation = _allocations;

            return;
        }

        var newSize = _memoryBlock.Size;
        do
        {
            newSize = Math.Min(newSize * 2, _memoryBlock.MaxSize);
            if (requestedBlockSize < newSize)
            {
                var previousSize = _memoryBlock.Size;
                _memoryBlock.Resize(newSize);
                Debug.Assert(_lastAllocation != null, "This should not happen, since this can't be reached without any allocations occuring.");
                Debug.Assert(_lastAllocation->BlockSize != 0, "The block size of the last allocation is 0. Suspecting overwrites.");
                var mem = (Header*)((byte*)_lastAllocation + _lastAllocation->BlockSize);
                mem->Next = null;
                mem->Previous = _lastAllocation;
                mem->BlockSize = _memoryBlock.Size - previousSize;
                mem->State = AllocationState.Free;
                _lastAllocation->Next = mem;
                _lastAllocation = mem;

                MergeWithNext(mem->Previous);
                return;
            }
        } while (newSize < _memoryBlock.MaxSize);

        Debug.Assert(true, "ran out of memory.");
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FreeBuffer(ref TitanBuffer buffer)
    {
        Free(buffer.AsPointer());
        buffer = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FreeArray<T>(ref TitanArray<T> array) where T : unmanaged
    {
        Free(array.AsPointer());
        array = default;
    }

    public void Free(void* ptr)
    {
        Debug.Assert(ptr != null);
        Debug.Assert(_memoryBlock.MaxSize > 0);
        var header = (Header*)ptr - 1;
        // Check bounds
        Debug.Assert(header >= _memoryBlock.Mem);
        Debug.Assert(header < (byte*)_memoryBlock.Mem + _memoryBlock.Size);
        if (header->State != AllocationState.Allocated)
        {
            return;
        }
        header->State = AllocationState.Free;

        // See if we can merge blocks that are nearby
        MergeWithNext(header);
        MergeWithNext(header->Previous);
    }


    private void MergeWithNext(Header* current)
    {
        //NOTE(Jens): if both current and next are free, merge them
        if (IsFree(current) && IsFree(current->Next))
        {
            var next = current->Next;
            var nextNext = next->Next;
            current->Next = nextNext;
            current->BlockSize += next->BlockSize;
            
            if (nextNext != null)
            {
                nextNext->Previous = current;
            }
            else
            {
                //NOTE(Jens): this happens when the current node is merged with the last allocated block.
                _lastAllocation = current;
            }
        }

        static bool IsFree(Header* header) => header != null && header->State == AllocationState.Free;
    }

    internal void Release()
    {
        _memoryBlock.Release();
        _memoryBlock = default;
    }

    //NOTE(Jens): Current header size is 24 bytes, we can reduce that by packing and doing some extra calculations. Memory/Performance trade. 
    private struct Header
    {
        //NOTE(Jens): we could pack the State into 2 bytes and the size into 6 bytes if we want to support greater sizes.
        public AllocationState State;
        public uint BlockSize;

        //NOTE(Jens): next pointer is not really required since it is the same as  (Header)((byte*)&this + BlockSize). But if we do this we need to check that it's not out of bounds. This will save 8 bytes per allocation but use more CPU per allocation.
        public Header* Next;
        public Header* Previous;
    }

    private enum AllocationState : uint
    {
        Allocated = 0xeeffffee,
        Free = 0xfffaafff
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsWithinMemoryBlock(void* ptr)
        => ptr >= _memoryBlock.Mem && ptr < (byte*)_memoryBlock.Mem + _memoryBlock.Size;


    public static void* Alloc(void* allocator, uint size)
        => ((GeneralAllocator*)allocator)->Alloc(size);

    public static void Free(void* allocator, void* ptr)
        => ((GeneralAllocator*)allocator)->Free(ptr);

    public static void Release(void* allocator, IMemoryManager memoryManager)
    {
        throw new NotImplementedException();
    }

    public readonly Allocator AsAllocator() => Allocator.Create(ref Unsafe.AsRef(in this));

    public static implicit operator Allocator(in GeneralAllocator allocator) => allocator.AsAllocator();

    [Conditional("DEBUG")]
    public void AssertCircularMemmory()
    {
        //NOTE(Jens): We can disable this if it slows things down to much. 
        if (_allocations == null)
        {
            return;
        }
        
        // maybe we can replace this with a stack alloc version.
        Set.Clear();
        //var set = new HashSet<nuint>();
        var alloc = _allocations;
        while (alloc != null)
        {
            Debug.Assert(alloc->BlockSize != 0);
            Debug.Assert(Set.Add((nuint)alloc), "The memory address is already in the list, circual dependencies.");
            Debug.Assert(alloc->Previous is null || !Set.Add((nuint)alloc->Previous), "The previous element points at some memory that is not in the list.");
            alloc = alloc->Next;
        }
    }
#if DEBUG
    private static HashSet<nuint> Set = new(10_000);
#endif
}
