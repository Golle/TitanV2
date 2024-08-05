#include "common.hlsli"

#define NUM_INPUTS 1
#include "Shader.Engine.Defaults.hlsli"

struct BackbufferTest
{
    float4 Color;
};

ConstantBuffer<BackbufferTest> RootConstantBuffer : register(b1, space7);

float4 main(FullScreenVertexOutput input) : SV_TARGET
{
    Texture2D tex = GetInputTexture(0);
    float4 c = tex.Sample(PointSampler, input.Texture);
    float4 v = RootConstantBuffer.Color;
    return v;
}