using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Generators;
internal static class TitanTypes
{

    public const string Resources = "Titan.Resources";
    public const string Assets = "Titan.Assets";
    public const string Systems = "Titan.Systems";
    public const string Events = "Titan.Events";
    public const string Configurations = "Titan.Configurations";
    public const string Core = "Titan.Core";
    public const string ECS = "Titan.ECS";

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

    public const string Handle = $"{Core}.Handle";
    public const string TitanBuffer = $"{Core}.TitanBuffer";

    public const string EventAttribute = $"{Events}.EventAttribute";
    public const string EventsGenerator = $"{Events}.EventId";
    public const string EventReader = $"{Events}.EventReader";
    public const string EventWriter = $"{Events}.EventWriter";
    public const string IEvent = $"{Events}.IEvent";

    public const string AssetLoaderAttributeMetadataName = $"{Assets}.AssetLoaderAttribute`1";
    public const string AssetAttribute = $"{Assets}.AssetAttribute";
    public const string IAsset = $"{Assets}.IAsset";
    public const string IAssetLoader = $"{Assets}.IAssetLoader";
    public const string AssetType = $"{Assets}.AssetType";
    public const string AssetLoaderInitializer = $"{Assets}.AssetLoaderInitializer";
    public const string AssetDescriptor = $"{Assets}.AssetDescriptor";
    public const string AssetLoaderDescriptor = $"{Assets}.AssetLoaderDescriptor";

    public const string ComponentAttribute = $"{ECS}.ComponentAttribute";
    public const string IComponent = $"{ECS}.IComponent";
    public const string ComponentType = $"{ECS}.ComponentType";
    public const string EntityManager = $"{ECS}.EntityManager";
    public const string Entity = $"{ECS}.Entity";
    public const string CachedQuery = $"{ECS}.Archetypes.CachedQuery";

    public const string IConfiguration = $"{Configurations}.IConfiguration";

    public const string MemoryUtils = $"{Core}.Memory.MemoryUtils";

    public static readonly string MethodImplAttribute = typeof(MethodImplAttribute).FullName!;
    public static readonly string MethodImplOptions = typeof(MethodImplOptions).FullName!;
    public static readonly string MemoryMarshal = typeof(MemoryMarshal).FullName!;
    public static readonly string Unsafe = typeof(Unsafe).FullName!;


    public const string Span = "System.Span";
    public const string ReadOnlySpan = "System.ReadOnlySpan";
}
