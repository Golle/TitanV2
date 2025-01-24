#include "Shader.Engine.Defaults.hlsli"
#include "Shader.GBuffer.hlsli"

static const uint PositionOffset = 0;
static const uint UVOffset = 12;
static const uint NormalOffset = UVOffset + 8;
static const uint VertexSize = sizeof(Vertex);

Vertex LoadVertex(uint vertexId)
{
    uint index = IndexBuffer.Load(PassData.IndexOffset + vertexId * 4);
    uint offset = PassData.VertexOffset + index * VertexSize;

    Vertex v;
    v.Position = asfloat(GBuffer1.Load3(offset + PositionOffset));
    v.UV = asfloat(GBuffer1.Load2(offset + UVOffset));
    v.Normal = asfloat(GBuffer1.Load3(offset + NormalOffset));
    return v;
}


GBufferVertexOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID) 
{
    uint instanceOffset = PassData.InstanceOffset;

    MeshInstance instance = MeshInstances[instanceOffset + InstanceIdx];
    Vertex vertex = LoadVertex(VertexIdx);
    GBufferVertexOutput output;
    output.Position = mul(float4(vertex.Position, 1.0), mul(instance.ModelMatrix, FrameDataBuffer.ViewProjection));
    output.WorldPosition = mul(float4(vertex.Position, 1.0), instance.ModelMatrix).xyz;
    output.WorldNormal = normalize(mul(vertex.Normal, (float3x3)instance.ModelMatrix));
    output.Texture = vertex.UV;
    output.MaterialIndex = instance.MaterialIndex;

    return output;
}

