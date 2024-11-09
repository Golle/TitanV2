using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Rendering;
using Titan.Resources;
using Titan.Systems;

namespace Titan.UI;

[StructLayout(LayoutKind.Sequential)]
internal struct UIElement
{
    public Color Color;
    public SizeF Size;
    public Vector2 Offset;
}

[UnmanagedResource]
internal unsafe partial struct UISystem
{
    public Handle<Rendering.Buffer> Instances;
    public uint Count;
    private UIElement* ElementsGPU;
    private TitanArray<UIElement> ElementsCPU;

    [System(SystemStage.Init)]
    public static void Init(UISystem* stuff, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<UIConfig>();
        var s = sizeof(UIElement) * config.MaxElements;
        // Allocate memory for CPU side
        if (!memoryManager.TryAllocArray(out stuff->ElementsCPU, config.MaxElements))
        {
            Logger.Error<UISystem>($"Failed to allocate memory for UIElements. Count = {config.MaxElements} Size = {sizeof(UIElement) * config.MaxElements}");
            return;
        }

        // Set up GPU resources
        stuff->Instances = resourceManager.CreateBuffer(CreateBufferArgs.Create<UIElement>(1024, BufferType.Structured, cpuVisible: true, shaderVisible: true));
        if (stuff->Instances.IsInvalid)
        {
            Logger.Error<UISystem>("Failed to create a structured buffer for UI Elements.");
            return;
        }

        var hr = resourceManager.Access(stuff->Instances)->Resource.Get()->Map(0, null, (void**)&stuff->ElementsGPU);
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<UISystem>("Failed to Map the UI elemnets buffer.");
            return;
        }
    }

    [System(SystemStage.PreUpdate)]
    public static void PreUpdate(ref UISystem system)
    {
        system.Count = 0;
    }

    [System(SystemStage.PostUpdate, SystemExecutionType.Inline)]
    public static void Update(ref UISystem system)
    {
        MemoryUtils.Copy(system.ElementsGPU, system.ElementsCPU.AsPointer(), (uint)sizeof(UIElement) * system.Count);
        
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(UISystem* stuff, in D3D12ResourceManager resourceManager)
    {
        if (stuff->Instances.IsValid)
        {
            Logger.Warning<UISystem>("cant shut down this..");
            //resourceManager.DestroyBuffer(stuff->Instances);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in UIElement element)
    {
        var index = Interlocked.Increment(ref Count) - 1;
        ElementsCPU[index] = element;
    }

}
