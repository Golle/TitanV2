#ifndef _SHADER_UI
#define _SHADER_UI

struct UIElement 
{
    float4 Color;
    float2 Size;
    float2 Offset;
    float2 UVMin;
    float2 UVMax;
    int TextureId;
    int Type;
    float Repeat;
    float1 Padding;
};

struct UIVertexOutput 
{
    float4 Position : SV_POSITION;
    float2 Texture : TEXCOORD0;
    int TextureId : TEXID;
    uint InstanceId : UI_ELEMENT_ID;
};


StructuredBuffer<UIElement> UIElements : register(t0, space0);

#endif