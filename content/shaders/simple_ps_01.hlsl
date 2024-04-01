#include "sprite.hlsli"

float4 main(VSOutput input) : SV_TARGET
{
    float4 color = input.Color; // tex.Sample(splr, textureCoords);
    
    return color;
}