using Titan.Assets.Types;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Shaders;
internal class ShaderProcessor : AssetProcessor<ShaderMetadata>
{
    private readonly ShaderCompiler _compiler = new();
    protected override Task OnProcess(ShaderMetadata metadata, IAssetDescriptorContext context) =>
        Task.Run(() =>
        {
            var result = _compiler.CompileShader(metadata.ContentFileFullPath, metadata.EntryPoint, metadata.ShaderType, metadata.ShaderVersion);
            if (result.Succeeded)
            {
                var bytes = result.GetByteCode();
                var type = metadata.ShaderType;
                var shaderDescriptor = new ShaderDescriptor
                {
                    Type = type
                };
                if (!context.TryAddShader(shaderDescriptor, bytes, metadata))
                {
                    context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the shader to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
                }
            }
            else
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to compile shader. Id = {metadata.Id}. Path = {metadata.ContentFileRelativePath} Error = {result.Error}");
            }
        });
}
