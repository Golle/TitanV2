#include "common.hlsli"

FullScreenVertexOutput main( uint id : SV_VertexID )
{
	float2 uv = float2((id << 1) & 2, id & 2);
	float4 pos = float4(uv * float2(2, -2) + float2(-1, 1), 0, 1);

	FullScreenVertexOutput output;
	output.Position = pos;
	output.Texture = uv;
	return output;
}