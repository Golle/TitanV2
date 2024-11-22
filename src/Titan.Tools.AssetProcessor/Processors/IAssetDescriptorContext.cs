using Titan.Assets;
using Titan.Assets.Types;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.Tools.AssetProcessor.Processors.Audio;
using Titan.UI.Text;

namespace Titan.Tools.AssetProcessor.Processors;

internal enum DiagnosticsLevel
{
    Info,
    Warning,
    Error
}


internal interface IAssetDescriptorContext
{
    string TempFolderPath { get; }
    bool TryAddTexture2D(in Texture2DDescriptor texture2D, ReadOnlySpan<byte> data, AssetFileMetadata metadata);
    bool TryAddSprite(in SpriteDescriptor sprite, ReadOnlySpan<byte> data, AssetFileMetadata metadata);
    bool TryAddShader(in ShaderDescriptor shader, ReadOnlySpan<byte> data, ShaderMetadata metadata);
    bool TryAddMesh(in MeshDescriptor mesh, ReadOnlySpan<byte> data, AssetFileMetadata metadata);
    bool TryAddAudio(in AudioDescriptor audio, ReadOnlySpan<byte> data, AudioMetadata metadata);
    void AddDiagnostics(DiagnosticsLevel level, string message);
    IEnumerable<AssetFileMetadata> GetMetadataByFilename(string filename);

    Task Complete();
    ReadOnlyMemory<(AssetDescriptor Descriptor, AssetFileMetadata Metadata)> GetAssets();
    ReadOnlyMemory<byte> GetData();
    bool HasErrors { get; }
    IEnumerable<(DiagnosticsLevel Level, string Message)> Diagnostics { get; }
    IEnumerable<T> GetMetadataByType<T>() where T : AssetFileMetadata;
    bool TryAddFont(in FontDescriptor font, ReadOnlySpan<GlyphInfo> glyphInfo, ReadOnlySpan<byte> pixelData, FontMetadata metadata);
}
