using System.Runtime.InteropServices;
using Titan.Rendering.Resources;

namespace Titan.Assets.Types;

[StructLayout(LayoutKind.Sequential)]
public struct ShaderDescriptor
{
    public ShaderType Type;
}
