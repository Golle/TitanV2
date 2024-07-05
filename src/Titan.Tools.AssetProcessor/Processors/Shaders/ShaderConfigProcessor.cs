using System.Text.Json;
using System.Text.Json.Serialization;
using Titan.Assets.Types;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Shaders;
internal class ShaderConfigProcessor : AssetProcessor<ShaderConfigMetadata>
{
    protected override async Task OnProcess(ShaderConfigMetadata metadata, IAssetDescriptorContext context)
    {
        var path = metadata.ContentFileFullPath;
        try
        {
            await using var fileStream = File.OpenRead(path);
            var config = await JsonSerializer.DeserializeAsync(fileStream, ShaderConfigJsonContext.Default.ShaderConfigFile);
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

            metadata.Dependencies = [vertexShaderMetadata!, pixelShaderMetadata!];

            context.TryAddShaderConfig(new ShaderConfigDescriptor
            {
                NumberOfDescriptors = 1,
                NumberOfParameters = 2,
                NumberOfSamplers = 3
            }, metadata);
        }
        catch (Exception e)
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to process the shader config file. Path = {path}. Exception = {e.GetType().Name} Message = {e.Message}");
        }
    }
}

internal record ShaderConfigFile(ShaderConfig? VertexShader, ShaderConfig? PixelShader);
internal record ShaderConfig(string Name);


[JsonSourceGenerationOptions]
[JsonSerializable(typeof(ShaderConfigFile))]
internal partial class ShaderConfigJsonContext : JsonSerializerContext;

