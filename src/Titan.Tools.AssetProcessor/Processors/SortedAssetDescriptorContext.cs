using Titan.Assets;
using Titan.Assets.Types;
using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors;

internal struct SortedAsset
{
    public required AssetDescriptor Descriptor;
    public required AssetFileMetadata Metadata;
    public required byte[] Data;
}

internal class SortedAssetDescriptorContext : IAssetDescriptorContext
{
    private readonly List<SortedAsset> _assets = new();
    private readonly List<(DiagnosticsLevel Level, string Message)> _diagnostics = new();
    private (AssetDescriptor descriptor, AssetFileMetadata Metadata)[]? _finalizedAssets;
    private byte[]? _finalizedData;

    public IEnumerable<(DiagnosticsLevel Level, string Message)> Diagnostics => _diagnostics;
    public bool HasErrors => _diagnostics.Any(static d => d.Level == DiagnosticsLevel.Error);
    public bool TryAddTexture2D(in Texture2DDescriptor texture2D, ReadOnlySpan<byte> data, AssetFileMetadata metadata)
    {
        var descriptor = new AssetDescriptor
        {
            Type = AssetType.Texture,
            Texture2D = texture2D
        };
        return AddAsset(data, metadata, descriptor);
    }

    public bool TryAddShader(in ShaderDescriptor shader, ReadOnlySpan<byte> data, ShaderMetadata metadata)
    {
        var descriptor = new AssetDescriptor
        {
            Type = AssetType.Shader,
            Shader = shader,
        };
        return AddAsset(data, metadata, descriptor);
    }

    public bool TryAddMesh(in MeshDescriptor mesh, ReadOnlySpan<byte> data, AssetFileMetadata metadata)
    {
        var descriptor = new AssetDescriptor
        {
            Type = AssetType.Mesh,
            Mesh = mesh
        };

        return AddAsset(data, metadata, descriptor);
    }

    private bool AddAsset(ReadOnlySpan<byte> data, AssetFileMetadata metadata, AssetDescriptor descriptor)
    {
        lock (_assets)
        {
            if (_assets.Any(a => a.Metadata.Name != null && a.Metadata.Name == metadata.Name))
            {
                Logger.Error<SortedAssetDescriptorContext>($"Multiple assets with the same name. Name = {metadata.Name}");
                return false;
            }

            _assets.Add(new SortedAsset
            {
                Data = data.ToArray(),
                Descriptor = descriptor,
                Metadata = metadata,
            });
        }

        return true;
    }

    public void AddDiagnostics(DiagnosticsLevel level, string message)
    {
        lock (_diagnostics)
        {
            _diagnostics.Add((level, message));
        }
    }

    // ReSharper disable InconsistentlySynchronizedField
    public Task Complete()
    {
        List<(AssetDescriptor Descriptor, AssetFileMetadata Metadata)> descriptors = new(_assets.Capacity);
        var totalSize = _assets.Sum(static a => a.Data.Length);
        var data = new byte[totalSize];
        var offset = 0u;
        foreach (var sortedAsset in _assets.OrderBy(static a => a.Metadata.Name))
        {
            var length = (uint)sortedAsset.Data.Length;

            var updatedDescriptor = sortedAsset.Descriptor with
            {
                File = new()
                {
                    Offset = offset,
                    Length = length
                }
            };
            descriptors.Add((updatedDescriptor, sortedAsset.Metadata));
            sortedAsset.Data.CopyTo(data, offset);
            offset += length;
        }

        _finalizedAssets = descriptors.ToArray();
        _finalizedData = data;
        return Task.CompletedTask;
    }

    public ReadOnlyMemory<(AssetDescriptor Descriptor, AssetFileMetadata Metadata)> GetAssets()
        => _finalizedAssets ?? [];

    public ReadOnlyMemory<byte> GetData()
        => _finalizedData ?? [];
}
