#include "bindless.hlsli"
#include "common.hlsli"

GBufferPixelOutput main(GBufferVertexOutput input)
{
    GBufferPixelOutput output;
    output.Albedo = float4(1.0, 0.0, 0.0, 1.0);
    output.Normal = float4(0.0, 0.0, 0.0, 1.0);
    output.Specular = float4(0.0, 0.0, 0.0, 1.0);
    return output;
}