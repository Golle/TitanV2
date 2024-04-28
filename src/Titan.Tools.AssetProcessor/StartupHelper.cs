using System.Reflection;
using System.Text.Json.Serialization;
using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata;

namespace Titan.Tools.AssetProcessor;
internal static class StartupHelper
{
#if DEBUG
    public static bool VerifyAssetSerialization()
    {
#pragma warning disable IL2026 // we only do this in debug builds anyway.
        var metadataTypes = typeof(StartupHelper).Assembly
            .GetTypes()
#pragma warning restore IL2026
            .Where(t => t.IsAssignableTo(typeof(AssetFileMetadata)) && !t.IsAbstract);

        var attributes = typeof(AssetFileMetadata)
            .GetCustomAttributes()
            .OfType<JsonDerivedTypeAttribute>()
            .ToArray();
        var result = true;
        foreach (var metadataType in metadataTypes)
        {
            if (attributes.All(a => a.DerivedType != metadataType))
            {
                Logger.Error($"Missing {nameof(JsonDerivedTypeAttribute)} for type {metadataType.Name}", typeof(StartupHelper));
                result = false;
            }
        }

        return result;
    }
#else
    //NOTE(Jens): we don't want reflection in a NativeAOT build.
    public static bool VerifyAssetSerialization() => true;
#endif
}
