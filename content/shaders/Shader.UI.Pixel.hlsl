#define NUM_INPUTS 0
#include "Shader.Engine.Defaults.hlsli"
#include "Shader.UI.hlsli"

float4 main(UIVertexOutput input) : SV_TARGET
{
    UIElement element = UIElements[input.InstanceId];
    if(element.Type == UI_ELEMENT_COLOR)
    {
        return element.Color;
    }

    Texture2D texture =  Textures[element.TextureId];
    // sprite
    if(element.Type == UI_ELEMENT_SPRITE)
    {
        float4 color = texture.Sample(PointSampler, input.Texture);
        return element.Color * color;    
    }

    if(element.Type == UI_ELEMENT_TEXT)
    {
        float4 color = texture.Sample(LinearSampler, input.Texture);
        // font rendering
        if(color.a == 0)
        {
            discard;
        }
        
        return float4(element.Color.rgb,element.Color.a * color.r);
    }
    
    if(element.Type == UI_ELEMENT_NINE_PATCH_SPRITE)
    {
        // not supported in shader yet. doing it on CPU side.
    }

    // can't get repeating to work :|
    // float2 uv = input.Texture;
    // uv.x *= element.Repeat;

    // float2 repeatedUv = frac(uv);
    // float2 spriteUv = lerp(element.UVMin, element.UVMax, repeatedUv);
    // float4 color = texture.Sample(PointSampler, spriteUv);
    float4 color = texture.Sample(PointSampler, input.Texture);
    return color * element.Color;
     // TODO: see if we can remove branching.
}