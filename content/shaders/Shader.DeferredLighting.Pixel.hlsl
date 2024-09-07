#include "Shader.Engine.Defaults.hlsli"
#include "Shader.DeferredLighting.hlsli"

float4 main(FullScreenVertexOutput input): SV_TARGET0
{
    Texture2D albedo = GetInputTexture(AlbedoIndex);
    Texture2D normal = GetInputTexture(NormalIndex);
    Texture2D specular = GetInputTexture(2);

    return albedo.Sample(PointSampler, input.Texture);
}