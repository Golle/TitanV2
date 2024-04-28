using System.Runtime.InteropServices;
using Titan.Graphics.Resources;

namespace Titan.Assets.Types;

[StructLayout(LayoutKind.Sequential)]
public struct ShaderDescriptor
{
    public ShaderType Type;
}
