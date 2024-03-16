namespace Titan.Tools.AssetProcessor.Processors.Shaders;

internal interface IShaderCompiler
{
    bool IsSupported(ShaderVersion version);
    ShaderCompilationResult CompileShader(string filePath, string entryPoint, ShaderType type, ShaderVersion version);
}

