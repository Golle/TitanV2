using Titan.Application;
using Titan.Core.Logging;

namespace Titan.Rendering.D3D12.Utils;
internal class D3D12UtilsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddService(new D3D12DebugLayer())
            .AddService(new D3D12DebugMessages());

        return true;
    }

    public static bool Init(IApp app)
    {
        var config = app.GetConfigOrDefault<RenderingConfig>();

        if (!config.Debug)
        {
            return true;
        }

        var debugLayer = app.GetService<D3D12DebugLayer>();
        if (!debugLayer.Init(true))
        {
            Logger.Warning<D3D12UtilsModule>($"Failed to init the {nameof(D3D12DebugLayer)}. Debug information might be incomplete.");
        }

        var device = app.GetService<D3D12Device>();
        var debugMessages = app.GetService<D3D12DebugMessages>();
        if (!debugMessages.Init(device))
        {
            Logger.Warning<D3D12UtilsModule>($"Failed to init the {nameof(D3D12DebugMessages)}. Debug information might be incomplete.");
        }

        return true;
    }

    public static bool Shutdown(IApp app)
    {


        return true;
    }
}
