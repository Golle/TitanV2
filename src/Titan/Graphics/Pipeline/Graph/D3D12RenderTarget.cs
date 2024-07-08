using Titan.Core;
using Titan.Core.Strings;
using Titan.Platform.Win32.DXGI;

namespace Titan.Graphics.Pipeline.Graph;

internal struct D3D12RenderTarget
{
    public Handle<Texture> Resource;
    public DXGI_FORMAT Format;

    //not used yet. Scaling render target sizes.
    public float X;
    public float Y;

    public StringRef Identifier;
}