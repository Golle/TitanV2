using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Titan.Core;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Rendering.Resources;

namespace Titan.Rendering;

public ref struct RootSignatureBuilder
{
    private Inline16<RootSignatureParameter> _parameters;
    private byte _numberOfParameters;

    [UnscopedRef]
    public ref RootSignatureBuilder WithConstant(byte count, ShaderVisibility visibility = ShaderVisibility.All, byte register = 0, byte space = 0) =>
        ref Add(new RootSignatureParameter
        {
            Type = RootSignatureParameterType.Constant,
            Space = space,
            Count = count,
            Register = register,
            Visibility = visibility
        });

    [UnscopedRef]
    public ref RootSignatureBuilder WithSampler(SamplerState state, ShaderVisibility visibility = ShaderVisibility.All, byte register = 0, byte space = 0) =>
        ref Add(new RootSignatureParameter
        {
            Type = RootSignatureParameterType.Sampler,
            Space = space,
            SamplerState = state,
            Register = register,
            Visibility = visibility
        });

    [UnscopedRef]
    public ref RootSignatureBuilder WithConstantBuffer(ConstantBufferFlags flags, ShaderVisibility visibility = ShaderVisibility.All, byte register = 0, byte space = 0) =>
        ref Add(new RootSignatureParameter
        {
            Type = RootSignatureParameterType.ConstantBuffer,
            Space = space,
            ConstantBufferFlags = flags,
            Register = register,
            Visibility = visibility
        });

    [UnscopedRef]
    public ref RootSignatureBuilder WithRanges(byte count, ShaderDescriptorRangeType type = ShaderDescriptorRangeType.ShaderResourceView, byte register = 0, byte space = 0) =>
        ref Add(new RootSignatureParameter
        {
            Type = RootSignatureParameterType.DescriptorRange,
            Space = space,
            Register = register,
            RangeType = type,
            Count = count
        });

    public CreateRootSignatureArgs Build() =>
        new()
        {
            Parameters = _parameters.AsReadOnlySpan()[.._numberOfParameters]
        };

    [UnscopedRef]
    public unsafe ref RootSignatureBuilder WithRootConstant<T>(ShaderVisibility visibility = ShaderVisibility.All, byte register = 0, byte space = 0) where T : unmanaged
    {
        var size = sizeof(T);
        Debug.Assert(size % 4 == 0, $"The size of type {typeof(T).Name} must be a multiple of 4 bytes. Current size = {size}");
        return ref WithConstant((byte)(size / 4), visibility, register, space);
    }

    [UnscopedRef]
    private ref RootSignatureBuilder Add(in RootSignatureParameter parameter)
    {
        _parameters[_numberOfParameters++] = parameter;
        return ref this;
    }
}
