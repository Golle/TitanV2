using System.Runtime.InteropServices;
using Titan.Graphics.Resources;

namespace Titan.Assets.Types;

[StructLayout(LayoutKind.Sequential)]
public struct ShaderDescriptor
{
    public ShaderType Type;
}

[StructLayout(LayoutKind.Sequential)]
public struct ShaderConfigDescriptor
{
    public byte NumberOfDescriptorRanges;
    public byte NumberOfConstantBuffers;
    public byte NumberOfSamplers;
    public byte NumberOfConstants;
}
