using System.Diagnostics;
using Titan.Core;

namespace Titan.Assets;

public interface IAssetsManager : IService
{
    //TODO(Jens): Add Load/Unload functions here.
}


internal sealed unsafe class AssetsManager : IAssetsManager
{
    private UnmanagedResource<AssetsContext> _context;
    public bool Init(IReadOnlyList<AssetRegistryDescriptor> registries, UnmanagedResource<AssetsContext> assetsContext)
    {
        ref var context = ref assetsContext.AsRef;
        context = default;

        Debug.Assert(registries.Count < context.Registries.Size);

        for (var i = 0; i < registries.Count; ++i)
        {
            context.Registries[i].Descriptor = registries[i];
        }

        context.NumberOfRegistries = (uint)registries.Count;

        _context = assetsContext;
        return true;
    }
}
