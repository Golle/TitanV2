using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.Tools.AssetProcessor.Processors.Audio;

namespace Titan.Tools.AssetProcessor.Metadata;

internal class MetadataBuilder
{
    public AssetFileMetadata? CreateFromContent(string filename, Stream stream)
    {
        var fileSize = stream.Length;
        var fileExtension = Path.GetExtension(filename).ToLowerInvariant();
        var fileName = Path.GetFileNameWithoutExtension(filename);

        AssetFileMetadata? metadata = fileExtension switch
        {
            ".png" or ".jpg" or ".aseprite" => new ImageMetadata(),
            ".obj" => new ObjModelMetadata(),
            ".mtl" => new MtlMetadata(),
            ".hlsl" => new ShaderMetadata(),
            ".ttf" => new FontMetadata(),
            ".shaderconf" => new ShaderInfoMetadata(),
            ".ogg" or ".wav" or ".mp3" => new AudioMetadata(),
            ".tmat" => new MaterialMetadata(),
            _ => null
        };
        if (metadata == null)
        {
            return null;
        }
        metadata.FileSize = fileSize;
        metadata.Name = StringHelper.ToPropertyName(fileName);
        return metadata;
    }
}
