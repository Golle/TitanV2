#ifndef _SHADER_UI
#define _SHADER_UI

struct Glyph
{
    float2 MinUV;
    float2 MaxUV;
    uint Advance;
};

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
StructuredBuffer<Glyph> Glyphs : register(t0, space1);

#endif