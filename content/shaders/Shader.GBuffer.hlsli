#ifndef _SHADER_GBUFFER
#define _SHADER_GBUFFER

struct GBufferDrawData 
{
    uint MeshInstanceIndex;
};

struct MeshInstance
{
    float4x4 ModelMatrix;
    // should be material
    int AlbedoIndex;
    float3 _padding;
};

struct Vertex
{
    float3 Position;
    float2 UV;
    float3 Normal;
};


ConstantBuffer<GBufferDrawData> PassData : register(b0, space0);

StructuredBuffer<Vertex> GBuffer : register(t0, space0);
StructuredBuffer<uint> IndexBuffer : register(t0, space1);
StructuredBuffer<MeshInstance> MeshInstances : register(t0, space2);

struct GBufferVertexOutput
{
    float4 Position : SV_POSITION;
    float3 WorldPosition : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float2 Texture : TEXCOORD2;
};

struct GBufferPixelOutput
{
    float4 Position: SV_TARGET0;
    float4 Albedo : SV_Target1;
    float4 Normal : SV_Target2;
    float4 Specular : SV_Target3;
};

//todo: implement later
// struct MaterialData 
// {
//     uint AlbedoIndex;
// };

#endif