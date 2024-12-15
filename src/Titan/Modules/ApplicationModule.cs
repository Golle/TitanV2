using Titan.Application;
using Titan.Audio;
using Titan.Core.Logging;
using Titan.Input;
using Titan.Materials;
using Titan.Meshes;
using Titan.Rendering;
using Titan.UI;
using Titan.Windows;

namespace Titan.Modules;

public class ApplicationModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        Logger.Trace<ApplicationModule>($"Application Type = {config.ApplicationType}");
        if (config.ApplicationType == ApplicationType.Game)
        {
            builder
                .AddModule<WindowModule>()
                .AddModule<RenderingModule>()
                .AddModule<MaterialsModule>()
                .AddModule<MeshModule>()
                .AddModule<UIModule>()
                .AddModule<InputModule>()
                .AddModule<AudioModule>()
                ;
        }

        return true;
    }

    public static bool Init(IApp app) => true;
    public static bool Shutdown(IApp app) => true;
}
