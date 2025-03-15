#include "Shader.Engine.Defaults.hlsli"

float4  main(FullScreenVertexOutput input) : SV_Target
{
    Texture2D normalTexture = GetInputTexture(0);
    Texture2D depthTexture = GetInputTexture(1);

    // Sample depth & normal
    float depth = depthTexture.Sample(LinearSampler, input.Texture).r;
    float3 normal = normalTexture.Sample(LinearSampler, input.Texture).rgb * 2.0 - 1.0;

    // Compute occlusion (simplified approach)
    float occlusion = 1.0;
    for (int i = 0; i < 8; i++)
    {
        float2 offset = float2(sin(i), cos(i)) * 0.01;
        float sampleDepth = depthTexture.Sample(LinearSampler, input.Texture + offset).r;
        
        if (sampleDepth < depth)
        {
            occlusion -= 0.125; // Reduce occlusion if sample is closer
        }
    }

    return float4(occlusion, occlusion, occlusion, 1.0);
}