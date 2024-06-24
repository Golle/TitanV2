

struct VSPosTexColorOutput
{
    float4 Position : SV_Position;
    float2 Texture : TEXCOORD;
    float4 Color : COLOR;
};

/* GBuffer Structs */
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
