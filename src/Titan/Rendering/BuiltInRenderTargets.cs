using Titan.Core.Maths;
using Titan.Core.Strings;

namespace Titan.Rendering;

public static class BuiltInRenderTargets
{
    public static readonly RenderTargetConfig GBufferAlbedo = new(StringRef.Create("GBuffer_Albedo"), RenderTargetFormat.RGBA8, Color.Yellow);
    public static readonly RenderTargetConfig GBufferNormal = new(StringRef.Create("GBuffer_Normal"), RenderTargetFormat.RGBA8, Color.Magenta);
    public static readonly RenderTargetConfig GBufferSpecular = new(StringRef.Create("GBuffer_Specular"), RenderTargetFormat.RGBA8, Color.Magenta);
    public static readonly RenderTargetConfig DeferredLighting = new(StringRef.Create("DeferredLighting"), RenderTargetFormat.RGBA8, Color.Magenta);
    public static readonly RenderTargetConfig Backbuffer = new(StringRef.Create("Backbuffer"), RenderTargetFormat.BackBuffer, Color.Magenta);
}

public static class BuiltInDepthsBuffers
{
    public static readonly DepthBufferConfig GbufferDepthBuffer = new(StringRef.Create("GBuffer_Depth"), DepthBufferFormat.D32);
}
