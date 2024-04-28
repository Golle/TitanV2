using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Metadata;

internal class MetadataBuilder
{
    public AssetFileMetadata? CreateFromContent(string filename, Stream stream)
    {
        var fileSize = stream.Length;
        var fileExtension = Path.GetExtension(filename);

        AssetFileMetadata? metadata = fileExtension switch
        {
            ".png" or ".jpg" => new ImageMetadata(),
            ".obj" => new ObjModelMetadata(),
            ".mtl" => new MtlMetadata(),
            ".aseprite" => new AsepriteMetadata(),
            ".hlsl" => new ShaderMetadata(),
            ".ttf" => new FontMetadata(),
            _ => null
        };
        if (metadata == null)
        {
            return null;
        }
        metadata.FileSize = fileSize;
        return metadata;
    }
}
