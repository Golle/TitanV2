#ifndef _SHADER_DEFERRED_LIGHTING
#define _SHADER_DEFERRED_LIGHTING

#include "Shader.Engine.Defaults.hlsli"

#define PositionIndex 0
#define AlbedoIndex 1
#define NormalIndex 2
#define SpecularIndex 3


struct LightsVertexOutput 
{
    float4 Position : SV_POSITION;
    float2 Texture : TEXCOORD0;
    uint InstanceId : INSTANCE_ID;
};


struct LightInstanceData
{
    float3 Position;
    float3 Direction;
    float3 Color; // RGB (no Alpha channel)
    float IntensityOrRadius;
    float2 _padding;
};

StructuredBuffer<LightInstanceData> LightBuffer : register(t0, space0);

LightInstanceData GetLight(uint index) {
    return LightBuffer[index];
}



#endif