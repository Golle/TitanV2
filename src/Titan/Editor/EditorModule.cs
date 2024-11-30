using Titan.Application;

namespace Titan.Editor;
internal class EditorModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddSystems<DebugUISystem>();
        return true;
    }
}
