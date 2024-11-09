#ifndef _SHADER_UI
#define _SHADER_UI

struct UIElement 
{
    float4 Color;
    float2 Size;
    float2 Offset;
};

struct UIVertexOutput 
{
    float4 Position : SV_POSITION;
    float2 Texture : TEXCOORD0;
    float4 Color : COLOR;
};


StructuredBuffer<UIElement> UIElements : register(t0, space0);

#endif