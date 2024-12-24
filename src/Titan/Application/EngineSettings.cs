using Titan.Graphics.D3D12.Adapters;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Application;

/// <summary>
/// Public interface to read values that might be useful for configuration
/// </summary>
public unsafe partial struct EngineSettings
{
    private static D3D12Adapter* Adapters;

    [System(SystemStage.Startup)]
    internal static void Startup(UnmanagedResourceRegistry registry)
    {
        if (registry.HasResource<D3D12Adapter>())
        {
            Adapters = registry.GetResourcePointer<D3D12Adapter>();
        }
    }
    
    
    public static ReadOnlySpan<AdapterInfo> GraphicAdapters
        => Adapters != null ? Adapters->Adapters.AsReadOnlySpan()[..(int)Adapters->AdapterCount] : [];
}
