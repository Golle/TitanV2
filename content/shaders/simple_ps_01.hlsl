#include "sprite.hlsli"

float4 main(VSOutput input) : SV_TARGET
{
    // float4 color = float4(1.0,0.0,1.0,1.0); //input.Color; // tex.Sample(splr, textureCoords);
//  return 1;
    float4 color = Textures[TextureIndex].Sample(LinearSampler, input.Texture);
    return color *WoofColor * input.Color;
}