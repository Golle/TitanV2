using System.Buffers;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Tools.AssetProcessor.Metadata;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.Tools.AssetProcessor.Parsers.WavefrontObj;

namespace Titan.Tools.AssetProcessor.Processors.Models;
internal class MtlProcessor : AssetProcessor<MtlMetadata>
{
    protected override async Task OnProcess(MtlMetadata metadata, IAssetDescriptorContext context)
    {
        var content = await File.ReadAllLinesAsync(metadata.ContentFileFullPath);

        var materials = MtlParser.Parse(content);

        if (materials.Length == 0)
        {
            Logger.Warning<MtlProcessor>($"No materials in the file. Path = {metadata.ContentFileRelativePath}");
            return;
        }

        var buffer = ArrayPool<byte>.Shared.Rent((int)(MemoryUtils.MegaByte * 5));
        List<AssetFileMetadata> dependencies = new();
        var names = new string?[materials.Length];
        try
        {
            TitanBinaryWriter writer = new(buffer);

            for(var i = 0; i < materials.Length;++i)
            {
                ref readonly var material = ref materials[i];
                writer.Write(material.Diffuse);
                writer.WriteBoolAsByte(material.DiffuseMap != null);
                if (material.DiffuseMap != null)
                {
                    var diffuseMap = context
                        .GetMetadataByFilename(material.DiffuseMap)
                        .ToArray();

                    if (diffuseMap.Length == 0)
                    {
                        context.AddDiagnostics(DiagnosticsLevel.Error, $"Could not find the dependency specified in the material file. Path = {metadata.ContentFileRelativePath}. Dependency = {material.DiffuseMap}");
                        return;
                    }

                    if (diffuseMap.Length > 1)
                    {
                        context.AddDiagnostics(DiagnosticsLevel.Error, $"Multiple files with the same name is not supported right now. Path = {metadata.ContentFileRelativePath}. Dependency = {material.DiffuseMap}");
                        return;
                    }

                    dependencies.Add(diffuseMap[0]);
                }
                names[i] = material.Name;
            }

            var descriptor = new MaterialDescriptor
            {
                MaterialCount = (byte)materials.Length
            };

            metadata.Dependencies = dependencies;
            metadata.MaterialNames = names;
            if (!context.TryAddMaterial(descriptor, writer.GetData(), metadata))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the material to the context. Path = {metadata.ContentFileRelativePath}");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
