#define D3D12_DEBUG_LAYER 
using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Rendering.D3D12.Adapters;
using Titan.Rendering.D3D12.Memory;
using Titan.Rendering.D3D12.Utils;
using Titan.Windows.Win32;

namespace Titan.Rendering.D3D12;

public record struct ResourceConfig(uint MaxTextures, uint MaxMaterials, uint MaxBuffers, uint MaxPipelineStates, uint MaxRootSignatures);

internal class D3D12Module : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        Logger.Trace<D3D12Module>("Using D3D12 Rendering");

        builder
            .AddService(new D3D12Adapter())
            .AddService(new D3D12Device())
            .AddService(new D3D12CommandQueue())
            .AddService(new D3D12Allocator())
            .AddService(new DXGISwapchain())
#if D3D12_DEBUG_LAYER
            .AddService(new D3D12DebugLayer())
            .AddService(new D3D12DebugMessages())
#endif
            ;
        return true;
    }

    public static bool Init(IApp app)
    {
        var adapters = app.GetService<D3D12Adapter>();
        var device = app.GetService<D3D12Device>();
        var commandQueue = app.GetService<D3D12CommandQueue>();
        var allocator = app.GetService<D3D12Allocator>();
        var swapchain = app.GetService<DXGISwapchain>();
        var window = app.GetService<IWindow>();

        var memorySystem = app.GetService<IMemorySystem>();
        var config = app.GetConfigOrDefault<RenderingConfig>();
        var d3d12Config = app.GetConfigOrDefault<D3D12Config>();

        if (!adapters.Init(memorySystem, config.Adapter, config.Debug))
        {
            Logger.Error<D3D12Module>($"Failed to init {nameof(D3D12Adapter)}");
            return false;
        }

#if D3D12_DEBUG_LAYER
        //NOTE(Jens): Must be enabled before the Device is created
        var debugLayer = app.GetService<D3D12DebugLayer>();
        if (config.Debug && !debugLayer.Init())
        {
            Logger.Warning<D3D12Module>($"Failed to init {nameof(D3D12DebugLayer)}. D3D12 Debug information might be incomplete.");
        }
#endif
        if (!device.Init(adapters, d3d12Config.FeatureLevel))
        {
            Logger.Error<D3D12Module>($"Failed to init {nameof(D3D12Device)}");
            return false;
        }

#if D3D12_DEBUG_LAYER
        var debugMessage = app.GetService<D3D12DebugMessages>();
        if (config.Debug && !debugMessage.Init(device))
        {
            //NOTE(Jens): This is expected to fail at the moment. Need newer version of windows.
            //Logger.Warning<D3D12Module>($"Failed to init {nameof(D3D12DebugMessages)}. D3D12 Debug information might be incomplete.");
        }
#endif

        if (!allocator.Init(memorySystem, device, d3d12Config.MemoryConfig))
        {
            Logger.Error<D3D12Module>($"Failed to init {nameof(D3D12Allocator)}");
            return false;
        }
        if (!commandQueue.Init(device))
        {
            Logger.Error<D3D12Module>($"Failed to init {nameof(D3D12CommandQueue)}");
            return false;
        }

        if (!swapchain.Init(commandQueue, window, config.Debug))
        {
            Logger.Error<D3D12Module>($"Failed to init {nameof(DXGISwapchain)}");
            return false;
        }


        return true;
    }

    public static bool Shutdown(IApp app)
    {
        app.GetService<D3D12Device>()
            .Shutdown();

        app.GetService<D3D12Adapter>()
            .Shutdown();
        
        app.GetService<DXGISwapchain>()
            .Shutdown();

        app.GetService<D3D12CommandQueue>()
            .Shutdown();

        app.GetService<D3D12Allocator>()
            .Shutdown();


        var debugLayer = app.GetService<D3D12DebugLayer>();
        debugLayer.ReportLiveObjects();
        debugLayer.Shutdown();

        app.GetService<D3D12DebugMessages>()
            .Shutdown();

        

        return true;
    }
}
