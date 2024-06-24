#include "common.hlsli"


VSPosTexColorOutput main(in uint VertexIdx : SV_VertexID, in uint InstanceIdx : SV_InstanceID)
{
    VSPosTexColorOutput output;

    output.Position = float4(0.0,0.0,0.0,0.0);
    return output;
}