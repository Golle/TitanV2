#ifndef _SHADER_GBUFFER
#define _SHADER_GBUFFER

struct RootConstant 
{
    uint VertexBufferIndex;
    uint MeshInstancesIndex;
};

struct MeshStuff 
{
    uint MeshInstanceId;
};

struct MeshInstance 
{
    float4x4 ModelMatrix;
    float4 TestColor;
    // uint MaterialIndex;
};

struct Vertex
{
    float3 Position;
    float2 UV;
};

ConstantBuffer<RootConstant> Root : register(b0, space0);
ConstantBuffer<MeshStuff> MeshStuffs : register(b1, space0);
StructuredBuffer<Vertex> GBuffer[] : register(t0, space13);
StructuredBuffer<MeshInstance> MeshInstances[] : register(t0, space15);


struct GBufferVertexOutput
{
    float4 Position : SV_POSITION;
    float3 WorldPosition : TEXCOORD0;
    float3 WorldNormal : TEXCOORD1;
    float2 Texture : TEXCOORD2;
};

struct GBufferPixelOutput
{
    float4 Albedo : SV_Target0;
    float4 Normal : SV_Target1;
    float4 Specular : SV_Target2;
};

struct MaterialData 
{
    uint AlbedoIndex;
};

#endif