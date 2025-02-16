using Titan.Assets;
using Titan.Core;
using Titan.Core.Strings;
using Titan.Graphics;
using Titan.Platform.Win32;
using Titan.Rendering.Resources;

namespace Titan.Rendering;

public struct RenderPass
{
    public StringRef Name;

    public Handle<RootSignature> RootSignature;

    public TitanArray<Handle<PipelineState>> PipelineStates;

    public TitanArray<AssetHandle<ShaderAsset>> VertexShaders;
    public TitanArray<AssetHandle<ShaderAsset>> PixelShaders;

    public TitanArray<Handle<Texture>> Inputs;
    public TitanArray<Handle<Texture>> Outputs;
    public Handle<Texture> DepthBuffer;

    public CommandList CommandList;
    public unsafe delegate*<ReadOnlySpan<Ptr<Texture>>, TitanOptional<Texture>, in CommandList, void> ClearFunction;
    public BlendStateType BlendState;
    public CullMode CullMode;
    public FillMode FillMode;
    public DepthBufferMode DepthBufferMode;
    public PrimitiveTopology Topology;
    public Viewport Viewport;
    public Rect ScissorRect;
    public sbyte Order;
    public bool UseDefaultViewport;


#if HOT_RELOAD_ASSETS
    // to support hot reload
    internal TitanArray<ulong> ShaderHashes;
#endif
}
