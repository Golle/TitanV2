using System.Text.Json;
using System.Text.Json.Serialization;
using Titan.Assets.Types;
using Titan.Core;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Rendering.Resources;
using Titan.Tools.AssetProcessor.Metadata.Types;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

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

            if (!Validate(config, context, path))
            {
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
            var bytesWritten = WriteDataToBuffer(config!, buffer);
            context.TryAddShaderConfig(new ShaderConfigDescriptor
            {
                NumberOfDescriptorRanges = (byte)config.Parameters.DescriptorRanges.Length,
                NumberOfSamplers = (byte)config.Samplers.Length,
                NumberOfConstantBuffers = (byte)config.Parameters.ConstantBuffers.Length,
                NumberOfConstants = (byte)config.Parameters.Constants.Length
            }, buffer[..bytesWritten], metadata);
        }
        catch (Exception e)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to process the shader config file. Path = {path}. Exception = {e.GetType().Name} Message = {e.Message}");
        }
    }

    private static bool Validate(ShaderConfigFile? config, IAssetDescriptorContext context, string path)
    {
        if (config == null)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to deserilize the shader config. Path = {path}");
            return false;
        }

        if (config.Parameters is null)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"No Parameters specified. Path = {path}");
            return false;
        }

        if (config.VertexShader is null)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"No VertexShader specified. Path = {path}");
            return false;
        }

        if (config.PixelShader is null)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"No PixelShader specified. Path = {path}");
            return false;
        }

        //NOTE(Jens): Not sure if this will be correct :) 
        var registers = config.Parameters.ConstantBuffers.Select(static c => (c.Register, c.Space))
            .Concat(config.Parameters.Constants.Select(static c => (c.Register, c.Space)))
            .ToArray();

        if (registers.Length != registers.Distinct().Count())
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, "Conflicting registers for Constants and ConstantBuffers.");
            return false;
        }

        return true;
    }

    private static int WriteDataToBuffer(ShaderConfigFile config, Span<byte> buffer)
    {

        /*
         * Order of data
         * Samplers (4 bytes, State+Visibility+Register+Space)
         * DescriptorRanges(4 bytes, Count+Type+Register+Space)
         * ConstantBuffer (4 bytes, Flags+Visibility+Register+Space)
         * Constants(4 bytes, Count+Visibility+Register+Space)
         */

        TitanBinaryWriter writer = buffer;
        foreach (var sampler in config.Samplers)
        {
            writer.Write(new SamplerInfo
            {
                State = sampler.State,
                Visibility = sampler.Visibility,
                Register = (byte)sampler.Register,
                Space = (byte)sampler.Space,
            });
        }
        foreach (var range in config.Parameters.DescriptorRanges)
        {
            writer.Write(new DescriptorRangesInfo
            {
                Count = (byte)range.Count,
                Type = range.Type,
                Register = (byte)range.Register,
                Space = (byte)range.Space
            });
        }

        foreach (var constantBuffer in config.Parameters.ConstantBuffers)
        {
            writer.Write(new ConstantBufferInfo
            {
                Flags = constantBuffer.Flags,
                Visibility = constantBuffer.Visibility,
                Register = (byte)constantBuffer.Register,
                Space = (byte)constantBuffer.Space
            });
        }

        foreach (var constant in config.Parameters.Constants)
        {
            writer.Write(new ConstantsInfo
            {
                Count = (byte)constant.Count,
                Visibility = constant.Visibility,
                Register = (byte)constant.Register,
                Space = (byte)constant.Space
            });
        }

        return writer.Length;
    }
}

internal record ShaderConfigFile
{
    public ShaderSamplerConfig[] Samplers { get; set; } = [];
    public ShaderParameters? Parameters { get; init; }
    public ShaderConfig? VertexShader { get; init; }
    public ShaderConfig? PixelShader { get; init; }
}

internal record ShaderConfig(string Name);
internal record ShaderSamplerConfig(SamplerState State, ShaderVisibility Visibility, int Register, int Space);

internal record ShaderParameters
{
    public ShaderConstants[] Constants { get; set; } = [];
    public ShaderConstantBuffers[] ConstantBuffers { get; set; } = [];
    public ShaderDescriptorRange[] DescriptorRanges { get; set; } = [];
}
internal record ShaderDescriptorRange(ShaderDescriptorRangeType Type, int Count, int Register, int Space);
internal record ShaderConstantBuffers(ConstantBufferFlags Flags, ShaderVisibility Visibility, int Register, int Space);
internal record ShaderConstants(uint Count, ShaderVisibility Visibility, int Register, int Space);


[JsonSourceGenerationOptions(JsonSerializerDefaults.General, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ShaderConfigFile))]
internal partial class ShaderInfoJsonContext : JsonSerializerContext;

