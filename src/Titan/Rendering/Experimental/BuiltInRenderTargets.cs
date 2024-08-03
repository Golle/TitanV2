using Titan.Core.Strings;

namespace Titan.Rendering.Experimental;

public static class BuiltInRenderTargets
{
    public static readonly RenderTarget GBufferAlbedo = new(StringRef.Create("GBuffer_Albedo"), RenderTargetFormat.RGBA8);
    public static readonly RenderTarget GBufferNormal = new(StringRef.Create("GBuffer_Normal"), RenderTargetFormat.RGBA8);
    public static readonly RenderTarget GBufferSpecular = new(StringRef.Create("GBuffer_Specular"), RenderTargetFormat.RGBA8);
    public static readonly RenderTarget GBufferDepth = new(StringRef.Create("GBuffer_Depth"), RenderTargetFormat.RGBA8);

    public static readonly RenderTarget DeferredLighting= new(StringRef.Create("DeferredLighting"), RenderTargetFormat.RGBA8);

    public static readonly RenderTarget Backbuffer = new(StringRef.Create("Backbuffer"), RenderTargetFormat.BackBuffer);
}
