#ifndef _SHADER_DEFERRED_LIGHTING
#define _SHADER_DEFERRED_LIGHTING

#include "Shader.Engine.Defaults.hlsli"

#define PositionIndex 0
#define AlbedoIndex 1
#define NormalIndex 2
#define SpecularIndex 3


struct LightDrawData 
{
    int LightIndex;
};

struct Light 
{
    float3 Position;
    float3 Direction;
    float3 Color; // RGB (no Alpha channel)
    float IntensityOrRadius;
    float2 _padding;
};

StructuredBuffer<Light> LightBuffer : register(t0, space0);
ConstantBuffer<LightDrawData> PassData : register(b0, space0);

Light GetCurrentLight() {
    int index = PassData.LightIndex;
    return LightBuffer[index];
}

#endif