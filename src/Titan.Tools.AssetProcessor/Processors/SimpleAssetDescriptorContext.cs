using Titan.Core.Logging;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors;

/// <summary>
/// Simple implementation of the context. This will not care about the order of the assets and will just write them as they come in.
/// A better implementation should group similar asset types together.
/// </summary>
internal class SimpleAssetDescriptorContext(uint maxBufferSize, uint maxDescriptors) : IAssetDescriptorContext
{
    private readonly byte[] _buffer = new byte[maxBufferSize];
    private volatile uint _offset;
    private volatile uint _nextDescriptor;
    private readonly (AssetDescriptor Descriptor, AssetFileMetadata Metadata)[] _assets = new (AssetDescriptor, AssetFileMetadata)[maxDescriptors];

    private readonly ISet<string> _assetNames = new HashSet<string>();
    private readonly List<(DiagnosticsLevel Level, string Message)> _diagnostics = new();
    public IEnumerable<(DiagnosticsLevel Level, string Message)> Diagnostics => _diagnostics;
    public bool HasErrors => _diagnostics.Any(static d => d.Level == DiagnosticsLevel.Error);

    public bool TryAddTexture2D(in Texture2DDescriptor texture2D, ReadOnlySpan<byte> data, AssetFileMetadata metadata)
    {
        if (!TryAddName(metadata))
        {
            Logger.Error<SimpleAssetDescriptorContext>($"Muliple assets with the same name. Name = {metadata.Name}");
            return false;
        }
        ref var asset = ref WriteDataAndCreateAssetDescriptor(data, AssetType.Texture2D, metadata);
        asset.Texture2D = texture2D;
        return true;
    }

    public bool TryAddShader(AssetType type, in ShaderDescriptor shader, ReadOnlySpan<byte> data, ShaderMetadata metadata)
    {
        if (!TryAddName(metadata))
        {
            Logger.Error<SimpleAssetDescriptorContext>($"Muliple assets with the same name. Name = {metadata.Name}");
            return false;
        }
        ref var asset = ref WriteDataAndCreateAssetDescriptor(data, type, metadata);
        asset.Shader = shader;
        return true;
    }

    private bool TryAddName(AssetFileMetadata metadata)
    {
        if (metadata.Name == null)
        {
            return true;
        }
        lock (_assetNames)
        {
            return _assetNames.Add(metadata.Name);
        }
    }

    private ref AssetDescriptor WriteDataAndCreateAssetDescriptor(ReadOnlySpan<byte> data, AssetType type, AssetFileMetadata metadata)
    {
        var length = (uint)data.Length;
        var offset = Interlocked.Add(ref _offset, length) - length;
        data.CopyTo(_buffer.AsSpan((int)offset));
        var index = Interlocked.Increment(ref _nextDescriptor) - 1;
        ref var asset = ref _assets[index];
        asset.Descriptor = new AssetDescriptor
        {
            Type = type,
            File = { Length = length, Offset = offset },
        };
        asset.Metadata = metadata;
        return ref asset.Descriptor;
    }

    public void AddDiagnostics(DiagnosticsLevel level, string message)
    {
        lock (_diagnostics)
        {
            _diagnostics.Add((level, message));
        }
    }

    public Task Complete() => Task.CompletedTask;

    public ReadOnlyMemory<(AssetDescriptor Descriptor, AssetFileMetadata Metadata)> GetAssets()
        => _assets.AsMemory(0, (int)_nextDescriptor);

    public ReadOnlyMemory<byte> GetData()
        => _buffer.AsMemory(0, (int)_offset);
}
