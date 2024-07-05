using Titan.Assets;
using Titan.Core;
using Titan.Core.Maths;

namespace Titan.Graphics.Resources;



[Asset(AssetType.Material)]
internal partial struct Material
{
    public Color DiffuseColor;

    public Handle<Texture> AlbedoMap;
    public Handle<Texture> NormalMap;
}


[AssetLoader<Material>]
internal partial struct MaterialLoader
{
    public bool Init(in AssetLoaderInitializer init)
    {
        throw new NotImplementedException();
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        throw new NotImplementedException();
    }

    public unsafe Material* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        throw new NotImplementedException();
    }

    public unsafe void Unload(Material* asset)
    {
        throw new NotImplementedException();
    }
}

