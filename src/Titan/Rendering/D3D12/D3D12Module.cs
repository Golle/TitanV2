using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Rendering.D3D12;


internal class D3D12Module : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        Logger.Trace<D3D12Module>("Using D3D12 Rendering");

        builder.AddService(new D3D12Adapter());
        return true;
    }

    public static bool Init(IApp app)
    {
        var adapters = app.GetService<D3D12Adapter>();
        var memorySystem = app.GetService<IMemorySystem>();
        var config = app.GetConfigOrDefault<RenderingConfig>();

        if (!adapters.Init(memorySystem, config.Debug))
        {
            Logger.Error<D3D12Module>($"Failed to init {nameof(D3D12Adapter)}");
            return false;
        }


        return true;
    }

    public static bool Shutdown(IApp app)
    {
        app.GetService<D3D12Adapter>()
            .Shutdown();

        return true;
    }
}
