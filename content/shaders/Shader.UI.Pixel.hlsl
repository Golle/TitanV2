#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"

float4 main(FullScreenVertexOutput input) : SV_TARGET
{
    return float4(1.0,1.0,1.0,0.8);
}