#include "sprite.hlsli"

VSOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID)
{

    float2 position = 0.0f;
    float2 texCoords = float2(0.0f, 1.0f);
    if(VertexIdx == 1)
    {
        position = float2(0.0f, 1.0f);
        texCoords = float2(0.0f, 0.0f);
    }
    else if(VertexIdx == 2)
    {
        position = float2(1.0f, 1.0f);
        texCoords = float2(1.0f, 0.0f);
    }
    else if(VertexIdx == 3)
    {
        position = float2(1.0f, 0.0f);
        texCoords = float2(1.0f, 1.0f);
    }
    
    VSOutput output;
    output.Position = float4(position, 1.0, 1.0);
    output.Texture = texCoords;
    output.Color = float4(1.0, 0.5, 0.8, 1.0);
    return output;
}