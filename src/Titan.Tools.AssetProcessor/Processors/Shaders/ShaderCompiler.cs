using Titan.Tools.AssetProcessor.Processors.Shaders.DXC;

namespace Titan.Tools.AssetProcessor.Processors.Shaders;

public class ShaderCompiler
{
    private readonly DxcCompiler _compiler = new();
    public ShaderCompilationResult CompileShader(string filePath, string entryPoint, ShaderType type, ShaderVersion version)
    {
        if (!_compiler.IsSupported(version))
        {
            throw new NotSupportedException($"The version {version} is not supported.");
        }
        return _compiler.CompileShader(filePath, entryPoint, type, version);
    }
}
