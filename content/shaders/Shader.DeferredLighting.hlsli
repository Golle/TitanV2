#ifndef _SHADER_DEFERRED_LIGHTING
#define _SHADER_DEFERRED_LIGHTING

#include "Shader.Engine.Defaults.hlsli"

#define PositionIndex 0
#define AlbedoIndex 1
#define NormalIndex 2
#define SpecularIndex 3
#define ShadowMapIndex 4


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
    int type;
    float _padding;
    float4x4 LightViewProj;
};


struct DeferredLightingPassData {
    float3 AmbientLight;
};

StructuredBuffer<LightInstanceData> LightBuffer : register(t0, space0);
ConstantBuffer<DeferredLightingPassData> PassData : register(b0, space0);

LightInstanceData GetLight(uint index) {
    return LightBuffer[index];
}

float3 GetAmbientLight()
{
    return PassData.AmbientLight;
}



#endif