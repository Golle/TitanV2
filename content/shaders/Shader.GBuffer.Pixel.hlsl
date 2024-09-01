#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"
#include "Shader.GBuffer.hlsli"

GBufferPixelOutput main(GBufferVertexOutput input)
{
    GBufferPixelOutput output;

    MeshInstance instance =  MeshInstances[PassData.MeshInstancesIndex][1]; //MeshInstances[Root.MeshInstancesIndex][MeshStuffs.MeshInstanceId];

    Texture2D albedoTexture =  Textures[instance.AlbedoIndex];
    float4  albedo = albedoTexture.Sample(PointSampler, input.Texture);
    // output.Albedo = float4(1.0, 0.6, 0.2, 1.0);
    output.Albedo = float4(0.0,1.0,1.0,1.0);
    output.Albedo = albedo;
    // output.Albedo = float4(instance.TestColor.r, instance.TestColor.g, instance.TestColor.b, 1.0);
    // output.Albedo = instance.TestColor;
    // output.Albedo = FrameDataBuffer.TESTColor;
    output.Normal = float4(0.0, 1.0, 0.0, 1.0);
    output.Specular = float4(0.0, 0.0, 1.0, 1.0);
    return output;
}