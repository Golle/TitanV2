#include "Shader.GBuffer.hlsli"
#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"

GBufferVertexOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID) 
{
	// float2 uv = float2((VertexIdx << 1) & 2, VertexIdx & 2);
	// float4 pos = float4(uv * float2(2, -2) + float2(-1, 1), 0, 1);

    Vertex v = GBuffer[VertexIdx];
    
    GBufferVertexOutput output;
    output.Position = mul(float4(v.Position, 1.0), FrameDataBuffer.ViewProjection);
    output.Texture = v.UV;
    output.WorldPosition = mul(FrameDataBuffer.ViewProjection, float4(v.Position, 0)).xyz;
    output.WorldNormal = float3(0.0,0.0,0.0);
    
    return output;
}

