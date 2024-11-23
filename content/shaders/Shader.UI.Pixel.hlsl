#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"
#include "Shader.UI.hlsli"

float4 main(UIVertexOutput input) : SV_TARGET
{
    if(input.Type == 0)
    {
        return input.Color;
    }
    
    Texture2D texture =  Textures[input.TextureId];
    // sprite
    if(input.Type == 1)
    {
        float4 color = texture.Sample(PointSampler, input.Texture);
        return input.Color * color;    
    }

    float4 color = texture.Sample(LinearSampler, input.Texture);
    // font rendering
    if(color.a == 0)
    {
        discard;
    }
    
    return float4(input.Color.rgb, input.Color.a * color.r);
    // TODO: see if we can remove branching.
}