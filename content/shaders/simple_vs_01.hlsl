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
float3 RotateAroundAxis(float3 p, float3 axis, float angle)
{
    float3 rotatedPoint;
    float cosAngle = cos(angle);
    float sinAngle = sin(angle);
    float oneMinusCos = 1 - cosAngle;

    // Rotation matrix
    float3x3 rotationMatrix;
    rotationMatrix[0] = float3(cosAngle + axis.x * axis.x * oneMinusCos, axis.x * axis.y * oneMinusCos - axis.z * sinAngle, axis.x * axis.z * oneMinusCos + axis.y * sinAngle);
    rotationMatrix[1] = float3(axis.y * axis.x * oneMinusCos + axis.z * sinAngle, cosAngle + axis.y * axis.y * oneMinusCos, axis.y * axis.z * oneMinusCos - axis.x * sinAngle);
    rotationMatrix[2] = float3(axis.z * axis.x * oneMinusCos - axis.y * sinAngle, axis.z * axis.y * oneMinusCos + axis.x * sinAngle, cosAngle + axis.z * axis.z * oneMinusCos);

    rotatedPoint.x = dot(p, rotationMatrix[0]);
    rotatedPoint.y = dot(p, rotationMatrix[1]);
    rotatedPoint.z = dot(p, rotationMatrix[2]);

    return rotatedPoint;
}
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
