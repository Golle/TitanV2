#include "Shader.GBuffer.hlsli"
#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"

// GBufferVertexOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID) 
GBufferVertexOutput main( uint id : SV_VertexID )
{
	float2 uv = float2((id << 1) & 2, id & 2);
	float4 pos = float4(uv * float2(2, -2) + float2(-1, 1), 0, 1);

    GBufferVertexOutput output;

    output.Position = pos;
    output.WorldPosition = float3(0.0,0.0,0.0);
    output.WorldNormal = float3(0.0,0.0,0.0);
    output.Texture = uv;
    return output;
}