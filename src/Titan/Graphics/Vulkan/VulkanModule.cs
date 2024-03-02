using Titan.Application;

namespace Titan.Graphics.Vulkan;
internal sealed class VulkanModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
        => throw new NotSupportedException("Vulkan rendering has not been implemented yet.");
    public static bool Init(IApp app)
        => throw new NotSupportedException("Vulkan rendering has not been implemented yet.");
    public static bool Shutdown(IApp app)
        => throw new NotSupportedException("Vulkan rendering has not been implemented yet.");
}
