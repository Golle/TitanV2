#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"
#include "Shader.UI.hlsli"

float4 main(UIVertexOutput input) : SV_TARGET
{
    float4 color = input.Color;
    return color;
    // color.a = 1.0f;
    // color.r = 1.0f;
    // return color;
    return float4(1.0,0.0,1.0,0.8);
}