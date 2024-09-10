#include "bindless.hlsli"
#include "static_samplers.hlsli"

// cbuffer PerDrawData : register(b1, space1)
// {
//     float2 ViewportSize; // do we need this?
//     float2 ViewportOffset;
//     float2 ViewportScale;
//     uint BufferIndex; // the offset in the SRV
//     uint LinearSampling; 
// }

struct VSOutput
{
    float4 Position : SV_Position;
    float2 Texture : TEXCOORD;
    float4 Color : COLOR;
    uint TextureId : TextureId;
};

struct SpriteInstanceData
{
    float2 Offset;
    float2 TextureSize;
    float2 Pivot;
    float2 Scale;
    float4 Color;
    float4 DrawRect;        //Type RectangleF in C#. struct {X,Y, Width, Height} 
    float2 SinCosRotation;
    uint TextureId;
};

StructuredBuffer<SpriteInstanceData> SpriteBuffers[] : register(t0, space2);