using Titan.Assets;
using Titan.Assets.Types;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors;

internal enum DiagnosticsLevel
{
    Info,
    Warning,
    Error
}

internal interface IAssetDescriptorContext
{
    bool TryAddTexture2D(in Texture2DDescriptor texture2D, ReadOnlySpan<byte> data, AssetFileMetadata metadata);
    bool TryAddShader(in ShaderDescriptor shader, ReadOnlySpan<byte> data, ShaderMetadata metadata);
    void AddDiagnostics(DiagnosticsLevel level, string message);

    Task Complete();
    ReadOnlyMemory<(AssetDescriptor Descriptor, AssetFileMetadata Metadata)> GetAssets();
    ReadOnlyMemory<byte> GetData();
    bool HasErrors { get; }
    IEnumerable<(DiagnosticsLevel Level, string Message)> Diagnostics { get; }
}
