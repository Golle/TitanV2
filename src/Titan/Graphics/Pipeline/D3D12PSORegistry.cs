using System.Diagnostics;
using Titan.Assets;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Utils;
using Titan.Graphics.Resources;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Services;
using Titan.Systems;

namespace Titan.Graphics.Pipeline;

public record struct DepthStencilDesc
{
    public bool DepthEnabled { get; init; }
    public bool StencilEnabled { get; init; }
    public DXGI_FORMAT Format { get; init; }
}

public record struct PipelineStateArgs
{
    public ComPtr<ID3D12RootSignature> RootSignature { get; init; }
    public AssetDescriptor? PixelShader { get; init; }
    public AssetDescriptor? VertexShader { get; init; }
    public DepthStencilDesc? DepthStencil { get; init; }

    //TODO(Jens): Replace this with something nicer.
    public D3D12_RT_FORMAT_ARRAY RenderTargets { get; init; }
}


internal unsafe struct D3D12CachedPipelineState
{
    public int HashCode;

    //TODO(Jens): Convert this to bitmask later
    public bool Depth;
    public bool Stencil;
    public bool Loaded;
    public DXGI_FORMAT DepthFormat;
    public D3D12_RT_FORMAT_ARRAY RenderTargets;

    public ComPtr<ID3D12PipelineState> PipelineStateObject;
    public ComPtr<ID3D12RootSignature> RootSignature;

    public AssetHandle<ShaderAsset> VertexShader;
    public AssetHandle<ShaderAsset> PixelShader;
}

[UnmanagedResource]
internal unsafe partial struct D3D12PipelineStateObjectRegistry
{
    private static readonly object _lock = new();

    private TitanArray<D3D12CachedPipelineState> _pipelineCache;
    private uint _count;

    private AssetsManager _assetsManager;
    private UnmanagedResource<D3D12Device> _device;

    [System(SystemStage.Init)]
    public static void Init(ref D3D12PipelineStateObjectRegistry registry, AssetsManager assetsManager, IMemoryManager memoryManager, IConfigurationManager configurationManager, ServiceRegistry services, UnmanagedResourceRegistry resources)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();

        if (!memoryManager.TryAllocArray(out registry._pipelineCache, config.Resources.MaxPipelineStates))
        {
            Logger.Error<D3D12PipelineStateObjectRegistry>($"Failed to allocate array for pipeline states. Size = {sizeof(D3D12CachedPipelineState) * config.Resources.MaxPipelineStates} bytes.");
            return;
        }

