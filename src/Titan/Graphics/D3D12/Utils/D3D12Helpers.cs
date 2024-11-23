using System.Diagnostics;
using Titan.Core;
using Titan.Core.Maths;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using static Titan.Platform.Win32.Win32Common;
using static Titan.Platform.Win32.D3D12.D3D12_COMPARISON_FUNC;
using static Titan.Platform.Win32.D3D12.D3D12_FILTER;
using static Titan.Platform.Win32.D3D12.D3D12_STATIC_BORDER_COLOR;
using static Titan.Platform.Win32.D3D12.D3D12_TEXTURE_ADDRESS_MODE;
using static Titan.Platform.Win32.D3D12.D3D12Constants;


namespace Titan.Graphics.D3D12.Utils;

internal static class D3D12Helpers
{
    private static readonly D3D12_BLEND_DESC[] BlendStateDescs = new D3D12_BLEND_DESC[(int)BlendStateType.Count];
    private static readonly Inline4<D3D12_HEAP_PROPERTIES> _heaps;
    static unsafe D3D12Helpers()
    {
        // set up the blend states

        {
            ref var blendDesc = ref BlendStateDescs[(int)BlendStateType.Disabled];
            blendDesc.RenderTarget[0].BlendEnable = 0;
            blendDesc.RenderTarget[0].BlendOp = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD;
            blendDesc.RenderTarget[0].BlendOpAlpha = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD;
            blendDesc.RenderTarget[0].DestBlend = D3D12_BLEND.D3D12_BLEND_INV_SRC_ALPHA;
            blendDesc.RenderTarget[0].DestBlendAlpha = D3D12_BLEND.D3D12_BLEND_ONE;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE.D3D12_COLOR_WRITE_ENABLE_ALL;
            blendDesc.RenderTarget[0].SrcBlend = D3D12_BLEND.D3D12_BLEND_SRC_ALPHA;
            blendDesc.RenderTarget[0].SrcBlendAlpha = D3D12_BLEND.D3D12_BLEND_ONE;
        }
        {
            ref var blendDesc = ref BlendStateDescs[(int)BlendStateType.AlphaBlend];
            blendDesc.RenderTarget[0].BlendEnable = 1;
            blendDesc.RenderTarget[0].BlendOp = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD;
            blendDesc.RenderTarget[0].BlendOpAlpha = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD;
            blendDesc.RenderTarget[0].DestBlend = D3D12_BLEND.D3D12_BLEND_INV_SRC_ALPHA;
            blendDesc.RenderTarget[0].DestBlendAlpha = D3D12_BLEND.D3D12_BLEND_ONE;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE.D3D12_COLOR_WRITE_ENABLE_ALL;
            blendDesc.RenderTarget[0].SrcBlend = D3D12_BLEND.D3D12_BLEND_SRC_ALPHA;
            blendDesc.RenderTarget[0].SrcBlendAlpha = D3D12_BLEND.D3D12_BLEND_ONE;
        }

        {
            ref var blendDesc = ref BlendStateDescs[(int)BlendStateType.Additive];
            blendDesc.RenderTarget[0].BlendEnable = 1;
            blendDesc.RenderTarget[0].BlendOp = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD;
            blendDesc.RenderTarget[0].BlendOpAlpha = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD;
            blendDesc.RenderTarget[0].DestBlend = D3D12_BLEND.D3D12_BLEND_ONE;
            blendDesc.RenderTarget[0].DestBlendAlpha = D3D12_BLEND.D3D12_BLEND_ZERO;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D12_COLOR_WRITE_ENABLE.D3D12_COLOR_WRITE_ENABLE_ALL;
            blendDesc.RenderTarget[0].SrcBlend = D3D12_BLEND.D3D12_BLEND_ONE;
            blendDesc.RenderTarget[0].SrcBlendAlpha = D3D12_BLEND.D3D12_BLEND_ONE;
        }
#if DEBUG
        Debug.Assert(Enum.GetValues(typeof(BlendStateType)).Length == 4, "Added new blend state descs but didn't update this code.");
#endif

        _heaps[(int)D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD] = new D3D12_HEAP_PROPERTIES
        {
            Type = D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD,
            CPUPageProperty = D3D12_CPU_PAGE_PROPERTY.D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
            MemoryPoolPreference = D3D12_MEMORY_POOL.D3D12_MEMORY_POOL_UNKNOWN,
        };
        _heaps[(int)D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT] = new D3D12_HEAP_PROPERTIES
        {
            Type = D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT,
            CPUPageProperty = D3D12_CPU_PAGE_PROPERTY.D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
            MemoryPoolPreference = D3D12_MEMORY_POOL.D3D12_MEMORY_POOL_UNKNOWN,
        };
        _heaps[(int)D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_READBACK] = new D3D12_HEAP_PROPERTIES()
        {
            Type = D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_READBACK,
            CPUPageProperty = D3D12_CPU_PAGE_PROPERTY.D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
            MemoryPoolPreference = D3D12_MEMORY_POOL.D3D12_MEMORY_POOL_UNKNOWN,
        };

    }
    public static D3D12_BLEND_DESC GetBlendState(BlendStateType type)
    {
        Debug.Assert((int)type <= BlendStateDescs.Length && (int)type >= 0);
        return BlendStateDescs[(int)type];

    }

