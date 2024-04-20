#include "sprite.hlsli"

float4 main(VSOutput input) : SV_TARGET
{
    // float4 color = float4(1.0,0.0,1.0,1.0); //input.Color; // tex.Sample(splr, textureCoords);

    float4 color = Textures[0].Sample(LinearSampler, input.Texture);
    // float4 color = input.Color;
    
    return color;
}