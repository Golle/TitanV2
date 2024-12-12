#include "Shader.Engine.Defaults.hlsli"
#include "Shader.GBuffer.hlsli"

GBufferVertexOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID) 
{
	// float2 uv = float2((VertexIdx << 1) & 2, VertexIdx & 2);
	// float4 pos = float4(uv * float2(2, -2) + float2(-1, 1), 0, 1);

    Vertex v = GBuffer[VertexIdx];
    uint offset = PassData.InstanceOffset;
    MeshInstance instance = MeshInstances[offset+InstanceIdx];
    
    GBufferVertexOutput output;
    output.Position = mul(float4(v.Position, 1.0), mul(instance.ModelMatrix, FrameDataBuffer.ViewProjection));
    output.WorldPosition = mul(float4(v.Position, 1.0), instance.ModelMatrix).xyz;
    output.WorldNormal = normalize(mul(v.Normal, (float3x3)instance.ModelMatrix));
    output.Texture = v.UV;
    output.MaterialIndex = instance.MaterialIndex;

    return output;
}

