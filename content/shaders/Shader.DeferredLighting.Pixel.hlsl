#include "Shader.DeferredLighting.hlsli"

#define NUM_INPUTS 3
#include "Shader.Engine.Defaults.hlsli"

float4 main(FullScreenVertexOutput input): SV_TARGET0
{
    Texture2D albedo = GetInputTexture(AlbedoIndex);
    Texture2D normal = GetInputTexture(NormalIndex);
    Texture2D specular = GetInputTexture(SpecularIndex);

    return normal.Sample(PointSampler, input.Texture);
    // return float4(1.0, 1.0, 1.0, 1.0);
}