using System.Text.Json;
using System.Text.Json.Serialization;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Graphics.Resources;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Shaders;
internal class ShaderInfoProcessor : AssetProcessor<ShaderInfoMetadata>
{
    protected override async Task OnProcess(ShaderInfoMetadata metadata, IAssetDescriptorContext context)
    {
        var path = metadata.ContentFileFullPath;
        try
        {
            await using var fileStream = File.OpenRead(path);
            var config = await JsonSerializer.DeserializeAsync(fileStream, ShaderInfoJsonContext.Default.ShaderConfigFile);
            if (config == null)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to deserilize the shader config. Path = {path}");
                return;
            }

            if (config.VertexShader is null)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"No VertexShader specified. Path = {path}");
                return;
            }

            if (config.PixelShader is null)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"No PixelShader specified. Path = {path}");
                return;
            }

            var vertexShaderMetadata = context.GetMetadataByType<ShaderMetadata>().SingleOrDefault(s => s.Name == config.VertexShader.Name);
            var pixelShaderMetadata = context.GetMetadataByType<ShaderMetadata>().SingleOrDefault(s => s.Name == config.PixelShader.Name);
            if (vertexShaderMetadata == null)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"No metadata file found for Vertex Shader. Name = {config.VertexShader.Name}");
                return;
            }

            if (pixelShaderMetadata == null)
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"No metadata file found for Pixel Shader. Name = {config.PixelShader.Name}");
                return;
            }

            metadata.Dependencies = [vertexShaderMetadata, pixelShaderMetadata];


            var buffer = new byte[1024];
            var bytesWritten = WriteDataToBuffer(config, buffer);
            context.TryAddShaderConfig(new ShaderConfigDescriptor
            {
                NumberOfDescriptorRanges = (byte)config.Parameters.DescriptorRanges.Length,
                NumberOfSamplers = (byte)config.Samplers.Length,
                NumberOfConstantBuffers = (byte)config.Parameters.ConstantBuffers
            }, buffer[..bytesWritten], metadata);
        }
        catch (Exception e)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to process the shader config file. Path = {path}. Exception = {e.GetType().Name} Message = {e.Message}");
        }
    }

    private static int WriteDataToBuffer(ShaderConfigFile config, Span<byte> buffer)
    {

        /*
         * Order of data
         * Samplers (2 bytes, State+Visibility)
         * ConstantBuffer (int)
         * DescriptorRanges(2 bytes, Count+Type)
         */

        TitanBinaryWriter writer = buffer;
        foreach (var sampler in config.Samplers)
        {
            writer.WriteByte((byte)sampler.State);
            writer.WriteByte((byte)sampler.Visibility);
        }
        foreach (var range in config.Parameters.DescriptorRanges)
        {
            writer.WriteByte((byte)range.Count);
            writer.WriteByte((byte)range.Type);
        }

        return writer.Length;
    }
}

internal record ShaderConfigFile(ShaderConfig? VertexShader, ShaderConfig? PixelShader, ShaderSamplerConfig[] Samplers, ShaderParameters Parameters);
internal record ShaderConfig(string Name);
internal record ShaderSamplerConfig(SamplerState State, ShaderVisibility Visibility);
internal record ShaderParameters(int ConstantBuffers, ShaderDescriptorRange[] DescriptorRanges);
internal record ShaderDescriptorRange(ShaderDescriptorRangeType Type, int Count);



[JsonSourceGenerationOptions(Converters = [
    typeof(JsonStringEnumConverter<ShaderVisibility>),
    typeof(JsonStringEnumConverter<ShaderDescriptorRangeType>),
    typeof(JsonStringEnumConverter<SamplerState>)
])]
[JsonSerializable(typeof(ShaderConfigFile))]
internal partial class ShaderInfoJsonContext : JsonSerializerContext;

