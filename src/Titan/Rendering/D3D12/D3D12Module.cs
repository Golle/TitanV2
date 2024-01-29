#define D3D12_DEBUG_LAYER 
using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32.D3D;
using Titan.Rendering.D3D12.Adapters;
using Titan.Rendering.D3D12.Utils;

namespace Titan.Rendering.D3D12;

internal class D3D12Module : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        Logger.Trace<D3D12Module>("Using D3D12 Rendering");

        builder
            .AddService(new D3D12Adapter())
            .AddService(new D3D12Device())
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

        var memorySystem = app.GetService<IMemorySystem>();
        var config = app.GetConfigOrDefault<RenderingConfig>();


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
        if (!device.Init(adapters, D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_12_0))
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

        return true;
    }

    public static bool Shutdown(IApp app)
    {
        app.GetService<D3D12Device>()
            .Shutdown();

        app.GetService<D3D12Adapter>()
            .Shutdown();

        var debugLayer = app.GetService<D3D12DebugLayer>();
        debugLayer.ReportLiveObjects();
        debugLayer.Shutdown();

        app.GetService<D3D12DebugMessages>()
            .Shutdown();

        return true;
    }
}
