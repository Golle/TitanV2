#include "Shader.DeferredLighting.hlsli"

LightsVertexOutput main( uint id : SV_VertexID, uint InstanceIdx : SV_InstanceID )
{
	float2 uv = float2((id << 1) & 2, id & 2);
	float4 pos = float4(uv * float2(2, -2) + float2(-1, 1), 0, 1);

	LightsVertexOutput output;
	output.Position = pos;
	output.Texture = uv;
	output.InstanceId = InstanceIdx;
	return output;
}