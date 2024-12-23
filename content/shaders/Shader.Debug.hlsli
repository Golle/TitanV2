#include "Shader.Engine.Defaults.hlsli"

struct DebugLineVertexOut
{
    float4 Position: SV_POSITION;
    float3 Color: COLOR;
};


struct Line {
    float3 Values[2];
    float3 Color[2];
};

StructuredBuffer<Line> Lines : register(t0, space0);