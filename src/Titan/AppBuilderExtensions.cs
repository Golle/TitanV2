using Titan.Application;
using Titan.Resources;
using Titan.Systems;

namespace Titan;

public static class AppBuilderExtensions
{
    /// <summary>
    /// Helper function for structs that have systems and is also a resource. 
    /// </summary>
    /// <typeparam name="T">The struct that contains the system functions and is also marked with the [<see cref="UnmanagedResourceAttribute"/>]</typeparam>
    public static IAppBuilder AddSystemsAndResource<T>(this IAppBuilder builder) where T : unmanaged, ISystem, IResource =>
        builder
            .AddResource<T>()
            .AddSystems<T>();
}
