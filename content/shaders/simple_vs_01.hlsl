#include "sprite.hlsli"

VSOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID)
{
    float2 textureCoords = float2((VertexIdx << 1) & 2, VertexIdx & 2);
	float4 position = float4(textureCoords * float2(2, -2) + float2(-1, 1), 0, 1);

    VSOutput output;
    output.Position = position;
    output.Texture = textureCoords;
    output.Color = float4(1.0, 1.0, 0.8, 1.0);
    return output;
}