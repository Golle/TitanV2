#define NUM_INPUTS 1
#include "Shader.Engine.Defaults.hlsli"

struct BackbufferTest
{
    uint TextureIndex;
};

ConstantBuffer<BackbufferTest> RootConstantBuffer : register(b1, space7);

float4 main(FullScreenVertexOutput input) : SV_TARGET
{
    Texture2D world = GetInputTexture(RootConstantBuffer.TextureIndex);
    float4 color = world.Sample(PointSampler, input.Texture);
    return color;
}