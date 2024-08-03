using System.Diagnostics.CodeAnalysis;
using Titan.Core;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Rendering.Resources;

namespace Titan.Rendering.Experimental;

public ref struct RootSignatureBuilder
{
    private Inline8<ConstantsInfo> _constants;
    private Inline8<SamplerInfo> _samplers;
    private Inline8<ConstantBufferInfo> _constantBuffers;
    private Inline8<DescriptorRangesInfo> _ranges;
    private byte _numberOfConstants;
    private byte _numberOfSamplers;
    private byte _numberOfConstantBuffers;
    private byte _numberOfRanges;


    [UnscopedRef]
    public ref RootSignatureBuilder WithConstant(byte count, ShaderVisibility visibility = ShaderVisibility.All, byte register = 0, byte space = 0)
    {
        _constants[_numberOfConstants++] = new()
        {
            Space = space,
            Visibility = visibility,
            Register = register,
            Count = count
        };
        return ref this;
    }

    [UnscopedRef]
    public ref RootSignatureBuilder WithSampler(SamplerState state, ShaderVisibility visibility = ShaderVisibility.All, byte register = 0, byte space = 0)
    {
        _samplers[_numberOfSamplers++] = new()
        {
            Space = space,
            Register = register,
            Visibility = visibility,
            State = state
        };
        return ref this;
    }

    [UnscopedRef]
    public ref RootSignatureBuilder WithConstantBuffer(ConstantBufferFlags flags, ShaderVisibility visibility = ShaderVisibility.All, byte register = 0, byte space = 0)
    {
        _constantBuffers[_numberOfConstantBuffers++] = new()
        {
            Space = space,
            Visibility = visibility,
            Register = register,
            Flags = flags
        };
        return ref this;
    }

    [UnscopedRef]
    public ref RootSignatureBuilder WithRanges(byte count, ShaderDescriptorRangeType type = ShaderDescriptorRangeType.ShaderResourceView, byte register = 0, byte space = 0)
    {
        _ranges[_numberOfRanges++] = new()
        {
            Space = space,
            Register = register,
            Count = count,
            Type = type
        };
        return ref this;
    }

    public CreateRootSignatureArgs Build() =>
        new()
        {
            Constants = _constants.AsReadOnlySpan()[.._numberOfConstants],
            Samplers = _samplers.AsReadOnlySpan()[.._numberOfSamplers],
            Ranges = _ranges.AsReadOnlySpan()[.._numberOfRanges],
            ConstantBuffers = _constantBuffers.AsReadOnlySpan()[.._numberOfConstantBuffers],
        };
}
