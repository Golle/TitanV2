using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Generators;
internal static class TitanTypes
{

    public const string Resources = "Titan.Resources";
    public const string Systems = "Titan.Systems";
    public const string Events = "Titan.Events";
    public const string Configurations = "Titan.Configurations";
    public const string Core = "Titan.Core";

    public const string UnmanagedResourceAttribute = $"{Resources}.UnmanagedResourceAttribute";
    public const string UnmanagedResourceGenerator = $"{Resources}.UnmanagedResourceId";
    public const string IResource = $"{Resources}.IResource";
    public const string SystemAttribute = $"{Systems}.SystemAttribute";
    public const string ISystem = $"{Systems}.ISystem";
    public const string SystemDescriptor = $"{Systems}.SystemDescriptor";
    public const string Debug = "System.Diagnostics.Debug";
    public const string StringRef = $"{Core}.Strings.StringRef";
    public const string SystemInitializer = $"{Systems}.SystemInitializer";
    public const string ManagedResource = $"{Core}.ManagedResource";
    public const string SystemStage = $"{Systems}.SystemStage";
    public const string SystemExecutionType = $"{Systems}.SystemExecutionType";

    public const string EventAttribute = $"{Events}.EventAttribute";
    public const string EventsGenerator = $"{Events}.EventId";
    public const string EventReader = $"{Events}.EventReader";
    public const string EventWriter = $"{Events}.EventWriter";
    public const string IEvent = $"{Events}.IEvent";

    public const string IConfiguration = $"{Configurations}.IConfiguration";


    public static readonly string MethodImplAttribute = typeof(MethodImplAttribute).FullName!;
    public static readonly string MethodImplOptions = typeof(MethodImplOptions).FullName!;
    public static readonly string MemoryMarshal = typeof(MemoryMarshal).FullName!;
    public static readonly string Unsafe = typeof(Unsafe).FullName!;
    public const string Span = "System.Span";
    public const string ReadOnlySpan = "System.ReadOnlySpan";
}
