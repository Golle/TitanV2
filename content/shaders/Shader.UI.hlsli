#ifndef _SHADER_UI
#define _SHADER_UI

#define UI_ELEMENT_COLOR 0
#define UI_ELEMENT_SPRITE 1
#define UI_ELEMENT_NINE_PATCH_SPRITE 2
#define UI_ELEMENT_TEXT 3
#define UI_ELEMENT_SPRITE_REPEAT 4


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