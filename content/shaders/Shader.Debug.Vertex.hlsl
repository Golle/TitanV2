#include "Shader.Debug.hlsli"

DebugLineVertexOut main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID) 
{
    Line l = Lines[InstanceIdx];

    DebugLineVertexOut output;
    output.Position = mul(float4(l.Values[VertexIdx], 1.0), FrameDataBuffer.ViewProjection);
    output.Color = l.Color[VertexIdx];
    return output;
}