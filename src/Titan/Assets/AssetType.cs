namespace Titan.Assets;

/// <summary>
/// All types that we currently support. Maybe this should be extended with user defined types.
/// </summary>
public enum AssetType
{
    Texture = 1,
    Sprite = 2,
    Mesh = 3,
    Shader = 4,
    Font = 5,
    Material = 6,
    Audio = 7,
    
    //NOTE(Jens): A custom type will not have any descriptors with it, everything will be inside the file and have to be read my the loader.
    CustomType = 100 // Register loaders/types with a ID greater than 100 for custom
    ,

}
