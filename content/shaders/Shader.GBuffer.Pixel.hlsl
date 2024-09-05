#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"
#include "Shader.GBuffer.hlsli"

GBufferPixelOutput main(GBufferVertexOutput input)
{
    GBufferPixelOutput output;

    MeshInstance instance =  MeshInstances[PassData.MeshInstanceIndex];

    Texture2D albedoTexture =  Textures[instance.AlbedoIndex];
    output.Albedo = albedoTexture.Sample(PointSampler, input.Texture);
    output.Normal = float4(0.0, 1.0, 0.0, 1.0);
    output.Specular = float4(0.0, 0.0, 1.0, 1.0);
    return output;
}