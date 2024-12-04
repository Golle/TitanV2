using Titan.Assets;
using Titan.Core.Memory.Allocators;
using Titan.Input;
using Titan.Resources;

namespace Titan.UI2;
public unsafe struct UIContext
{
    private AssetsManager _assetsManager;
    private InputState* _inputState;
    private byte _contextId; // this should be used to uniquely identify a context.

    internal UIContext(AssetsManager assetsManager, InputState* inputState)
    {

    }

    public void Begin(uint maxNumberOfElements = 128)
    {
        // request memory for elements.
    }

    public void End()
    {
        //transmit elements for rendering as a single bulk
    }
}

internal unsafe struct UIWidget
{

}

[UnmanagedResource]
internal unsafe partial struct UISystem2
{
    private AtomicBumpAllocator _allocator;
    public BumpAllocator CreateAllocator(uint count)
    {
        var size = (uint)sizeof(UIWidget) * count;
        var memory = _allocator.Alloc(size);
        return new BumpAllocator((byte*)memory, size);
    }
}


