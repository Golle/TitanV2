using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Core.Maths;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.Tools.AssetProcessor.Utils;

namespace Titan.Tools.AssetProcessor.Processors.Materials;

internal record MaterialInfo
{
    public Color Diffuse { get; init; } = Color.White;
    public string? AlbedoMap { get; init; }
    public string? NormalMap { get; init; }
}

internal class MaterialsProcessor : AssetProcessor<MaterialMetadata>
{
    protected override async Task OnProcess(MaterialMetadata metadata, IAssetDescriptorContext context)
    {
        return;
        await using var handle = File.OpenRead(metadata.ContentFileFullPath);
        var material = JsonSerializer.Deserialize(handle, MaterialJsonContext.Default.MaterialInfo);
        if (material == null)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to read content of {metadata.ContentFileRelativePath}.");
            return;
        }
        var descriptor = new MaterialDescriptor();

        List<AssetFileMetadata> dependencies = new();
        if (material.AlbedoMap != null)
        {
            var image = context.GetMetadataByType<ImageMetadata>()
                .SingleOrDefault(m => material.AlbedoMap.Equals(m.Name));

            if (image == null)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to find AlbedoMap with Name = {material.AlbedoMap}");
                return;
            }

            if (image.Type != ImageType.Texture)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"AlbedoMap is of wrong type, expected {ImageType.Texture}, found {image.Type}. Name = {image.Name} FilePath = {image.ContentFileRelativePath}");
                return;
            }

            dependencies.Add(image);
            //descriptor.HasAlbedoMap = true;
        }
        metadata.Dependencies = dependencies;

        Span<byte> buffer = stackalloc byte[512];
        var writer = new TitanBinaryWriter(buffer);
        writer.Write(material.Diffuse);
        
        if (!context.TryAddMaterial(descriptor, writer.GetData(), metadata))
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, "Failed to add the material to the contenxt.");
        }
    }
}

[JsonSerializable(typeof(MaterialInfo))]
[JsonSourceGenerationOptions(Converters = [typeof(ColorConverter)])]
internal partial class MaterialJsonContext : JsonSerializerContext;
