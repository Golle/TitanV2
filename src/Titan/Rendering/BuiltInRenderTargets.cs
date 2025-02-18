using Titan.Core.Maths;
using Titan.Core.Strings;

namespace Titan.Rendering;

public static class BuiltInRenderTargets
{
    public static readonly RenderTargetConfig GBufferPosition = new(StringRef.Create("GBuffer_Position"), RenderTargetFormat.RGBA32F, Color.Black);
    public static readonly RenderTargetConfig GBufferAlbedo = new(StringRef.Create("GBuffer_Albedo"), RenderTargetFormat.RGBA8, Color.Black);
    public static readonly RenderTargetConfig GBufferNormal = new(StringRef.Create("GBuffer_Normal"), RenderTargetFormat.RGBA8, Color.Black);
    public static readonly RenderTargetConfig GBufferSpecular = new(StringRef.Create("GBuffer_Specular"), RenderTargetFormat.RGBA8, Color.Magenta);
    public static readonly RenderTargetConfig DeferredLighting = new(StringRef.Create("DeferredLighting"), RenderTargetFormat.RGBA8, Color.Transparent);
    public static readonly RenderTargetConfig PostProcessing = new(StringRef.Create("PostProcessing"), RenderTargetFormat.RGBA8, Color.Transparent);
    public static readonly RenderTargetConfig Backbuffer = new(StringRef.Create("Backbuffer"), RenderTargetFormat.BackBuffer, Color.Magenta);
    public static readonly RenderTargetConfig UI = new(StringRef.Create("UI"), RenderTargetFormat.RGBA8, Color.Transparent);
    public static readonly RenderTargetConfig Debug = new(StringRef.Create("Debug"), RenderTargetFormat.RGBA8, Color.Transparent);
}

public static class BuiltInDepthsBuffers
{
    public static readonly DepthBufferConfig GbufferDepthBuffer = new(StringRef.Create("GBuffer_Depth"), DepthBufferFormat.D32);
    public static readonly DepthBufferConfig ShadowMapDepthBuffer = new(StringRef.Create("ShadowMap_Depth"), DepthBufferFormat.D32, ClearValue: 1f, ShaderVisible: true);
}
