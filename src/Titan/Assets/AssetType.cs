namespace Titan.Assets;

/// <summary>
/// All types that we currently support. Maybe this should be extended with user defined types.
/// </summary>
public enum AssetType
{
    Texture2D = 1,
    Model3D = 2,
    ComputeShader = 3,
    VertexShader = 4,
    PixelShader = 5,
    Font = 6,

    //NOTE(Jens): A custom type will not have any descriptors with it, everything will be inside the file and have to be read my the loader.
    CustomType = 100 // Register loaders/types with a ID greater than 100 for custom
}