    public static unsafe D3D12_HEAP_PROPERTIES* GetHeap(D3D12_HEAP_TYPE type)
    {
        Debug.Assert(type != 0 && type != D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_CUSTOM);
        return _heaps.GetPointer((int)type);
    }
    public static void InitDescriptorRanges(Span<D3D12_DESCRIPTOR_RANGE1> ranges, D3D12_DESCRIPTOR_RANGE_TYPE type, uint registerStart = 0, uint space = 0)
    {
        for (var i = 0; i < ranges.Length; ++i)
        {
            ranges[i] = new D3D12_DESCRIPTOR_RANGE1
            {
                BaseShaderRegister = registerStart,
                Flags = D3D12_DESCRIPTOR_RANGE_FLAGS.D3D12_DESCRIPTOR_RANGE_FLAG_DESCRIPTORS_VOLATILE,
                NumDescriptors = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND,
                OffsetInDescriptorsFromTableStart = 0,
                RangeType = type,
                RegisterSpace = (uint)i + space
            };
        }
    }

    public static unsafe void SetName(void* resource, ReadOnlySpan<char> name)
        => SetName((ID3D12Resource*)resource, name);
    public static unsafe void SetName(ID3D12Resource* resource, ReadOnlySpan<char> name)
    {
        //NOTE(Jens): can we use a common interface for this maybe? or just create overloads?
        fixed (char* namePtr = name)
        {
            var hr = resource->SetName(namePtr);
            Debug.Assert(SUCCEEDED(hr), $"Failed to set the name for resource {name} with HRESULT {hr}");
        }
    }

    public static D3D12_STATIC_SAMPLER_DESC CreateStaticSamplerDesc(SamplerState state, uint register, uint registerSpace, D3D12_SHADER_VISIBILITY visibiliy = D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_ALL) =>
        state switch
        {
            SamplerState.Linear => new D3D12_STATIC_SAMPLER_DESC
            {
                AddressU = D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                AddressV = D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                AddressW = D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                BorderColor = D3D12_STATIC_BORDER_COLOR_OPAQUE_BLACK,
                ComparisonFunc = D3D12_COMPARISON_FUNC_NEVER,
                Filter = D3D12_FILTER_COMPARISON_MIN_MAG_MIP_LINEAR,
                MaxAnisotropy = 1,
                MaxLOD = D3D12_FLOAT32_MAX,
                MinLOD = 0,
                MipLODBias = 0,
                ShaderVisibility = visibiliy,
                RegisterSpace = registerSpace,
                ShaderRegister = register
            },
            SamplerState.Point => new D3D12_STATIC_SAMPLER_DESC
            {
                AddressU = D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                AddressV = D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                AddressW = D3D12_TEXTURE_ADDRESS_MODE_CLAMP,
                BorderColor = D3D12_STATIC_BORDER_COLOR_OPAQUE_BLACK,
                ComparisonFunc = D3D12_COMPARISON_FUNC_NEVER,
                Filter = D3D12_FILTER_COMPARISON_MIN_MAG_MIP_POINT,
                MaxAnisotropy = 1,
                MaxLOD = D3D12_FLOAT32_MAX,
                MinLOD = 0,
                MipLODBias = 0,
                ShaderVisibility = visibiliy,
                RegisterSpace = registerSpace,
                ShaderRegister = register
            },
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    public static unsafe void InitSquareIndexBuffer(ushort* buffer, uint size) => InitSquareIndexBuffer(new Span<ushort>(buffer, (int)size));
    public static void InitSquareIndexBuffer(Span<ushort> indices)
    {
        Debug.Assert(indices.Length == 6, "Index buffer only supports a size of 6 elements.");
        IndexBufferSquare6.CopyTo(indices);
    }

    //public static ReadOnlySpan<ushort> IndexBufferSquare6 => [0, 1, 2, 3, 0, 2];
    public static ReadOnlySpan<ushort> IndexBufferSquare6 => [0, 1, 2, 0, 2, 3];


    public static unsafe D3D12_RESOURCE_BARRIER Transition(ID3D12Resource* resource, D3D12_RESOURCE_STATES before, D3D12_RESOURCE_STATES after) =>
        new()
        {
            Flags = D3D12_RESOURCE_BARRIER_FLAGS.D3D12_RESOURCE_BARRIER_FLAG_NONE,
            Type = D3D12_RESOURCE_BARRIER_TYPE.D3D12_RESOURCE_BARRIER_TYPE_TRANSITION,
            Transition = new()
            {
                StateAfter = after,
                StateBefore = before,
                Subresource = 0,
                pResource = resource
            }
        };

    public static unsafe D3D12_CLEAR_VALUE ClearColor(DXGI_FORMAT format, in Color color)
    {
        D3D12_CLEAR_VALUE value = default;
        value.Format = format;
        *(Color*)value.Color = color;
        return value;
    }
}
