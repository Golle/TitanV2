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
    public byte NumberOfDescriptors;
    public byte NumberOfParameters;
    public byte NumberOfSamplers;
}
