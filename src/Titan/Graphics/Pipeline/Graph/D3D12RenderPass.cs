using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Strings;
using Titan.Graphics.Rendering;
using Titan.Graphics.Resources;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.Pipeline.Graph;

internal struct D3D12RenderPass
{
    public RenderPass RenderPass;
    public StringRef Identifier;

    public Inline4<Handle<Texture>> Inputs;
    public Inline4<Handle<Texture>> Outputs;
    public Inline4<RenderTargetFormat> OutputFormats;

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

    internal CommandList CommandList;
    internal D3D12CachedResources CachedResources;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<Handle<Texture>> GetOutputs() => Outputs.AsReadOnlySpan()[..OutputCount];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<Handle<Texture>> GetInputs() => Inputs.AsReadOnlySpan()[..InputCount];
}

internal unsafe struct D3D12CachedResources
{
    public ID3D12RootSignature* RootSignature;
    public ID3D12PipelineState* PipelineState;
    public Inline4<D3D12_CPU_DESCRIPTOR_HANDLE> Outputs;
    public Inline4<D3D12_CPU_DESCRIPTOR_HANDLE> Inputs;

    //NOTE(Jens): Doing it this way can create states where we set  barriers between passes like: Common -> Writable -> Common ->Writeable
    //NOTE(Jens): Instead of just a single transition. This is a naive approach and have to be reworked.
    
    public Inline8<D3D12_RESOURCE_BARRIER> BarriersBegin;
    public Inline8<D3D12_RESOURCE_BARRIER> BarriersEnd;

    public byte BarriersBeginCount;
    public byte BarriersEndCount;

}
