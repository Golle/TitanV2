#include "bindless.hlsli"
#include "common.hlsli"

struct OutTemp
{
    float4 Position : SV_Position;
};

float4 main(OutTemp input): SV_TARGET
{
    return float4(1.0, 1.0, 1.0, 1.0);
}