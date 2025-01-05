#include "Shader.Debug.hlsli"

float4 main(DebugLineVertexOut input) : SV_TARGET
{
    return float4(input.Color, 1.0);
}