        //NOTE(Jens): Not sure if we should do this.
        registry._assetsManager = assetsManager;
        registry._device = resources.GetResourceHandle<D3D12Device>();
    }


    public D3D12CachedPipelineState* CreatePipelineState(in PipelineStateArgs args)
    {
        Debug.Assert(args.RootSignature.IsValid);
        Debug.Assert(args.RenderTargets.NumRenderTargets > 0);

        var hash = args.GetHashCode();
        lock (_lock)
        {
            var existing = GetExisting(hash);
            if (existing != null)
            {
                return existing;
            }

            var pso = _pipelineCache.GetPointer(_count++);

            //NOTE(Jens): This will be sorted by the "ShaderConfig/ShaderInfo"
            pso->VertexShader = args.VertexShader.HasValue
                ? _assetsManager.Load<ShaderAsset>(args.VertexShader.Value)
                : AssetHandle<ShaderAsset>.Invalid;

            pso->PixelShader = args.PixelShader.HasValue
                ? _assetsManager.Load<ShaderAsset>(args.PixelShader.Value)
                : AssetHandle<ShaderAsset>.Invalid;

            if (args.DepthStencil.HasValue)
            {
                var depthArgs = args.DepthStencil.Value;

                pso->Depth = depthArgs.DepthEnabled;
                pso->Stencil = depthArgs.StencilEnabled;
                pso->DepthFormat = depthArgs.Format == DXGI_FORMAT.DXGI_FORMAT_UNKNOWN
                    ? DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT
                    : depthArgs.Format;
            }

            pso->RenderTargets = args.RenderTargets;
            pso->RootSignature = args.RootSignature;
            return pso;
        }
    }

    private D3D12CachedPipelineState* GetExisting(int hashCode)
    {
        for (var i = 0; i < _count; ++i)
        {
            if (_pipelineCache[i].HashCode == hashCode)
            {
                return _pipelineCache.GetPointer(i);
            }
        }

        return null;
    }

    [System]
    public static void Update(ref D3D12PipelineStateObjectRegistry registry, AssetsManager assetsManager)
    {
        for (var i = 0; i < registry._count; ++i)
        {
            var pso = registry._pipelineCache.GetPointer(i);
            if (pso->Loaded)
            {
                continue;
            }

            if (pso->VertexShader.IsValid && !assetsManager.IsLoaded(pso->VertexShader))
            {
                continue;
            }

            if (pso->PixelShader.IsValid && !assetsManager.IsLoaded(pso->PixelShader))
            {
                continue;
            }

            var psoStream = new D3D12PipelineSubobjectStream()
                .Blend(D3D12Helpers.GetBlendState(BlendStateType.AlphaBlend)) //TODO(Jens): Should be configurable, but keep it simple for now.
                .Topology(D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE)
                .Razterizer(D3D12_RASTERIZER_DESC.Default() with
                {
                    CullMode = D3D12_CULL_MODE.D3D12_CULL_MODE_BACK
                })
                .RenderTargetFormat(pso->RenderTargets)
                .RootSignature(pso->RootSignature)
                .Sample(new DXGI_SAMPLE_DESC
                {
                    Count = 1,
                    Quality = 0
                })
                .SampleMask(uint.MaxValue)
                ;

            // set the pixel shader
            if (pso->PixelShader.IsValid)
            {
                var shader = assetsManager.Get(pso->PixelShader);
                Debug.Assert(shader.ShaderType is ShaderType.Pixel);
                psoStream = psoStream.PS(new()
                {
                    BytecodeLength = shader.ShaderByteCode.Size,
                    pShaderBytecode = shader.ShaderByteCode.AsPointer()
                });
            }

            // set the vertex shader
            if (pso->VertexShader.IsValid)
            {
                var shader = assetsManager.Get(pso->VertexShader);
                Debug.Assert(shader.ShaderType is ShaderType.Vertex);
                psoStream = psoStream.VS(new()
                {
                    BytecodeLength = shader.ShaderByteCode.Size,
                    pShaderBytecode = shader.ShaderByteCode.AsPointer()
                });
            }


            if (pso->Depth)
            {
                D3D12_DEPTH_STENCIL_DESC depthStencilDesc = new()
                {
                    DepthEnable = 1,
                    DepthWriteMask = D3D12_DEPTH_WRITE_MASK.D3D12_DEPTH_WRITE_MASK_ALL,
                    DepthFunc = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_LESS,
                    StencilEnable = pso->Stencil ? 1 : 0
                };

                psoStream = psoStream
                    .DepthStencil(depthStencilDesc)
                    .DepthStencilfFormat(pso->DepthFormat);

            }

            pso->PipelineStateObject = registry._device.AsRef.CreatePipelineStateObject(psoStream.AsStreamDesc());
            Debug.Assert(pso->PipelineStateObject.IsValid);

            pso->Loaded = true;
        }
    }

    public D3D12CachedPipelineState* state;
    public bool done;
    
    [System]
    public static void T(ref D3D12PipelineStateObjectRegistry reg, in D3D12ResourceManager resourceManager)
    {

        if (reg.state == null)
        {
            var ranges = stackalloc D3D12_DESCRIPTOR_RANGE1[6];
            D3D12Helpers.InitDescriptorRanges(new Span<D3D12_DESCRIPTOR_RANGE1>(ranges, 6), D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_SRV);

            ReadOnlySpan<D3D12_ROOT_PARAMETER1> rootParameters = [
                CD3DX12_ROOT_PARAMETER1.AsDescriptorTable(6, ranges),
                CD3DX12_ROOT_PARAMETER1.AsConstantBufferView(0 , 0,D3D12_ROOT_DESCRIPTOR_FLAGS.D3D12_ROOT_DESCRIPTOR_FLAG_DATA_STATIC)
            ];
            ReadOnlySpan<D3D12_STATIC_SAMPLER_DESC> samplers = [
                D3D12Helpers.CreateStaticSamplerDesc(SamplerState.Linear, 0, 0, D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_PIXEL),
                D3D12Helpers.CreateStaticSamplerDesc(SamplerState.Linear, 1, 0, D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_PIXEL)
            ];
            var root = reg._device.AsPointer->CreateRootSignature(D3D12_ROOT_SIGNATURE_FLAGS.D3D12_ROOT_SIGNATURE_FLAG_NONE, rootParameters, samplers);

            var pso = reg.CreatePipelineState(new PipelineStateArgs
            {
                DepthStencil = new DepthStencilDesc
                {
                    DepthEnabled = true,
                    StencilEnabled = false
                },
                PixelShader = EngineAssetsRegistry.DebugTextPixelShader,
                VertexShader = EngineAssetsRegistry.DebugTextVertexShader,
                RenderTargets = new(DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM),
                RootSignature = root
            });
            reg.state = pso;
        }
        else if (!reg.done && reg.state->Loaded)
        {
            reg.done = true;
            Logger.Error<D3D12PipelineStateObjectRegistry>("All done!");
        }
        else if (!reg.done)
        {
            Logger.Error<D3D12PipelineStateObjectRegistry>("still loading...");
        }
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(ref D3D12PipelineStateObjectRegistry registry, IMemoryManager memoryManager)
    {
        if (registry._pipelineCache.IsValid)
        {
            memoryManager.FreeArray(ref registry._pipelineCache);
        }
    }
}

