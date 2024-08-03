using Titan.Assets;
using Titan.Core;
using Titan.Core.Strings;
using Titan.Rendering.Resources;

namespace Titan.Rendering;

public struct RenderPass
{
    public StringRef Name;

    public Handle<RootSignature> RootSignature;
    public Handle<PipelineState> PipelineState;
    public AssetHandle<ShaderAsset> VertexShader;
    public AssetHandle<ShaderAsset> PixelShader;

    public TitanArray<Handle<Texture>> Inputs;
    public TitanArray<Handle<Texture>> Outputs;

    public CommandList CommandList;
}
