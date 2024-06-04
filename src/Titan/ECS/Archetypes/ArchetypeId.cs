using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Core;

namespace Titan.ECS.Archetypes;

[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay("Signature: {Signature} Size: {ComponentsSize} Count: {_count}")]
internal readonly unsafe struct ArchetypeId
{
    public readonly ulong Signature;
    public readonly uint ComponentsSize;
    private readonly Inline10<ComponentType> _components;
    private readonly int _count;

    public ReadOnlySpan<ComponentType> GetComponents() => _components.AsReadOnlySpan()[.._count];

    public ArchetypeId(in ComponentType type)
    {
        _components[0] = type;
        _count = 1;
        Signature = type.Id;
        ComponentsSize = type.Size;
    }

    private ArchetypeId(in Inline10<ComponentType> components, ulong signature, int count, uint size)
    {
        _components = components;
        _count = count;
        Signature = signature;
        ComponentsSize = size;
    }

    public ArchetypeId Add(in ComponentType type)
    {
        Debug.Assert(Signature % type.Id != 0, "Trying to add a component to an ArchetypeId that already has that ID.");

        var newComponents = _components;
        newComponents[_count] = type;
        var newCount = _count + 1;

        newComponents
            .AsSpan()[..newCount]
            .Sort(static (lh, rh) => lh.Id.CompareTo(rh.Id));

        return new ArchetypeId(newComponents, Signature * type.Id, newCount, ComponentsSize + type.Size);
    }

    public ArchetypeId Remove(in ComponentType type)
    {
        Debug.Assert(Signature % type.Id == 0, "Trying to remove a Component from an ArchetypeID that does not exist.");
        Debug.Assert(_count > 0);
        var newCount = _count - 1;
        if (newCount == 0)
        {
            //NOTE(Jens): This is basically an invalid state
            return default;
        }

        var count = 0;
        Inline10<ComponentType> newComponents = default;
        for (var i = 0; i < _count; ++i)
        {
            if (_components[i].Id != type.Id)
            {
                newComponents[count++] = _components[i];
            }
        }

        Debug.Assert(newCount == count);
        var newSize = ComponentsSize - type.Size;
        var newSignature = Signature / type.Id;
        return new(newComponents, newSignature, newCount, newSize);
    }
}
