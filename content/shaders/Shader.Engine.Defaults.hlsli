#ifndef _SHADER_ENGINE_DEFAULT
#define _SHADER_ENGINE_DEFAULT

Texture2D Textures[] : register(t0, space10);

// nyi
Texture3D Textures3D[] : register(t0, space11);
ByteAddressBuffer BufferTable[] : register(t0, space12);
// ByteAddressBuffer VertexBufferTable[] : register(t0, space13);
ByteAddressBuffer IndexBufferTable[] : register(t0, space14);
// end nyi

SamplerState PointSampler : register(s0, space10);
SamplerState LinearSampler : register(s1, space10);

struct InputTextures
{
    int4 Indicies;
};

struct FrameData 
{
    float4x4 ViewProjection;
    float4 TESTColor;
};

ConstantBuffer<InputTextures> Inputs : register(b0, space10);
ConstantBuffer<FrameData> FrameDataBuffer:  register(b0, space11);

Texture2D GetInputTexture(uint index)
{
    // We only support 4 textures at the moment.
    int textureIndex =  Inputs.Indicies[index];
    return Textures[textureIndex];
}


/* Shared structs */

/* Full screen */

struct FullScreenVertexOutput 
{
    float4 Position : SV_POSITION;
    float2 Texture : TEXCOORD0;
};

#endif