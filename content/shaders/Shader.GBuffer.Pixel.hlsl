#include "Shader.GBuffer.hlsli"
#include "Shader.Engine.Defaults.hlsli"

GBufferPixelOutput main(GBufferVertexOutput input)
{
    GBufferPixelOutput output;

    MeshInstance instance =  MeshInstances[PassData.MeshInstanceIndex];

    Texture2D albedoTexture =  Textures[instance.AlbedoIndex];
    output.Albedo = albedoTexture.Sample(PointSampler, input.Texture); // Alpha channel is free for other data
    output.Normal = float4(input.WorldNormal, 1.0); // Alpha channel is free for other data
    output.Specular = float4(0.0, 0.0, 1.0, 1.0);
    // output.Normal = albedoTexture.Sample(PointSampler, input.Texture);
    return output;
}