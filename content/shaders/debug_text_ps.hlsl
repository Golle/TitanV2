#include "common.hlsli"

float4 main(VSPosTexColorOutput input) : SV_TARGET
{
    return float4(1.0, 0.5, 0.5, 1.0);
}