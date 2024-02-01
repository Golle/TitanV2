using Titan.Application;
using Titan.Core.Logging;

namespace Titan.Windows.Win32;

internal class Win32WindowModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        var window = new Win32Window($"{config.Name} - {config.Version}");
        builder
            .AddService<IWindow, Win32Window>(window);

        return true;
    }

    public static bool Init(IApp app)
    {
        var window = app.GetService<Win32Window>();
        var config = app.GetConfigOrDefault<WindowConfig>();

        if (!window.Init(config))
        {
            Logger.Error<Win32WindowModule>($"Failed to init the {nameof(Win32Window)}");
            return false;
        }

        return true;
    }

    public static bool Shutdown(IApp app)
    {
        var window = app.GetService<Win32Window>();
        app.UpdateConfig(app.GetConfigOrDefault<WindowConfig>() with
        {
            Height = window.Height,
            Width = window.Width,
            Title = window.Title,
            X = -1,
            Y = -1,
            Windowed = true
        });

        window
            .Shutdown();

        return true;
    }
}

