#include "bindless.hlsli"
#include "common.hlsli"

struct OutTemp
{
    float4 Position : SV_Position;
};

OutTemp main()
{
    OutTemp output;
    output.Position = float4(0.0,0.0,0.0,0.0);
    return output;
}