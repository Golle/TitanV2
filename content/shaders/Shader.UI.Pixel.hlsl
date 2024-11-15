#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"
#include "Shader.UI.hlsli"

float4 main(UIVertexOutput input) : SV_TARGET
{
    if(input.TextureId == 0)
    {
        return input.Color;
    }

    // TODO: see if we can remove branching.
    
    Texture2D texture =  Textures[input.TextureId];
    float alpha = texture.Sample(LinearSampler, input.Texture).r;
    if(alpha == 0){
        discard;
    }

    float4 final = float4(input.Color.rgb, input.Color.a * alpha);

    return final;
}