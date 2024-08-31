#include "sprite.hlsli"

// VSOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID)
// {
//     float2 textureCoords = float2((VertexIdx << 1) & 2, VertexIdx & 2);
// 	float4 position = float4(textureCoords * float2(2, -2) + float2(-1, 1), 0, 1);

//     VSOutput output;
//     output.Position = position * float4(0.2, 0.1, 1.0, 1.0);
//     output.Texture = textureCoords;
//     output.Color =  float4(1.0, 1.0, 1.0, 1.0);
//     return output;
// }

VSOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID)
{
    // float2 textureCoords = float2((VertexIdx << 1) & 2, VertexIdx & 2);
	// float4 position = float4(textureCoords * float2(2, -2) + float2(-1, 1), 0, 1);


// Apply rotation to vertex position
    float angle = time; // Use time parameter for rotation animation
    float3 center = float3(0, 0, 0); // Center of rotation (adjust as needed)
    

    Vertex  v = GBuffer[2][VertexIdx];
    // float3 pos = normalize(v.Position);
    float3 pos = v.Position;
    //float3 rotatedPosition = RotateAroundAxis(pos, float3(1, 0, 0), angle);
    float3 rotatedPosition = RotateAroundAxis(pos,  float3(0, 1, 0), angle);
    // rotatedPosition = RotateAroundAxis(rotatedPosition,  float3(0, 0, 1), angle);
// float3 rotatedPosition = RotateAroundPoint(v.Position, center, float3(1, 0, 0), angle);
    VSOutput output;
    output.Position = mul(float4(rotatedPosition,1), ViewProject);// float4(v.Position, 1.0);
    // output.Position = position;
    output.Texture = v.UV;
    output.Color =  float4(1.0, 1.0, 1.0, 1.0);
    return output;
}
