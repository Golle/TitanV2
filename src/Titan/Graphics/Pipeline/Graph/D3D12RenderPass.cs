using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Strings;
using Titan.Graphics.Resources;
using Titan.Platform.Win32.D3D;

namespace Titan.Graphics.Pipeline.Graph;

internal struct D3D12RenderPass
{
    public RenderPass RenderPass;
    public StringRef Identifier;

    public Inline4<Handle<Texture>> Inputs;
    public Inline4<Handle<Texture>> Outputs;
    public Handle<RootSignature> RootSignature;
    public Handle<PipelineState> PipelineState;

    public Handle<Texture> DepthBufferInput;
    public Handle<Texture> DepthBufferOutput;

    public AssetHandle<ShaderInfo> Shader;
    public byte InputCount;
    public byte OutputCount;
    public byte Group;

    public D3D_PRIMITIVE_TOPOLOGY Topology;
    
    public RenderPassType Type => RenderPass.Type;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<Handle<Texture>> GetOutputs() => Outputs.AsReadOnlySpan()[..OutputCount];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<Handle<Texture>> GetInputs() => Inputs.AsReadOnlySpan()[..InputCount];
}
