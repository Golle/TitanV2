using Titan.Application;

namespace Titan;

/// <summary>
/// Modules are initialized in the order they are registered, these should be system critical systems that rely on other systems
/// </summary>
public interface IModule
{
    static abstract bool Build(IAppBuilder builder, AppConfig config);
    static abstract bool Init(IApp app);
    static abstract bool Shutdown(IApp app);
}
