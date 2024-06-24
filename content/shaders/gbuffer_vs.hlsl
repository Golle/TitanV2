#include "bindless.hlsli"
#include "common.hlsli"



GBufferVertexOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID) 
{
    GBufferVertexOutput output;

    output.Position = float4(0.0,0.0,0.0,0.0);
    return output;
}