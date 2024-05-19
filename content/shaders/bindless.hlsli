Texture2D Textures[] : register(t0, space0);
ByteAddressBuffer BufferTable[] : register(t0, space1);
// ByteAddressBuffer VertexTable[] : register(t0, space2);


struct Vertex
{
    float3 Position;
    float2 UV;
};
StructuredBuffer<Vertex> GBuffer[] : register(t0, space2);


cbuffer TestData : register(b0, space0) {

    float4 WoofColor;
    matrix ViewProject;
    int TextureIndex;
    float time;
};