using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core.Memory;

namespace Titan.Core;

public unsafe struct TitanQueue<T>(T* ptr, uint length) where T : unmanaged
{
    private int _head = 0;
    private int _tail = (int)(length - 1);
    private int _count = 0;
    private readonly int _capacity = (int)length;

    public uint Count => (uint)_count;
    public void Enqueue(in T value)
    {
        Debug.Assert(!IsFull());
        _tail = (_tail + 1) % _capacity;
        ptr[_tail] = value;
        _count++;
    }


    /// <summary>
    /// Removes one item from the queue, will return default value if the queue is empty.
    /// </summary>
    /// <returns>Value or default</returns>
    public T Dequeue() 
        => TryDequeue(out var value) ? value : default;

    public bool TryDequeue(out T value)
    {
        Unsafe.SkipInit(out value);
        if (IsEmpty())
        {
            return false;
        }

        value = ptr[_head];
        _head = (_head + 1) % _capacity;
        _count--;

        return true;
    }

    public bool IsFull() => _count == _capacity;
    public bool IsEmpty() => _count == 0;
    public bool HasItems() => !IsEmpty();

    public static implicit operator TitanQueue<T>(Span<T> data) => new(MemoryUtils.AsPointer(data.GetPinnableReference()), (uint)data.Length);
}
