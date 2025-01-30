using Titan.Core;
using Titan.Core.Maths;
using Titan.Graphics.D3D12;
using Titan.Rendering;

namespace Titan.Materials;

public ref struct CreateMaterialArgs
{
    public Handle<Texture> AlbedoTexture { get; init; }
    public Color Color { get; init; }
}

public readonly unsafe struct MaterialsManager
{
    private readonly MaterialsSystem* _system;
    private readonly D3D12ResourceManager* _resourceManager;

    internal MaterialsManager(MaterialsSystem* system, D3D12ResourceManager* resourceManager)
    {
        _system = system;
        _resourceManager = resourceManager;
    }

    public Handle<MaterialData> CreateMaterial(in CreateMaterialArgs args)
    {
        //TODO(Jens): Rework this so we can use the Texture handle. This is garbage.
        var texture = args.AlbedoTexture.IsValid ? _resourceManager->Access(args.AlbedoTexture) : null;

        return _system->CreateMaterial(texture, args.Color);
    }

    public void DestroyMaterial(Handle<MaterialData> handle)
        => _system->DestroyMaterial(handle);
}
