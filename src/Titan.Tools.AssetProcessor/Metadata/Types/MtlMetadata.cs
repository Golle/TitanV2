namespace Titan.Tools.AssetProcessor.Metadata.Types;

internal sealed class MtlMetadata : AssetFileMetadata
{
    public string?[] MaterialNames { get; set; } = [];
}
